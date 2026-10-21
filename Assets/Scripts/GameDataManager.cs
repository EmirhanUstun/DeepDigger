
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    // Oyuncunun kaydedilen verileri
    private int money;
    private int energy;
    private int maxEnergy;
    private int depth;
    private int level;
    private int pickaxePower;
    private int miningEnergyCost;
    private int bagCapacity;

    // Simdilik mevcut 8 maden
    private int[] inventory = new int[8];

    private readonly int[] orePrices =
    {
        1, 3, 7, 15, 30, 60, 120, 250
    };

    public bool HasSavedData { get; private set; }

    public int Money => money;
    public int Energy => energy;
    public int Depth => depth;

    // Singleton: Baska sahneden de erisilebilir
    public static GameDataManager GetInstance()
    {
        if (Instance == null)
        {
            GameDataManager existing =
                FindFirstObjectByType<GameDataManager>();

            if (existing != null)
            {
                Instance = existing;
            }
            else
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

        // Sahne degisse bile bu nesne silinmez.
        DontDestroyOnLoad(gameObject);
    }

    // MADENCILIK VERILERINI SAKLA
    public void SaveFromMining(
        MiningManager mining,
        int[] bag
    )
    {
        if (mining == null || bag == null)
            return;

        money = mining.money;
        energy = mining.energy;
        maxEnergy = mining.maxEnergy;
        depth = mining.depth;
        level = mining.level;

        pickaxePower = mining.pickaxePower;
        miningEnergyCost = mining.miningEnergyCost;
        bagCapacity = mining.bagCapacity;

        inventory = (int[])bag.Clone();

        HasSavedData = true;
    }

    // MADENCILIK VERILERINI GERI YUKLE
    public void LoadIntoMining(
        MiningManager mining,
        int[] bag
    )
    {
        if (!HasSavedData || mining == null || bag == null)
            return;

        mining.money = money;
        mining.energy = energy;
        mining.maxEnergy = maxEnergy;
        mining.depth = depth;
        mining.level = level;

        mining.pickaxePower = pickaxePower;
        mining.miningEnergyCost = miningEnergyCost;
        mining.bagCapacity = bagCapacity;

        System.Array.Clear(bag, 0, bag.Length);

        System.Array.Copy(
            inventory,
            bag,
            Mathf.Min(inventory.Length, bag.Length)
        );
    }

    // CANTADAKI MADEN MIKTARI
    public int GetOreCount(int oreIndex)
    {
        if (oreIndex < 0 || oreIndex >= inventory.Length)
            return 0;

        return inventory[oreIndex];
    }

    // CANTANIN DOLULUGU
    public int BagCount()
    {
        int total = 0;

        foreach (int amount in inventory)
            total += amount;

        return total;
    }

    // CANTANIN TOPLAM DEGERI
    public int BagValue()
    {
        int total = 0;

        int count = Mathf.Min(
            inventory.Length,
            orePrices.Length
        );

        for (int i = 0; i < count; i++)
        {
            total += inventory[i] * orePrices[i];
        }

        return total;
    }

    // MAGAZA ICIN: HEPSINI SAT
    public int SellAllOres()
    {
        if (!HasSavedData)
            return 0;

        int earnings = BagValue();

        if (earnings <= 0)
            return 0;

        money += earnings;

        System.Array.Clear(
            inventory,
            0,
            inventory.Length
        );

        Debug.Log(
            "Tum madenler satildi. Kazanc: " + earnings
        );

        return earnings;
    }
        


    // 6x6 MADEN HARITASI
    // -1 = Kirilmis, bos hucre
    //  0 ve ustu = Maden turu

    private int[,] savedOreGrid;

    public void SaveMineGrid(int[,] grid)
    {
        if (grid == null)
            return;

        savedOreGrid = (int[,])grid.Clone();
    }

    public bool TryLoadMineGrid(
        int rows,
        int columns,
        out int[,] grid
    )
    {
        grid = null;

        if (savedOreGrid == null)
            return false;

        if (savedOreGrid.GetLength(0) != rows ||
            savedOreGrid.GetLength(1) != columns)
        {
            return false;
        }

        grid = (int[,])savedOreGrid.Clone();
        return true;
    }

    }
