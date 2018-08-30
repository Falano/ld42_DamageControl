using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField]
    terrainTypeEnum _type;
    public terrainTypeEnum Type
    {
        get
        {
            return _type;
        }
        set
        {
            if (_type != terrainTypeEnum.healthy && value == terrainTypeEnum.healthy && !BoardManager.instance.settingTheBoard)
            {
                BoardManager.instance.SaneTiles++;
            }
            else if (_type == terrainTypeEnum.healthy && value != terrainTypeEnum.healthy && !BoardManager.instance.settingTheBoard)
            {
                BoardManager.instance.SaneTiles--;
            }
            _type = value;
            mesh.mesh = BoardManager.instance.Terrains[_type].mesh[Random.Range(0, BoardManager.instance.Terrains[_type].mesh.Count)];

        }
    }
    public Vector2 pos;
    [SerializeField]
    occupantEnum _state;
    public occupantEnum State
    {
        get { return _state; }
        set
        {
            GrowthManager.instance.Occupants[_state].listTiles.Remove(pos);
            _state = value;
            occupantSpriteHolder.mesh = GrowthManager.instance.Occupants[_state].meshes[Random.Range(0, GrowthManager.instance.Occupants[_state].meshes.Count)];
            GrowthManager.instance.Occupants[_state].listTiles.Add(pos);
            occ.lastMove = GrowthManager.instance.currentTurn;
            occ.BypassSpecial = false;
            occ.lastMove = GrowthManager.instance.currentTurn + Random.Range(0, occ.manager.moveCooldown);

            // we reset the personal cooldown so it can match the new state
            occ._personalCooldown = -1;

            occ.lastTurnActive = GrowthManager.instance.currentTurn;

            if (!BoardManager.instance.settingTheBoard && Type != terrainTypeEnum.damaged && State != occupantEnum.empty)
            {
                Type = terrainTypeEnum.damaged;
            }
        }
    }
    [SerializeField]
    List<MeshRenderer> sides;
    [SerializeField]
    MeshRenderer ground;
    [SerializeField]
    MeshFilter mesh;
    [SerializeField]
    MeshFilter occupantSpriteHolder;
    public OccupantInstance occ;

    private void Start()
    {
        occ = new OccupantInstance(this);
        occupantSpriteHolder.transform.Rotate(0, Random.Range(0, 360), 0);
        transform.Rotate(0, 90*Random.Range(0, 4), 0);
        ground.material = BoardManager.instance.Materials[Random.Range(0, BoardManager.instance.Materials.Count)];

    }

}

public enum terrainTypeEnum
{
    healthy,
    mountain,
    water,
    field,
    damaged
}

public enum occupantEnum
{
    empty,
    plant,
    rabbit,
    cat,
    fox,
    eagle,
    hunter,
    ranger
}