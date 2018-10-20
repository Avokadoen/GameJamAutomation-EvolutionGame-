using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Agent), typeof(Animator))]
public class AgentBrain : MonoBehaviour
{

    public Agent agent;
    public int decisionInterval; // in miliseconds
    public bool completedDecision;

    private Animator animator;
    private StateMachineBehaviour smb;
    private float decisisionCooldown;

    // Use this for initialization
    void Start()
    {
        agent = GetComponent<Agent>();
        animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        decisisionCooldown += Time.fixedDeltaTime;
        
        if(decisisionCooldown >= decisionInterval)
        {
            updateAnimatorData();
        }

    }

    private void updateAnimatorData()
    {
        animator.SetFloat("fed", agent.state.fed);
        animator.SetFloat("metabolism", agent.state.metabolism);
        animator.SetFloat("wakeFullness", agent.state.wakeFullness);
        animator.SetFloat("maxMoveSpeed", agent.state.maxMoveSpeed);
        animator.SetFloat("maxStamina", agent.state.maxStamina);
        animator.SetFloat("stamina", agent.state.stamina);
        animator.SetFloat("perception", agent.state.perception);
        animator.SetFloat("maxDurability", agent.state.maxDurability);
        animator.SetFloat("currentPace", agent.state.getPace());
        animator.SetFloat("threat", 0f);
        animator.SetInteger("currentMentalState", (int)agent.state.currentMentalState);
        animator.SetInteger("eaterType", (int)agent.state.eaterType);
    }
}
