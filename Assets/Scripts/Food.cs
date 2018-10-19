using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum food_type { plant = 1, meat = 3 } // rotten

[RequireComponent(typeof(PhysicalObject))]
public class Food : MonoBehaviour {

    public PhysicalObject physicalObject;
    public food_type thisFoodType;

    // Use this for initialization
    void Start () {
        physicalObject = GetComponent<PhysicalObject>();
    }
	
    float onEaten()
    {
        return (float)thisFoodType * transform.localScale.magnitude;
    }
}
