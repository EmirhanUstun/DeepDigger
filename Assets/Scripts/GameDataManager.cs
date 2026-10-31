
using System;
using System.IO;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    [Header("Kayit Durumu")]
    [SerializeField]
    private bool hasSavedData = false;

    public bool HasSavedData => hasSavedData;

    [Header("Madencilik Verileri")]
    public int money = 0;
    public int energy = 100;
    public int maxEnergy = 100;
    public int depth = 0;
    public int level = 1;
    public int pickaxePower = 1;
    public int miningEnergyCost = 5;
    public int bagCapacity = 20;

    [Header("Canta")]
    public int[] inventory = new int[8];

    [Header("Ozel Para Birimi")]
    public int premiumDiamonds = 0;

    private readonly int[] orePrices =
    {
        1, 3, 7, 15, 30, 60, 120, 250
    };

    private int[,] savedOreGrid;

    private bool hasMinerPosition = false;
    private int savedMinerRow = -1;
    private int savedMinerColumn = 2;

    private const string SaveFileName =
        "deep_digger_save_v1.json";

    [Serializable]
    private class SaveData
    {
        public bool hasSavedData;

        public int money;
        public int energy;
        public int maxEnergy;
        public int depth;
        public int level;
        public int pickaxePower;
        public int miningEnergyCost;
        public int bagCapacity;

        public int premiumDiamonds;
        public int[] inventory;

        public int gridRows;
        public int gridColumns;
        public int[] gridCells;

        public bool hasMinerPosition;
        public int minerRow;
        public int minerColumn;
    }

    private string SavePath =>
        Path.Combine(
            Application.persistentDataPath,
            SaveFileName
        );

    public static GameDataManager GetInstance()
    {
        if (Instance == null)
        {
            Instance =
                FindFirstObjectByType<GameDataManager>();

            if (Instance == null)
            {
                GameObject obj =
                    new GameObject("GameDataManager");

                Instance =
                    obj.AddComponent<GameDataManager>();
            }
        }

        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        LoadFromDisk();
    }

    // ===========================================
    // MADENCILIK VERILERINI KAYDET
    // ===========================================

    public void SaveFromMining(
        MiningManager mining,
        int[] bag)
    {
        if (mining == null)
            return;

        money = mining.money;
        energy = mining.energy;
        maxEnergy = mining.maxEnergy;
        depth = mining.depth;
        level = mining.level;
        pickaxePower = mining.pickaxePower;
        miningEnergyCost = mining.miningEnergyCost;
        bagCapacity = mining.bagCapacity;

        if (bag != null)
        {
            inventory = (int[])bag.Clone();
        }

        hasSavedData = true;

        SaveToDisk();
    }

    // ===========================================
    // MADENCILIK VERILERINI YUKLE
    // ===========================================

    public bool LoadIntoMining(
        MiningManager mining,
        int[] bag)
    {
        if (!hasSavedData || mining == null)
            return false;

        mining.money = money;
        mining.energy = energy;
        mining.maxEnergy = maxEnergy;
        mining.depth = depth;
        mining.level = level;
        mining.pickaxePower = pickaxePower;
        mining.miningEnergyCost = miningEnergyCost;
        mining.bagCapacity = bagCapacity;

        if (bag != null)
        {
            Array.Clear(
                bag,
                0,
                bag.Length
            );

            if (inventory != null)
            {
                Array.Copy(
                    inventory,
                    bag,
                    Mathf.Min(
                        inventory.Length,
                        bag.Length
                    )
                );
            }
        }

        return true;
    }

    // ===========================================
    // CANTA
    // ===========================================

    public int GetOreCount(int oreIndex)
    {
        if (inventory == null ||
            oreIndex < 0 ||
            oreIndex >= inventory.Length)
        {
            return 0;
        }

        return inventory[oreIndex];
    }

    public int BagCount()
    {
        if (inventory == null)
            return 0;

        int count = 0;

        foreach (int amount in inventory)
        {
            count += amount;
        }

        return count;
    }

    public int BagValue()
    {
        if (inventory == null)
            return 0;

        int value = 0;

        int length = Mathf.Min(
            inventory.Length,
            orePrices.Length
        );

        for (int i = 0; i < length; i++)
        {
            value +=
                inventory[i] * orePrices[i];
        }

        return value;
    }

    public int SellAllOres()
    {
        int earnings = BagValue();

        if (earnings <= 0)
            return 0;

        money += earnings;

        if (inventory != null)
        {
            Array.Clear(
                inventory,
                0,
                inventory.Length
            );
        }

        hasSavedData = true;

        SaveToDisk();

        Debug.Log(
            "Madenler satildi. Kazanc: " +
            earnings
        );

        return earnings;
    }

    // ===========================================
    // MADEN HARITASI
    // ===========================================

    public void SaveMineGrid(int[,] grid)
    {
        if (grid == null)
            return;

        savedOreGrid =
            (int[,])grid.Clone();

        SaveToDisk();
    }

    public bool TryLoadMineGrid(
        int rows,
        int columns,
        out int[,] grid)
    {
        grid = null;

        if (savedOreGrid == null)
            return false;

        if (savedOreGrid.GetLength(0) != rows ||
            savedOreGrid.GetLength(1) != columns)
        {
            return false;
        }

        grid =
            (int[,])savedOreGrid.Clone();

        return true;
    }

    // ===========================================
    // MADENCI KONUMU
    // ===========================================

    public void SaveMinerPosition(
        int row,
        int column)
    {
        savedMinerRow = row;
        savedMinerColumn = column;
        hasMinerPosition = true;

        SaveToDisk();
    }

    public bool TryLoadMinerPosition(
        out int row,
        out int column)
    {
        row = savedMinerRow;
        column = savedMinerColumn;

        return hasMinerPosition;
    }

    // ===========================================
    // DISKE KAYDET
    // ===========================================

    public void SaveToDisk()
    {
        try
        {
            SaveData save = new SaveData();

            save.hasSavedData = hasSavedData;

            save.money = money;
            save.energy = energy;
            save.maxEnergy = maxEnergy;
            save.depth = depth;
            save.level = level;
            save.pickaxePower = pickaxePower;
            save.miningEnergyCost = miningEnergyCost;
            save.bagCapacity = bagCapacity;

            save.premiumDiamonds = premiumDiamonds;

            save.inventory =
                inventory != null
                ? (int[])inventory.Clone()
                : new int[8];

            save.hasMinerPosition =
                hasMinerPosition;

            save.minerRow =
                savedMinerRow;

            save.minerColumn =
                savedMinerColumn;

            // Unity JsonUtility cok boyutlu
            // dizileri dogrudan kaydetmez.
            // Bu nedenle haritayi duzlestiriyoruz.
            if (savedOreGrid != null)
            {
                save.gridRows =
                    savedOreGrid.GetLength(0);

                save.gridColumns =
                    savedOreGrid.GetLength(1);

                save.gridCells = new int[
                    save.gridRows *
                    save.gridColumns
                ];

                int index = 0;

                for (int row = 0;
                     row < save.gridRows;
                     row++)
                {
                    for (int col = 0;
                         col < save.gridColumns;
                         col++)
                    {
                        save.gridCells[index++] =
                            savedOreGrid[row, col];
                    }
                }
            }

            string json =
                JsonUtility.ToJson(save, true);

            File.WriteAllText(
                SavePath,
                json
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "Oyun kaydedilemedi: " +
                exception.Message
            );
        }
    }

    // ===========================================
    // DISKTEN YUKLE
    // ===========================================

    public void LoadFromDisk()
    {
        if (!File.Exists(SavePath))
            return;

        try
        {
            string json =
                File.ReadAllText(SavePath);

            SaveData save =
                JsonUtility.FromJson<SaveData>(json);

            if (save == null)
                return;

            hasSavedData = save.hasSavedData;

            money = save.money;
            energy = save.energy;
            maxEnergy = Mathf.Max(
                1,
                save.maxEnergy
            );

            energy = Mathf.Clamp(
                energy,
                0,
                maxEnergy
            );

            depth = save.depth;

            level = Mathf.Max(
                1,
                save.level
            );

            pickaxePower = Mathf.Max(
                1,
                save.pickaxePower
            );

            miningEnergyCost = Mathf.Max(
                0,
                save.miningEnergyCost
            );

            bagCapacity = Mathf.Max(
                1,
                save.bagCapacity
            );

            premiumDiamonds = Mathf.Max(
                0,
                save.premiumDiamonds
            );

            inventory =
                save.inventory != null
                ? (int[])save.inventory.Clone()
                : new int[8];

            hasMinerPosition =
                save.hasMinerPosition;

            savedMinerRow =
                save.minerRow;

            savedMinerColumn =
                save.minerColumn;

            // 6x6 haritayi tekrar olustur.
            if (save.gridRows > 0 &&
                save.gridColumns > 0 &&
                save.gridCells != null &&
                save.gridCells.Length ==
                    save.gridRows * save.gridColumns)
            {
                savedOreGrid = new int[
                    save.gridRows,
                    save.gridColumns
                ];

                int index = 0;

                for (int row = 0;
                     row < save.gridRows;
                     row++)
                {
                    for (int col = 0;
                         col < save.gridColumns;
                         col++)
                    {
                        savedOreGrid[row, col] =
                            save.gridCells[index++];
                    }
                }
            }

            Debug.Log(
                "Deep Digger kaydi yuklendi."
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "Oyun kaydi okunamadi: " +
                exception.Message
            );
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            SaveToDisk();
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused)
            SaveToDisk();
    }

    private void OnApplicationQuit()
    {
        SaveToDisk();
    }
}
