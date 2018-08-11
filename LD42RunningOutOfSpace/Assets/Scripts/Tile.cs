using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{

    type _type;
    public type Type
    {
        get
        {
            return _type;
        }
        set
        {
            ground.material = BoardManager.instance.Terrains[_type].material;
            _type = value;
        }
    }
    public Vector2 pos;
    state _state;
    public state State
    {
        get { return _state; }
        set
        {
            foreach(MeshRenderer holder in occupantSpriteHolder)
            {
                holder.material = GrowthManager.instance.Occupants[_state].material;
            }
            _state = value;
        }
    }
    List<MeshRenderer> sides;
    MeshRenderer ground;
    List<MeshRenderer> occupantSpriteHolder;
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