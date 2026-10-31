
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MineGrid2D : MonoBehaviour
{
    [Header("Oyun Sistemleri")]
    public MiningManager miningManager;
    public MinerController miner;

    [Header("6x6 Harita")]
    public int columns = 6;
    public int rows = 6;
    public float tileSize = 0.60f;

    [Header("Maden Gorselleri")]
    public Sprite[] oreSprites = new Sprite[10];

    [Header("Kazma Seviyesi")]
    [Range(0, 9)]
    public int equippedPickaxeTier = 0;

    [Header("Test")]
    public bool debugClicks = false;

    // -1 = kirilmis / bos hucre
    // 0 ve uzeri = maden turu
    private int[,] oreGrid;
    private GameObject[,] tileObjects;

    private Texture2D fallbackTexture;
    private Sprite fallbackSprite;

    private bool ready = false;
    private bool isBusy = false;
    private bool isHolding = false;

    private int activeTouchId = -1;

    // Tas, Komur, Bakir, Demir,
    // Gumus, Altin, Kristal, Elmas,
    // Zumrut, Safir
    private static readonly int[] OreDifficultyTier =
    {
        0, 0, 2, 3, 4, 5, 6, 7, 8, 9
    };

    // Vector2Int:
    // X = sutun
    // Y = satir
    private static readonly Vector2Int[] Directions =
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0)
    };

    private readonly Color[] oreColors =
    {
        new Color(0.48f, 0.43f, 0.39f),
        new Color(0.16f, 0.17f, 0.20f),
        new Color(0.75f, 0.38f, 0.18f),
        new Color(0.49f, 0.57f, 0.61f),
        new Color(0.78f, 0.82f, 0.87f),
        new Color(0.98f, 0.72f, 0.13f),
        new Color(0.38f, 0.25f, 0.88f),
        new Color(0.22f, 0.84f, 0.95f),
        new Color(0.19f, 0.75f, 0.43f),
        new Color(0.13f, 0.33f, 0.90f)
    };

    // ==================================================
    // BASLANGIC
    // ==================================================

    private void Start()
    {
        if (miningManager == null)
        {
            Debug.LogError(
                "MineGrid2D: MiningManager baglanmamis!",
                this
            );

            enabled = false;
            return;
        }

        if (miner == null)
        {
            miner =
                GetComponentInChildren<MinerController>();
        }

        if (miner == null ||
            miner.transform.parent != transform)
        {
            Debug.LogError(
                "Miner, MineGrid nesnesinin dogrudan altinda olmali!",
                this
            );

            enabled = false;
            return;
        }

        if (rows != 6 ||
            columns != 6 ||
            tileSize <= 0f)
        {
            Debug.LogError(
                "MineGrid2D: 6x6 harita gerekli!",
                this
            );

            enabled = false;
            return;
        }

        fallbackTexture = new Texture2D(1, 1);

        fallbackTexture.SetPixel(
            0,
            0,
            Color.white
        );

        fallbackTexture.Apply();

        fallbackSprite = Sprite.Create(
            fallbackTexture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f
        );

        tileObjects =
            new GameObject[rows, columns];

        GameDataManager data =
            GameDataManager.GetInstance();

        if (!data.TryLoadMineGrid(
            rows,
            columns,
            out oreGrid))
        {
            GenerateNewGrid();
            data.SaveMineGrid(oreGrid);
        }

        CreateHitAreas();
        CreateVisibleTiles();
        InitializeMiner(data);

        ready = true;
    }

    // ==================================================
    // UI BUTON KONTROLU
    // ==================================================

    private bool IsPointerOverUI(int pointerId)
    {
        if (EventSystem.current == null)
            return false;

        // Telefon / tablet
        if (pointerId >= 0)
        {
            return EventSystem.current
                .IsPointerOverGameObject(pointerId);
        }

        // Fare
        return EventSystem.current
            .IsPointerOverGameObject();
    }

    // ==================================================
    // FARE VE DOKUNMATIK GIRIS
    // ==================================================

    private bool PointerIsHeld()
    {
        if (!isHolding)
            return false;

        if (activeTouchId >= 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (touch.fingerId == activeTouchId)
                {
                    return
                        touch.phase != TouchPhase.Ended &&
                        touch.phase != TouchPhase.Canceled;
                }
            }

            return false;
        }

        return Input.GetMouseButton(0);
    }

    private void Update()
    {
        // Oyuncu biraktiysa kaziyi durdur.
        if (isHolding && !PointerIsHeld())
        {
            isHolding = false;
            activeTouchId = -1;
        }

        if (!ready ||
            isBusy ||
            miner == null ||
            miner.IsMoving)
        {
            return;
        }

        Vector2 screenPoint;
        int pointerId = -1;

        // Dokunmatik giris
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase != TouchPhase.Began)
                return;

            screenPoint = touch.position;
            pointerId = touch.fingerId;
        }
        else
        {
            // Fare girisi
            if (!Input.GetMouseButtonDown(0))
                return;

            screenPoint = Input.mousePosition;
        }

        // YENI:
        // UI butonuna basiliyorsa maden secme.
        if (IsPointerOverUI(pointerId))
            return;

        Camera cam = Camera.main;

        if (cam == null)
            return;

        Ray ray =
            cam.ScreenPointToRay(screenPoint);

        Plane plane = new Plane(
            transform.forward,
            transform.position
        );

        float distance;

        if (!plane.Raycast(ray, out distance))
            return;

        Vector3 point =
            transform.InverseTransformPoint(
                ray.GetPoint(distance)
            );

        int row;
        int col;

        if (!TryGetCell(
            point,
            out row,
            out col))
        {
            return;
        }

        activeTouchId = pointerId;
        isHolding = true;

        HandleCellClick(row, col);
    }

    // ==================================================
    // GORUNMEZ HUCRE TIKLAMA ALANLARI
    // ==================================================

    private void CreateHitAreas()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject hitObject =
                    new GameObject(
                        "CellClick_" + row + "_" + col
                    );

                hitObject.transform.SetParent(
                    transform,
                    false
                );

                hitObject.transform.localPosition =
                    GetTilePosition(row, col) +
                    new Vector3(0f, 0f, -0.05f);

                BoxCollider2D collider =
                    hitObject.AddComponent<BoxCollider2D>();

                collider.size =
                    Vector2.one * (tileSize * 0.98f);

                MineCellInput input =
                    hitObject.AddComponent<MineCellInput>();

                input.grid = this;
                input.row = row;
                input.column = col;
            }
        }
    }

    public void OnCellPressed(int row, int col)
    {
        if (!ready ||
            isBusy ||
            miner == null ||
            miner.IsMoving)
        {
            return;
        }

        int pointerId =
            Input.touchCount > 0
            ? Input.GetTouch(0).fingerId
            : -1;

        // YENI:
        // UI uzerinden gelen tiklamalari engelle.
        if (IsPointerOverUI(pointerId))
            return;

        activeTouchId = pointerId;
        isHolding = true;

        HandleCellClick(row, col);
    }

    public void OnCellReleased()
    {
        isHolding = false;
        activeTouchId = -1;
    }

    // ==================================================
    // HUCRE KOORDINATLARI
    // ==================================================

    private bool TryGetCell(
        Vector3 point,
        out int row,
        out int col)
    {
        float startX =
            -(columns - 1) * tileSize / 2f;

        float startY =
            (rows - 1) * tileSize / 2f - 0.5f;

        col = Mathf.RoundToInt(
            (point.x - startX) / tileSize
        );

        row = Mathf.RoundToInt(
            (startY - point.y) / tileSize
        );

        if (!IsInsideGrid(row, col))
            return false;

        Vector3 center =
            GetTilePosition(row, col);

        return
            Mathf.Abs(point.x - center.x)
                <= tileSize * 0.5f &&
            Mathf.Abs(point.y - center.y)
                <= tileSize * 0.5f;
    }

    private bool IsInsideGrid(int row, int col)
    {
        return
            row >= 0 &&
            row < rows &&
            col >= 0 &&
            col < columns;
    }

    private bool IsWalkable(int row, int col)
    {
        if (col < 0 || col >= columns)
            return false;

        // Giris koridoru
        if (row == -1)
            return true;

        return
            IsInsideGrid(row, col) &&
            oreGrid[row, col] == -1;
    }

    // ==================================================
    // HUCREYE BASILINCA
    // ==================================================

    private void HandleCellClick(int row, int col)
    {
        if (!ready ||
            isBusy ||
            !IsInsideGrid(row, col))
        {
            return;
        }

        if (debugClicks)
        {
            Debug.Log(
                "Hucre: " + row + ", " + col +
                " / Maden: " + oreGrid[row, col]
            );
        }

        // BOS HUCRE
        if (oreGrid[row, col] == -1)
        {
            List<Vector2Int> path =
                FindPath(row, col);

            if (path != null && path.Count > 0)
            {
                StartCoroutine(
                    WalkOnly(path)
                );
            }
        }
        else
        {
            // MADEN BLOGU
            List<Vector2Int> path =
                FindPathNextToOre(row, col);

            if (path == null)
            {
                Debug.Log(
                    "Madene ulasilabilecek bos yol bulunamadi."
                );

                return;
            }

            StartCoroutine(
                WalkAndMine(row, col, path)
            );
        }
    }

    // ==================================================
    // EN KISA YOL BULMA
    // ==================================================

    private List<Vector2Int> FindPath(
        int targetRow,
        int targetCol)
    {
        if (!IsWalkable(targetRow, targetCol))
            return null;

        Vector2Int start = new Vector2Int(
            miner.CurrentColumn,
            miner.CurrentRow
        );

        Vector2Int goal = new Vector2Int(
            targetCol,
            targetRow
        );

        if (!IsWalkable(start.y, start.x))
            return null;

        if (start == goal)
            return new List<Vector2Int>();

        Queue<Vector2Int> queue =
            new Queue<Vector2Int>();

        HashSet<Vector2Int> visited =
            new HashSet<Vector2Int>();

        Dictionary<Vector2Int, Vector2Int> previous =
            new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (Vector2Int direction in Directions)
            {
                Vector2Int next =
                    current + direction;

                if (visited.Contains(next) ||
                    !IsWalkable(next.y, next.x))
                {
                    continue;
                }

                visited.Add(next);
                previous[next] = current;

                if (next == goal)
                {
                    List<Vector2Int> path =
                        new List<Vector2Int>();

                    Vector2Int step = goal;

                    while (step != start)
                    {
                        path.Add(step);
                        step = previous[step];
                    }

                    path.Reverse();

                    return path;
                }

                queue.Enqueue(next);
            }
        }

        return null;
    }

    private List<Vector2Int> FindPathNextToOre(
        int row,
        int col)
    {
        List<Vector2Int> best = null;

        foreach (Vector2Int direction in Directions)
        {
            List<Vector2Int> candidate =
                FindPath(
                    row + direction.y,
                    col + direction.x
                );

            if (candidate != null &&
                (best == null ||
                 candidate.Count < best.Count))
            {
                best = candidate;
            }
        }

        return best;
    }

    // ==================================================
    // BOS HUCREDE YURUME
    // ==================================================

    private IEnumerator WalkOnly(
        List<Vector2Int> path)
    {
        isBusy = true;

        yield return StartCoroutine(
            FollowPath(path)
        );

        isBusy = false;
    }

    // ==================================================
    // KAZMA SEVIYESI VE KIRMA SURELERI
    // ==================================================


private int GetUpgradedPickaxeTier()
{
    GameDataManager data =
        GameDataManager.GetInstance();

    if (data == null)
        return 0;

    // UpgradeScene'de satin alinan seviye.
    int level = Mathf.Clamp(
        data.pickaxePower,
        1,
        5
    );

    // 5 seviye, 10 farkli kazma gucu.
    switch (level)
    {
        case 1:
            return 0;

        case 2:
            return 2;

        case 3:
            return 4;

        case 4:
            return 6;

        case 5:
            return 9;

        default:
            return 0;
    }
}


    private float GetMiningDuration(int oreIndex)
    {
        if (oreIndex < 0 ||
            oreIndex >= OreDifficultyTier.Length)
        {
            return float.PositiveInfinity;
        }

        int difference =
            GetUpgradedPickaxeTier() -
            OreDifficultyTier[oreIndex];

        if (difference >= 2)
            return 1f;

        if (difference == 1)
            return 1.5f;

        if (difference == 0)
            return 2f;

        if (difference == -1)
            return 4f;

        if (difference == -2)
            return 6f;

        return float.PositiveInfinity;
    }

    // ==================================================
    // BASILI TUTARAK BLOK KIRMA
    // ==================================================

    private IEnumerator WalkAndMine(
        int targetRow,
        int targetCol,
        List<Vector2Int> path)
    {
        isBusy = true;

        // Madenciyi madenin yanina getir.
        yield return StartCoroutine(
            FollowPath(path)
        );

        if (!IsInsideGrid(targetRow, targetCol) ||
            oreGrid[targetRow, targetCol] < 0 ||
            !miner.CanMine(targetRow, targetCol) ||
            !PointerIsHeld())
        {
            isBusy = false;
            yield break;
        }

        int oreIndex =
            oreGrid[targetRow, targetCol];

        float duration =
            GetMiningDuration(oreIndex);

        if (float.IsInfinity(duration))
        {
            Debug.Log(
                "Bu maden icin daha guclu kazma gerekli."
            );

            isBusy = false;
            yield break;
        }

        // Enerji veya canta kontrolu
        if (miningManager.energy <
                miningManager.miningEnergyCost ||
            miningManager.BagCount() >=
                miningManager.bagCapacity)
        {
            Debug.Log(
                "Enerji yetersiz veya canta dolu."
            );

            isBusy = false;
            yield break;
        }

        GameObject block =
            tileObjects[targetRow, targetCol];

        if (block == null)
        {
            isBusy = false;
            yield break;
        }

        Vector3 original =
            GetTilePosition(targetRow, targetCol);

        // Catlak efekti
        GameObject visual =
            new GameObject("MiningCrackVisual");

        visual.transform.SetParent(
            transform,
            false
        );

        visual.transform.localPosition =
            original;

        MiningCrackEffect crack =
            visual.AddComponent<MiningCrackEffect>();

        crack.Initialize(tileSize);

        float elapsed = 0f;

        // Basili tutuldugu surece kaz.
        while (elapsed < duration &&
               PointerIsHeld())
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsed / duration);

            crack.SetProgress(progress);

            block.transform.localPosition =
                original +
                new Vector3(
                    Mathf.Sin(Time.time * 40f)
                        * 0.006f,
                    0f,
                    0f
                );

            yield return null;
        }

        // Blok konumunu sifirla.
        block.transform.localPosition =
            original;

        Destroy(visual);

        // Erken birakildiysa kaziyi iptal et.
        if (!PointerIsHeld() ||
            elapsed < duration)
        {
            isBusy = false;
            yield break;
        }

        int targetDepth =
            miningManager.depth +
            targetRow + 1;

        // Enerji ve canta yalnizca
        // basarili kazida guncellenir.
        if (!miningManager.TryMineBlock(
            oreIndex,
            targetDepth))
        
        {
            isBusy = false;
            yield break;
        }

        // Blok artik bos.
        oreGrid[targetRow, targetCol] = -1;

        Destroy(block);

        tileObjects[targetRow, targetCol] =
            null;

        int destinationRow =
            targetRow;

        // En alt sira kirildiysa ilerle.
        if (targetRow == rows - 1)
        {
            miningManager.AdvanceDepth();

            ScrollGridUp();

            int shiftedRow =
                miner.CurrentRow >= 0
                ? miner.CurrentRow - 1
                : -1;

            miner.Initialize(
                shiftedRow,
                miner.CurrentColumn,
                GetTilePosition(
                    shiftedRow,
                    miner.CurrentColumn
                )
            );

            destinationRow = rows - 2;
        }

        GameDataManager data =
            GameDataManager.GetInstance();

        data.SaveMineGrid(oreGrid);

        // Madenci kirilan bloga girer.
        miner.MoveToCell(
            destinationRow,
            targetCol,
            GetTilePosition(
                destinationRow,
                targetCol
            )
        );

        yield return null;

        while (miner.IsMoving)
        {
            yield return null;
        }

        data.SaveMinerPosition(
            destinationRow,
            targetCol
        );

        isBusy = false;

        // =====================================
        // HAZINE KESFI
        // =====================================

        TreasureDropManager treasureManager =
            GetComponent<TreasureDropManager>();

        if (treasureManager != null &&
            treasureManager.isActiveAndEnabled)
        {
            treasureManager.QueueOreMined(
                targetDepth
            );
        }
    }

    // ==================================================
    // TUNELDE ADIM ADIM HAREKET
    // ==================================================

    private IEnumerator FollowPath(
        List<Vector2Int> path)
    {
        GameDataManager data =
            GameDataManager.GetInstance();

        foreach (Vector2Int step in path)
        {
            if (!IsWalkable(step.y, step.x))
                yield break;

            miner.MoveToCell(
                step.y,
                step.x,
                GetTilePosition(step.y, step.x)
            );

            yield return null;

            while (miner.IsMoving)
            {
                yield return null;
            }

            data.SaveMinerPosition(
                step.y,
                step.x
            );
        }
    }

    // Diger scriptlerle uyumluluk
    public void TryDig(OreTile tile)
    {
        if (tile != null &&
            tile.grid == this)
        {
            HandleCellClick(
                tile.row,
                tile.column
            );
        }
    }

    // ==================================================
    // MADEN IZGARASINI OLUSTUR
    // ==================================================

    private void GenerateNewGrid()
    {
        oreGrid = new int[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                oreGrid[row, col] =
                    miningManager.GetRandomOre(
                        miningManager.depth +
                        row + 1
                    );
            }
        }
    }

    private void CreateVisibleTiles()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (oreGrid[row, col] >= 0)
                {
                    CreateTile(
                        row,
                        col,
                        oreGrid[row, col]
                    );
                }
            }
        }
    }

    // ==================================================
    // MADENCI BASLANGIC KONUMU
    // ==================================================

    private void InitializeMiner(
        GameDataManager data)
    {
        int row = -1;
        int col = 2;

        int savedRow;
        int savedCol;

        if (data.TryLoadMinerPosition(
            out savedRow,
            out savedCol) &&
            IsWalkable(savedRow, savedCol))
        {
            row = savedRow;
            col = savedCol;
        }

        miner.Initialize(
            row,
            col,
            GetTilePosition(row, col)
        );

        data.SaveMinerPosition(
            row,
            col
        );
    }

    // ==================================================
    // MADEN BLOGU GORSELI
    // ==================================================

    private void CreateTile(
        int row,
        int col,
        int oreIndex)
    {
        GameObject tile =
            new GameObject(
                "Ore_" + col + "_" + row
            );

        tile.transform.SetParent(
            transform,
            false
        );

        tile.transform.localPosition =
            GetTilePosition(row, col);

        SpriteRenderer renderer =
            tile.AddComponent<SpriteRenderer>();

        Sprite selected = null;

        if (oreSprites != null &&
            oreIndex >= 0 &&
            oreIndex < oreSprites.Length)
        {
            selected =
                oreSprites[oreIndex];
        }

        renderer.sprite =
            selected != null
            ? selected
            : fallbackSprite;

        renderer.sortingOrder = 1;

        renderer.color =
            selected != null
            ? Color.white
            : GetFallbackColor(
                oreIndex,
                row
            );

        Vector2 spriteSize =
            renderer.sprite.bounds.size;

        float size =
            tileSize * 0.94f;

        tile.transform.localScale =
            new Vector3(
                size / spriteSize.x,
                size / spriteSize.y,
                1f
            );

        OreTile tileData =
            tile.AddComponent<OreTile>();

        tileData.grid = this;
        tileData.row = row;
        tileData.column = col;
        tileData.oreIndex = oreIndex;

        tileObjects[row, col] = tile;
    }

    private Vector3 GetTilePosition(
        int row,
        int col)
    {
        float startX =
            -(columns - 1) *
            tileSize / 2f;

        float startY =
            (rows - 1) *
            tileSize / 2f;

        return new Vector3(
            startX + col * tileSize,
            startY - row * tileSize - 0.5f,
            0f
        );
    }

    private Color GetFallbackColor(
        int oreIndex,
        int row)
    {
        Color color = oreColors[
            Mathf.Clamp(
                oreIndex,
                0,
                oreColors.Length - 1
            )
        ];

        return row > 0
            ? Color.Lerp(
                color,
                Color.black,
                0.25f
            )
            : color;
    }

    // ==================================================
    // 6x6 HARITAYI YUKARI KAYDIR
    // ==================================================

    private void ScrollGridUp()
    {
        // En ust siradaki eski bloklari sil.
        for (int col = 0; col < columns; col++)
        {
            if (tileObjects[0, col] != null)
            {
                Destroy(tileObjects[0, col]);
            }
        }

        // Eski 1-5 satirlari
        // yeni 0-4 satirlari olur.
        for (int row = 0; row < rows - 1; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                oreGrid[row, col] =
                    oreGrid[row + 1, col];

                tileObjects[row, col] =
                    tileObjects[row + 1, col];

                GameObject tile =
                    tileObjects[row, col];

                if (tile == null)
                    continue;

                tile.transform.localPosition =
                    GetTilePosition(row, col);

                OreTile data =
                    tile.GetComponent<OreTile>();

                data.row = row;
                data.column = col;

                SpriteRenderer renderer =
                    tile.GetComponent<SpriteRenderer>();

                renderer.color =
                    renderer.sprite == fallbackSprite
                    ? GetFallbackColor(
                        data.oreIndex,
                        row
                    )
                    : Color.white;
            }
        }

        // En alta 6 yeni blok ekle.
        int bottom = rows - 1;

        for (int col = 0; col < columns; col++)
        {
            int oreIndex =
                miningManager.GetRandomOre(
                    miningManager.depth +
                    bottom + 1
                );

            oreGrid[bottom, col] =
                oreIndex;

            tileObjects[bottom, col] =
                null;

            CreateTile(
                bottom,
                col,
                oreIndex
            );
        }
    }

    private void OnDestroy()
    {
        if (fallbackSprite != null)
            Destroy(fallbackSprite);

        if (fallbackTexture != null)
            Destroy(fallbackTexture);
    }
}

// ======================================================
// MADEN BLOGU VERISI
// ======================================================

public class OreTile : MonoBehaviour
{
    public MineGrid2D grid;
    public int oreIndex;
    public int row;
    public int column;
}

// ======================================================
// GORUNMEZ HUCRE GIRISI
// ======================================================

public class MineCellInput : MonoBehaviour
{
    public MineGrid2D grid;
    public int row;
    public int column;

    private void OnMouseDown()
    {
        if (grid != null)
        {
            grid.OnCellPressed(
                row,
                column
            );
        }
    }

    private void OnMouseUp()
    {
        if (grid != null)
        {
            grid.OnCellReleased();
        }
    }
}

// ======================================================
// CATLAMA EFEKTI
// ======================================================

public class MiningCrackEffect : MonoBehaviour
{
    private LineRenderer[] lines;
    private Material material;

    public void Initialize(float tileSize)
    {
        Shader shader =
            Shader.Find("Sprites/Default");

        if (shader == null)
        {
            Debug.LogWarning(
                "Sprites/Default shader bulunamadi."
            );

            return;
        }

        material = new Material(shader);
        lines = new LineRenderer[3];

        AddLine(
            0,
            tileSize,
            new Vector2[]
            {
                new Vector2(-0.18f, 0.37f),
                new Vector2(-0.07f, 0.17f),
                new Vector2(0.06f, 0.02f),
                new Vector2(-0.08f, -0.24f)
            }
        );

        AddLine(
            1,
            tileSize,
            new Vector2[]
            {
                new Vector2(0.06f, 0.02f),
                new Vector2(0.24f, 0.17f),
                new Vector2(0.35f, 0.31f)
            }
        );

        AddLine(
            2,
            tileSize,
            new Vector2[]
            {
                new Vector2(0.06f, 0.02f),
                new Vector2(0.27f, -0.13f),
                new Vector2(0.14f, -0.37f)
            }
        );

        SetProgress(0f);
    }

    private void AddLine(
        int index,
        float size,
        Vector2[] points)
    {
        GameObject obj =
            new GameObject(
                "Crack_" + index
            );

        obj.transform.SetParent(
            transform,
            false
        );

        LineRenderer line =
            obj.AddComponent<LineRenderer>();

        line.useWorldSpace = false;
        line.material = material;
        line.sortingOrder = 6;

        line.startWidth =
            size * 0.032f;

        line.endWidth =
            size * 0.024f;

        line.startColor =
            new Color(
                0.1f,
                0.06f,
                0.04f,
                1f
            );

        line.endColor =
            line.startColor;

        line.positionCount =
            points.Length;

        for (int i = 0; i < points.Length; i++)
        {
            line.SetPosition(
                i,
                new Vector3(
                    points[i].x * size,
                    points[i].y * size,
                    -0.01f
                )
            );
        }

        lines[index] = line;
    }

    public void SetProgress(float progress)
    {
        if (lines == null)
            return;

        lines[0].enabled =
            progress >= 0.25f;

        lines[1].enabled =
            progress >= 0.50f;

        lines[2].enabled =
            progress >= 0.75f;
    }

    private void OnDestroy()
    {
        if (material != null)
            Destroy(material);
    }
}
