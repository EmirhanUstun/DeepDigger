
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiningManager : MonoBehaviour
{
    [Header("Oyuncu")]
    public int money = 0;
    public int energy = 100;
    public int maxEnergy = 100;
    public int depth = 0;
    public int level = 1;

    [Header("Madencilik")]
    public int pickaxePower = 1;
    public int miningEnergyCost = 5;


[Header("Yuzeye Donus")]
public int surfaceReturnEnergyCost = 10;

[Header("Acil Cikis")]
[Min(0)]
public int emergencyRescueCost = 20;


    [Header("Envanter")]
    public int bagCapacity = 20;

    private readonly string[] oreNames =
    {
        "Tas", "Komur", "Bakir", "Demir",
        "Gumus", "Altin", "Kristal", "Elmas"
    };

    private readonly int[] orePrices =
    {
        1, 3, 7, 15, 30, 60, 120, 250
    };

    private readonly int[] unlockDepths =
    {
        0, 10, 25, 50, 100, 200, 350, 500
    };

    private readonly int[] oreWeights =
    {
        60, 25, 10, 4, 2, 1, 1, 1
    };

    private int[] inventory = new int[8];

    private string message = "Bir bloga tiklayarak kaz!";
    private int emergencyRescueFee;

    // OYUN ACILINCA VERILERI YUKLE
    private void Awake()
    {
        GameDataManager data =
            GameDataManager.GetInstance();

        if (data.HasSavedData)
        {
            data.LoadIntoMining(this, inventory);

            Debug.Log("Kayitli madencilik verileri yuklendi!");
        }
        else
        {
            data.SaveFromMining(this, inventory);

            Debug.Log("Yeni oyuncu verileri olusturuldu!");
        }
    }

    // OYUNCU VERILERINI SAKLA
    private void SaveState()
    {
        GameDataManager.GetInstance()
            .SaveFromMining(this, inventory);
    }


public void ReturnToSurface()
{
    int cost = Mathf.Max(0, surfaceReturnEnergyCost);

    // Yeterli enerji var mi?
    if (energy < cost)
    {
        message =
            "Yuzeye cikmak icin " +
            cost + " enerji gerekiyor!";

        Debug.Log(message);
        return;
    }

    // SurfaceHub sahnesi ekli mi?
    if (!Application.CanStreamedLevelBeLoaded("SurfaceHub"))
    {
        message = "SurfaceHub sahnesi Scene List'te yok!";
        Debug.LogError(message);
        return;
    }

    // Yuzeye donus enerji maliyeti
    energy -= cost;

    // Verileri kaydet
    SaveState();

    Debug.Log(
        "Yuzeye donuldu. Kalan enerji: " + energy
    );

    // Maden koyune don
    SceneManager.LoadScene("SurfaceHub");
}


public void EmergencyReturnToSurface()
{
    int normalCost = Mathf.Max(
        0, surfaceReturnEnergyCost
    );

    // Normal donus yapilabiliyorsa acil cikis gerekmez.
    if (energy >= normalCost)
    {
        message = "Normal donus icin yeterli enerjin var!";
        return;
    }

    // Sahne kontrolu
    if (!Application.CanStreamedLevelBeLoaded("SurfaceHub"))
    {
        Debug.LogError("SurfaceHub sahnesi bulunamadi!");
        return;
    }

    // Cantadaki madenlerin toplam satis degeri
    int bagWorth = BagValue();

    // Kurtarma ucreti
    int fee = Mathf.Max(0, emergencyRescueFee);

    // Tum madenler satilir.
    // Kurtarma ucreti toplam kaynaklardan dusulur.
    int totalFunds = money + bagWorth;

    money = Mathf.Max(0, totalFunds - fee);

    // Canta bosaltilir
    System.Array.Clear(
        inventory,
        0,
        inventory.Length
    );

    // Acil donus sonrasi enerji sifir kalir.
    energy = 0;

    // Oyuncu verilerini kaydet
    SaveState();

    Debug.Log(
        "Acil cikis yapildi! " +
        "Satilan maden degeri: " + bagWorth +
        " | Kalan para: " + money
    );

    // Maden koyune don
    SceneManager.LoadScene("SurfaceHub");
}



    public int BagCount()
    {
        int total = 0;

        foreach (int count in inventory)
            total += count;

        return total;
    }

    public int BagValue()
    {
        int total = 0;

        for (int i = 0; i < inventory.Length; i++)
        {
            total += inventory[i] * orePrices[i];
        }

        return total;
    }

    public int GetRandomOre(int targetDepth)
    {
        int totalWeight = 0;

        for (int i = 0; i < oreWeights.Length; i++)
        {
            if (targetDepth >= unlockDepths[i])
                totalWeight += oreWeights[i];
        }

        int roll = Random.Range(0, totalWeight);

        for (int i = 0; i < oreWeights.Length; i++)
        {
            if (targetDepth < unlockDepths[i])
                continue;

            roll -= oreWeights[i];

            if (roll < 0)
                return i;
        }

        return 0;
    }

    // MADEN KAZMA
  
public bool TryMineBlock(
    int oreIndex,
    int targetDepth = -1
)
{
    if (oreIndex < 0 || oreIndex >= inventory.Length)
        return false;

    int actualDepth = targetDepth >= 0
        ? targetDepth
        : depth + 1;

    if (actualDepth < unlockDepths[oreIndex])
        return false;

    if (energy < miningEnergyCost)
    {
        message = "Yeterli enerji yok!";
        return false;
    }

    if (BagCount() >= bagCapacity)
    {
        message = "Canta dolu! Once sat.";
        return false;
    }

    int amount = Mathf.Min(
        pickaxePower,
        bagCapacity - BagCount()
    );

    energy -= miningEnergyCost;
    inventory[oreIndex] += amount;

    // Burada artik depth++ YOK.
    message = amount + " " +
        oreNames[oreIndex] + " kazdin!";

    SaveState();

    return true;
}


public void AdvanceDepth()
{
    depth++;

    if (depth == 100)
    {
        message = "100 m! Hasarli asansor bulundu!";
    }
    else
    {
        message = "Derinlik: " + depth + " m";
    }

    SaveState();
}


    // TUM MADENLERI SAT
    public void Sell()
    {
        int value = BagValue();

        if (value == 0)
        {
            message = "Canta bos!";
            return;
        }

        money += value;

        for (int i = 0; i < inventory.Length; i++)
            inventory[i] = 0;

        message = value + " para kazandin!";

        // YENI
        SaveState();
    }

    // ENERJI YENILE
    public void Rest()
    {
        energy = maxEnergy;

        message = "Enerji yenilendi!";

        // YENI
        SaveState();
    }

    public int UpgradeCost()
    {
        return 50 * level * level;
    }

    // KAZMAYI GELISTIR
    public void UpgradePickaxe()
    {
        int cost = UpgradeCost();

        if (money < cost)
        {
            message = "Yeterli para yok!";
            return;
        }

        money -= cost;
        level++;
        pickaxePower++;
        maxEnergy += 10;
        energy = maxEnergy;
        bagCapacity += 5;

        message =
            "Kazma seviye " + level + " oldu!";

        // YENI
        SaveState();
    }

    // GECICI PROTOTIP ARAYUZU
    
private void OldDebugGUI_Disabled()
{
    float scale = Mathf.Min(
        Screen.width / 420f,
        Screen.height / 750f
    );

    GUI.matrix = Matrix4x4.Scale(
        new Vector3(scale, scale, 1f)
    );

    float h = Screen.height / scale;

    GUI.skin.label.fontSize = 19;
    GUI.skin.button.fontSize = 18;

    // UST BILGI PANELI
    GUILayout.BeginArea(
        new Rect(15, 10, 390, 180)
    );

    GUILayout.Label("DEEP DIGGER");

    GUILayout.Label(
        "Para: " + money +
        "   Derinlik: " + depth + " m"
    );

    GUILayout.Label(
        "Enerji: " + energy + "/" + maxEnergy
    );

    GUILayout.Label(
        "Canta: " + BagCount() + "/" + bagCapacity
    );

    GUILayout.Label(
        "Seviye: " + level +
        "   Canta degeri: " + BagValue()
    );

    GUILayout.EndArea();

    // ALT BUTON PANELI
    GUILayout.BeginArea(
        new Rect(15, h - 245, 390, 235)
    );

    GUILayout.Label(message);

    GUILayout.BeginHorizontal();

    if (GUILayout.Button(
        "SAT",
        GUILayout.Height(45)
    ))
    {
        Sell();
    }

    if (GUILayout.Button(
        "DINLEN",
        GUILayout.Height(45)
    ))
    {
        Rest();
    }

    GUILayout.EndHorizontal();

    if (GUILayout.Button(
        "KAZMAYI GELISTIR - " + UpgradeCost(),
        GUILayout.Height(45)
    ))
    {
        UpgradePickaxe();
    }

    
int normalCost = Mathf.Max(0, surfaceReturnEnergyCost);

if (energy >= normalCost)
{
    // NORMAL DONUS
    if (GUILayout.Button(
        "YUZEYE DON (-" + normalCost + " ENERJI)",
        GUILayout.Height(45)
    ))
    {
        ReturnToSurface();
    }
}
else
{
    // ACIL CIKIS
    if (GUILayout.Button(
        "ACIL CIKIS (CANTA SATILIR)",
        GUILayout.Height(45)
    ))
    {
        EmergencyReturnToSurface();
    }
}


    GUILayout.EndArea();
}

}
