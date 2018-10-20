using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Pace         {stop = 0, walking = 1, jogging = 3, running = 5, sprinting = 10}
public enum Mental_state {awake = -1, sleeping = 4}
public enum Eater_type { carnivore, omnivore, herbivore }

public struct AgentState
{

    public float fed;
    public float metabolism;
    public float wakeFullness;
    public float maxMoveSpeed;
    public float maxStamina;
    public float stamina;
    public float perception;
    public float maxDurability;
    public Pace currentPace;
    public Mental_state currentMentalState;
    public Eater_type eaterType;

    public bool IsMovingFast()
    {
        return (currentPace == Pace.sprinting || currentPace == Pace.running);
    }
    public float getPace() { return (float)currentPace / 10f; }
}

[RequireComponent(typeof(PhysicalObject), typeof(Rigidbody), typeof(AudioSource))]
public class Agent : MonoBehaviour {

    public const float FOOD_TO_STAMINA_RATIO = 100;
    public const float FED_ATTRITION = 0.015f;
    public const float HUNGER_HEALTH_EFFECT = 0.001f;
    public const float ARRIVED_MOVE_TOW = 0.01f;
    public const int DAY_DURATION = 10;    // how many seconds in one day
    public const int MAX_SOUND_DISTANCE = 100;

    public LayerMask plantsMask;
    public LayerMask meatMask;


    public AgentState state;
    public Rigidbody rb;
    public AudioSource audioSource;

    public PhysicalObject physicalObject;
    public PhysicalObject heldObject;

    // TODO: move to function stack
    public GameObject target;
    private int moveTowardsStatus;

    // Use this for initialization
    void Start () {
        physicalObject = GetComponent<PhysicalObject>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        // TODO: Agent testing values: remove
        state.fed = 1f;
        state.metabolism      = 0.8f;
        state.wakeFullness    = 1f;
        state.maxStamina      = 2f;
        state.stamina         = 1f;
        state.maxMoveSpeed    = 5f;
        //tempreture      = 38f;
        state.perception      = 10f;
        state.currentMentalState = Mental_state.awake;
        state.eaterType = Eater_type.omnivore;
        state.maxDurability = physicalObject.durability;
        state.currentPace = Pace.running;
        moveTowardsStatus = 404;
    }

    // Update is called once per frame
    void FixedUpdate () {
        UpdateStates();
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

    public int MoveTowardsTarget()
    {
        return MoveTowards(target.transform.position);
    }

    public int MoveTowards(Vector3 moveTowardsTarget)
    {
        Vector3 lookAt = moveTowardsTarget;
        lookAt.y = transform.position.y;
        transform.LookAt(lookAt);
        if (Physics.Raycast(transform.position, transform.forward, 1f)) { return 202; } // perception * (1 - sleepyness))

        else if (Mathf.Abs(moveTowardsTarget.x - transform.position.x) < ARRIVED_MOVE_TOW &&
                Mathf.Abs(moveTowardsTarget.z - transform.position.z) < ARRIVED_MOVE_TOW)
            return 200;

        float staminaToBeUsed = state.getPace() * Time.deltaTime;
        if (state.IsMovingFast() && state.stamina < staminaToBeUsed)
        {
            state.currentPace = Pace.jogging;
        }


        rb.velocity = state.maxMoveSpeed * state.getPace() * transform.forward;
        
        if(state.IsMovingFast())
        {
            state.stamina -= staminaToBeUsed;
        }

        return 404;
    }

    public void Sleep()
    {
        if (state.currentMentalState == Mental_state.awake)
        {
            state.currentMentalState = Mental_state.sleeping;
            transform.Rotate(90, 0, 0);
        }
    }

    public void WakeUp()
    {
        if (state.currentMentalState != Mental_state.awake)
        {
            state.currentMentalState = Mental_state.awake;
            transform.Rotate(-90, 0, 0);
        }
    }

    private void UpdateStates()
    {
        // FED UPDATE
        if (state.fed > 0)
        {
            state.fed -= state.metabolism * FED_ATTRITION * Time.fixedDeltaTime;
        }

        // STAMINA UPDATE
        if (state.stamina < state.maxStamina && state.fed > 0 && !state.IsMovingFast())
        {
            float foodToEnergy = state.metabolism * (1 - state.getPace()) * Time.fixedDeltaTime;

            if (foodToEnergy / FOOD_TO_STAMINA_RATIO > state.fed)
            {
                foodToEnergy = state.fed * FOOD_TO_STAMINA_RATIO;
                state.fed = 0;
            }
            else state.fed -= foodToEnergy / FOOD_TO_STAMINA_RATIO;
            state.stamina += foodToEnergy;
        }
        // HEALTH UPDATE
        // damage
        if (state.fed <= 0)
        {
            physicalObject.durability -= state.maxDurability * HUNGER_HEALTH_EFFECT * Time.fixedDeltaTime;
        }
        // healing
        if (state.fed > 0)
        {
            physicalObject.durability += state.maxDurability * HUNGER_HEALTH_EFFECT * Time.fixedDeltaTime;

            if (physicalObject.durability > state.maxDurability)
                physicalObject.durability = state.maxDurability;
        }

        state.wakeFullness += ((float)state.currentMentalState / DAY_DURATION) * Time.fixedDeltaTime;
        if (state.wakeFullness < 0)
            state.wakeFullness = 0;


        // AUDIO UPDATE
        audioSource.volume = state.getPace();
        audioSource.maxDistance = state.getPace() * MAX_SOUND_DISTANCE;
        audioSource.pitch = 0.9f + state.getPace() * 0.5f;

        // AWARENESS UPDATE
        
    }

    // Meta functions

    public bool LineOfSightToTarget()
    {
        return LineOfSight(target.GetComponent<PhysicalObject>());
    }

    public bool LineOfSight(PhysicalObject target)
    {
        Vector3 lookAt = target.transform.position;
        lookAt.y = transform.position.y;
        transform.LookAt(lookAt);
        RaycastHit hit;

        if (!Physics.Raycast(transform.position, transform.forward, out hit, state.perception * state.wakeFullness * 5))
            return false;

        if (hit.transform.gameObject == target.gameObject)
            return true;

        return false;
    }

    public int FindBestPlantNearby()
    {
        float bestSoFar = 999999;
        Food bestFood = null;
        List<Food> foodNearby = FindAllPlantsNearby();
        foreach(Food food in foodNearby)
        {
            float distanceMagnitude = (food.transform.position - transform.position).magnitude;
            if (distanceMagnitude < bestSoFar)
            {
                bestFood = food;
            }
        }
        if (bestFood == null) return 404;
        target = bestFood.gameObject;
        return 200;
    }

    public List<Food> FindAllPlantsNearby()
    {
        float surroundRange = state.perception * state.wakeFullness * 2.5f;
        Vector3 surroundingsBounds = new Vector3(surroundRange, surroundRange * 0.5f, surroundRange);
        Collider[] surroundColliders = Physics.OverlapBox(transform.position, surroundingsBounds, Quaternion.identity, plantsMask);

        //float visionRange = state.perception * state.wakeFullness;
        //Vector3 visionVector = new Vector3(0, 0, visionRange);
        //Collider[] hitColliders = Physics.OverlapBox(transform.position, surroundingsBounds, Quaternion.identity, plantsMask);
        List<Food> foodList = new List<Food>();
        foreach(Collider collider in surroundColliders)
        {
            foodList.Add(collider.gameObject.GetComponent<Food>());
        }
        return foodList; 
    }

    /// <summary>
    /// Finds possible threats nearby
    /// </summary>
    public List<Agent> IsThreatNearby()
    {
        return null;
    }

    /// <summary>
    /// This calculated the odds of agent winning against target in fight
    /// </summary>
    public float TargetThreat(Agent target)
    {
        float staminaModifier       = (target.state.maxStamina / state.stamina ) * 0.2f;
        float durabilityModifier    = (target.physicalObject.durability / physicalObject.durability) * 0.4f;
        float attackModifier        = (target.physicalObject.density / physicalObject.density) * 0.4f;
        return staminaModifier + durabilityModifier + attackModifier;
    }
}
