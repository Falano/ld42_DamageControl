using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

	public type Type;
	public Vector2 pos;
	public state State;
    
}

public enum type {
    empty,
	mountain,
    water,
    field
}

public enum state {
	healthy,
    plant,
    rabbit,
    cat,
    fox,
    eagle,
    hunter,
    ranger
}