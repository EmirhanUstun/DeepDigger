
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageStartup : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        int savedLanguage =
            PlayerPrefs.GetInt("DD_Language", 0);

        string code = savedLanguage == 1 ? "en" : "tr";

        Locale locale =
            LocalizationSettings.AvailableLocales.GetLocale(code);

        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
        }
        else
        {
            Debug.LogWarning("Dil bulunamadi: " + code);
        }
    }
}
