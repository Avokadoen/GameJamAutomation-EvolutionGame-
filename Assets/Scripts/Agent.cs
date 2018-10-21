using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Pace         {stop = 0, walking = 1, jogging = 3, running = 5, sprinting = 10}
public enum Mental_state {awake = -1, sleeping = 4}
public enum Eater_type { carnivore, omnivore, herbivore }
public enum Player_state { notDefined = 0, ok = 200, found = 202, attacking = 280, killedPrey = 290, danger = 304, notFound = 404, failedTask = 405 }

public struct AgentState
{
    public float attackCooldown;
    public float attackSpeed;
    public float fed;
    public float metabolism;
    public float wakeFullness;
    public float maxMoveSpeed;
    public float maxStamina;
    public float stamina;
    public float perception;
    public float maxDurability;
    public float actualDensity;
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
    public const int DAY_DURATION = 20;    // how many seconds in one day
    public const int MAX_SOUND_DISTANCE = 100;

    public Eater_type eaterType;

    public LayerMask plantsMask;
    public LayerMask meatMask;
    public LayerMask agentMask;

    public AgentState state;
    public Rigidbody rb;
    public AudioSource audioSource;

    public PhysicalObject physicalObject;
    public PhysicalObject heldObject;

    private GameObject target;
    private int moveTowardsStatus;
    private float prevTargetDistance;

    public

    // Use this for initialization
    void Start () {
        physicalObject = GetComponent<PhysicalObject>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        state.eaterType = eaterType;
        moveTowardsStatus = 404;
        prevTargetDistance = 9999f;
    }

    // Update is called once per frame
    void FixedUpdate () {
        UpdateStates();
        HoldObject();
    }


    // Actions
    public Player_state TryToPickUpTarget()
    {
        if (target != null)
        {
            return TryToPickUp(target.GetComponent<PhysicalObject>());
        }
        return Player_state.failedTask;
    }

    public Player_state TryToPickUp(PhysicalObject target)
    {
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, (target.transform.position - transform.position).normalized, out hit, 2f * transform.localScale.x))
            return Player_state.notFound;

        if (hit.transform.gameObject.name != target.gameObject.name)
            return Player_state.failedTask;

        heldObject = target;
        heldObject.OnPickUp(physicalObject);
        return Player_state.ok;
    }

    public Player_state MoveTowardsTarget()
    {
        return MoveTowards(target.transform.position);
    }

    public Player_state MoveTowards(Vector3 moveTowardsTarget)
    {
        Vector3 lookAt = moveTowardsTarget;
        lookAt.y = transform.position.y;
        transform.LookAt(lookAt);
        if (Physics.Raycast(transform.position, (target.transform.position - transform.position).normalized, 1f * transform.localScale.x))
            return Player_state.ok;

        else if (Mathf.Abs(moveTowardsTarget.x - transform.position.x) < ARRIVED_MOVE_TOW * transform.localScale.x &&
                Mathf.Abs(moveTowardsTarget.z - transform.position.z) < ARRIVED_MOVE_TOW * transform.localScale.z)
            return Player_state.ok;

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

        return Player_state.notFound;
    }

    public Player_state Attack()
    {
        if (target == null){
            physicalObject.density = state.actualDensity;
            return Player_state.killedPrey;
        }

        state.attackCooldown -= Time.deltaTime;
        float distance = (target.transform.position - transform.position).magnitude;
        if (state.attackCooldown <= 0)
        {
            physicalObject.density = state.actualDensity + 10f;
            rb.velocity = (target.transform.position - transform.position).normalized * 5f;
            state.attackCooldown = state.attackSpeed;
        }
        else if (distance >= prevTargetDistance)
        {
            physicalObject.density = state.actualDensity;
        }
        prevTargetDistance = distance;
        return Player_state.attacking;
    }
    public void Sleep()
    {
        if (state.currentMentalState == Mental_state.awake)
        {
            state.currentMentalState = Mental_state.sleeping;
            rb.isKinematic = true;
            transform.Rotate(90, 0, 0);
            gameObject.isStatic = true;
        }
    }

    public void WakeUp()
    {
        if (state.currentMentalState != Mental_state.awake)
        {
            state.currentMentalState = Mental_state.awake;
            rb.isKinematic = false;
            transform.Rotate(-90, 0, 0);
            gameObject.isStatic = false;
        }
    }

    private void UpdateStates()
    {
        rb.angularVelocity = new Vector3(0, 0, 0);

        if(state.stamina >= state.maxStamina)
        {
            state.currentPace = Pace.sprinting;
        }

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
            state.wakeFullness = 0.09f;


        // AUDIO UPDATE
        audioSource.volume = state.getPace();
        audioSource.maxDistance = state.getPace() * MAX_SOUND_DISTANCE;
        audioSource.pitch = 0.9f + state.getPace() * 0.5f;
        audioSource.loop = (rb.velocity.magnitude > 0.05f);

        // AWARENESS UPDATE

    }

    private void HoldObject()
    {
        if(heldObject != null)
        {
            heldObject.body.MovePosition(transform.position + transform.forward * 2f * transform.localScale.x);
        }
    }

    public void EatHeldObject()
    {
        if (heldObject == null)
            return;

        Food heldFood = heldObject.GetComponent<Food>();
        if (heldFood == null)
            return;

        if (heldFood.thisFoodType == food_type.meat && state.eaterType == Eater_type.herbivore)
            return;

        if (heldFood.thisFoodType == food_type.plant && state.eaterType == Eater_type.carnivore)
            return;

        state.fed += (float)heldFood.thisFoodType * 0.1f; // TODO: scale with foodtype and transform scale or other quantity

        Destroy(heldObject.gameObject);
        heldObject = null;
        target = null;
    }

    // Meta functions

    public bool LineOfSightToTarget()
    {
        if (target != null)
        {
            return LineOfSight(target.GetComponent<PhysicalObject>());
        }
        return false;
        
    }

    public bool LineOfSight(PhysicalObject target)
    {
        Vector3 lookAt = target.transform.position;
        lookAt.y = transform.position.y;
        transform.LookAt(lookAt);
        RaycastHit hit;

        if (!Physics.Raycast(transform.position, (target.transform.position - transform.position).normalized, out hit, (state.perception * state.wakeFullness * 5)))
            return false;

        if (hit.transform.gameObject.name != target.gameObject.name)
            return false;

        return true;
    }


    public Player_state FindBestPreyNearby()
    {
        float bestSoFar = 99999f;
        Agent easiestPrey = null;
        List<Agent> preyList = FindAllPreyNearby();
        foreach (Agent prey in preyList)
        {
            float threat = TargetThreat(prey);
 
            if (threat < bestSoFar && TargetThreat(prey) < prey.TargetThreat(this))
            {
                bestSoFar = threat;
                easiestPrey = prey;
            }
        }
        if (preyList.Count > 0 &&easiestPrey == null)
        {
            return Player_state.danger;
        }
        else if (preyList.Count == 0 && easiestPrey == null)
        {
            return Player_state.notFound;
        }
        target = easiestPrey.gameObject;
        return Player_state.attacking;
    }

    public List<Agent> FindAllPreyNearby()
    {
        float surroundRange = state.perception * state.wakeFullness * 2.5f * transform.localScale.x;
        Vector3 surroundingsBounds = new Vector3(surroundRange, surroundRange * 0.5f, surroundRange);
        Collider[] surroundColliders = Physics.OverlapBox(transform.position, surroundingsBounds, Quaternion.identity, agentMask);

        List<Agent> agentList = new List<Agent>();
        foreach (Collider collider in surroundColliders)
        {
            Agent agent = collider.gameObject.GetComponent<Agent>();
            if(agent != null)
                agentList.Add(agent);
            
            
        }
        return agentList;
    }

    public Player_state FindBestMeatNearby()
    {
        return FindBestFoodNearby(meatMask);
    }

    public Player_state FindBestPlantNearby()
    {
        return FindBestFoodNearby(plantsMask);
    }

    private Player_state FindBestFoodNearby(LayerMask mask)
    {
        float bestSoFar = 999999;
        Food bestFood = null;
        List<Food> foodNearby = FindAllFoodsNearby(mask);
        foreach(Food food in foodNearby)
        {
            float distanceMagnitude = Mathf.Abs((food.transform.position - transform.position).magnitude);
            if (distanceMagnitude < bestSoFar)
            {
                bestSoFar = distanceMagnitude;
                bestFood = food;
            }
        }
        if (bestFood == null) return Player_state.notFound;
        target = bestFood.gameObject;
        return Player_state.found;
    }

    private List<Food> FindAllFoodsNearby(LayerMask mask)
    {
        float surroundRange = state.perception * state.wakeFullness * 2.5f * transform.localScale.x;
        Vector3 surroundingsBounds = new Vector3(surroundRange, surroundRange * 0.5f, surroundRange);
        Collider[] surroundColliders = Physics.OverlapBox(transform.position, surroundingsBounds, Quaternion.identity, mask);

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
