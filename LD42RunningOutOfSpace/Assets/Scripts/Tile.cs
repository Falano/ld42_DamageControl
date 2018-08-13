using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField]
    type _type;
    public type Type
    {
        get
        {
            return _type;
        }
        set
        {
            _type = value;
            mesh.mesh = BoardManager.instance.Terrains[_type].mesh;
            if (value == type.empty && !BoardManager.instance.emptyTiles.Contains(pos))
            {
                GrowthManager.instance.Occupants[state.healthy].listTiles.Add(pos);
            }
            else if(value != type.empty && BoardManager.instance.emptyTiles.Contains(pos))
            {
                GrowthManager.instance.Occupants[state.healthy].listTiles.Remove(pos);

            }
        }
    }
    public Vector2 pos;
    state _state;
    public state State
    {
        get { return _state; }
        set
        {
            GrowthManager.instance.Occupants[_state].listTiles.Remove(pos);
            _state = value;
            occupantSpriteHolder.mesh = GrowthManager.instance.Occupants[_state].meshes[Random.Range(0, GrowthManager.instance.Occupants[_state].meshes.Count)];
            GrowthManager.instance.Occupants[_state].listTiles.Add(pos);
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

    private void Start()
    {
            occupantSpriteHolder.transform.Rotate(0, Random.Range(0, 360), 0);
            ground.material = BoardManager.instance.Materials[Random.Range(0, BoardManager.instance.Materials.Count)];
    }

}

public enum type
{
    empty,
    mountain,
    water,
    field
}

public enum state
{
    healthy,
    plant,
    rabbit,
    cat,
    fox,
    eagle,
    hunter,
    ranger
}