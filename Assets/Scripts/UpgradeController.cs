
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class UpgradeController : MonoBehaviour
{
    [Header("Para Gostergeleri")]
    public TMP_Text goldText;
    public TMP_Text diamondText;

    [Header("Kazma Yukseltmesi")]
    public TMP_Text pickaxeLevelText;
    public TMP_Text pickaxePriceText;
    public Button pickaxeUpgradeButton;

    [Header("Canta Yukseltmesi")]
    public TMP_Text bagCapacityText;
    public TMP_Text bagPriceText;
    public Button bagUpgradeButton;

    [Header("Enerji Yukseltmesi")]
    public TMP_Text energyCapacityText;
    public TMP_Text energyPriceText;
    public Button energyUpgradeButton;

    [Header("Bildirim")]
    public TMP_Text feedbackText;

    [Header("Yukseltme Ayarlari")]
    public int bagIncrease = 5;
    public int energyIncrease = 20;

    public int maxPickaxePower = 10;
    public int maxBagCapacity = 100;
    public int maxEnergyCapacity = 300;

    private GameDataManager data;

    // ====================================
    // BASLANGIC
    // ====================================

    private void Awake()
    {
        data = GameDataManager.GetInstance();
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged +=
            OnLanguageChanged;

        RefreshUI();
    }

    private void Start()
    {
        RefreshUI();

        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -=
            OnLanguageChanged;
    }

    // ====================================
    // DIL SISTEMI
    // ====================================

    private void OnLanguageChanged(Locale locale)
    {
        RefreshUI();
    }

    private bool IsEnglish()
    {
        Locale locale =
            LocalizationSettings.SelectedLocale;

        return locale != null &&
            locale.Identifier.Code.StartsWith("en");
    }

    private string Translate(string tr, string en)
    {
        return IsEnglish() ? en : tr;
    }

    // ====================================
    // YUKSELTME FIYATLARI
    // ====================================

    private int GetPickaxePrice()
    {
        int level = Mathf.Max(
            1,
            data.pickaxePower
        );

        return 250 + (level - 1) * 200;
    }

    private int GetBagPrice()
    {
        int increase = Mathf.Max(
            1,
            bagIncrease
        );

        int upgrades = Mathf.Max(
            0,
            (data.bagCapacity - 20) / increase
        );

        return 200 + upgrades * 150;
    }

    private int GetEnergyPrice()
    {
        int increase = Mathf.Max(
            1,
            energyIncrease
        );

        int upgrades = Mathf.Max(
            0,
            (data.maxEnergy - 100) / increase
        );

        return 300 + upgrades * 200;
    }

    // ====================================
    // KAZMA YUKSELT
    // ====================================

    public void UpgradePickaxe()
    {
        if (data == null)
            data = GameDataManager.GetInstance();

        if (data == null)
            return;

        if (data.pickaxePower >= maxPickaxePower)
        {
            ShowFeedback(
                Translate(
                    "Kazma maksimum seviyede!",
                    "Pickaxe is at maximum level!"
                )
            );

            return;
        }

        int price = GetPickaxePrice();

        if (!TrySpendGold(price))
            return;

        data.pickaxePower += 1;

        SaveUpgrade();

        ShowFeedback(
            Translate(
                "Kazma yükseltildi!",
                "Pickaxe upgraded!"
            )
        );
    }

    // ====================================
    // CANTA YUKSELT
    // ====================================

    public void UpgradeBag()
    {
        if (data == null)
            data = GameDataManager.GetInstance();

        if (data == null)
            return;

        if (data.bagCapacity >= maxBagCapacity)
        {
            ShowFeedback(
                Translate(
                    "Çanta maksimum kapasitede!",
                    "Bag is at maximum capacity!"
                )
            );

            return;
        }

        int price = GetBagPrice();

        if (!TrySpendGold(price))
            return;

        data.bagCapacity = Mathf.Min(
            maxBagCapacity,
            data.bagCapacity + bagIncrease
        );

        SaveUpgrade();

        ShowFeedback(
            Translate(
                "Çanta kapasitesi artırıldı!",
                "Bag capacity increased!"
            )
        );
    }

    // ====================================
    // ENERJI YUKSELT
    // ====================================

    public void UpgradeEnergy()
    {
        if (data == null)
            data = GameDataManager.GetInstance();

        if (data == null)
            return;

        if (data.maxEnergy >= maxEnergyCapacity)
        {
            ShowFeedback(
                Translate(
                    "Enerji maksimum seviyede!",
                    "Energy is at maximum level!"
                )
            );

            return;
        }

        int price = GetEnergyPrice();

        if (!TrySpendGold(price))
            return;

        int oldMaxEnergy = data.maxEnergy;

        data.maxEnergy = Mathf.Min(
            maxEnergyCapacity,
            data.maxEnergy + energyIncrease
        );

        // Kapasite artisinin kadar enerji ekle.
        int gainedCapacity =
            data.maxEnergy - oldMaxEnergy;

        data.energy = Mathf.Min(
            data.maxEnergy,
            data.energy + gainedCapacity
        );

        SaveUpgrade();

        ShowFeedback(
            Translate(
                "Maksimum enerji artırıldı!",
                "Maximum energy increased!"
            )
        );
    }

    // ====================================
    // ALTIN HARCAMA
    // ====================================

    private bool TrySpendGold(int price)
    {
        if (data == null)
            return false;

        if (data.money < price)
        {
            ShowFeedback(
                Translate(
                    "Yeterli altının yok!",
                    "Not enough gold!"
                )
            );

            return false;
        }

        data.money -= price;

        return true;
    }

    // ====================================
    // KAYIT
    // ====================================

    private void SaveUpgrade()
    {
        if (data == null)
            return;

        data.SaveToDisk();

        EnergyRecovery recovery =
            data.GetComponent<EnergyRecovery>();

        if (recovery != null)
        {
            recovery.SaveNow();
        }

        RefreshUI();
    }

    // ====================================
    // BILDIRIM
    // ====================================

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }

        Debug.Log("UPGRADE: " + message);
    }

    // ====================================
    // ARAYUZU YENILE
    // ====================================

    public void RefreshUI()
    {
        if (data == null)
        {
            data = GameDataManager.GetInstance();
        }

        if (data == null)
            return;

        // ====================================
        // ALTIN VE ELMAS
        // ====================================

        if (goldText != null)
        {
            goldText.text =
                data.money.ToString("N0");
        }

        if (diamondText != null)
        {
            diamondText.text =
                data.premiumDiamonds.ToString("N0");
        }

        // ====================================
        // KAZMA
        // ====================================

        bool pickaxeMax =
            data.pickaxePower >= maxPickaxePower;

        if (pickaxeLevelText != null)
        {
            string levelLabel =
                Translate("Sv.", "Lv.");

            if (pickaxeMax)
            {
                pickaxeLevelText.text =
                    levelLabel + " " +
                    data.pickaxePower +
                    " MAX";
            }
            else
            {
                pickaxeLevelText.text =
                    levelLabel + " " +
                    data.pickaxePower +
                    " → " +
                    (data.pickaxePower + 1);
            }
        }

        // Fiyat yesil butonun uzerinde.
        if (pickaxePriceText != null)
        {
            pickaxePriceText.text =
                pickaxeMax
                ? "MAX"
                : GetPickaxePrice().ToString("N0");
        }

        if (pickaxeUpgradeButton != null)
        {
            pickaxeUpgradeButton.interactable =
                !pickaxeMax;
        }

        // ====================================
        // CANTA
        // ====================================

        bool bagMax =
            data.bagCapacity >= maxBagCapacity;

        if (bagCapacityText != null)
        {
            if (bagMax)
            {
                bagCapacityText.text =
                    data.bagCapacity + " MAX";
            }
            else
            {
                int nextCapacity = Mathf.Min(
                    maxBagCapacity,
                    data.bagCapacity + bagIncrease
                );

                bagCapacityText.text =
                    data.bagCapacity +
                    " → " +
                    nextCapacity;
            }
        }

        // Fiyat yesil butonun uzerinde.
        if (bagPriceText != null)
        {
            bagPriceText.text =
                bagMax
                ? "MAX"
                : GetBagPrice().ToString("N0");
        }

        if (bagUpgradeButton != null)
        {
            bagUpgradeButton.interactable =
                !bagMax;
        }

        // ====================================
        // ENERJI
        // ====================================

        bool energyMax =
            data.maxEnergy >= maxEnergyCapacity;

        if (energyCapacityText != null)
        {
            if (energyMax)
            {
                energyCapacityText.text =
                    data.maxEnergy + " MAX";
            }
            else
            {
                int nextEnergy = Mathf.Min(
                    maxEnergyCapacity,
                    data.maxEnergy + energyIncrease
                );

                energyCapacityText.text =
                    data.maxEnergy +
                    " → " +
                    nextEnergy;
            }
        }

        // Fiyat yesil butonun uzerinde.
        if (energyPriceText != null)
        {
            energyPriceText.text =
                energyMax
                ? "MAX"
                : GetEnergyPrice().ToString("N0");
        }

        if (energyUpgradeButton != null)
        {
            energyUpgradeButton.interactable =
                !energyMax;
        }

        GetComponent<UpgradeLevelSlots>()?.RefreshSlots();

        

    }


#if UNITY_EDITOR

    [ContextMenu("TEST - 10000 Altin Ver")]
    private void GiveTestGold()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Bu testi Play modunda yap."
            );
            return;
        }

        if (data == null)
        {
            data = GameDataManager.GetInstance();
        }

        if (data == null)
            return;

        // 10.000 altin ekle.
        data.money += 10000;

        // Kalici olarak kaydet.
        data.SaveToDisk();

        // Ekrandaki miktari yenile.
        RefreshUI();

        Debug.Log(
            "TEST: 10.000 altin eklendi. " +
            "Yeni bakiye: " + data.money
        );
    }

#endif


#if UNITY_EDITOR

    [ContextMenu("TEST - Yukseltmeleri Sifirla")]
    private void ResetUpgrades()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Once Play moduna gir."
            );
            return;
        }

        GameDataManager data =
            GameDataManager.GetInstance();

        if (data == null)
            return;

        // Kazma baslangic seviyesi
        data.pickaxePower = 1;

        // Canta baslangic kapasitesi
        data.bagCapacity = 20;

        // Enerji baslangic kapasitesi
        data.maxEnergy = 100;

        // Mevcut enerji yeni maksimumu asmasin.
        data.energy = Mathf.Clamp(
            data.energy, 0, data.maxEnergy
        );

        // Hareket hizi yukseltmesi
        PlayerPrefs.DeleteKey(
            "DD_Upgrade_MoveSpeedLevel_v1"
        );
        PlayerPrefs.Save();

        // Kalici kayit
        data.SaveToDisk();

        EnergyRecovery recovery =
            data.GetComponent<EnergyRecovery>();

        if (recovery != null)
            recovery.SaveNow();

        // Arayuzu yenile
        RefreshUI();

        Debug.Log(
            "TEST: Yukseltmeler sifirlandi."
        );
    }

#endif

}
