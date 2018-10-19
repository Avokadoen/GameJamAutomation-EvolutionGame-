using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Pace         {stop = 0, walking = 1, jogging = 3, running = 5, sprinting = 10}
public enum Mental_state {awake = -1, sleeping = 4}


[RequireComponent(typeof(PhysicalObject), typeof(Rigidbody), typeof(AudioSource))]
public class Agent : MonoBehaviour {

    public const float FOOD_TO_STAMINA_RATIO = 100;
    public const float FED_ATTRITION = 0.015f;
    public const float HUNGER_HEALTH_EFFECT = 0.001f;
    public const float ARRIVED_MOVE_TOW = 0.01f;
    public const int DAY_DURATION = 60;    // how many seconds in one day
    public const int MAX_SOUND_DISTANCE = 100;

    Rigidbody rb;
    AudioSource audioSource;

    PhysicalObject physicalObject;
    PhysicalObject heldObject;

    public float fed;
    public float metabolism;
    public float wakeFullness;
    //public float tempreture;
    public float maxMoveSpeed;
    public float maxStamina;
    public float stamina;
    public float perception;
    public float maxDurability;
    public Pace currentPace;
    public Mental_state currentMentalState;

    // TODO: move to function stack
    GameObject moveTowardsTargetTest;
    private int moveTowardsStatus;


    private float getPace() { return (float)currentPace / 10f; }

    // Use this for initialization
    void Start () {
        physicalObject = GetComponent<PhysicalObject>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        // TODO: Agent testing values: remove
        fed             = 1f;
        metabolism      = 0.1f;
        wakeFullness    = 1f;
        maxStamina      = 2f;
        stamina         = 1f;
        maxMoveSpeed    = 5f;
        //tempreture      = 38f;
        perception      = 10f;
        moveTowardsStatus = 404;
        currentMentalState = Mental_state.awake;
        maxDurability = physicalObject.durability;
    }

    // Update is called once per frame
    void FixedUpdate () {
        UpdateStates();
		// assertState()
        // takeAction()
	}


    // Actions
    public int TryToPickUp(PhysicalObject target)
    {
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, transform.forward, out hit, 1f))
            return 404;

        if (hit.transform.gameObject == target.gameObject)
            return 401;

        heldObject = target;
        heldObject.OnPickUp(transform.position + transform.forward);
        return 200;
    }

    private int MoveTowards(Vector3 moveTowardsTarget)
    {
        Vector3 lookAt = moveTowardsTarget;
        lookAt.y = transform.position.y;
        transform.LookAt(lookAt);
        if (Physics.Raycast(transform.position, transform.forward, 1f)) { return 202; } // perception * (1 - sleepyness))

        else if (Mathf.Abs(moveTowardsTarget.x - transform.position.x) < ARRIVED_MOVE_TOW &&
                Mathf.Abs(moveTowardsTarget.z - transform.position.z) < ARRIVED_MOVE_TOW)
            return 200;

        float staminaToBeUsed = getPace() * Time.deltaTime;
        if (IsMovingFast() && stamina < staminaToBeUsed)
        {
            currentPace = Pace.jogging;
        }


        rb.velocity = maxMoveSpeed * getPace() * transform.forward;
        
        if(IsMovingFast())
        {
            stamina -= staminaToBeUsed;
        }

        return 404;
    }

    private void UpdateStates()
    {
        // FED UPDATE
        if (fed > 0)
        {
            fed -= metabolism * FED_ATTRITION * Time.fixedDeltaTime;
        }

        // STAMINA UPDATE
        if (stamina < maxStamina && fed > 0 && !IsMovingFast())
        {
            float foodToEnergy = metabolism * (1 - getPace()) * Time.fixedDeltaTime;

            if (foodToEnergy / FOOD_TO_STAMINA_RATIO > fed)
            {
                foodToEnergy = fed * FOOD_TO_STAMINA_RATIO;
                fed = 0;
            }
            else fed -= foodToEnergy / FOOD_TO_STAMINA_RATIO;
            stamina += foodToEnergy;
        }
        // HEALTH UPDATE
        // damage
        if (fed <= 0)
        {
            physicalObject.durability -= maxDurability * HUNGER_HEALTH_EFFECT * Time.fixedDeltaTime;
        }
        // healing
        if (fed > 0)
        {
            physicalObject.durability += maxDurability * HUNGER_HEALTH_EFFECT * Time.fixedDeltaTime;

            if (physicalObject.durability > maxDurability)
                physicalObject.durability = maxDurability;
        }

        wakeFullness += ((float)currentMentalState / DAY_DURATION) * Time.fixedDeltaTime;
        /* move to brain code
         * if (wakeFullness <= 0 && currentMentalState != mental_state.sleeping)
        {
            currentMentalState = mental_state.sleeping;
        }
        else if (wakeFullness >= 1 && currentMentalState != mental_state.sleeping ||  // Takes damage*/

        // AUDIO UPDATE
        audioSource.volume = getPace();
        audioSource.maxDistance = getPace() * MAX_SOUND_DISTANCE;
        audioSource.pitch = 0.9f + getPace() * 0.5f;
    }

    // Meta functions

    public bool LineOfSight(PhysicalObject target)
    {
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, transform.forward, out hit, perception * wakeFullness))
            return false;

        if (hit.transform.gameObject == target.gameObject)
            return true;

        return false;
    }

    private bool IsMovingFast()
    {
        return (currentPace == Pace.sprinting || currentPace == Pace.running);
    }
}
