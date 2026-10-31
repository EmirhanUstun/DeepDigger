
using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;

public class ShopPurchaseController : MonoBehaviour
{
    [Header("Enerji Satin Alma")]
    [Min(1)]
    public int fullEnergyGoldPrice = 1000;

    [Header("Magaza Bildirimleri")]
    public TMP_Text shopFeedbackText;

    [Header("Elmas Paketleri Paneli")]
    public GameObject diamondPackagesPanel;

    [Header("Mevcut Maden Satis Sistemi")]
    public ShopSalesController shopSalesController;

    [Header("Gunluk Odul")]
    public int dailyGoldReward = 50;

    private const string DailyRewardSaveKey =
        "DD_DailyReward_LastClaimUTC";

    private const long DailyCooldownSeconds = 86400;

    private GameDataManager data;
    private Coroutine feedbackRoutine;

    // =====================================
    // BASLANGIC
    // =====================================

    private void Start()
    {
        data = GameDataManager.GetInstance();

        if (diamondPackagesPanel != null)
        {
            diamondPackagesPanel.SetActive(false);
        }

        if (shopFeedbackText != null)
        {
            shopFeedbackText.text = "";
        }
    }

    // =====================================
    // 1000 ALTINA ENERJI DOLDUR
    // =====================================

    public void BuyFullEnergy()
    {
        if (data == null)
        {
            data = GameDataManager.GetInstance();
        }

        if (data == null)
        {
            ShowLocalizedFeedback("shop_data_error");
            return;
        }

        int maxEnergy = Mathf.Max(
            1,
            data.maxEnergy
        );

        // Enerji zaten doluysa para harcama.
        if (data.energy >= maxEnergy)
        {
            ShowLocalizedFeedback(
                "shop_energy_full"
            );

            return;
        }

        // Yeterli altin var mi?
        if (data.money < fullEnergyGoldPrice)
        {
            int missingGold =
                fullEnergyGoldPrice - data.money;

            ShowLocalizedFeedback(
                "shop_not_enough_gold",
                missingGold
            );

            return;
        }

        // Altin dus.
        data.money -= fullEnergyGoldPrice;

        // Enerjiyi tamamen doldur.
        data.energy = maxEnergy;

        // Kalici kayit.
        data.SaveToDisk();

        // Enerji yenileme sistemini senkronize et.
        EnergyRecovery recovery =
            data.GetComponent<EnergyRecovery>();

        if (recovery != null)
        {
            recovery.SaveNow();
        }

        // Magazadaki para sayacini guncelle.
        RefreshShopUI();

        // Secili dilde basari mesaji.
        ShowLocalizedFeedback(
            "shop_energy_success"
        );

        Debug.Log(
            "Enerji dolduruldu. Kalan altin: " +
            data.money +
            " | Enerji: " +
            data.energy + "/" +
            data.maxEnergy
        );
    }

    // =====================================
    // ELMAS PAKETLERI PANELI
    // =====================================

    public void OpenDiamondPackages()
    {
        if (diamondPackagesPanel == null)
        {
            Debug.LogWarning(
                "DiamondPackagesPanel baglanmamis!"
            );

            return;
        }

        diamondPackagesPanel.SetActive(true);
    }

    public void CloseDiamondPackages()
    {
        if (diamondPackagesPanel != null)
        {
            diamondPackagesPanel.SetActive(false);
        }
    }

    // =====================================
    // GUNLUK ODUL - 24 SAATTE 50 ALTIN
    // =====================================

    public void ClaimDailyReward()
    {
        if (data == null)
        {
            data = GameDataManager.GetInstance();
        }

        if (data == null)
        {
            ShowLocalizedFeedback("shop_data_error");
            return;
        }

        long now = DateTimeOffset.UtcNow
            .ToUnixTimeSeconds();

        string savedTime = PlayerPrefs.GetString(
            DailyRewardSaveKey,
            "0"
        );

        long lastClaim = 0;

        long.TryParse(
            savedTime,
            out lastClaim
        );

        // Son odulden beri 24 saat gecmis mi?
        if (lastClaim > 0)
        {
            long elapsed = now - lastClaim;

            if (elapsed < 0 ||
                elapsed < DailyCooldownSeconds)
            {
                ShowLocalizedFeedback(
                    "shop_reward_claimed"
                );

                return;
            }
        }

        // Gunluk altin odulunu ver.
        data.money += dailyGoldReward;

        // Son odul zamanini kaydet.
        PlayerPrefs.SetString(
            DailyRewardSaveKey,
            now.ToString()
        );

        PlayerPrefs.Save();

        // Altini kalici kaydet.
        data.SaveToDisk();

        // Ustteki altin sayacini yenile.
        RefreshShopUI();

        // Secili dilde bildirim goster.
        ShowLocalizedFeedback(
            "shop_reward_success"
        );

        Debug.Log(
            "Gunluk odul alindi. Kazanilan: " +
            dailyGoldReward +
            " | Toplam altin: " +
            data.money
        );
    }

    // =====================================
    // MAGAZA ARAYUZUNU YENILE
    // =====================================

    private void RefreshShopUI()
    {
        if (shopSalesController != null)
        {
            shopSalesController.RefreshUI();
        }
    }

    // =====================================
    // LOCALIZATION DESTEKLI BILDIRIM
    // =====================================

    private void ShowLocalizedFeedback(
        string key,
        params object[] arguments)
    {
        string translatedText = "";

        try
        {
            translatedText =
                LocalizationSettings.StringDatabase
                    .GetLocalizedString(
                        "GameTexts",
                        key,
                        arguments
                    );
        }
        catch (Exception e)
        {
            Debug.LogWarning(
                "Localization hatasi: " +
                e.Message
            );
        }

        // Ceviri bulunamazsa key gosterilir.
        if (string.IsNullOrEmpty(translatedText))
        {
            Debug.LogWarning(
                "Ceviri bulunamadi: " + key
            );

            translatedText = key;
        }

        // Console'da secili dili kontrol et.
        var locale =
            LocalizationSettings.SelectedLocale;

        Debug.Log(
            "Dil: " +
            (locale != null
                ? locale.Identifier.Code
                : "Bilinmiyor") +
            " | Key: " + key +
            " | Mesaj: " + translatedText
        );

        ShowFeedback(translatedText);
    }

    // =====================================
    // BILDIRIM GOSTERME
    // =====================================

    private void ShowFeedback(string message)
    {
        if (shopFeedbackText == null)
        {
            Debug.Log(message);
            return;
        }

        // Onceki bildirimi durdur.
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        feedbackRoutine = StartCoroutine(
            FeedbackRoutine(message)
        );
    }

    // =====================================
    // BILDIRIMI 3 SANIYE GOSTER
    // =====================================

    private IEnumerator FeedbackRoutine(
        string message)
    {
        shopFeedbackText.text = message;

        yield return new WaitForSecondsRealtime(3f);

        if (shopFeedbackText != null)
        {
            shopFeedbackText.text = "";
        }

        feedbackRoutine = null;
    }
}
