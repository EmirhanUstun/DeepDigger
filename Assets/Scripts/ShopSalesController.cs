
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class ShopSalesController : MonoBehaviour
{
    [Header("Para Gostergeleri")]
    public TMP_Text goldText;
    public TMP_Text diamondText;

    [Header("Maden Adi ve Adetleri (0-9)")]
    public TMP_Text[] oreNameTexts = new TMP_Text[10];

    [Header("Maden Satis Butonlari (0-9)")]
    public Button[] sellButtons = new Button[10];

    [Header("Toplam Satis")]
    public TMP_Text totalValueText;
    public Button sellAllButton;

    [Header("Hata Kontrolu")]
    public bool showDebugLogs = false;

    // =====================================
    // MADEN FIYATLARI
    // =====================================

    private static readonly int[] OrePrices =
    {
        1,    // Tas
        3,    // Komur
        7,    // Bakir
        15,   // Demir
        30,   // Gumus
        60,   // Altin
        120,  // Kristal
        250,  // Elmas madeni
        500,  // Zumrut
        700   // Safir
    };

    // =====================================
    // LOCALIZATION KEYLERI
    // =====================================

    private static readonly string[] OreKeys =
    {
        "ore_stone",
        "ore_coal",
        "ore_copper",
        "ore_iron",
        "ore_silver",
        "ore_gold",
        "ore_crystal",
        "ore_diamond",
        "ore_emerald",
        "ore_sapphire"
    };

    private static readonly string[] DefaultOreNames =
    {
        "Taş",
        "Kömür",
        "Bakır",
        "Demir",
        "Gümüş",
        "Altın",
        "Kristal",
        "Elmas",
        "Zümrüt",
        "Safir"
    };

    private GameDataManager data;

    private int lastInventoryHash = int.MinValue;
    private int lastMoney = int.MinValue;
    private int lastDiamonds = int.MinValue;

    private string lastTotalText = "";
    private float nextCheckTime;

    // =====================================
    // BASLANGIC
    // =====================================

    private void Awake()
    {
        data = GameDataManager.GetInstance();
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged +=
            OnLocaleChanged;
    }

    private void Start()
    {
        data = GameDataManager.GetInstance();

        // SAT butonlarini bagla.
        for (int i = 0; i < OrePrices.Length; i++)
        {
            if (sellButtons == null ||
                i >= sellButtons.Length ||
                sellButtons[i] == null)
            {
                continue;
            }

            int oreIndex = i;

            sellButtons[i].onClick.AddListener(
                () => SellOre(oreIndex)
            );
        }

        // HEPSINI SAT
        if (sellAllButton != null)
        {
            sellAllButton.onClick.AddListener(
                SellAllOres
            );
        }

        RefreshUI();
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -=
            OnLocaleChanged;
    }

    // =====================================
    // VERI DEGISIKLIGINI KONTROL ET
    // =====================================

    private void Update()
    {
        // Her karede degil,
        // 0.25 saniyede bir kontrol et.
        if (Time.unscaledTime < nextCheckTime)
            return;

        nextCheckTime = Time.unscaledTime + 0.25f;

        if (data == null)
        {
            data = GameDataManager.GetInstance();
        }

        if (data == null)
            return;

        int currentHash = GetInventoryHash();

        bool inventoryChanged =
            currentHash != lastInventoryHash;

        bool moneyChanged =
            data.money != lastMoney;

        bool diamondsChanged =
            data.premiumDiamonds != lastDiamonds;

        bool textChanged =
            totalValueText != null &&
            totalValueText.text != lastTotalText;

        if (inventoryChanged ||
            moneyChanged ||
            diamondsChanged ||
            textChanged)
        {
            RefreshUI();
        }
    }

    // =====================================
    // DIL DEGISIMI
    // =====================================

    private void OnLocaleChanged(Locale locale)
    {
        RefreshUI();
    }

    // =====================================
    // ENVANTERDEN MADEN ADEDI OKU
    // =====================================

    private int GetOreQuantity(int index)
    {
        if (data == null ||
            data.inventory == null)
        {
            return 0;
        }

        if (index < 0 ||
            index >= OrePrices.Length ||
            index >= data.inventory.Length)
        {
            return 0;
        }

        return Mathf.Max(
            0,
            data.inventory[index]
        );
    }

    // =====================================
    // TOPLAM SATIS DEGERINI HESAPLA
    // =====================================

    private int CalculateTotalValue()
    {
        int total = 0;

        for (int i = 0; i < OrePrices.Length; i++)
        {
            int quantity = GetOreQuantity(i);

            total += quantity * OrePrices[i];
        }

        return total;
    }

    // =====================================
    // TOPLAM MADEN ADEDI
    // =====================================

    private int CalculateTotalQuantity()
    {
        int total = 0;

        for (int i = 0; i < OrePrices.Length; i++)
        {
            total += GetOreQuantity(i);
        }

        return total;
    }

    // =====================================
    // ENVANTER DEGISIM KONTROLU
    // =====================================

    private int GetInventoryHash()
    {
        unchecked
        {
            int hash = 17;

            for (int i = 0; i < OrePrices.Length; i++)
            {
                hash = hash * 31 +
                       GetOreQuantity(i);
            }

            return hash;
        }
    }

    // =====================================
    // TEK MADEN TURUNU SAT
    // =====================================

    public void SellOre(int oreIndex)
    {
        if (data == null)
            data = GameDataManager.GetInstance();

        if (data == null)
            return;

        if (oreIndex < 0 ||
            oreIndex >= OrePrices.Length)
        {
            return;
        }

        int quantity = GetOreQuantity(oreIndex);

        if (quantity <= 0)
        {
            if (showDebugLogs)
            {
                Debug.Log("Satilacak maden yok.");
            }

            RefreshUI();
            return;
        }

        int earnings =
            quantity * OrePrices[oreIndex];

        // Madeni cantadan cikar.
        data.inventory[oreIndex] = 0;

        // Altin kazan.
        data.money += earnings;

        // Veriyi kaydet.
        data.SaveToDisk();

        if (showDebugLogs)
        {
            Debug.Log(
                "SATIS | Maden: " + oreIndex +
                " | Adet: " + quantity +
                " | Kazanc: " + earnings
            );
        }

        RefreshUI();
    }

    // =====================================
    // BUTUN MADENLERI SAT
    // =====================================

    public void SellAllOres()
    {
        if (data == null)
            data = GameDataManager.GetInstance();

        if (data == null ||
            data.inventory == null)
        {
            return;
        }

        // Ekranin kullandigi hesapla
        // ayni hesaplama kullanilir.
        int earnings = CalculateTotalValue();

        if (earnings <= 0)
        {
            RefreshUI();
            return;
        }

        int count = Mathf.Min(
            data.inventory.Length,
            OrePrices.Length
        );

        // Cantadaki madenleri sifirla.
        for (int i = 0; i < count; i++)
        {
            data.inventory[i] = 0;
        }

        // Altin ekle.
        data.money += earnings;

        // Kalici kaydet.
        data.SaveToDisk();

        if (showDebugLogs)
        {
            Debug.Log(
                "HEPSINI SAT | Kazanc: " +
                earnings
            );
        }

        RefreshUI();
    }

    // =====================================
    // CEVIRI OKUMA
    // =====================================

    private string GetTranslatedText(
        string key,
        string fallback)
    {
        try
        {
            string translated =
                LocalizationSettings.StringDatabase
                    .GetLocalizedString(
                        "GameTexts",
                        key
                    );

            if (!string.IsNullOrEmpty(translated))
            {
                return translated;
            }
        }
        catch (Exception e)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning(
                    "Localization: " + e.Message
                );
            }
        }

        return fallback;
    }

    // =====================================
    // TOPLAM DEGER METNI
    // =====================================

    private string GetTotalValueText(int totalValue)
    {
        string formattedValue =
            totalValue.ToString("N0");

        try
        {
            string localized =
                LocalizationSettings.StringDatabase
                    .GetLocalizedString(
                        "GameTexts",
                        "shop_total_value",
                        new object[] { formattedValue }
                    );

            // Ceviri dogru sayiyi iceriyorsa kullan.
            if (!string.IsNullOrEmpty(localized) &&
                localized.Contains(formattedValue))
            {
                return localized;
            }
        }
        catch (Exception e)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning(
                    "Toplam deger cevirisi: " +
                    e.Message
                );
            }
        }

        // Smart String veya tablo ayarinda sorun
        // varsa sayiyi yine dogru goster.
        string language = "tr";

        if (LocalizationSettings.SelectedLocale != null)
        {
            language =
                LocalizationSettings.SelectedLocale
                    .Identifier.Code;
        }

        string label =
            language.StartsWith("en",
                StringComparison.OrdinalIgnoreCase)
            ? "Total Value: "
            : "Toplam Değer: ";

        return label + formattedValue;
    }

    // =====================================
    // ARAYUZU GUNCELLE
    // =====================================

    public void RefreshUI()
    {
        if (data == null)
        {
            data = GameDataManager.GetInstance();
        }

        if (data == null)
            return;

        // ALTIN
        if (goldText != null)
        {
            goldText.text =
                data.money.ToString("N0");
        }

        // OZEL ELMAS
        if (diamondText != null)
        {
            diamondText.text =
                data.premiumDiamonds.ToString("N0");
        }

        // MADENLER
        for (int i = 0; i < OrePrices.Length; i++)
        {
            int quantity = GetOreQuantity(i);

            string oreName =
                GetTranslatedText(
                    OreKeys[i],
                    DefaultOreNames[i]
                );

            if (oreNameTexts != null &&
                i < oreNameTexts.Length &&
                oreNameTexts[i] != null)
            {
                oreNameTexts[i].text =
                    oreName + " x" + quantity;
            }

            if (sellButtons != null &&
                i < sellButtons.Length &&
                sellButtons[i] != null)
            {
                sellButtons[i].interactable =
                    quantity > 0;
            }
        }

        // TOPLAM DEGER
        int totalValue = CalculateTotalValue();

        if (totalValueText != null)
        {
            lastTotalText =
                GetTotalValueText(totalValue);

            totalValueText.text = lastTotalText;
        }

        // HEPSINI SAT BUTONU
        if (sellAllButton != null)
        {
            sellAllButton.interactable =
                CalculateTotalQuantity() > 0;
        }

        // Son degerleri sakla.
        lastInventoryHash = GetInventoryHash();
        lastMoney = data.money;
        lastDiamonds = data.premiumDiamonds;

        if (showDebugLogs)
        {
            Debug.Log(
                "SHOP UI | Toplam Deger: " +
                totalValue +
                " | Altin: " + data.money +
                " | Canta: " +
                CalculateTotalQuantity()
            );
        }
    }
}
