using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrowthManager : MonoBehaviour
{

    public static GrowthManager instance;

    public int currentTurn = 0;
    public bool createdSomethingThisTurn;
    public bool choseToKeepPlaying;
    public AudioSource ambient;
    public AudioSource sfx;

    [Header("occupants")]
    public Occupant healthy;
    public Occupant plant;
    public Occupant rabbit;
    public Occupant cat;
    public Occupant fox;
    public Occupant eagle;
    public Occupant hunter;
    public Occupant ranger;
    public Dictionary<state, Occupant> Occupants = new Dictionary<state, Occupant>();



    // Use this for initialization
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        Occupants.Add(state.healthy, healthy);
        Occupants.Add(state.plant, plant);
        Occupants.Add(state.rabbit, rabbit);
        Occupants.Add(state.cat, cat);
        Occupants.Add(state.fox, fox);
        Occupants.Add(state.eagle, eagle);
        Occupants.Add(state.hunter, hunter);
        Occupants.Add(state.ranger, ranger);

        ambient.loop = true;

    }

    public void EndOfTurn()
    {
        StartCoroutine(UIManager.instance.rotateTimeHolder());

        createdSomethingThisTurn = false;

        //Debug.Log("1) Starting end of turn " + currentTurn);
        // for each state
        foreach (Occupant occupant in Occupants.Values)
        {
            if (occupant.State != state.healthy)
            {
                int total = occupant.listTiles.Count;
                // for each instance / tile occupied
                for (int i = 0; i < total; i++)
                {
                    //Debug.Log("2-3) tile: " + occupant.listTiles[i].ToString() + " of " + occupant.State.ToString());
                    // we make the special or normal move

                    // it's fine if we grew new members, but if we lost someone it creates a bug
                    if (total <= occupant.listTiles.Count)
                    {
                        occupant.updateMoveStats(occupant.listTiles[i]);
                        if (occupant.haveNeighbourhoodTilesASpecialType)
                        {
                            occupant.specialMove();
                        }
                        else
                        {
                            for (int j = 0; j < occupant.ToCleanTilesNumber; j++)
                            {
                                occupant.move();
                            }
                        }
                    }
                }
                // we reset its availibility
                //occupant.isAvailable = null;
                if (occupant.button)
                {
                    occupant.button.interactable = occupant.isAvailable;
                }
            }

            if (occupant.firstApparition == currentTurn && occupant.introImage)
            {
                UIManager.instance.ToggleNewAnimal(occupant.introImage);
                //occupant.button.interactable = true;
            }

            //Debug.Log(occupant.State.ToString() +  " is available? " + occupant.isAvailable + "; last call: " + occupant.lastCall + "; currentTurn: " + currentTurn)
        }

        if (!choseToKeepPlaying && (Occupants[state.healthy].listTiles.Count < 5 || eagle.listTiles.Count > BoardManager.instance.length))
        {
            UIManager.instance.ToggleEndGame(true);
            choseToKeepPlaying = true;
        }

        currentTurn++;
        ambient.volume = ((float)Occupants[state.healthy].listTiles.Count / BoardManager.instance.Tiles.Count);

    }

    public void CreateOccupant(string occupant)
    {
        StopAllCoroutines();
        //Debug.Log("1) creating occupant " + occupant);
        state Occupant = (state)System.Enum.Parse(typeof(state), occupant);
        StartCoroutine(CreateOccupantCoroutine(Occupant));
    }

    IEnumerator CreateOccupantCoroutine(state Occupant)
    {
        for (int i = 0; i < 2; i++)
        {
            yield return null;
        }
        // check time
        //cooldown for adding amx
        // button.interactive = isAvailable
        // non si fa qui

        //Debug.Log("2) start coroutine");
        // create the occupant at the right place
        switch (Occupant)
        {
            // the hunter appears at a random point
            case state.hunter:
            case state.plant:
                int tmpPos;
                do
                {
                    tmpPos = Random.Range(0, BoardManager.instance.emptyTiles.Count);
                }
                while (BoardManager.instance.Tiles[BoardManager.instance.emptyTiles[tmpPos]].Type != type.empty);

                CreateAtPos(Occupant, BoardManager.instance.emptyTiles[tmpPos]);
                //Debug.Log("3a) creating hunter at random point");
                break;
            // all the others appear on the tile clicked
            default:
                //.Log("3b) waiting for click to choose tile to occupy");
                yield return StartCoroutine(CreatingOccupant(Occupant));
                break;
        }
        //Occupants[Occupant].isAvailable = null;
    }

    IEnumerator CreatingOccupant(state Occupant)
    {
        Vector2 pos = new Vector2(0, 0);
        // wait until someone clicked a tile empty in a healthy or prey state
        yield return new WaitUntil(delegate
        {
            RaycastHit hit;
            if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                if (BoardManager.instance.Tiles[hit.transform.position].Type == type.empty && (BoardManager.instance.Tiles[hit.transform.position].State == state.healthy || Occupants[Occupant].preys.Contains(BoardManager.instance.Tiles[hit.transform.position].State)))
                {
                    pos = hit.transform.position;
                    return true;
                }
            }
            return false;
        });

        //Debug.Log("3.5b) tile clicked!");
        CreateAtPos(Occupant, pos);

    }

    public void CreateAtPos(state occupant, Vector2 pos)
    {
        //Debug.Log("4) actually creating occupant " + occupant.ToString() + " at pos " + pos.ToString());

        BoardManager.instance.Tiles[pos].State = occupant;

        // and we keep the player from adding more
        createdSomethingThisTurn = true;
        foreach (Occupant occ in Occupants.Values)
        {
            if (occ.button)
            {
                occ.button.interactable = occ.isAvailable;
            }
        }
        Occupants[occupant].lastCall = currentTurn;
        //Debug.Log(Occupants[occupant].State.ToString() + "'s last call (in CreateAtPos): " + Occupants[occupant].lastCall);
        sfx.clip = Occupants[occupant].sound;
        sfx.Play();
    }
}

[System.Serializable]
public class Occupant
{
    public static Dictionary<state, Occupant> Occs { get { return GrowthManager.instance.Occupants; } }
    public static Dictionary<Vector2, Tile> Tiles { get { return BoardManager.instance.Tiles; } }

    public state State;
    public List<Mesh> meshes;
    //public List<state> predators;
    public List<state> preys;
    public type specialType;
    public Button button;
    public Sprite introImage;
    public AudioClip sound;

    [Header("game design stuff")]
    public int firstApparition;

    [Tooltip("cooldown between two calls from the player to add a new animal of this type")]
    public int cooldown;
    [Tooltip("cooldown between two movements of the animals of this type on the map")]
    public int moveCooldown;
    [Tooltip("clean all the tiles in cleanable tiles, or choose only a few of those?")]
    [SerializeField]
    bool cleanAllAvailableTiles;
    [Tooltip("Of all the tiles we could clean, how many will we")]
    [SerializeField]
    int _toCleanTilesNumber;
    [HideInInspector]
    public int ToCleanTilesNumber
    {
        get
        {
            if (cleanAllAvailableTiles)
            {
                return relativeTilesAvailableToBeCleaned.Count;
            }
            else
            {
                return _toCleanTilesNumber;
            }
        }
    }
    [Tooltip("position relative to the animal")]
    [SerializeField]
    List<Vector2> relativeTilesAvailableToBeCleaned;
    public int numberOfKids;
    [Tooltip("and free the space where you were")]
    public bool shouldMove;
    public moveType moveType;
    public List<Vector2> relativeTilesAvailableForMovement;
    public int longevity;
    [Tooltip("can this animal end the game if there are too many of them")]
    public bool canOverrun;
    public int overrunPercentage;

    public int maxNumberOfInstances;
    [Tooltip("you need at least one of these on the map to create one of this")]
    public List<state> necessaryPrecursors;

    bool hasEnoughPrecursors;

    public List<Vector2> listTiles = new List<Vector2>();
    [HideInInspector]
    public int lastCall;
    public bool isAvailable
    {
        get
        {
            if (GrowthManager.instance.currentTurn < firstApparition || GrowthManager.instance.createdSomethingThisTurn)
            {
                return false;
            }

            // does it have the precursors it needs to be available in button?
            hasEnoughPrecursors = false;
            foreach (state s in necessaryPrecursors)
            {
                if (Occs[s].listTiles.Count > 0)
                {
                    hasEnoughPrecursors = true;
                }
            }

            // if it has one of the precursors it needs and won't be more numerous than allowed
            if ((necessaryPrecursors.Count == 0 || hasEnoughPrecursors) && listTiles.Count < maxNumberOfInstances)
            {
                // it can be spawned when the cooldown is finished
                return (GrowthManager.instance.currentTurn - this.lastCall >= cooldown);
            }

            return false;
        }
    }


    int tilesCleaned;
    Vector2 currentPos;
    Vector2 currentMove;
    List<Vector2> possibleMoves = new List<Vector2>();
    List<Vector2> preferedMoves = new List<Vector2>();
    List<Vector2> toCleanAbsolute;
    List<Vector2> _toMoveAbsolute;
    List<Vector2> toMoveAbsolute
    {
        get
        {
            switch (moveType)
            {
                case moveType.anywhere:
                    _toMoveAbsolute = Occs[state.healthy].listTiles;
                    break;
                case moveType.cleanTiles:
                    _toMoveAbsolute = toCleanAbsolute;
                    break;
                case moveType.list:
                    _toMoveAbsolute.Clear();
                    foreach (Vector2 tile in relativeTilesAvailableForMovement)
                    {
                        if (Tiles[(currentPos + tile)].State == state.healthy || preys.Contains(Tiles[(currentPos + tile)].State))
                        {
                            _toMoveAbsolute.Add(currentPos + tile);
                        }
                    }
                    break;
            }
            return _toMoveAbsolute;
        }
    }
    List<Vector2> _neighbourhoodTiles = new List<Vector2>();
    List<Vector2> NeighbourhoodTiles
    {
        get
        {
            if (_neighbourhoodTiles.Count == 0)
            {

                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        _neighbourhoodTiles.Add(new Vector2(i, j));
                    }
                }
            }
            return _neighbourhoodTiles;
        }
        set
        {
            _neighbourhoodTiles = value;
        }
    }
    /*
    List<Vector2> _movementRange = new List<Vector2>();
    List<Vector2> MovementRange
    {
        get
        {
            if (_movementRange.Count == 0)
            {
                switch (State)
                {
                    case state.rabbit:
                    case state.plant:
                    case state.hunter:
                        _movementRange.Add(new Vector2(0, 1));
                        _movementRange.Add(new Vector2(0, -1));
                        _movementRange.Add(new Vector2(1, 0));
                        _movementRange.Add(new Vector2(-1, 0));
                        break;
                    case state.cat:
                    case state.ranger:
                        for (int i = -1; i <= 1; i++)
                        {
                            for (int j = -1; j <= 1; j++)
                            {
                                _movementRange.Add(new Vector2(i, j));
                            }
                        }
                        break;
                    case state.fox:
                        for (int i = -2; i <= 2; i++)
                        {
                            for (int j = -2; j <= 2; j++)
                            {
                                _movementRange.Add(new Vector2(i, j));
                            }
                        }
                        break;
                }
            }

            return _movementRange;
        }
    }*/

    [HideInInspector]
    public bool haveNeighbourhoodTilesASpecialType;

    Tile currentTile;
    bool canMove;

    public void move()
    {
        if (State == state.healthy)
        {
            //Debug.Log("healthy is a victim that can't spread on its own");
            return;
        }

        /*
         // farli muorire? dare una variabile (per species) Longevity
        if(Random.Range(0, 5) == 0)
        {
            BoardManager.instance.Tiles[currentPos].State = state.healthy;
            return;
        }*/

        // actual movement
        switch (State)
        {
            // those that move
            case state.ranger:
            case state.hunter:
                if (canMove)
                {
                    BoardManager.instance.Tiles[currentPos].State = state.healthy;
                }
                if (preferedMoves.Count > 0)
                {
                    // trying to always go to the highest prey; to do later
                    /*                    List<Vector2> preferedMovesSorted = new List<Vector2>();
                                        foreach(Vector2 tilePos in preferedMoves)
                                        {
                                            if((int)BoardManager.instance.Tiles[preferedMoves[tilePos]].State <= )
                                        }*/
                    currentTile = BoardManager.instance.Tiles[preferedMoves[Random.Range(0, preferedMoves.Count)]];
                }
                else if (possibleMoves.Count > 0)
                {
                    {
                        currentTile = BoardManager.instance.Tiles[possibleMoves[Random.Range(0, possibleMoves.Count)]];
                    }
                }
                break;
            // those that grow
            case state.plant:
            case state.rabbit:
            case state.cat:
            case state.fox:
                if (preferedMoves.Count > 0)
                {
                    currentTile = BoardManager.instance.Tiles[preferedMoves[Random.Range(0, preferedMoves.Count)]];
                }
                else if (possibleMoves.Count > 0)
                {
                    {
                        currentTile = BoardManager.instance.Tiles[possibleMoves[Random.Range(0, possibleMoves.Count)]];
                    }
                }
                break;

            case state.eagle: // pulizia // tranne le piante
                if ((GrowthManager.instance.currentTurn - lastCall) % 4 == 0)
                {
                    /*
                    if (Random.Range(0, 7) != 1)
                    {
                        BoardManager.instance.Tiles[currentPos].State = state.healthy;
                    }
                    */
                    if (preferedMoves.Count > 0)
                    {
                        currentTile = BoardManager.instance.Tiles[preferedMoves[Random.Range(0, preferedMoves.Count)]];
                    }
                    else if (possibleMoves.Count > 0)
                    {
                        {
                            currentTile = BoardManager.instance.Tiles[possibleMoves[Random.Range(0, possibleMoves.Count)]];
                        }
                    }
                    foreach (Vector2 tile in NeighbourhoodTiles)
                    {
                        if (BoardManager.instance.Tiles.ContainsKey(currentPos + tile) && tile != Vector2.zero && BoardManager.instance.Tiles[currentPos + tile].State != state.plant)
                        {
                            BoardManager.instance.Tiles[currentPos + tile].State = state.healthy;
                        }
                    }
                }
                break;
            default:
                //Debug.Log("No move: returning");
                break;
        }
        //Debug.Log("can move?" + canMove.ToString() + "; turning " + currentTile.pos.ToString() + " from " + currentTile.State.ToString() + " to " + State.ToString());
        if (canMove)
        {
            currentTile.State = State;
        }
    }




    public void updateMoveStats(Vector2 pos)
    {
        currentPos = pos;
        canMove = true;


        // check which we can clean
        toCleanAbsolute.Clear();
        foreach( Vector2 tile in relativeTilesAvailableToBeCleaned)
        {
            if(preys.Contains(Tiles[currentPos + tile].State))
            {
                toCleanAbsolute.Add(currentPos + tile);
            }
        }
        // decide which we will clean
        // we can clean Count tiles
        int count = toCleanAbsolute.Count;
        // we need to clean up to ToCleanTilesNumber tiles
        // so we trim ToCleanAbsolute as needed
        while(count < ToCleanTilesNumber)
        {
            toCleanAbsolute.RemoveAt(Random.Range(0, toCleanAbsolute.Count));
        }


        // are we allowed our special move?
        haveNeighbourhoodTilesASpecialType = false;
        //Debug.Log("1) are neighbours special (" + specialType.ToString() + ") ? " + haveNeighbourhoodTilesASpecialType.ToString());
        if (specialType != type.empty)
        {
            foreach (Vector2 tile in NeighbourhoodTiles)
            {
                if (BoardManager.instance.Tiles.ContainsKey(tile + currentPos) && BoardManager.instance.Tiles[tile + currentPos].Type == specialType)
                {
                    haveNeighbourhoodTilesASpecialType = true;
                }
            }
        }


        // where can we or would we like to go?
        possibleMoves.Clear();
        preferedMoves.Clear();

        foreach (Vector2 move in toMoveAbsolute)
        {
            currentMove = currentPos + move;
            if (Tiles.ContainsKey(currentMove) && Tiles[currentMove].Type == type.empty)
            {
                if (Tiles[currentMove].State == state.healthy)
                {
                    possibleMoves.Add(currentMove);
                }
                else if (preys.Contains(BoardManager.instance.Tiles[currentMove].State))
                {
                    preferedMoves.Add(currentMove);
                }
            }


            //Debug.Log("eagle; current turn: " + GrowthManager.instance.currentTurn + "; lastCall: " + lastCall + "; modulo 4: " + (GrowthManager.instance.currentTurn - lastCall) % 4);

            if ((GrowthManager.instance.currentTurn - lastCall) % 4 == 0)
            {
                foreach (Vector2 position in BoardManager.instance.Tiles.Keys)
                {
                    if (BoardManager.instance.Tiles[position].Type == type.empty)
                    {
                        if (BoardManager.instance.Tiles[position].State == state.healthy)
                        {
                            possibleMoves.Add(position);
                        }
                        else if (preys.Contains(BoardManager.instance.Tiles[position].State))
                        {
                            preferedMoves.Add(position);
                        }
                    }
                }
            }
            else { possibleMoves.Add(currentPos); }
        }
        //Debug.Log("6) possible moves: " + possibleMoves.Count + "; prefered moves: " + preferedMoves.Count + "; can Move? " + canMove.ToString());

        if (possibleMoves.Count == 0 && preferedMoves.Count == 0)
        {
            canMove = false;
        }
    }


    public void specialMove()
    {
        if (State == state.healthy)
        {
            //Debug.Log("healthy is a victim that can't spread on its own");
            return;
        }

        switch (State)
        {
            case state.cat:
                move();
                updateMoveStats(currentPos);
                move();
                break;
            case state.eagle:
                NeighbourhoodTiles.Clear();
                for (int i = -2; i <= 2; i++)
                {
                    for (int j = -2; j <= 2; j++)
                    {
                        NeighbourhoodTiles.Add(new Vector2(i, j));
                    }
                }

                move();

                NeighbourhoodTiles.Clear();
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        NeighbourhoodTiles.Add(new Vector2(i, j));
                    }
                }
                break;
            case state.plant:
                List<Vector2> toColonizeTile = new List<Vector2>();
                List<Vector2> colonizedTile = new List<Vector2>();
                Vector2 currentTile = currentPos;
                Vector2 specialPlant;
                do
                {
                    specialPlant = Vector2.zero;
                    foreach (Vector2 tile in NeighbourhoodTiles)
                    {
                        if (BoardManager.instance.Tiles.ContainsKey(currentTile + tile) && BoardManager.instance.Tiles[currentTile + tile].Type == specialType && !colonizedTile.Contains(currentTile + tile))
                        {
                            specialPlant = tile;
                        }
                    }
                    if (specialPlant != Vector2.zero)
                    {
                        currentTile += specialPlant;
                        colonizedTile.Add(currentTile);

                        foreach (Vector2 tile2 in NeighbourhoodTiles)
                        {
                            if (BoardManager.instance.Tiles.ContainsKey(currentTile + tile2) && BoardManager.instance.Tiles[currentTile + tile2].Type == type.empty && BoardManager.instance.Tiles[currentTile + tile2].State == state.healthy && !toColonizeTile.Contains(currentTile + tile2))
                            {
                                toColonizeTile.Add(currentTile + tile2);
                            }
                        }
                    }
                }
                while (specialPlant != Vector2.zero);

                foreach (Vector2 tile in toColonizeTile)
                {
                    BoardManager.instance.Tiles[tile].State = state.plant;
                }

                break;
        }
    }
}

public enum moveType
{
    cleanTiles,
    anywhere,
    list
}