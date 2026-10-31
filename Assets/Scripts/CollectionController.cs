
using UnityEngine;
using TMPro;

public class CollectionController : MonoBehaviour
{
    [Header("Bulunan Hazine Gorselleri")]
    public GameObject[] foundVisuals = new GameObject[6];

    [Header("Koleksiyon Ilerlemesi")]
    public TMP_Text progressCountText;

    private const int TreasureCount = 6;

    private const string SaveKey =
        "DD_Collection_FoundMask_v1";

    // =====================================
    // SAHNE ACILDIGINDA
    // =====================================

    private void OnEnable()
    {
        RefreshCollectionUI();
    }

    // =====================================
    // HAZINE BULUNDU MU?
    // =====================================

    public static bool IsTreasureFound(int index)
    {
        if (index < 0 || index >= TreasureCount)
        {
            return false;
        }

        int savedMask = PlayerPrefs.GetInt(
            SaveKey,
            0
        );

        int treasureBit = 1 << index;

        return (savedMask & treasureBit) != 0;
    }

    // =====================================
    // YENI HAZINE KESFET
    // =====================================

    public static bool DiscoverTreasure(int index)
    {
        if (index < 0 || index >= TreasureCount)
        {
            Debug.LogWarning(
                "Gecersiz hazine numarasi: " + index
            );

            return false;
        }

        // Daha once bulunmussa tekrar kaydetme.
        if (IsTreasureFound(index))
        {
            return false;
        }

        int savedMask = PlayerPrefs.GetInt(
            SaveKey,
            0
        );

        // Ilgili hazineyi bulundu olarak isaretle.
        savedMask |= (1 << index);

        // Kalici kayit.
        PlayerPrefs.SetInt(
            SaveKey,
            savedMask
        );

        PlayerPrefs.Save();

        Debug.Log(
            "Yeni hazine bulundu! Numara: " + index
        );

        // Koleksiyon sahnesi aciksa yenile.
        CollectionController[] controllers =
            Object.FindObjectsByType<CollectionController>(
                FindObjectsSortMode.None
            );

        foreach (CollectionController controller
                 in controllers)
        {
            if (controller.isActiveAndEnabled)
            {
                controller.RefreshCollectionUI();
            }
        }

        return true;
    }

    // =====================================
    // BULUNAN HAZINELERI SAY
    // =====================================

    public static int GetFoundCount()
    {
        int count = 0;

        for (int i = 0; i < TreasureCount; i++)
        {
            if (IsTreasureFound(i))
            {
                count++;
            }
        }

        return count;
    }

    // =====================================
    // KOLEKSIYON EKRANINI YENILE
    // =====================================

    public void RefreshCollectionUI()
    {
        for (int i = 0; i < TreasureCount; i++)
        {
            bool isFound = IsTreasureFound(i);

            if (foundVisuals != null &&
                i < foundVisuals.Length &&
                foundVisuals[i] != null)
            {
                // Bulunduysa yesil tik goster.
                // Bulunmadiysa gizle.
                foundVisuals[i].SetActive(isFound);
            }
        }

        // Ornek: 2/6
        if (progressCountText != null)
        {
            progressCountText.text =
                GetFoundCount() + "/" + TreasureCount;
        }
    }

    // =====================================
    // TEST: ILK HAZINEYI BUL
    // =====================================

#if UNITY_EDITOR
    [ContextMenu("TEST - Ilk Hazineyi Bul")]
    private void TestFirstTreasure()
    {
        DiscoverTreasure(0);
        RefreshCollectionUI();
    }

    [ContextMenu("TEST - Koleksiyonu Sifirla")]
    private void ResetCollectionForTesting()
    {
        // Dikkat: Kayitli koleksiyon ilerlemesini siler.
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        RefreshCollectionUI();

        Debug.Log("TEST: Koleksiyon sifirlandi.");
    }
#endif
}
