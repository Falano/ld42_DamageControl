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
            ground.material = BoardManager.instance.Terrains[_type].material;
            mesh.mesh = BoardManager.instance.Terrains[_type].mesh;
            if (value == type.empty && !BoardManager.instance.emptyTiles.Contains(pos))
            {
                BoardManager.instance.emptyTiles.Add(pos);
            }
            else if(BoardManager.instance.emptyTiles.Contains(pos))
            {
                BoardManager.instance.emptyTiles.Remove(pos);
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
            _state = value;
            foreach(MeshRenderer holder in occupantSpriteHolder)
            {
                holder.material = GrowthManager.instance.Occupants[_state].images[Random.Range(0, GrowthManager.instance.Occupants[_state].images.Count)];
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