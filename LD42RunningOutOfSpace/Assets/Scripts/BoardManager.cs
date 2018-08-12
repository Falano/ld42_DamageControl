using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;

    [Header("base infos")]
    public int length;
    public int width;
    public int initialPlantsNumber;

    public GameObject tilePrefab;
    [HideInInspector]
    public GameObject cameraPivot;

    public List<Material> Materials;
    [Header("terrains")]
    public Terrain empty;
    public Terrain water;
    public Terrain mountain;
    public Terrain field;
    public Dictionary<type, Terrain> Terrains = new Dictionary<type, Terrain>();
    public Dictionary<Vector2, Tile> Tiles = new Dictionary<Vector2, Tile>();
    [HideInInspector]
    public List<Vector2> emptyTiles { get { return GrowthManager.instance.Occupants[state.healthy].listTiles; } }

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

        // we set up the Terrain enums
        Terrains.Add(type.empty, empty);
        Terrains.Add(type.water, water);
        Terrains.Add(type.mountain, mountain);
        Terrains.Add(type.field, field);

        StartCoroutine(CreateBoard());
        FirstTurn();
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
                if (Tiles.ContainsKey(currentTilePos + currentPosModifier) && Tiles[currentTilePos + currentPosModifier].Type == type.empty)
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
    }

    void UpdateTerrainType()
    {
        foreach (Tile tile in Tiles.Values)
        {
            tile.State = state.healthy;
        }
        // we change the type of the tiles
        // for each terrain
        foreach (KeyValuePair<type, Terrain> pair in Terrains)
        {
            type currentType = pair.Key;
            // we plant the right number of seeds
            for (int i = 0; i < pair.Value.number; i++)
            {
                // we change the first tile
                Vector2 currentTilePos = emptyTiles[Random.Range(0, emptyTiles.Count)];
                Tiles[currentTilePos].Type = pair.Key;
                emptyTiles.Remove(currentTilePos);
                availableNeighbours.Clear();

                // we check where it can grow
                CheckAvailableNeighbours(currentTilePos);

                // and we grow it appropriately
                for (int j = 1; j < Random.Range(pair.Value.Size.x, pair.Value.Size.y + 1); j++)
                {
                    Vector2 chosenNeighbour = availableNeighbours[Random.Range(0, availableNeighbours.Count)];
                    availableNeighbours.Remove(chosenNeighbour);
                    CheckAvailableNeighbours(chosenNeighbour);
                    Tiles[chosenNeighbour].Type = pair.Key;
                }
            }
        }

        GrowthManager.instance.currentTurn = 0;
    }

    void ResetBoard()
    {
        emptyTiles.Clear();
        foreach (Tile tile in Tiles.Values)
        {
            tile.Type = type.empty;
        }
    }

    void DestroyBoard()
    {
        foreach (Tile tile in Tiles.Values)
        {
            Destroy(tile.gameObject);
        }
        Tiles.Clear();
        emptyTiles.Clear();
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

    public void FirstTurn()
    {
        for(int i = 0; i < initialPlantsNumber; i++)
        {
            GrowthManager.instance.CreateOccupant(state.plant.ToString());
        }
    }
}

[System.Serializable]
public class Terrain
{
    public type Type;
    public int number; // the number of seeds of this type of terrain
    public Vector2 Size; // how big each colony of this type of terrain is (random between x (included) and y (included))
    //public Material material;
    public Mesh mesh;
    // public Sprite imageSides;
    // public Sprite imageTop;
}
