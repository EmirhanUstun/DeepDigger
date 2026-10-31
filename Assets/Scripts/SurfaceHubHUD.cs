
using UnityEngine;
using TMPro;

public class SurfaceHubHUD : MonoBehaviour
{
    [Header("Para Gostergeleri")]
    public TMP_Text goldText;
    public TMP_Text diamondText;

    private GameDataManager data;

    private void Start()
    {
        data = GameDataManager.GetInstance();

        RefreshHUD();
    }

    private void LateUpdate()
    {
        RefreshHUD();
    }

    public void RefreshHUD()
    {
        if (data == null)
        {
            data = GameDataManager.GetInstance();
        }

        if (data == null)
            return;

        // ALTIN
        // Magazada kazanilan ayni para bakiyesi
        if (goldText != null)
        {
            goldText.text =
                data.money.ToString("N0");
        }

        // OZEL ELMAS
        // Cantadaki Elmas madeninden farklidir
        if (diamondText != null)
        {
            diamondText.text =
                data.premiumDiamonds.ToString("N0");
        }
    }
}
