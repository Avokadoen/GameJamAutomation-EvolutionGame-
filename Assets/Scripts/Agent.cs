using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum pace {walking = 1, jogging = 3, running = 5, sprinting = 10}

[RequireComponent(typeof(PhysicalObject), typeof(Rigidbody))]
public class Agent : MonoBehaviour {

    Rigidbody rb;

    PhysicalObject physicalObject;
    PhysicalObject heldObject;

    public float hunger;
    public float sleepyness;
    public float tempreture;
    public float maxMoveSpeed;
    public float stamina;
    public float perception;
    public pace currentPace;

    // TODO: move to function stack
    public GameObject moveTowardsTarget;


    private float getPace() { return (float)currentPace / 10f; }

    // Use this for initialization
    void Start () {
        physicalObject = GetComponent<PhysicalObject>();
        rb = GetComponent<Rigidbody>();

        // TODO: Agent testing values: remove
        hunger          = 0f;
        sleepyness      = 0f;
        stamina         = 1f;
        maxMoveSpeed    = 5f;
        tempreture      = 38f;
        perception      = 10f;
    }

    private void Update()
    {
        moveTowards();
    }

    // Update is called once per frame
    void FixedUpdate () {
		// assertState()
        // takeAction()
	}

    // Actions

    private void moveTowards()
    {
        Vector3 lookAt = moveTowardsTarget.transform.position;
        lookAt.y = transform.position.y;
        transform.LookAt(lookAt);
        if (Physics.Raycast(transform.position, transform.forward, 1f)) { return; } // perception * (1 - sleepyness))

        else if (moveTowardsTarget.transform.position.x == transform.position.x && 
                moveTowardsTarget.transform.position.z == transform.position.z) return;

        float staminaToBeUsed = getPace() * Time.deltaTime;
        if (isMovingFast() && stamina < staminaToBeUsed)
        {
            currentPace = pace.jogging;
        }


        rb.velocity = maxMoveSpeed * getPace() * transform.forward;
        
        if(isMovingFast())
        {
            stamina -= staminaToBeUsed;
        }
        
    }

    private void updateStates()
    {

    }

    private bool isMovingFast()
    {
        return (currentPace == pace.sprinting || currentPace == pace.running);
    }
}
