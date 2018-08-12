﻿using System.Collections;
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
            GrowthManager.instance.Occupants[_state].listTiles.Remove(pos);
            _state = value;
            foreach(MeshRenderer holder in occupantSpriteHolder)
            {
                holder.material = GrowthManager.instance.Occupants[_state].images[Random.Range(0, GrowthManager.instance.Occupants[_state].images.Count)];
            }
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
    List<MeshRenderer> occupantSpriteHolder;


    private void Start()
    {
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