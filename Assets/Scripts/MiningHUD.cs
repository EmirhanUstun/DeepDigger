
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiningHUD : MonoBehaviour
{
    [Header("Oyun Verileri")]
    public MiningManager miningManager;

    [Header("Altin")]
    public TMP_Text goldText;

    [Header("Elmas")]
    public TMP_Text diamondText;

    // Gecici ozel para birimi bakiyesi.
    // Daha sonra GameDataManager'a baglanacak.
    public int premiumDiamonds = 0;

    [Header("Enerji")]
    public Image energyFill;
    public TMP_Text energyText;

    [Header("Canta")]
    public Image bagFill;
    public TMP_Text bagText;

    [Header("Animasyon")]
    public float fillSpeed = 3f;

    private void Start()
    {
        SetupFill(energyFill);
        SetupFill(bagFill);

        // Oyun basladiginda barlar dogru dolulukta olsun.
        if (miningManager != null)
        {
            if (energyFill != null)
            {
                energyFill.fillAmount =
                    GetEnergyRatio();
            }

            if (bagFill != null)
            {
                bagFill.fillAmount =
                    GetBagRatio();
            }
        }

        RefreshHUD();
    }

    private void SetupFill(Image image)
    {
        if (image == null)
            return;

        image.type = Image.Type.Filled;
        image.fillMethod =
            Image.FillMethod.Horizontal;

        image.fillOrigin =
            (int)Image.OriginHorizontal.Left;

        image.raycastTarget = false;
    }

    private float GetEnergyRatio()
    {
        if (miningManager == null)
            return 0f;

        return Mathf.Clamp01(
            (float)miningManager.energy /
            Mathf.Max(1, miningManager.maxEnergy)
        );
    }

    private float GetBagRatio()
    {
        if (miningManager == null)
            return 0f;

        return Mathf.Clamp01(
            (float)miningManager.BagCount() /
            Mathf.Max(1, miningManager.bagCapacity)
        );
    }

    private void Update()
    {
        RefreshHUD();
    }

    private void RefreshHUD()
    {
        if (miningManager == null)
            return;

        // ALTIN
        if (goldText != null)
        {
            goldText.text =
                miningManager.money.ToString("N0");
        }

        // OZEL ELMAS BAKIYESI
        if (diamondText != null)
        {
            diamondText.text =
                premiumDiamonds.ToString();
        }

        // ENERJI
        float energyRatio = GetEnergyRatio();

        if (energyFill != null)
        {
            energyFill.fillAmount =
                Mathf.MoveTowards(
                    energyFill.fillAmount,
                    energyRatio,
                    fillSpeed * Time.deltaTime
                );

            energyFill.color = energyRatio <= 0.25f
                ? new Color(0.86f, 0.22f, 0.17f)
                : new Color(0.91f, 0.71f, 0.28f);
        }

        if (energyText != null)
        {
            energyText.text =
                miningManager.energy + "/" +
                miningManager.maxEnergy;
        }

        // CANTA
        int bagCount =
            miningManager.BagCount();

        float bagRatio = GetBagRatio();

        if (bagFill != null)
        {
            bagFill.fillAmount =
                Mathf.MoveTowards(
                    bagFill.fillAmount,
                    bagRatio,
                    fillSpeed * Time.deltaTime
                );

            bagFill.color = bagRatio >= 0.90f
                ? new Color(0.95f, 0.50f, 0.20f)
                : new Color(0.23f, 0.80f, 0.65f);
        }

        if (bagText != null)
        {
            bagText.text =
                bagCount + "/" +
                miningManager.bagCapacity;
        }
    }
}
