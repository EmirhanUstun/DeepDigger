
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnergyRecovery : MonoBehaviour
{
    [Header("Sahne")]
    public string surfaceSceneName = "SurfaceHub";

    [Header("Yuzeyde Enerji")]
    [Min(1f)]
    public float surfaceInterval = 10f;

    [Header("Oyun Kapaliyken Enerji")]
    [Min(1f)]
    public float offlineInterval = 15f;

    [Header("Her Seferinde Kazanilan Enerji")]
    [Min(1)]
    public int energyPerTick = 5;

    private GameDataManager data;
    private MiningManager miningManager;

    private float surfaceTimer;
    private float saveTimer;
    private float offlineRemainder;

    private bool inBackground;
    private bool hasEnergyState;
    private bool pendingFirstBind;

    private int restoredEnergy;
    private int lastSavedEnergy;

    // Onceki EnergyRecovery anahtarlariyla uyumlu.
    private const string EnergyKey =
        "DD_Recovery_Energy_v1";

    private const string TimeKey =
        "DD_Recovery_LastUTC_v1";

    private const string RemainderKey =
        "DD_Recovery_Remainder_v1";

    // Oyun hangi sahneden acilirsa acilsin
    // enerji sistemi olusturulur.
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        GameDataManager manager =
            GameDataManager.GetInstance();

        if (manager.GetComponent<EnergyRecovery>() == null)
        {
            manager.gameObject
                .AddComponent<EnergyRecovery>();
        }
    }

    private void Awake()
    {
        data = GetComponent<GameDataManager>();

        if (data == null)
        {
            Debug.LogError(
                "EnergyRecovery, GameDataManager " +
                "nesnesinde bulunmali."
            );

            enabled = false;
            return;
        }

        // Onceki oturumun enerjisini yukle.
        if (ReadOfflineRecovery())
        {
            hasEnergyState = true;
            pendingFirstBind = true;
            restoredEnergy = data.energy;
        }

        SaveNow();
    }

    // MainGame sahnesindeki MiningManager ile baglanti.
    public void Bind(MiningManager manager)
    {
        if (manager == null || data == null)
            return;

        miningManager = manager;

        data.maxEnergy = Mathf.Max(
            1,
            manager.maxEnergy
        );

        if (pendingFirstBind)
        {
            // Ilk acilista kalici kayittan gelen
            // enerji, sahnenin varsayilan
            // enerjisinden onceliklidir.
            data.energy = Mathf.Clamp(
                restoredEnergy,
                0,
                data.maxEnergy
            );

            pendingFirstBind = false;
        }
        else if (!hasEnergyState)
        {
            // Oyunun ilk kez baslatilmasi.
            data.energy = Mathf.Clamp(
                manager.energy,
                0,
                data.maxEnergy
            );

            hasEnergyState = true;
        }
        else
        {
            // Yuzeyden kazıya donus.
            data.energy = Mathf.Clamp(
                data.energy,
                0,
                data.maxEnergy
            );
        }

        manager.energy = data.energy;

        // Mevcut para, canta ve ilerleme
        // verilerini koru.
        data.SaveFromMining(
            manager,
            data.inventory
        );

        SaveNow();
    }

    private void Update()
    {
        if (data == null || inBackground)
            return;

        bool isSurface = IsSurfaceScene();

        if (isSurface)
        {
            RecoverOnSurface();
        }
        else
        {
            // Madende veya diger sahnelerde
            // otomatik enerji yenilenmez.
            surfaceTimer = 0f;

            if (miningManager != null)
            {
                data.maxEnergy = Mathf.Max(
                    1,
                    miningManager.maxEnergy
                );

                data.energy = Mathf.Clamp(
                    miningManager.energy,
                    0,
                    data.maxEnergy
                );
            }
        }

        // Oyun beklenmedik sekilde kapanirsa
        // veri kaybini azaltmak icin otomatik kayit.
        saveTimer += Time.unscaledDeltaTime;

        if (data.energy != lastSavedEnergy ||
            saveTimer >= 5f)
        {
            SaveNow();
            saveTimer = 0f;
        }
    }

    private void RecoverOnSurface()
    {
        if (data.energy >= data.maxEnergy)
        {
            surfaceTimer = 0f;
            return;
        }

        surfaceTimer += Time.unscaledDeltaTime;

        if (surfaceTimer < surfaceInterval)
            return;

        int ticks = Mathf.FloorToInt(
            surfaceTimer / surfaceInterval
        );

        surfaceTimer -=
            ticks * surfaceInterval;

        data.energy = Mathf.Min(
            data.maxEnergy,
            data.energy + ticks * energyPerTick
        );

        if (data.energy >= data.maxEnergy)
            surfaceTimer = 0f;

        // Uygulama dogrudan yuzey sahnesinden
        // acildiyse ilk baglanti icin sakla.
        if (pendingFirstBind)
            restoredEnergy = data.energy;

        SaveNow();
    }

    // Uygulama kapaliyken veya arka plandayken
    // gecen gercek sureyi hesapla.
    private bool ReadOfflineRecovery()
    {
        if (!PlayerPrefs.HasKey(EnergyKey))
            return false;

        int savedEnergy =
            PlayerPrefs.GetInt(
                EnergyKey,
                data.energy
            );

        data.energy = Mathf.Clamp(
            savedEnergy,
            0,
            Mathf.Max(1, data.maxEnergy)
        );

        offlineRemainder = Mathf.Max(
            0f,
            PlayerPrefs.GetFloat(
                RemainderKey,
                0f
            )
        );

        string storedTime =
            PlayerPrefs.GetString(TimeKey, "");

        long ticks;

        if (!long.TryParse(
            storedTime,
            out ticks))
        {
            return true;
        }

        if (ticks < DateTime.MinValue.Ticks ||
            ticks > DateTime.MaxValue.Ticks)
        {
            return true;
        }

        DateTime lastTime =
            new DateTime(
                ticks,
                DateTimeKind.Utc
            );

        double elapsedSeconds = Math.Max(
            0d,
            (DateTime.UtcNow - lastTime)
                .TotalSeconds
        );

        double totalSeconds =
            elapsedSeconds + offlineRemainder;

        int missingEnergy = Mathf.Max(
            0,
            data.maxEnergy - data.energy
        );

        double possibleGain =
            Math.Floor(
                totalSeconds / offlineInterval
            ) * energyPerTick;

        int gainedEnergy = (int)Math.Min(
            missingEnergy,
            possibleGain
        );

        data.energy = Mathf.Min(
            data.maxEnergy,
            data.energy + gainedEnergy
        );

        if (data.energy >= data.maxEnergy)
        {
            offlineRemainder = 0f;
        }
        else
        {
            offlineRemainder = (float)(
                totalSeconds % offlineInterval
            );
        }

        return true;
    }

    // Sahne degismeden veya uygulama
    // arka plana gitmeden once kaydet.
    public void SaveNow()
    {
        if (data == null)
            return;

        bool isSurface = IsSurfaceScene();

        if (!isSurface && miningManager != null)
        {
            data.energy = Mathf.Clamp(
                miningManager.energy,
                0,
                Mathf.Max(1, miningManager.maxEnergy)
            );
        }

        PlayerPrefs.SetInt(
            EnergyKey,
            data.energy
        );

        PlayerPrefs.SetString(
            TimeKey,
            DateTime.UtcNow.Ticks.ToString()
        );

        PlayerPrefs.SetFloat(
            RemainderKey,
            offlineRemainder
        );

        PlayerPrefs.Save();

        lastSavedEnergy = data.energy;
    }

    private void EnterBackground()
    {
        if (inBackground || data == null)
            return;

        SaveNow();

        inBackground = true;
    }

    private void ReturnFromBackground()
    {
        if (!inBackground || data == null)
            return;

        inBackground = false;

        if (ReadOfflineRecovery())
        {
            hasEnergyState = true;

            if (pendingFirstBind)
                restoredEnergy = data.energy;

            if (miningManager != null)
                miningManager.energy = data.energy;
        }

        SaveNow();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            EnterBackground();
        else
            ReturnFromBackground();
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused)
            EnterBackground();
        else
            ReturnFromBackground();
    }

    private void OnApplicationQuit()
    {
        if (!inBackground)
            SaveNow();
    }

    private bool IsSurfaceScene()
    {
        string scene = 
        SceneManager.GetActiveScene().name;

        return
            scene == "SurfaceHub" ||
            scene == "ShopScene" ||
            scene == "CollectionScene" ||
            scene == "UpgradeScene";
        }
}
