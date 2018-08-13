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
                    occupant.updateMoveStats(occupant.listTiles[i]);
                    if (occupant.haveNeighbourhoodTilesASpecialType)
                    {
                        occupant.specialMove();
                    }
                    else
                    {
                        for (int j = 0; j < occupant.movesNumber; j++)
                        {
                            occupant.move();
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

        if (!choseToKeepPlaying && Occupants[state.healthy].listTiles.Count < 5 || eagle.listTiles.Count > BoardManager.instance.length)
        {
            UIManager.instance.ToggleEndGame(true);
            choseToKeepPlaying = true;
        }

        currentTurn++;
        ambient.volume = ((float)Occupants[state.healthy].listTiles.Count / BoardManager.instance.Tiles.Count);

    }

    void EndGame()
    {

    }

    public void CreateOccupant(string occupant)
    {
        //StopAllCoroutines();
        //Debug.Log("1) creating occupant " + occupant);
        state Occupant = (state)System.Enum.Parse(typeof(state), occupant);
        StartCoroutine(CreateOccupantCoroutine(Occupant));
    }

    IEnumerator CreateOccupantCoroutine(state Occupant)
    {
        for (int i = 0; i < 6; i++)
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
                CreateAtPos(Occupant, BoardManager.instance.emptyTiles[Random.Range(0, BoardManager.instance.emptyTiles.Count)]);
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

    public state State;
    public List<Mesh> meshes;
    //public List<state> predators;
    public List<state> preys;
    public int firstApparition;
    public int cooldown;
    public int movesNumber;
    public type specialType;
    public Button button;
    public Sprite introImage;
    public AudioClip sound;

    //[HideInInspector]
    public List<Vector2> listTiles = new List<Vector2>();
    [HideInInspector]
    public int lastCall;
    //[HideInInspector]
    //bool? _isAvailable;
    public bool isAvailable
    {
        get
        {
            /*if (_isAvailable == null)
            {
                _isAvailable = false;*/
            if (GrowthManager.instance.currentTurn < firstApparition || GrowthManager.instance.createdSomethingThisTurn)
            {
                return false;
            }
            switch (State)
            {
                case state.hunter:
                    if (GrowthManager.instance.Occupants[state.eagle].listTiles.Count > 1)
                    {
                        //    _isAvailable = (GrowthManager.instance.currentTurn - lastCall >= Mathf.FloorToInt(10 / GrowthManager.instance.Occupants[state.eagle].listTiles.Count));
                        return (GrowthManager.instance.currentTurn - this.lastCall >= Mathf.FloorToInt(10 / GrowthManager.instance.Occupants[state.eagle].listTiles.Count));
                    }
                    else if (GrowthManager.instance.Occupants[state.eagle].listTiles.Count == 1)
                    {
                        //  _isAvailable = (GrowthManager.instance.currentTurn - lastCall >= 10);
                        return (GrowthManager.instance.currentTurn - this.lastCall >= 10);
                    }
                    break;
                case state.ranger:
                    //_isAvailable = (GrowthManager.instance.Occupants[state.hunter].listTiles.Count > 0 && listTiles.Count == 0);
                    return (GrowthManager.instance.Occupants[state.hunter].listTiles.Count > 0 && listTiles.Count == 0);
                    break;
                default:
                    //_isAvailable = (GrowthManager.instance.currentTurn - lastCall >= cooldown);
                    return (GrowthManager.instance.currentTurn - this.lastCall >= cooldown);
                    break;
            }
            return false;
        }/*
            return _isAvailable;
        }
        set
        {
            _isAvailable = value;
        }*/
    }


    Vector2 currentPos;
    Vector2 currentMove;
    List<Vector2> possibleMoves = new List<Vector2>();
    List<Vector2> preferedMoves = new List<Vector2>();
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
    [HideInInspector]
    public bool haveNeighbourhoodTilesASpecialType;
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
    }

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
                BoardManager.instance.Tiles[currentPos].State = state.healthy;
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

            case state.eagle: // pulizia
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
                        if (BoardManager.instance.Tiles.ContainsKey(currentPos + tile) && tile != Vector2.zero)
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

        if (State != state.eagle)
        {
            foreach (Vector2 move in MovementRange)
            {
                currentMove = currentPos + move;
                if (BoardManager.instance.Tiles.ContainsKey(currentMove) && BoardManager.instance.Tiles[currentMove].Type == type.empty)
                {
                    if (BoardManager.instance.Tiles[currentMove].State == state.healthy)
                    {
                        //Debug.Log("5.5aa) Adding tile " + currentMove + " to possible Moves");
                        possibleMoves.Add(currentMove);
                    }
                    else if (preys.Contains(BoardManager.instance.Tiles[currentMove].State))
                    {
                        //Debug.Log("5.5ab) Adding tile " + currentMove + " to prefered Moves");
                        preferedMoves.Add(currentMove);
                    }
                }
            }
        }
        else // eagle can go anywhere // but only each 3 turns
        {
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