using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;
    [SerializeField]
    bool showIntro;
    [HideInInspector]
    public bool settingTheBoard = true;


    [Header("base infos")]
    public int length;
    public int width;
    public int initialPlantsNumber;
    [Tooltip("if .2, you lose if you have less than 20% of the available tiles that are sane (mountains and fields and water don't count as available)")]
    public float saneTilesPercentageToLose;
    [Tooltip("if 20, you lose if you have less than 20 sane tiles (sane are grassy green)")]
    public int minNumberOfSaneTiles;

    public GameObject tilePrefab;
    [HideInInspector]
    public GameObject cameraPivot;

    public List<Material> Materials;
    [Header("terrains")]
    public Terrain empty;
    public Terrain water;
    public Terrain mountain;
    public Terrain field;
    public Terrain damaged;
    public Dictionary<terrainTypeEnum, Terrain> Terrains = new Dictionary<terrainTypeEnum, Terrain>();
    public Dictionary<Vector2, Tile> Tiles = new Dictionary<Vector2, Tile>();
    //[HideInInspector]
    //public List<Vector2> emptyTiles { get { return GrowthManager.instance.Occupants[state.healthy].listTiles; } }
    [HideInInspector]
    public int emptyTilesAtStart;
    public int SaneTiles;

    List<Vector2> availableNeighbours = new List<Vector2>();


    public void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        if (showIntro)
        {
            UIManager.instance.UpdateIntroImages();
        }

        // we set up the Terrain enums
        Terrains.Add(terrainTypeEnum.healthy, empty);
        Terrains.Add(terrainTypeEnum.water, water);
        Terrains.Add(terrainTypeEnum.mountain, mountain);
        Terrains.Add(terrainTypeEnum.field, field);
        Terrains.Add(terrainTypeEnum.damaged, damaged);


        StartCoroutine(CreateBoard());
    }

    /// <summary>
    /// checks around the current tile all those it can expand into 
    /// </summary>
    /// <param name="currentTilePos"></param>
    void CheckAvailableNeighbours(Vector2 currentTilePos)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2 currentPosModifier = new Vector2(x, y);
                if (Tiles.ContainsKey(currentTilePos + currentPosModifier) && Tiles[currentTilePos + currentPosModifier].Type == terrainTypeEnum.healthy)
                {
                    availableNeighbours.Add(currentTilePos + currentPosModifier);
                }
            }
        }
    }

    /// <summary>
    /// instantiate each tile of the board
    /// </summary>
    IEnumerator CreateBoard()
    {
        settingTheBoard = true;
        // we create the board
        for (int L = 0; L < length; L++)
        {
            for (int W = 0; W < width; W++)
            {
                GameObject tileObj = Instantiate(tilePrefab, this.transform);
                Tile tile = tileObj.GetComponent<Tile>();
                tile.pos.x = L;
                tile.pos.y = W;
                tileObj.transform.position = new Vector3(
                    L * tileObj.transform.localScale.x,
                    W * tileObj.transform.localScale.y,
                    tileObj.transform.position.z);
                Tiles.Add(tile.pos, tile);
            }
        }

        yield return null;

        UpdateTerrainType();

        if (cameraPivot == null)
        {
            cameraPivot = new GameObject("cameraPivot");
            Camera.main.transform.SetParent(cameraPivot.transform);
        }
        cameraPivot.transform.position = new Vector3(length / 2, width / 2, 0);
        UIManager.instance.SetCameraReset();

        FirstTurn();
    }

    void UpdateTerrainType()
    {
        emptyTilesAtStart = Tiles.Values.Count;
        foreach (Tile tile in Tiles.Values)
        {
            tile.State = occupantEnum.empty;
        }
        // we change the type of the tiles
        // for each terrain
        foreach (KeyValuePair<terrainTypeEnum, Terrain> pair in Terrains)
        {
            terrainTypeEnum currentType = pair.Key;
            // we plant the right number of seeds
            for (int i = 0; i < pair.Value.number; i++)
            {
                // we change the first tile
                Vector2 currentTilePos = GrowthManager.instance.Occupants[occupantEnum.empty].listTiles[Random.Range(0, GrowthManager.instance.Occupants[occupantEnum.empty].listTiles.Count)];
                Tiles[currentTilePos].Type = pair.Key;
                GrowthManager.instance.Occupants[occupantEnum.empty].listTiles.Remove(currentTilePos);
                availableNeighbours.Clear();
                emptyTilesAtStart--;

                // we check where it can grow
                CheckAvailableNeighbours(currentTilePos);

                // and we grow it appropriately
                for (int j = 1; j < Random.Range(pair.Value.Size.x, pair.Value.Size.y + 1); j++)
                {
                    Vector2 chosenNeighbour = availableNeighbours[Random.Range(0, availableNeighbours.Count)];
                    availableNeighbours.Remove(chosenNeighbour);
                    CheckAvailableNeighbours(chosenNeighbour);
                    Tiles[chosenNeighbour].Type = pair.Key;
                    emptyTilesAtStart--;
                }
            }
        }

        GrowthManager.instance.currentTurn = 0;
        SaneTiles = emptyTilesAtStart;

    }

    void ResetBoard()
    {
        settingTheBoard = true;
        //emptyTiles.Clear();
        foreach (Tile tile in Tiles.Values)
        {
            tile.Type = terrainTypeEnum.healthy;
        }
    }

    void DestroyBoard()
    {
        settingTheBoard = true;
        foreach (Tile tile in Tiles.Values)
        {
            Destroy(tile.gameObject);
        }
        Tiles.Clear();
        //emptyTiles.Clear();
    }

    public void NewGameLight()
    {
        ResetBoard();
        UpdateTerrainType();
        FirstTurn();
    }

    public void NewGameHeavy()
    {
        DestroyBoard();
        StartCoroutine(CreateBoard());
        FirstTurn();
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void FirstTurn()
    {
        SaneTiles = emptyTilesAtStart;
        settingTheBoard = false;
        for (int i = 0; i < initialPlantsNumber; i++)
        {
            GrowthManager.instance.CreateAtRandomPosition("plant");
        }

        GrowthManager.instance.ambientHealthy.Play();
        GrowthManager.instance.ambientInvasive.Play();
        GrowthManager.instance.currentTurn = 0;
        GrowthManager.instance.choseToKeepPlaying = false;

        foreach (OccupantManager occupant in GrowthManager.instance.Occupants.Values)
        {
            if (occupant.button)
            {
                occupant.lastCall = -1;
                occupant.button.interactable = occupant.isAvailable;
            }
        }
    }


}

[System.Serializable]
public class Terrain
{
    public terrainTypeEnum Type;
    public int number; // the number of seeds of this type of terrain
    public Vector2 Size; // how big each colony of this type of terrain is (random between x (included) and y (included))
    //public Material material;
    public List<Mesh> mesh;
    // public Sprite imageSides;
    // public Sprite imageTop;
}
