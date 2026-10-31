
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class MovementSpeedUpgrade : MonoBehaviour
{
    [Header("Hareket Hizi Arayuzu")]
    public TMP_Text speedLevelText;
    public TMP_Text speedPriceText;
    public Button speedUpgradeButton;

    [Header("Bildirim")]
    public TMP_Text feedbackText;

    [Header("Yukseltme Ayarlari")]
    public int maxLevel = 6;
    public int firstPrice = 400;
    public int priceIncrease = 300;

    private GameDataManager data;

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

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -=
            OnLanguageChanged;
    }

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

    private int GetCurrentLevel()
    {
        return MinerMovementSpeedApplier.GetSavedLevel();
    }

    private int GetPrice(int level)
    {
        return firstPrice +
            (level - 1) * priceIncrease;
    }

    public void UpgradeSpeed()
    {
        if (data == null)
            data = GameDataManager.GetInstance();

        if (data == null)
            return;

        int level = GetCurrentLevel();

        if (level >= maxLevel)
        {
            ShowFeedback(
                Translate(
                    "Hareket hizi maksimum seviyede!",
                    "Movement speed is at maximum level!"
                )
            );
            return;
        }

        int price = GetPrice(level);

        if (data.money < price)
        {
            ShowFeedback(
                Translate(
                    "Yeterli altinin yok!",
                    "Not enough gold!"
                )
            );
            return;
        }

        // Altini dus.
        data.money -= price;

        // Yeni seviyeyi kaydet.
        int newLevel = level + 1;

        PlayerPrefs.SetInt(
            MinerMovementSpeedApplier.LevelKey,
            newLevel
        );

        PlayerPrefs.Save();
        data.SaveToDisk();

        // Arayuzu yenile.
        RefreshUI();

        UpgradeController mainController =
            GetComponent<UpgradeController>();

        if (mainController != null)
            mainController.RefreshUI();

        ShowFeedback(
            Translate(
                "Hareket hizi yukseltildi!",
                "Movement speed upgraded!"
            )
        );
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;

        Debug.Log(message);
    }

    public void RefreshUI()
    {
        int level = GetCurrentLevel();

        float speedMultiplier =
            MinerMovementSpeedApplier.GetMultiplier(level);

        if (speedLevelText != null)
        {
            speedLevelText.text =
                Translate(
                    "Hareket Hizi Seviye ",
                    "Movement Speed Level "
                ) +
                level +
                "  (x" +
                speedMultiplier.ToString("0.00") +
                ")";
        }

        bool isMax = level >= maxLevel;

        if (speedPriceText != null)
        {
            speedPriceText.text = isMax
                ? Translate("MAKSIMUM", "MAXIMUM")
                : GetPrice(level).ToString("N0");
        }

        if (speedUpgradeButton != null)
            speedUpgradeButton.interactable = !isMax;
    }
}
