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
    public AudioSource ambientHealthy;
    public AudioSource ambientInvasive;
    public AudioSource sfx;

    [Header("occupants")]
    public OccupantManager healthy;
    public OccupantManager plant;
    public OccupantManager rabbit;
    public OccupantManager cat;
    public OccupantManager fox;
    public OccupantManager eagle;
    public OccupantManager hunter;
    public OccupantManager ranger;
    public Dictionary<state, OccupantManager> Occupants = new Dictionary<state, OccupantManager>();



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

        foreach (OccupantManager occ in Occupants.Values)
        {
            occ.preys.Sort();
            occ.preys.Reverse();
        }

        ambientHealthy.loop = true;
        ambientInvasive.loop = true;

    }

    public void EndOfTurn()
    {
        StartCoroutine(UIManager.instance.rotateTimeHolder());

        createdSomethingThisTurn = false;

        //Debug.Log("1) Starting end of turn " + currentTurn);
        // for each state
        foreach (OccupantManager occupant in Occupants.Values)
        {
            // for each of its members, we move
            foreach (Vector2 tile in occupant.listTiles)
            {
                BoardManager.instance.Tiles[tile].occ.move();
            }
            // we reset its availibility
            if (occupant.button)
            {
                occupant.button.interactable = occupant.isAvailable;
            }

            if (occupant.isAvailable && !occupant.hasShownTuto && occupant.introImage)
            {
                UIManager.instance.ToggleNewAnimal(occupant.introImage);
                occupant.hasShownTuto = true;
            }

            //Debug.Log(occupant.State.ToString() +  " is available? " + occupant.isAvailable + "; last call: " + occupant.lastCall + "; currentTurn: " + currentTurn)

            if (!choseToKeepPlaying && occupant.canOverrun && occupant.listTiles.Count >= BoardManager.instance.emptyTilesAtStart * occupant.overrunPercentage)
            {
                UIManager.instance.ToggleEndGame(true);
                choseToKeepPlaying = true;
            }
        }

        if (!choseToKeepPlaying && (Occupants[state.healthy].listTiles.Count < 5))
        {
            UIManager.instance.ToggleEndGame(true);
            choseToKeepPlaying = true;
        }

        currentTurn++;
        float currentVolume = ((float)Occupants[state.healthy].listTiles.Count / BoardManager.instance.Tiles.Count);
        ambientHealthy.volume = currentVolume;
        ambientInvasive.volume = 1 - currentVolume;

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
                    tmpPos = Random.Range(0, Occupants[state.healthy].listTiles.Count);
                }
                while (BoardManager.instance.Tiles[Occupants[state.healthy].listTiles[tmpPos]].Type != type.empty);

                CreateAtPos(Occupant, Occupants[state.healthy].listTiles[tmpPos]);
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
        foreach (OccupantManager occ in Occupants.Values)
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

public class OccupantInstance
{
    public static Dictionary<state, OccupantManager> Occs { get { return GrowthManager.instance.Occupants; } }
    public static Dictionary<Vector2, Tile> Tiles { get { return BoardManager.instance.Tiles; } }

    public state State { get { return tile.State; } }
    public Vector2 pos;
    public Tile tile;
    public OccupantManager manager { get { return Occs[tile.State]; } }
    public int lastMove;
    public bool bypassSpecial = false; // otherwise a plant born close to water will never leave and grow

    Vector2 currentMove;
    int currentNumberOfKids
    {
        get
        {
            return (bestMoves.Count + preferedMoves.Count + possibleMoves.Count);
        }
    }
    List<Vector2> possibleMoves = new List<Vector2>();
    List<Vector2> preferedMoves = new List<Vector2>();
    List<Vector2> bestMoves = new List<Vector2>();
    List<Vector2> toCleanAbsolute = new List<Vector2>();
    List<Vector2> toSpawnInAbsolute = new List<Vector2>();
    List<Vector2> _toMoveAbsoluteRaw = new List<Vector2>();
    List<Vector2> toMoveAbsoluteRaw
    {
        get
        {
            switch (manager.moveType)
            {
                case moveType.anywhere:
                    _toMoveAbsoluteRaw = Occs[state.healthy].listTiles;
                    break;
                case moveType.cleanTiles:
                    _toMoveAbsoluteRaw = toCleanAbsolute;
                    break;
                case moveType.list:
                    _toMoveAbsoluteRaw.Clear();
                    foreach (Vector2 tile in manager.relativeTilesAvailableForMovement)
                    {
                        if (Tiles[(pos + tile)].State == state.healthy || manager.preys.Contains(Tiles[(pos + tile)].State))
                        {
                            _toMoveAbsoluteRaw.Add(pos + tile);
                        }
                    }
                    break;
            }
            return _toMoveAbsoluteRaw;
        }
    }


    [HideInInspector]
    public bool haveNeighbourhoodTilesASpecialType
    {
        get
        {
            if (manager.specialType != type.empty)
            {
                foreach (Vector2 tile in manager.NeighbourhoodTiles)
                {
                    if (BoardManager.instance.Tiles.ContainsKey(tile + pos) && BoardManager.instance.Tiles[tile + pos].Type == manager.specialType)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
    bool canMove
    {
        get
        {
            return /*((GrowthManager.instance.currentTurn - lastMove != 0 && (GrowthManager.instance.currentTurn - lastMove) % manager.moveCooldown == 0)) || */!(possibleMoves.Count == 0 && preferedMoves.Count == 0 && bestMoves.Count == 0);
        }
    }


    public OccupantInstance(Tile _tile)
    {
        tile = _tile;
        pos = tile.pos;
        lastMove = GrowthManager.instance.currentTurn + Random.Range(0, manager.moveCooldown);
    }




    public void move(bool bypassSpecial = false)
    {
        // healthy doesn't move nor clean
        if (tile.State == state.healthy)
        {
            if (tile.Type == type.damaged)
            {
                tile.Type = type.empty;
            }
            //Debug.Log("healthy is a victim that can't spread on its own");
            return;
        }

        // if you're too old you die
        if (manager.longevity != 0 && Random.Range(0, manager.longevity) == 0)
        {
            BoardManager.instance.Tiles[pos].State = state.healthy;
            return;
        }

        // can special move?
        if (haveNeighbourhoodTilesASpecialType && !bypassSpecial)
        {
            specialMove();
            return;
        }

        updateMoveStats();


        // clean
        foreach (Vector2 tile in toCleanAbsolute)
        {
            Tiles[tile].State = state.healthy;
        }

        // check before move
        if (!canMove)
        {
            return;
        }

        // spawn kids (which is a Move really)
        foreach (Vector2 tile in toSpawnInAbsolute)
        {
            Tiles[tile].State = State;
        }

        // you moved and left the place you were at empty
        if (manager.shouldParentDie)
        {
            tile.State = state.healthy;
        }
    }



    public void updateMoveStats()
    {
        // clear all
        toCleanAbsolute.Clear();
        toSpawnInAbsolute.Clear();

        if (manager.moveCooldown != 0 && GrowthManager.instance.currentTurn % manager.moveCooldown != 0)
        {
            return;
        }

        //
        // building toCleanAbsolute
        //

        //check which we can clean
        foreach (Vector2 tile in manager.relativeTilesAvailableToBeCleaned)
        {
            if (manager.preys.Contains(Tiles[pos + tile].State))
            {
                toCleanAbsolute.Add(pos + tile);
            }
        }
        // decide which we will clean
        // we need to clean up to ToCleanTilesNumber tiles
        // so we trim ToCleanAbsolute as needed
        while (toCleanAbsolute.Count > manager.ToCleanTilesNumber)
        {
            toCleanAbsolute.RemoveAt(Random.Range(0, toCleanAbsolute.Count));
        }

        //
        // building toSpawnInAbsolute
        //

        // we leave it void if we don't want to spawn anything
        if (manager.ChanceToSpawnKids == 0 || Random.Range(0, 100) < manager.ChanceToSpawnKids || (manager.maxNumberOfInstances != 0 && manager.listTiles.Count >= manager.maxNumberOfInstances))
        {
            return;
        }

        // where can we or would we like to go?
        possibleMoves.Clear();
        preferedMoves.Clear();
        bestMoves.Clear();

        // we get all the moves available
        foreach (Vector2 move in toMoveAbsoluteRaw)
        {
            currentMove = pos + move;
            if (Tiles.ContainsKey(currentMove) && (Tiles[currentMove].Type == type.empty || Tiles[currentMove].Type == type.damaged))
            {
                // we like highesst-ranked prey best
                if ((Tiles[currentMove].State) == manager.preys[0])
                {
                    bestMoves.Add(currentMove);
                }
                else if (manager.preys.Contains(BoardManager.instance.Tiles[currentMove].State))
                {
                    preferedMoves.Add(currentMove);
                }
                // but we can also spread on empty tiles if there's nothing better around
                else if (Tiles[currentMove].State == state.healthy)
                {
                    possibleMoves.Add(currentMove);
                }
            }
        }

        // we trim the excess (ina separate step so it's kinda random)
        while (currentNumberOfKids < manager.NumberOfKids)
        {
            if (possibleMoves.Count > 0)
            {
                possibleMoves.RemoveAt(Random.Range(0, possibleMoves.Count));
            }
            else if (preferedMoves.Count > 0)
            {
                preferedMoves.RemoveAt(Random.Range(0, preferedMoves.Count));
            }
            else if (bestMoves.Count > 0)
            {
                bestMoves.RemoveAt(Random.Range(0, bestMoves.Count));
            }
        }

        // we merge the lists into a new one
        foreach (Vector2 v in possibleMoves)
        {
            toSpawnInAbsolute.Add(v);
        }
        foreach (Vector2 v in preferedMoves)
        {
            toSpawnInAbsolute.Add(v);
        }
        foreach (Vector2 v in bestMoves)
        {
            toSpawnInAbsolute.Add(v);
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
                move(true);
                move(true);
                break;
            case state.eagle:
                manager.relativeTilesAvailableToBeCleaned.Clear();
                for (int i = -2; i <= 2; i++)
                {
                    for (int j = -2; j <= 2; j++)
                    {
                        manager.relativeTilesAvailableToBeCleaned.Add(new Vector2(i, j));
                    }
                }

                move();

                manager.relativeTilesAvailableToBeCleaned.Clear();
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        manager.relativeTilesAvailableToBeCleaned.Add(new Vector2(i, j));
                    }
                }
                break;
            case state.plant:
                List<Vector2> toColonizeTile = new List<Vector2>();
                List<Vector2> colonizedTile = new List<Vector2>();
                Vector2 specialPlant;
                Vector2 currentTile = pos;
                do
                {
                    specialPlant = Vector2.zero;
                    foreach (Vector2 tile in manager.NeighbourhoodTiles)
                    {
                        if (BoardManager.instance.Tiles.ContainsKey(pos + tile) && BoardManager.instance.Tiles[pos + tile].Type == manager.specialType && !colonizedTile.Contains(pos + tile))
                        {
                            specialPlant = tile;
                        }
                    }
                    if (specialPlant != Vector2.zero)
                    {
                        currentTile += specialPlant;
                        colonizedTile.Add(currentTile);

                        foreach (Vector2 tile2 in manager.NeighbourhoodTiles)
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
                    BoardManager.instance.Tiles[tile].occ.bypassSpecial = true;
                }

                break;
        }
    }

}

[System.Serializable]
public class OccupantManager
{

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
    public int cooldown; // rabbit 1; cat 2; fox 4; eagle 8
    [Tooltip("cooldown between two movements of the animals of this type on the map")]
    public int moveCooldown; // eagle 4; others 0
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
    public List<Vector2> relativeTilesAvailableToBeCleaned;
    int _chanceToSpawnKids = -1;
    public int ChanceToSpawnKids
    {
        get
        {
            if (_chanceToSpawnKids == -1)
            {
                if (_numberOfKids % 1 == 0)
                {
                    _chanceToSpawnKids = 1;
                }
                else if (_numberOfKids == 0)
                {
                    _chanceToSpawnKids = 0;
                }
                else
                {
                    _chanceToSpawnKids =(int) ((_numberOfKids % 1) * 100);
                }
            }
            return _chanceToSpawnKids;
        }
    } // eagles: 10; hunters: 20; bushes, rabbits, cats, foxes: 1 (always); rangers: 0 (never)
    [Tooltip("if more than 1, spawns every turn; if less decimal, has a probability to spawn a kid (.2: a probability of 20% of one kid; 1.2: always spawns one, 20% chance to spawn another)")]
    [SerializeField]
    float _numberOfKids; // eagles: 1; bushes: 8;
    public int NumberOfKids
    {
        get
        {
            if (_numberOfKids < 1) { return 1; }
            else
            {
                return (int) _numberOfKids;
            }
        }
    } // eagles: 1; bushes: 8;
    public bool shouldParentDie { get { return longevity == 0; } } // bushes: no; eagles: yes
    public moveType moveType; // list, all clean (bushes), anywhere (eagles)
    public List<Vector2> relativeTilesAvailableForMovement;
    [Tooltip("if 0, never dies, if 1, dies at every turn; if 2, dies once out of 2, if 3, dies once out of 3, etc.; if longevity 1 e numberOfKids 1, si sposta ogni torno")]
    public int longevity;
    [Tooltip("can this animal end the game if there are too many of them")]
    public bool canOverrun;
    [Tooltip("from 0 to 1; on 1 you need this one animal to cover every empty tile to lose")]
    public float overrunPercentage;

    public int maxNumberOfInstances;
    [Tooltip("for necessary precursors[X], you need a number Number of necessary precursors[X] of them; the button will only show if there are enough necessary precursors")]
    public List<state> necessaryPrecursors;
    [Tooltip("for necessary precursors[X], you need a number Number of necessary precursors[X] of them; the button will only show if there are enough necessary precursors")]
    public List<int> numberOfNecessaryPrecursors;
    public bool hasEnoughPrecursors
    {
        get
        {
            if (necessaryPrecursors.Count == 0)
            {
                return true;
            }
            for (int i = 0; i < necessaryPrecursors.Count; i++)
            {
                if (GrowthManager.instance.Occupants[necessaryPrecursors[i]].listTiles.Count > numberOfNecessaryPrecursors[i])
                {
                    return true;
                }
            }
            return false;
        }
    }

    public List<Vector2> listTiles = new List<Vector2>();

    [HideInInspector]
    public int lastCall;
    public bool isAvailable // is the button available (the ones already there grow even if the button is hidden)
    {
        get
        {
            if (GrowthManager.instance.currentTurn < firstApparition || GrowthManager.instance.createdSomethingThisTurn)
            {
                return false;
            }

            // if it has one of the precursors it needs and won't be more numerous than allowed
            if ((hasEnoughPrecursors) && (maxNumberOfInstances == 0 || listTiles.Count < maxNumberOfInstances))
            {
                // it can be spawned when the cooldown is finished
                return (GrowthManager.instance.currentTurn - this.lastCall >= cooldown);
            }

            return false;
        }
    }

    [HideInInspector]
    public bool hasShownTuto = false;

    //    int tilesCleaned;
    List<Vector2> _neighbourhoodTiles = new List<Vector2>();
    public List<Vector2> NeighbourhoodTiles
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

    //    Tile currentTile;
}

public enum moveType
{
    cleanTiles,
    anywhere,
    list
}