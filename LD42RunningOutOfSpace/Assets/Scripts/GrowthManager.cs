using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowthManager : MonoBehaviour {

    public static GrowthManager instance;

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
    void Start () {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        Occupants.Add(healthy.State, healthy);
        Occupants.Add(healthy.State, healthy);
        Occupants.Add(plant.State, plant);
        Occupants.Add(rabbit.State, rabbit);
        Occupants.Add(cat.State, cat);
        Occupants.Add(fox.State, fox);
        Occupants.Add(eagle.State, eagle);
        Occupants.Add(hunter.State, hunter);
        Occupants.Add(ranger.State, ranger);


    }
}

[System.Serializable]
public class Occupant
{
    public state State;
    public List<Material> images;
    public List<state> predators;
    public List<state> preys;
    public int firstApparition;
    public int cooldown;

}