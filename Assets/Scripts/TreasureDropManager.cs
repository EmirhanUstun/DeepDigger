
using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;

public class TreasureDropManager : MonoBehaviour
{
    [Header("Yeni Hazine Bildirimi")]
    public TMP_Text notificationText;

    public float notificationDuration = 3f;

    [Header("Sadece Editor Testi")]
    public bool forceTreasureOnNextOre = false;

    // Hazine sirasi CollectionController ile ayni.
    private const int TreasureCount = 6;

    // Hazinelerin acildigi derinlikler.
    private static readonly int[] MinDepths =
    {
        0,   // Antik Vazo
        50,  // Kraliyet Taci
        100, // Kristal Kafatasi
        20,  // Hazine Haritasi
        160, // Zumrut Totemi
        250  // Madenci Madalyonu
    };

    // Her basarili blok kirma icin olasiliklar.
    private static readonly float[] DropChances =
    {
        0.07f,
        0.02f,
        0.015f,
        0.04f,
        0.01f,
        0.007f
    };

    private static readonly string[] TreasureKeys =
    {
        "collection_vase",
        "collection_crown",
        "collection_skull",
        "collection_map",
        "collection_idol",
        "collection_medallion"
    };

    private static readonly string[] DefaultNames =
    {
        "Antik Vazo",
        "Kraliyet Tacı",
        "Kristal Kafatası",
        "Hazine Haritası",
        "Zümrüt Totemi",
        "Madenci Madalyonu"
    };

    private Coroutine notificationRoutine;

    private void Start()
    {
        if (notificationText != null)
        {
            notificationText.text = "";
        }
    }


    // =====================================
    // KAZI TAMAMLANDIKTAN SONRA HAZINE KONTROLU
    // =====================================

    public void QueueOreMined(int currentDepth)
    {
        if (!isActiveAndEnabled)
            return;

        StartCoroutine(
            ProcessTreasureNextFrame(currentDepth)
        );
    }

    private IEnumerator ProcessTreasureNextFrame(
        int currentDepth)
    {
        // Kazma isleminin mevcut karede
        // tamamlanmasina izin ver.
        yield return null;

        try
        {
            OnOreMined(currentDepth);
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                "Hazine kesif sisteminde hata olustu!"
            );

            Debug.LogException(e, this);
        }
    }


    // =====================================
    // BASARILI MADEN KIRMA
    // =====================================

    public void OnOreMined(int currentDepth)
    {
        int discoveredIndex = -1;

#if UNITY_EDITOR
        // Test icin siradaki bulunmamis
        // hazineyi kesin olarak kesfeder.
        if (forceTreasureOnNextOre)
        {
            forceTreasureOnNextOre = false;

            for (int i = 0; i < TreasureCount; i++)
            {
                if (!CollectionController.IsTreasureFound(i))
                {
                    discoveredIndex = i;
                    break;
                }
            }
        }
#endif

        // Normal rastgele hazine kesfi.
        if (discoveredIndex == -1)
        {
            float roll = UnityEngine.Random.value;

            for (int i = 0; i < TreasureCount; i++)
            {
                // Zaten bulunmus hazineyi atla.
                if (CollectionController.IsTreasureFound(i))
                    continue;

                // Gerekli derinlige ulasilmadiysa atla.
                if (currentDepth < MinDepths[i])
                    continue;

                float chance = DropChances[i];

                if (roll < chance)
                {
                    discoveredIndex = i;
                    break;
                }

                roll -= chance;
            }
        }

        if (discoveredIndex == -1)
            return;

        // CollectionController kalici kayit yapar.
        bool newlyDiscovered =
            CollectionController.DiscoverTreasure(
                discoveredIndex
            );

        if (!newlyDiscovered)
            return;

        ShowTreasureNotification(discoveredIndex);
    }

    // =====================================
    // LOCALIZATION
    // =====================================

    private string GetLocalizedText(
        string key,
        string fallback)
    {
        try
        {
            string result =
                LocalizationSettings.StringDatabase
                    .GetLocalizedString(
                        "GameTexts",
                        key
                    );

            if (!string.IsNullOrEmpty(result))
                return result;
        }
        catch (Exception e)
        {
            Debug.LogWarning(
                "Localization: " + e.Message
            );
        }

        return fallback;
    }

    // =====================================
    // HAZINE BILDIRIMI
    // =====================================

    private void ShowTreasureNotification(int index)
    {
        string treasureName = GetLocalizedText(
            TreasureKeys[index],
            DefaultNames[index]
        );

        string message =
            "Yeni hazine bulundu: " +
            treasureName + "!";

        try
        {
            string localizedMessage =
                LocalizationSettings.StringDatabase
                    .GetLocalizedString(
                        "GameTexts",
                        "collection_treasure_discovered",
                        new object[] { treasureName }
                    );

            if (!string.IsNullOrEmpty(localizedMessage))
            {
                message = localizedMessage;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
        }

        Debug.Log(message);

        if (notificationText == null)
            return;

        if (notificationRoutine != null)
        {
            StopCoroutine(notificationRoutine);
        }

        notificationRoutine = StartCoroutine(
            NotificationRoutine(message)
        );
    }

    private IEnumerator NotificationRoutine(
        string message)
    {
        notificationText.text = message;

        yield return new WaitForSecondsRealtime(
            notificationDuration
        );

        if (notificationText != null)
        {
            notificationText.text = "";
        }

        notificationRoutine = null;
    }
}
