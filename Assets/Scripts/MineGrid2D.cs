
using UnityEngine;

public class MineGrid2D : MonoBehaviour
{
    public MiningManager miningManager;

    [Header("Harita")]
    public int columns = 6;
    public int rows = 6;
    public float tileSize = 0.60f;

    private Sprite tileSprite;

    // -1 = kirilmis hucre
    //  0+ = maden turu
    private int[,] oreGrid;

    // Ekrandaki blok nesneleri
    private GameObject[,] tileObjects;

    private readonly Color[] oreColors =
    {
        new Color(0.48f, 0.43f, 0.39f), // Tas
        new Color(0.16f, 0.17f, 0.20f), // Komur
        new Color(0.75f, 0.38f, 0.18f), // Bakir
        new Color(0.49f, 0.57f, 0.61f), // Demir
        new Color(0.78f, 0.82f, 0.87f), // Gumus
        new Color(0.98f, 0.72f, 0.13f), // Altin
        new Color(0.38f, 0.25f, 0.88f), // Kristal
        new Color(0.22f, 0.84f, 0.95f)  // Elmas
    };

    private void Start()
    {
        if (miningManager == null)
        {
            Debug.LogError("MiningManager baglanmadi!");
            enabled = false;
            return;
        }

        if (columns < 1 || rows < 1 || tileSize <= 0f)
        {
            Debug.LogError("Gecersiz harita boyutu!");
            enabled = false;
            return;
        }

        // Gecici renkli kare sprite
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        tileSprite = Sprite.Create(
            texture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f
        );

        tileObjects = new GameObject[rows, columns];

        GameDataManager data = GameDataManager.GetInstance();

        // Once eski kazilmis haritayi yuklemeyi dene.
        if (!data.TryLoadMineGrid(
                rows, columns, out oreGrid))
        {
            GenerateNewGrid();
            data.SaveMineGrid(oreGrid);
        }

        CreateVisibleTiles();
    }

    // YENI HARITA
    private void GenerateNewGrid()
    {
        oreGrid = new int[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int targetDepth =
                    miningManager.depth + row + 1;

                oreGrid[row, col] =
                    miningManager.GetRandomOre(targetDepth);
            }
        }
    }

    // KAYITLI HARITAYI EKRANA CIZ
    private void CreateVisibleTiles()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int oreIndex = oreGrid[row, col];

                if (oreIndex < 0)
                    continue;

                CreateTile(row, col, oreIndex);
            }
        }
    }

    // TEK BLOK OLUSTUR
    private void CreateTile(
        int row,
        int col,
        int oreIndex
    )
    {
        if (oreIndex < 0 || oreIndex >= oreColors.Length)
        {
            Debug.LogError("Gecersiz maden indexi!");
            return;
        }

        GameObject tile = new GameObject(
            "Ore_" + col + "_" + row
        );

        tile.transform.SetParent(transform, false);
        tile.transform.localPosition =
            GetTilePosition(row, col);

        tile.transform.localScale =
            Vector3.one * (tileSize * 0.94f);

        SpriteRenderer renderer =
            tile.AddComponent<SpriteRenderer>();

        renderer.sprite = tileSprite;
        renderer.sortingOrder = 1;
        renderer.color = GetTileColor(oreIndex, row);

        tile.AddComponent<BoxCollider2D>();

        OreTile oreTile =
            tile.AddComponent<OreTile>();

        oreTile.grid = this;
        oreTile.row = row;
        oreTile.column = col;
        oreTile.oreIndex = oreIndex;

        tileObjects[row, col] = tile;
    }

    // BLOK KONUMU
    private Vector3 GetTilePosition(int row, int col)
    {
        float startX =
            -(columns - 1) * tileSize / 2f;

        float startY =
            (rows - 1) * tileSize / 2f;

        return new Vector3(
            startX + col * tileSize,
            startY - row * tileSize - 0.5f,
            0f
        );
    }

    // BLOK RENGI
    private Color GetTileColor(int oreIndex, int row)
    {
        Color color = oreColors[oreIndex];

        if (row > 0)
        {
            color = Color.Lerp(
                color,
                Color.black,
                0.25f
            );
        }

        return color;
    }

    // HUCRE BOS MU?
    private bool IsEmpty(int row, int col)
    {
        if (row < 0 || row >= rows ||
            col < 0 || col >= columns)
        {
            return false;
        }

        return oreGrid[row, col] == -1;
    }

    // BU BLOK KIRILABILIR MI?
    private bool CanDig(int row, int col)
    {
        // Ust sira her zaman erisilebilir.
        if (row == 0)
            return true;

        // Acilmis bosluklara komsu bloklar kirilabilir.
        return
            IsEmpty(row - 1, col) ||
            IsEmpty(row + 1, col) ||
            IsEmpty(row, col - 1) ||
            IsEmpty(row, col + 1);
    }

    // BLOK KIRMA
    public void TryDig(OreTile tile)
    {
        if (tile == null)
            return;

        int row = tile.row;
        int col = tile.column;

        if (row < 0 || row >= rows ||
            col < 0 || col >= columns)
        {
            return;
        }

        // Hucre zaten kirilmissa islem yapma.
        if (oreGrid[row, col] < 0)
            return;

        // Bu nesne bu hucreye ait mi?
        if (tileObjects[row, col] != tile.gameObject)
            return;

        // Erisilebilir olmayan blok kirilamaz.
        if (!CanDig(row, col))
        {
            Debug.Log("Once bu bloga bir yol ac!");
            return;
        }

        int oreIndex = oreGrid[row, col];

        int targetDepth =
            miningManager.depth + row + 1;

        // Enerji, canta ve maden kontrolu
        if (!miningManager.TryMineBlock(
                oreIndex, targetDepth))
        {
            return;
        }

        // Yalnizca tiklanan blogu kir.
        oreGrid[row, col] = -1;
        tileObjects[row, col] = null;

        Destroy(tile.gameObject);

        // En alt sira kirildiysa bir sira ilerle.
        if (row == rows - 1)
        {
            miningManager.AdvanceDepth();
            ScrollGridUp();
        }

        // Kirik blok durumunu kaydet.
        GameDataManager.GetInstance()
            .SaveMineGrid(oreGrid);
    }

    // HARITAYI BIR SIRA YUKARI KAYDIR
    private void ScrollGridUp()
    {
        // 1. En ust sirayi yok et.
        for (int col = 0; col < columns; col++)
        {
            if (tileObjects[0, col] != null)
            {
                Destroy(tileObjects[0, col]);
            }
        }

        // 2. Diger bes sirayi bir yukari tasi.
        for (int row = 0; row < rows - 1; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                oreGrid[row, col] =
                    oreGrid[row + 1, col];

                tileObjects[row, col] =
                    tileObjects[row + 1, col];

                GameObject moved = tileObjects[row, col];

                if (moved != null)
                {
                    moved.transform.localPosition =
                        GetTilePosition(row, col);

                    OreTile oreTile =
                        moved.GetComponent<OreTile>();

                    oreTile.row = row;
                    oreTile.column = col;

                    SpriteRenderer renderer =
                        moved.GetComponent<SpriteRenderer>();

                    renderer.color = GetTileColor(
                        oreTile.oreIndex,
                        row
                    );
                }
            }
        }

        // 3. En alta yeni maden sirasi ekle.
        int bottomRow = rows - 1;

        for (int col = 0; col < columns; col++)
        {
            int targetDepth =
                miningManager.depth + bottomRow + 1;

            int oreIndex =
                miningManager.GetRandomOre(targetDepth);

            oreGrid[bottomRow, col] = oreIndex;

            tileObjects[bottomRow, col] = null;

            CreateTile(bottomRow, col, oreIndex);
        }

        Debug.Log(
            "Yeni maden sirasi! Derinlik: " +
            miningManager.depth + " m"
        );
    }

    private void OnDestroy()
    {
        if (tileSprite != null)
        {
            Texture2D texture = tileSprite.texture;

            Destroy(tileSprite);
            Destroy(texture);
        }
    }
}

// HER MADEN BLOGUNUN TIKLAMA BILESENI
public class OreTile : MonoBehaviour
{
    public MineGrid2D grid;

    public int oreIndex;
    public int row;
    public int column;

    private void OnMouseDown()
    {
        if (grid != null)
        {
            grid.TryDig(this);
        }
    }
}
