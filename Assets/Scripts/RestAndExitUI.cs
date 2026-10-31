
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(500)]
public class RestAndExitUI : MonoBehaviour
{
    [Header("Oyun Sistemi")]
    public MiningManager miningManager;

    [Header("Yuzeye Don Butonu")]
    public Button normalExitButton;

    [Header("Eski Butonlar (Kaldirilacak)")]
    public Button restButton;
    public Button emergencyExitButton;

    [Header("Eski Dinlenme Paneli")]
    public GameObject restOverlay;

    [Header("Sahne")]
    public string surfaceSceneName = "SurfaceHub";

    private EnergyRecovery energyRecovery;

    private void Start()
    {
        // Eski gereksiz arayuzleri kapat.
        if (restButton != null)
            restButton.gameObject.SetActive(false);

        if (emergencyExitButton != null)
            emergencyExitButton.gameObject.SetActive(false);

        if (restOverlay != null)
            restOverlay.SetActive(false);

        GameDataManager data =
            GameDataManager.GetInstance();

        energyRecovery =
            data.GetComponent<EnergyRecovery>();

        if (energyRecovery == null)
        {
            energyRecovery =
                data.gameObject
                    .AddComponent<EnergyRecovery>();
        }

        energyRecovery.Bind(miningManager);

        if (normalExitButton != null)
            normalExitButton.interactable = true;
    }

    // YUZEYE DON BUTONU
    public void GoToSurface()
    {
        if (miningManager == null)
        {
            Debug.LogError(
                "MiningManager baglantisi eksik!"
            );

            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(
            surfaceSceneName))
        {
            Debug.LogError(
                "SurfaceHub sahnesi Build Profiles " +
                "Scene List icinde bulunamadi!"
            );

            return;
        }

        GameDataManager data =
            GameDataManager.GetInstance();

        // Yuzeye donmek enerji harcamaz.
        // Para, enerji, canta ve ilerlemeyi
        // mevcut GameDataManager'a kaydet.
        data.SaveFromMining(
            miningManager,
            data.inventory
        );

        if (energyRecovery != null)
            energyRecovery.SaveNow();

        // Canta satilmaz.
        // Enerji kesilmez.
        // Mevcut maden haritasi korunur.
        SceneManager.LoadScene(
            surfaceSceneName
        );
    }
}
