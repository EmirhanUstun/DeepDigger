
using UnityEngine;

[RequireComponent(typeof(MinerController))]
public class MinerMovementSpeedApplier : MonoBehaviour
{
    public const string LevelKey =
        "DD_Upgrade_MoveSpeedLevel_v1";

    private MinerController miner;

    public static int GetSavedLevel()
    {
        return Mathf.Clamp(
            PlayerPrefs.GetInt(LevelKey, 1),
            1,
            6
        );
    }

    public static float GetMultiplier(int level)
    {
        level = Mathf.Clamp(level, 1, 6);

        return 1f + (level - 1) * 0.15f;
    }

    private void Awake()
    {
        miner = GetComponent<MinerController>();

        if (miner == null)
            return;

        // Inspector'daki mevcut hiz temel alinir.
        float baseMoveSpeed = miner.moveSpeed;

        int level = GetSavedLevel();

        miner.moveSpeed =
            baseMoveSpeed * GetMultiplier(level);

        Debug.Log(
            "Madenci hareket hizi: " +
            miner.moveSpeed +
            " | Seviye: " + level
        );
    }
}
