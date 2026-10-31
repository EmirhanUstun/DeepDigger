
using UnityEngine;
using UnityEngine.UI;

public class UpgradeLevelSlots : MonoBehaviour
{
    [Header("Kazma Kutucuklari")]
    public Image[] pickaxeSlots = new Image[4];

    [Header("Enerji Kutucuklari")]
    public Image[] energySlots = new Image[6];

    [Header("Canta Kutucuklari")]
    public Image[] bagSlots = new Image[4];

    [Header("Hareket Hizi Kutucuklari")]
public Image[] speedSlots = new Image[4];

    [Header("Dolu Kutu Rengi")]
    public Color filledColor = new Color(
        1f, 0.72f, 0.15f, 1f
    );

    private UpgradeController upgradeController;

    private void Awake()
    {
        upgradeController =
            GetComponent<UpgradeController>();
    }

    private void OnEnable()
    {
        RefreshSlots();
    }

    private void Start()
    {
        RefreshSlots();
    }

    public void RefreshSlots()
    {
        GameDataManager data =
            GameDataManager.GetInstance();

        if (data == null)
            return;

        if (upgradeController == null)
        {
            upgradeController =
                GetComponent<UpgradeController>();
        }

        int maxPickaxe = upgradeController != null
            ? upgradeController.maxPickaxePower
            : 10;

        int maxEnergy = upgradeController != null
            ? upgradeController.maxEnergyCapacity
            : 300;

        int maxBag = upgradeController != null
            ? upgradeController.maxBagCapacity
            : 100;

        // Her bolumde kac kutu dolacak?
        int pickaxeFilled = GetFilledCount(
            data.pickaxePower,
            1,
            maxPickaxe,
            pickaxeSlots.Length
        );

        int energyFilled = GetFilledCount(
            data.maxEnergy,
            100,
            maxEnergy,
            energySlots.Length
        );

        int bagFilled = GetFilledCount(
            data.bagCapacity,
            20,
            maxBag,
            bagSlots.Length
        );

        int speedLevel =
    MinerMovementSpeedApplier.GetSavedLevel();

UpdateSlots(
    speedSlots,
    Mathf.Clamp(
        speedLevel - 1,
        0,
        speedSlots.Length
    )
);

        UpdateSlots(pickaxeSlots, pickaxeFilled);
        UpdateSlots(energySlots, energyFilled);
        UpdateSlots(bagSlots, bagFilled);
    }

    private int GetFilledCount(
        int current,
        int startingValue,
        int maxValue,
        int slotCount)
    {
        if (slotCount <= 0)
            return 0;

        if (current <= startingValue)
            return 0;

        if (maxValue <= startingValue)
            return slotCount;

        float progress = Mathf.Clamp01(
            (float)(current - startingValue) /
            (maxValue - startingValue)
        );

        // Ilk yukseltmeden sonra
        // ilk kutu tamamen dolsun.
        int filled = Mathf.CeilToInt(
            progress * slotCount
        );

        return Mathf.Clamp(
            filled,
            1,
            slotCount
        );
    }

    private void UpdateSlots(
        Image[] slots,
        int filledCount)
    {
        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            Image slot = slots[i];

            if (slot == null)
                continue;

            // Kutu tamamen dolu veya bos.
            slot.type = Image.Type.Simple;
            slot.fillAmount = 1f;
            slot.color = filledColor;

            slot.enabled = i < filledCount;
            slot.raycastTarget = false;
        }
    }
}
