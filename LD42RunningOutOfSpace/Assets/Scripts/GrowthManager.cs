using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    public Dictionary<occupantEnum, OccupantManager> Occupants = new Dictionary<occupantEnum, OccupantManager>();



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

        Occupants.Add(occupantEnum.empty, healthy);
        Occupants.Add(occupantEnum.plant, plant);
        Occupants.Add(occupantEnum.rabbit, rabbit);
        Occupants.Add(occupantEnum.cat, cat);
        Occupants.Add(occupantEnum.fox, fox);
        Occupants.Add(occupantEnum.eagle, eagle);
        Occupants.Add(occupantEnum.hunter, hunter);
        Occupants.Add(occupantEnum.ranger, ranger);

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
            //foreach (Vector2 tile in occupant.listTiles)
            if (occupant.listTiles.Count != 0)
            {
                int max = occupant.listTiles.Count;
                for (int i = 0; i < max && i < occupant.listTiles.Count; i++)
                {
                    //Debug.Log("moving " + tile);
                    BoardManager.instance.Tiles[occupant.listTiles[i]].occ.move();
                }
            }
            // we reset its availibility
            if (occupant.button)
            {
                //occupant.button.interactable = occupant.isAvailable;
                occupant.button.gameObject.SetActive(occupant.isAvailable);
            }

            if (occupant.isAvailable && !occupant.hasShownTuto && occupant.introImage)
            {
                UIManager.instance.ToggleNewAnimal(occupant.introImage);
                occupant.hasShownTuto = true;
                UIManager.instance.ChangeTutoImage(occupant.menuTutoImage);
            }

            //Debug.Log(occupant.State.ToString() +  " is available? " + occupant.isAvailable + "; last call: " + occupant.lastCall + "; currentTurn: " + currentTurn)

            if (!choseToKeepPlaying && ((occupant.canOverrun && occupant.listTiles.Count >= BoardManager.instance.emptyTilesAtStart * occupant.overrunPercentage) || ((float)BoardManager.instance.SaneTiles / (float)BoardManager.instance.emptyTilesAtStart < BoardManager.instance.saneTilesPercentageToLose) || BoardManager.instance.SaneTiles < BoardManager.instance.minNumberOfSaneTiles))
            {
                UIManager.instance.ToggleEndGame(true);
                choseToKeepPlaying = true;
            }
        }

        if (!choseToKeepPlaying && (Occupants[occupantEnum.empty].listTiles.Count < 5))
        {
            UIManager.instance.ToggleEndGame(true);
            choseToKeepPlaying = true;
        }
        else if (!choseToKeepPlaying && currentTurn == 75)
        {
            UIManager.instance.EndGame100Screen.SetActive(true);
            choseToKeepPlaying = true;
        }
        else if (Occupants[occupantEnum.rabbit].listTiles.Count == 0 &&
            Occupants[occupantEnum.cat].listTiles.Count == 0 &&
            Occupants[occupantEnum.fox].listTiles.Count == 0 &&
            Occupants[occupantEnum.eagle].listTiles.Count == 0 &&
            Occupants[occupantEnum.hunter].listTiles.Count == 0 &&
            Occupants[occupantEnum.plant].listTiles.Count == 0
            )
        {
            UIManager.instance.EndGame999Screen.SetActive(true);
        }

        currentTurn++;
        UIManager.instance.DaysText.text = "Day " + currentTurn;
        UIManager.instance.SaneText.text = "Healthy acres of land: " + BoardManager.instance.SaneTiles;
        float currentVolume = (((float)Occupants[occupantEnum.empty].listTiles.Count - BoardManager.instance.minNumberOfSaneTiles * 2) / (BoardManager.instance.Tiles.Count));
        ambientHealthy.volume = currentVolume;
        ambientInvasive.volume = 1 - currentVolume;

    }

    public void CreateOccupant(string occupant)
    {
        UIManager.instance.ActiveAnimalImage.gameObject.SetActive(false);
        StopAllCoroutines();
        //Debug.Log("1) creating occupant " + occupant);
        occupantEnum Occupant = (occupantEnum)System.Enum.Parse(typeof(occupantEnum), occupant);
        if (Occupants[Occupant].isAvailable)
        {
            StartCoroutine(CreatingOccupant(Occupant));
        }
    }

    /*
    IEnumerator CreateOccupantCoroutine(occupantEnum Occupant)
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
        // create the occupant on the tile clicked
        yield return StartCoroutine(CreatingOccupant(Occupant));
        //Occupants[Occupant].isAvailable = null;
    }*/

    IEnumerator CreatingOccupant(occupantEnum Occupant)
    {
        RaycastHit hit;
        UIManager.instance.ActiveAnimalImage.gameObject.SetActive(true);
        UIManager.instance.ActiveAnimalImage.sprite = Occupants[Occupant].mouseImageOnButtonClicked;

        yield return null;
        yield return null;

        bool x = true;
        while (x == true)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                if ((BoardManager.instance.Tiles[hit.transform.position].Type == terrainTypeEnum.healthy || BoardManager.instance.Tiles[hit.transform.position].Type == terrainTypeEnum.damaged) && (BoardManager.instance.Tiles[hit.transform.position].State == occupantEnum.empty || Occupants[Occupant].preys.Contains(BoardManager.instance.Tiles[hit.transform.position].State)))
                {
                    UIManager.instance.ActiveAnimalImage.color = UIManager.instance.freeTileColor;
                    if (Input.GetMouseButtonDown(0) && (!EventSystem.current.IsPointerOverGameObject(-1) && !EventSystem.current.IsPointerOverGameObject(0) && !EventSystem.current.IsPointerOverGameObject(1)))
                    {
                        CreateAtPos(Occupants[Occupant].State, hit.transform.position);
                        // attenzione che ActiveAnimalImage potrebbe rimanere cosi se la coroutine è stopped dall'esterno
                        UIManager.instance.ActiveAnimalImage.gameObject.SetActive(false);
                        x = false;
                    }
                }
                else
                {
                    UIManager.instance.ActiveAnimalImage.color = UIManager.instance.unusableTileColor;
                    if (Input.GetMouseButtonDown(0))
                    {
                        UIManager.instance.ActiveAnimalImage.gameObject.SetActive(false);
                        x = false;
                    }
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    UIManager.instance.ActiveAnimalImage.gameObject.SetActive(false);
                    x = false;
                }
            }
            yield return null;
        }

        /*
        Vector2 pos = new Vector2(0, 0);

        UIManager.instance.ActiveAnimalImage.gameObject.SetActive(true);
        UIManager.instance.ActiveAnimalImage.sprite = Occupants[Occupant].mouseImageOnButtonClicked;

        // wait until someone clicked a tile empty in a healthy or prey state
        yield return new WaitUntil(delegate
        {
            RaycastHit hit;
            if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                if ((BoardManager.instance.Tiles[hit.transform.position].Type == terrainTypeEnum.healthy || BoardManager.instance.Tiles[hit.transform.position].Type == terrainTypeEnum.damaged) && (BoardManager.instance.Tiles[hit.transform.position].State == occupantEnum.empty || Occupants[Occupant].preys.Contains(BoardManager.instance.Tiles[hit.transform.position].State)))
                {
                    pos = hit.transform.position;
                    return true;
                }
            }
            return false;
        });

        //Debug.Log("3.5b) tile clicked!");
        CreateAtPos(Occupant, pos);*/
    }

    public void CreateAtRandomPosition(string occupant)
    {
        UIManager.instance.ActiveAnimalImage.gameObject.SetActive(false);
        int tmpPos;
        do
        {
            tmpPos = Random.Range(0, Occupants[occupantEnum.empty].listTiles.Count);
        }
        while (BoardManager.instance.Tiles[Occupants[occupantEnum.empty].listTiles[tmpPos]].Type != terrainTypeEnum.healthy && BoardManager.instance.Tiles[Occupants[occupantEnum.empty].listTiles[tmpPos]].Type != terrainTypeEnum.damaged);
        CreateAtPos((occupantEnum)System.Enum.Parse(typeof(occupantEnum), occupant), Occupants[occupantEnum.empty].listTiles[tmpPos]);
    }

    public void CreateAtPos(occupantEnum occupant, Vector2 pos)
    {
        //Debug.Log("4) actually creating occupant " + occupant.ToString() + " at pos " + pos.ToString());

        BoardManager.instance.Tiles[pos].State = occupant;

        // and we keep the player from adding more
        createdSomethingThisTurn = true;
        foreach (OccupantManager occ in Occupants.Values)
        {
            if (occ.button)
            {
                occ.button.gameObject.SetActive(occ.isAvailable);
            }
        }
        Occupants[occupant].lastCall = currentTurn;
        //Debug.Log(Occupants[occupant].State.ToString() + "'s last call (in CreateAtPos): " + Occupants[occupant].lastCall);
        sfx.clip = Occupants[occupant].sound;
        sfx.Play();
        if (currentTurn != 0)
        {
            EndOfTurn();
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            CreateOccupant("rabbit");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            CreateOccupant("cat");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            CreateOccupant("fox");
        }

        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            CreateOccupant("eagle");
        }

        else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            CreateOccupant("hunter");
        }

        else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
        {
            CreateOccupant("ranger");
        }

        else if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            EndOfTurn();
        }


        // hide new animal screen on space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UIManager.instance.NewAnimalScreen.transform.parent.gameObject.SetActive(false);

            if (UIManager.instance.IntroScreen.transform.parent.gameObject.activeSelf)
            {
                UIManager.instance.UpdateIntroImages();
            }
        }

    }
}

[System.Serializable]
public class OccupantInstance
{
    public static Dictionary<occupantEnum, OccupantManager> Occs { get { return GrowthManager.instance.Occupants; } }
    public static Dictionary<Vector2, Tile> Tiles { get { return BoardManager.instance.Tiles; } }

    public int _personalCooldown = -1;
    int PersonalCooldown
    {
        get
        {
            if (_personalCooldown == -1)
            {
                if (manager.moveCooldown == 0)
                {
                    _personalCooldown = 0;
                }
                else if (manager.moveCooldown <= 2)
                {
                    _personalCooldown = manager.moveCooldown + Random.Range(0, 1);
                }
                else
                {
                    _personalCooldown = manager.moveCooldown + Random.Range(-1, 1);
                }

            }
            return _personalCooldown;
        }
    }
    public int lastTurnActive;


    public occupantEnum State { get { return tile.State; } }
    public Vector2 pos;
    public Tile tile;
    public OccupantManager manager { get { return Occs[tile.State]; } }
    public int lastMove;
    public bool BypassSpecial = false; // otherwise a plant born close to water will never leave and grow

    List<Vector2> possibleMoves = new List<Vector2>();
    List<Vector2> preferedMoves = new List<Vector2>();
    List<Vector2> bestMoves = new List<Vector2>();
    List<Vector2> toCleanAbsolute = new List<Vector2>();
    List<Vector2> toCleanAbsoluteSave = new List<Vector2>();
    List<Vector2> toSpawnInAbsolute = new List<Vector2>();
    List<Vector2> _toMoveAbsoluteRaw = new List<Vector2>();
    List<Vector2> toMoveAbsoluteRaw
    {
        get
        {
            switch (manager.moveType)
            {
                case moveType.anywhere:

                    /*
                    if (manager.LastTurnCheckedAnywhereMoveAvailibility != GrowthManager.instance.currentTurn)
                    {
                        manager.relativeTilesAvailableForMovement.Clear();
                        for(int i = -4; i < 4; i++)
                        {
                            for (int j = -4; j < 4; j++)
                            {
                                manager.relativeTilesAvailableForMovement.Add(new Vector2(i,j));
                            }
                        }


                        _toMoveAbsoluteRaw.Clear();
                        foreach (Vector2 tile in manager.relativeTilesAvailableForMovement)
                        {
                            _toMoveAbsoluteRaw.Add(pos + tile);
                        }

                        manager.LastTurnCheckedAnywhereMoveAvailibility = GrowthManager.instance.currentTurn;
                    }*/



                    if (manager.LastTurnCheckedAnywhereMoveAvailibility != GrowthManager.instance.currentTurn && !ExemptFromCalculatingToSpawnIn)
                    {
                        _toMoveAbsoluteRaw.Clear();

                        foreach (occupantEnum prey in manager.preys)
                        {
                            foreach (Vector2 v in Occs[prey].listTiles)
                            {
                                _toMoveAbsoluteRaw.Add(v);
                            }
                        }
                        if (_toMoveAbsoluteRaw.Count < manager.NumberOfKids)
                        {
                            foreach (Vector2 v in Occs[occupantEnum.empty].listTiles)
                            {
                                _toMoveAbsoluteRaw.Add(v);
                            }
                        }
                        manager.LastTurnCheckedAnywhereMoveAvailibility = GrowthManager.instance.currentTurn;
                    }
                    break;
                case moveType.cleanTiles:
                    _toMoveAbsoluteRaw = toCleanAbsolute;
                    break;
                case moveType.list:

                    _toMoveAbsoluteRaw.Clear();
                    foreach (Vector2 tile in manager.relativeTilesAvailableForMovement)
                    {
                        _toMoveAbsoluteRaw.Add(pos + tile);
                    }
                    break;
            }
            return _toMoveAbsoluteRaw;
        }
    }

    public bool ExemptFromCalculatingToSpawnIn;
    public bool haveNeighbourhoodTilesASpecialType
    {
        get
        {
            if (manager.specialType != terrainTypeEnum.healthy)
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
            return toSpawnInAbsolute.Count != 0;
        }
    }

    public OccupantInstance(Tile _tile)
    {
        tile = _tile;
        pos = tile.pos;
    }




    public void move(bool bypassSpec = false)
    {
        // healthy doesn't move nor clean
        if (tile.State == occupantEnum.empty)
        {
            if (tile.Type == terrainTypeEnum.damaged)
            {
                // damaged heals
                //Debug.Log("damaged heals");
                tile.Type = terrainTypeEnum.healthy;
            }
            //Debug.Log("healthy is a victim that can't spread on its own");
            return;
        }


        // can special move?
        bypassSpec = bypassSpec || BypassSpecial;
        if (haveNeighbourhoodTilesASpecialType && !bypassSpec)
        {
            specialMove();
            return;
        }

        updateMoveStats();
        ExemptFromCalculatingToSpawnIn = false;
        // clean
        foreach (Vector2 tile in toCleanAbsolute)
        {
            if (Tiles.ContainsKey(tile) && Tiles[tile].State != occupantEnum.empty && manager.preys.Contains(Tiles[tile].State))
            {
                Tiles[tile].State = occupantEnum.empty;
            }
        }

        // check before move
        if (canMove)
        {
            // spawn kids (which is a Move really)
            foreach (Vector2 tile in toSpawnInAbsolute)
            {
                if (Tiles[tile].State == occupantEnum.empty || manager.preys.Contains(Tiles[tile].State))
                    Tiles[tile].State = State;
            }
        }

        // if you're too old you die
        if (manager.longevity != 0 && Random.Range(0, manager.longevity) == 0)
        {
            BoardManager.instance.Tiles[pos].State = occupantEnum.empty;
            return;
        }
    }



    public void updateMoveStats()
    {

        if (manager.moveCooldown != 0 && GrowthManager.instance.currentTurn % PersonalCooldown != 0)
        {
            return;
        }
        //
        // building toCleanAbsolute
        //



        // clear all
        toCleanAbsolute.Clear();

        //check which we can clean
        foreach (Vector2 tile in manager.relativeTilesAvailableToBeCleaned)
        {
            if (Tiles.ContainsKey(pos + tile) &&
                (manager.preys.Contains(Tiles[pos + tile].State) || Tiles[pos + tile].State == occupantEnum.empty))
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
        if (!ExemptFromCalculatingToSpawnIn)
        {
            //
            // building toSpawnInAbsolute
            //
            toSpawnInAbsolute.Clear();

            // we leave it void if we don't want to spawn anything
            if (manager.ChanceToSpawnKids == 0 /*|| Random.Range(0, 100) > manager.ChanceToSpawnKids */ || (manager.maxNumberOfInstances != 0 && manager.listTiles.Count >= manager.maxNumberOfInstances))
            {
                toSpawnInAbsolute.Clear();
                return;
            }

            // where can we or would we like to go?
            possibleMoves.Clear();
            preferedMoves.Clear();
            bestMoves.Clear();


            /*
            // per qualsiasi ragione, questo lo fa impazzire; tenuto qui per la scienza
            if (manager.State == occupantEnum.eagle)
            {
                Debug.Log("2) number of toMoveAbsoluteRaw: " + toMoveAbsoluteRaw.Count);
            }
            
            // the bug is caused by toMoveAbsoluteRaw around this place
            // at some random point in space
             
             */

            bool goOn = true;
            // we get all the moves available
            //foreach (Vector2 move in toMoveAbsoluteRaw)
            int x = toMoveAbsoluteRaw.Count;
            for (int i = 0; i < x && goOn; i++)
            {
                if (manager.State == occupantEnum.eagle && x > 30)
                {
                    //Debug.Log("0) i: " + i);
                    //Debug.Log("999) to clean absolute: " + toCleanAbsolute.Count);

                    // MUTANT EAGLES !!!!!
                    // toggle mutant eagles ending

                }
                if (Tiles.ContainsKey(toMoveAbsoluteRaw[i]) && (Tiles[toMoveAbsoluteRaw[i]].Type == terrainTypeEnum.healthy || Tiles[toMoveAbsoluteRaw[i]].Type == terrainTypeEnum.damaged))
                {
                    // if it's the first prey
                    if (manager.preys.Count != 0 && (Tiles[toMoveAbsoluteRaw[i]].State) == manager.preys[0])
                    {
                        bestMoves.Add(toMoveAbsoluteRaw[i]);
                    }
                    // if we already have enough fine prey don't bother with rabble
                    else if (bestMoves.Count <= manager.NumberOfKids)
                    {
                        // or any prey really
                        if (manager.preys.Contains(BoardManager.instance.Tiles[toMoveAbsoluteRaw[i]].State))
                        {
                            preferedMoves.Add(toMoveAbsoluteRaw[i]);
                        }
                        // but we can also spread on empty tiles if there's nothing better around
                        else if (Tiles[toMoveAbsoluteRaw[i]].State == occupantEnum.empty)
                        {
                            possibleMoves.Add(toMoveAbsoluteRaw[i]);
                        }
                    }
                }
            }

            if (manager.moveType == moveType.anywhere)
            {
                goOn = true;
                // we trim the excess (in a separate step so it's kinda random) and
                // we merge the lists into a new one
                while (toSpawnInAbsolute.Count < manager.NumberOfKids && goOn)
                {
                    if (bestMoves.Count != 0)
                    {
                        int random = Random.Range(0, bestMoves.Count);
                        toSpawnInAbsolute.Add(bestMoves[random]);
                        bestMoves.RemoveAt(random);
                    }
                    else if (preferedMoves.Count != 0)
                    {
                        int random = Random.Range(0, preferedMoves.Count);
                        toSpawnInAbsolute.Add(preferedMoves[random]);
                        preferedMoves.RemoveAt(random);
                    }
                    else if (possibleMoves.Count != 0)
                    {
                        int random = Random.Range(0, possibleMoves.Count);
                        toSpawnInAbsolute.Add(possibleMoves[random]);
                        possibleMoves.RemoveAt(random);
                    }
                    else
                    {
                        goOn = false;
                    }
                }

                foreach (Vector2 v in manager.listTiles)
                {
                    Tiles[v].occ.toSpawnInAbsolute = toSpawnInAbsolute;
                    Tiles[v].occ.ExemptFromCalculatingToSpawnIn = true;
                }

            }

            else
            {
                int numOfKidsSpecial = manager.NumberOfKids;
                if (manager.ChanceToSpawnKids == 0 || Random.Range(0, 100) > manager.ChanceToSpawnKids)
                {
                    numOfKidsSpecial++;
                }

                // we trim the excess (in a separate step so it's kinda random)
                while (possibleMoves.Count + preferedMoves.Count + bestMoves.Count > numOfKidsSpecial)
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
        }
    }


    public void specialMove()
    {
        if (State == occupantEnum.empty)
        {
            //Debug.Log("healthy is a victim that can't spread on its own");
            return;
        }

        switch (State)
        {
            case occupantEnum.cat:
                move(true);
                foreach (Vector2 v in manager.NeighbourhoodTiles)
                {
                    if (Tiles.ContainsKey(pos + v) && (Tiles[pos + v].Type == terrainTypeEnum.healthy || Tiles[pos + v].Type == terrainTypeEnum.damaged) && Tiles[pos + v].State == occupantEnum.empty)
                    {
                        Tiles[pos + v].State = occupantEnum.cat;
                    }
                }
                break;
            case occupantEnum.eagle:
                manager.relativeTilesAvailableToBeCleaned.Clear();
                for (int i = -2; i <= 2; i++)
                {
                    for (int j = -2; j <= 2; j++)
                    {
                        manager.relativeTilesAvailableToBeCleaned.Add(new Vector2(i, j));
                    }
                }

                move(true);

                manager.relativeTilesAvailableToBeCleaned.Clear();
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        manager.relativeTilesAvailableToBeCleaned.Add(new Vector2(i, j));
                    }
                }
                break;
            case occupantEnum.plant:
                List<Vector2> toColonizeTile = new List<Vector2>();
                List<Vector2> waterTile = new List<Vector2>();
                Vector2 specialPlant = Vector2.zero;
                Vector2 currentTile = pos;
                bool foundWater = true;
                do
                {
                    foundWater = false;
                    //foreach (Vector2 tile in manager.NeighbourhoodTiles)
                    for (int i = 0; i < manager.NeighbourhoodTiles.Count && foundWater == false; i++)
                    {
                        if (BoardManager.instance.Tiles.ContainsKey(currentTile + manager.NeighbourhoodTiles[i]) && BoardManager.instance.Tiles[currentTile + manager.NeighbourhoodTiles[i]].Type == manager.specialType && !waterTile.Contains(currentTile + manager.NeighbourhoodTiles[i]))
                        {
                            specialPlant = manager.NeighbourhoodTiles[i];
                            foundWater = true;
                        }
                    }
                    if (foundWater)
                    {
                        currentTile += specialPlant;
                        waterTile.Add(currentTile);

                        foreach (Vector2 tile2 in manager.NeighbourhoodTiles)
                        {
                            if (BoardManager.instance.Tiles.ContainsKey(currentTile + tile2) && (BoardManager.instance.Tiles[currentTile + tile2].Type == terrainTypeEnum.healthy || BoardManager.instance.Tiles[currentTile + tile2].Type == terrainTypeEnum.damaged) && BoardManager.instance.Tiles[currentTile + tile2].State == occupantEnum.empty && !toColonizeTile.Contains(currentTile + tile2))
                            {
                                toColonizeTile.Add(currentTile + tile2);
                            }
                        }
                    }
                }
                while (foundWater);

                foreach (Vector2 tile in toColonizeTile)
                {
                    BoardManager.instance.Tiles[tile].State = occupantEnum.plant;
                    BoardManager.instance.Tiles[tile].occ.BypassSpecial = true;
                }
                break;
        }
    }

}

[System.Serializable]
public class OccupantManager
{

    public occupantEnum State;
    public List<Mesh> meshes;
    //public List<state> predators;
    public List<occupantEnum> preys = new List<occupantEnum>();
    public terrainTypeEnum specialType;
    public Button button;
    public Sprite introImage;
    public Sprite menuTutoImage;
    public AudioClip sound;
    public Sprite mouseImageOnButtonClicked;

    int _lastTurnCheckedAnywhereMoveAvailablility = -1;
    public int LastTurnCheckedAnywhereMoveAvailibility { get { return _lastTurnCheckedAnywhereMoveAvailablility; } set { _lastTurnCheckedAnywhereMoveAvailablility = value; } }

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
                    _chanceToSpawnKids = 100;
                }
                else if (_numberOfKids == 0)
                {
                    _chanceToSpawnKids = 0;
                }
                else
                {
                    _chanceToSpawnKids = (int)((_numberOfKids % 1) * 100);
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
                return (int)_numberOfKids;
            }
        }
    } // eagles: 1; bushes: 8;
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
    public List<occupantEnum> necessaryPrecursors;
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


    int _lastCall = -1;
    public int lastCall { get { return _lastCall; } set { _lastCall = value; } }
    public bool isAvailable // is the button available (the ones already there grow even if the button is hidden)
    {
        get
        {
            if (BoardManager.instance.canAlwaysSpawn)
            {
                return true;
            }

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


    bool _hasShownTuto = false;
    public bool hasShownTuto { get { return _hasShownTuto; } set { _hasShownTuto = value; } }

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
                        if (!(i == 0 && j == 0))
                        {
                            _neighbourhoodTiles.Add(new Vector2(i, j));
                        }
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