using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager: MonoBehaviour
{
    public int length;
    public int width;

    public GameObject tilePrefab;

    [Header("terrains")]
    public Terrain water;
    public Terrain mountain;
    public Terrain field;
    Dictionary<type, Terrain> Terrains = new Dictionary<type, Terrain>();
    Dictionary<Vector2, Tile> Tiles = new Dictionary<Vector2, Tile>();
    List<Vector2> emptyTiles = new List<Vector2>();

    List<Vector2> availableNeighbours = new List<Vector2>();

    public void Start()
    {
        // we set up the Terrain enums
        Terrains.Add(type.water, water);
        Terrains.Add(type.mountain, mountain);
        Terrains.Add(type.field, field);

        // we create the board
        for (int L = 0; L < length; L++)
        {
            for (int W = 0; W < width; W++)
            {
                GameObject tileObj = Instantiate(tilePrefab, this.transform);
                Tile tile = tileObj.GetComponent<Tile>();
                tile.pos.x = L;
                tile.pos.y = W;
                tileObj.transform.position.x = L * tileObj.transform.size.x; // check once we know what a tile is
                tileObj.transform.position.y = W * tileObj.transform.size.y; // check once we know what a tile is
                Tiles.Add(tile.pos, tile);
                emptyTiles.Add(tile.pos);
            }
        }
        

        // we change the type of the tiles
        // for each terrain
        foreach(KeyValuePair pair in Terrains)
        {
            type currentType = pair.Value.Type;
            // we plant the right number of seeds
            for (int i = 0; i < pair.Value.number; i++)
            {
                // we change the first tile
                Vector2 currentTilePos = Random.Range(0, emptyTiles.Count);
                Tiles[emptyTiles[currentTilePos]].Type = pair.Key;
                emptyTiles.RemoveAt(currentTilePos);
                availableNeighbours.Clear();

                // we check where it can grow
                CheckAvailableNeighbours(currentTilePos);

                // and we grow it appropriately
                for (int j = 0; j < Random.Range(pair.Value.Size.x, pair.Value.Size.y+1); j ++)
                {
                    Vector2 chosenNeighbour = availableNeighbours[Random.Range(0, availableNeighbours.Count)];
                    availableNeighbours.Remove(chosenNeighbour);
                    CheckAvailableNeighbours(chosenNeighbour);
                }
            }
        }
    }

    /// <summary>
    /// checks around the current tile all those it can expand into 
    /// </summary>
    /// <param name="currentTilePos"></param>
    public void CheckAvailableNeighbours (Vector2 currentTilePos)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2 currentPosModifier = new Vector2(x, y);
                if (Tiles.HasKey(currentTilePos + currentPosModifier) && Tiles[currentTilePos + currentPosModifier].Type == type.empty)
                {
                    availableNeighbours.Add(currentTilePos + currentPosModifier);
                }
            }
        }
    }

}

[System.Serializable]
public class Terrain
{
    type Type;
    int number; // the number of seed of this type of terrain
    Vector2 Size; // how big each colony of this type of terrain is (random between x (included) and y (included))
    Color color;
    Sprite image;
}