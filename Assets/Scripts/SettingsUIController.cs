
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class SettingsUIController : MonoBehaviour
{
    // =========================
    // MUZIK
    // =========================

    [Header("Muzik")]
    public Image musicButtonImage;
    public TMP_Text musicButtonText;
    public AudioSource backgroundMusic;

    [Header("Muzik Anahtar Gorseli")]
    public GameObject musicOffVisual;


    // =========================
    // SES EFEKTLERI
    // =========================

    [Header("Ses Efektleri")]
    public Image soundButtonImage;
    public TMP_Text soundButtonText;
    public AudioSource[] soundEffectSources;

    [Header("Ses Efektleri Anahtar Gorseli")]
    public GameObject soundOffVisual;


    // =========================
    // TITRESIM
    // =========================

    [Header("Titresim")]
    public Image vibrationButtonImage;
    public TMP_Text vibrationButtonText;


[Header("Titresim Anahtar Gorseli")]
public GameObject vibrationOffVisual;



    // =========================
    // BILDIRIMLER
    // =========================

    [Header("Bildirimler")]
    public Image notificationButtonImage;
    public TMP_Text notificationButtonText;


[Header("Bildirimler Anahtar Gorseli")]
public GameObject notificationOffVisual;



    // =========================
    // DIL
    // =========================

    [Header("Dil")]
    public TMP_Text languageButtonText;


    // =========================
    // AYAR DEGISKENLERI
    // =========================

    private bool musicEnabled;
    private bool soundEnabled;
    private bool vibrationEnabled;
    private bool notificationsEnabled;

    // 0 = Turkce
    // 1 = English
    private int languageIndex;


    // =========================
    // BASLANGIC
    // =========================

    private void Start()
    {
        LoadSettings();
    }


    // =========================
    // KAYITLI AYARLARI YUKLE
    // =========================

    private void LoadSettings()
    {
        musicEnabled =
            PlayerPrefs.GetInt("DD_Music", 1) == 1;

        soundEnabled =
            PlayerPrefs.GetInt("DD_Sound", 1) == 1;

        vibrationEnabled =
            PlayerPrefs.GetInt("DD_Vibration", 1) == 1;

        notificationsEnabled =
            PlayerPrefs.GetInt("DD_Notifications", 1) == 1;

        languageIndex = Mathf.Clamp(
            PlayerPrefs.GetInt("DD_Language", 0),
            0,
            1
        );

        UpdateAllUI();

        StartCoroutine(ApplyLanguageAsync());
    }


    // =========================
    // MUZIK AC / KAPAT
    // =========================

    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;

        UpdateAllUI();

        Debug.Log(
            "Muzik: " +
            (musicEnabled ? "ACIK" : "KAPALI")
        );
    }


    // =========================
    // SES EFEKTLERI AC / KAPAT
    // =========================

    public void ToggleSoundEffects()
    {
        soundEnabled = !soundEnabled;

        UpdateAllUI();

        Debug.Log(
            "Ses Efektleri: " +
            (soundEnabled ? "ACIK" : "KAPALI")
        );
    }


    // =========================
    // TITRESIM AC / KAPAT
    // =========================

    public void ToggleVibration()
    {
        vibrationEnabled = !vibrationEnabled;

        UpdateAllUI();

        Debug.Log(
            "Titresim: " +
            (vibrationEnabled ? "ACIK" : "KAPALI")
        );
    }


    // =========================
    // BILDIRIMLER AC / KAPAT
    // =========================

    public void ToggleNotifications()
    {
        notificationsEnabled = !notificationsEnabled;

        UpdateAllUI();

        Debug.Log(
            "Bildirimler: " +
            (notificationsEnabled ? "ACIK" : "KAPALI")
        );
    }


    // =========================
    // DIL DEGISTIR
    // =========================

    public void ChangeLanguage()
    {
        languageIndex = (languageIndex + 1) % 2;

        UpdateAllUI();

        StartCoroutine(ApplyLanguageAsync());
    }


    // =========================
    // BUTON GORUNUMU
    // =========================

    private void UpdateToggle(
        Image buttonImage,
        TMP_Text buttonText,
        bool enabled,
        bool changeImageColor = true
    )
    {
        // Sadece PNG sistemi kullanilmayan
        // butonlarin rengini degistir.

        if (changeImageColor && buttonImage != null)
        {
            buttonImage.color = enabled
                ? new Color32(51, 204, 26, 255)
                : new Color32(115, 64, 38, 255);
        }

        // Eski metinli butonlar icin
        // Turkce ve Ingilizce destegi.

        if (buttonText != null)
        {
            if (languageIndex == 0)
            {
                buttonText.text =
                    enabled ? "AÇIK" : "KAPALI";
            }
            else
            {
                buttonText.text =
                    enabled ? "ON" : "OFF";
            }
        }
    }


    // =========================
    // TUM GORUNUMU GUNCELLE
    // =========================

    
private void UpdateAllUI()
{
    // MUZIK
    UpdateToggle(
        musicButtonImage,
        musicButtonText,
        musicEnabled,
        musicOffVisual == null
    );

    // SES EFEKTLERI
    UpdateToggle(
        soundButtonImage,
        soundButtonText,
        soundEnabled,
        soundOffVisual == null
    );

    // TITRESIM
    UpdateToggle(
        vibrationButtonImage,
        vibrationButtonText,
        vibrationEnabled,
        vibrationOffVisual == null
    );

    // BILDIRIMLER
    UpdateToggle(
        notificationButtonImage,
        notificationButtonText,
        notificationsEnabled,
        notificationOffVisual == null
    );

    // DIL SECIMI
    if (languageButtonText != null)
    {
        languageButtonText.text =
            languageIndex == 0
                ? "Türkçe"
                : "English";
    }

    // MUZIK SES KONTROLU
    if (backgroundMusic != null)
    {
        backgroundMusic.mute = !musicEnabled;
    }

    // SES EFEKTLERI KONTROLU
    if (soundEffectSources != null)
    {
        foreach (AudioSource source in soundEffectSources)
        {
            if (source != null)
            {
                source.mute = !soundEnabled;
            }
        }
    }

    // MUZIK KAPALI GORSELI
    if (musicOffVisual != null)
    {
        musicOffVisual.SetActive(!musicEnabled);
    }

    // SES EFEKTLERI KAPALI GORSELI
    if (soundOffVisual != null)
    {
        soundOffVisual.SetActive(!soundEnabled);
    }

    // TITRESIM KAPALI GORSELI
    if (vibrationOffVisual != null)
    {
        vibrationOffVisual.SetActive(!vibrationEnabled);
    }

    // BILDIRIMLER KAPALI GORSELI
    if (notificationOffVisual != null)
    {
        notificationOffVisual.SetActive(!notificationsEnabled);
    }
}



    // =========================
    // AYARLARI KAYDET
    // =========================

    public void SaveSettings()
    {
        PlayerPrefs.SetInt(
            "DD_Music",
            musicEnabled ? 1 : 0
        );

        PlayerPrefs.SetInt(
            "DD_Sound",
            soundEnabled ? 1 : 0
        );

        PlayerPrefs.SetInt(
            "DD_Vibration",
            vibrationEnabled ? 1 : 0
        );

        PlayerPrefs.SetInt(
            "DD_Notifications",
            notificationsEnabled ? 1 : 0
        );

        PlayerPrefs.SetInt(
            "DD_Language",
            languageIndex
        );

        PlayerPrefs.Save();

        Debug.Log("Tum ayarlar kaydedildi!");
    }


    // =========================
    // VARSAYILANA DON
    // =========================

    public void ResetDefaults()
    {
        musicEnabled = true;
        soundEnabled = true;
        vibrationEnabled = true;
        notificationsEnabled = true;

        languageIndex = 0;

        UpdateAllUI();

        StartCoroutine(ApplyLanguageAsync());

        Debug.Log(
            "Varsayilan ayarlar yuklendi. " +
            "Kaydet butonuna basarak kalici yapabilirsiniz."
        );
    }


    // =========================
    // UNITY LOCALIZATION
    // =========================

    private IEnumerator ApplyLanguageAsync()
    {
        yield return
            LocalizationSettings.InitializationOperation;

        string code =
            languageIndex == 0 ? "tr" : "en";

        Locale locale =
            LocalizationSettings.AvailableLocales
                .GetLocale(code);

        if (locale == null)
        {
            Debug.LogError(
                "Dil bulunamadi: " + code
            );

            yield break;
        }

        if (LocalizationSettings.SelectedLocale != locale)
        {
            LocalizationSettings.SelectedLocale = locale;
        }

        Debug.Log("Aktif dil: " + code);
    }
}
