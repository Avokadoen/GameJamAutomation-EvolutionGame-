using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentBrainSensor : StateMachineBehaviour {

    private Player_state agentStatus;
    private Player_state moveTowardsStatus;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Agent agentScript = animator.GetComponent<Agent>();

        if (stateInfo.IsName("sleepy"))
        {
            agentScript.Sleep();
        }
        if (stateInfo.IsName("findPlant"))
        {
            animator.SetInteger("playerStatus", 0);
            agentStatus = agentScript.FindBestPlantNearby();
            moveTowardsStatus = Player_state.notFound;
            if (agentStatus != Player_state.ok)
                animator.SetInteger("playerStatus", (int)agentStatus);
        }
        if (stateInfo.IsName("findMeat"))
        {
            animator.SetInteger("playerStatus", 0);
            agentStatus = agentScript.FindBestMeatNearby();
            moveTowardsStatus = Player_state.notFound;
            if (agentStatus != Player_state.ok)
                animator.SetInteger("playerStatus", (int)agentStatus);
        }
        if (stateInfo.IsName("findPrey"))
        {
            Debug.Log("find prey start");
            animator.SetInteger("playerStatus", 0);
            agentStatus = agentScript.FindBestPreyNearby();
            moveTowardsStatus = Player_state.notFound;
            if (agentStatus != Player_state.attacking)
                animator.SetInteger("playerStatus", (int)agentStatus);
        }
        if (stateInfo.IsName("eat"))
        {
            agentScript.TryToPickUpTarget();
            agentScript.EatHeldObject();

            agentStatus = Player_state.notDefined;
            moveTowardsStatus = 0;
            animator.SetInteger("playerStatus", (int)agentStatus);

        }
    }


    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        Agent agentScript = animator.GetComponent<Agent>();

        if (stateInfo.IsName("findPlant"))
        {
            if (agentStatus == Player_state.found && agentScript.LineOfSightToTarget() && moveTowardsStatus == Player_state.notFound)
            {
                moveTowardsStatus = agentScript.MoveTowardsTarget();
                Debug.Log("moving");
                if(moveTowardsStatus == Player_state.ok)
                {
                    animator.SetInteger("playerStatus", (int)moveTowardsStatus);
                }
            }
            else if(agentStatus != Player_state.found)
            {
                agentStatus = agentScript.FindBestPlantNearby();
                animator.SetInteger("playerStatus", (int)agentStatus);
            }
        }
        if (stateInfo.IsName("findMeat"))
        {
            if (agentStatus == Player_state.found && agentScript.LineOfSightToTarget() && moveTowardsStatus == Player_state.notFound)
            {
                moveTowardsStatus = agentScript.MoveTowardsTarget();
                Debug.Log("moving");
                if (moveTowardsStatus == Player_state.ok)
                {
                    animator.SetInteger("playerStatus", (int)moveTowardsStatus);
                }
            }
            else if (agentStatus != Player_state.found)
            {
                Debug.Log("i am blind");
                agentStatus = agentScript.FindBestMeatNearby();
                animator.SetInteger("playerStatus", (int)agentStatus);
            }
        }
        if (stateInfo.IsName("findPrey"))
        {
            if (agentStatus == Player_state.attacking && agentScript.LineOfSightToTarget() && moveTowardsStatus == Player_state.notFound)
            {
                moveTowardsStatus = agentScript.MoveTowardsTarget();
                Debug.Log("moving");
                if (moveTowardsStatus == Player_state.ok)
                {
                    animator.SetInteger("playerStatus", (int)agentStatus);
                }
            }
        }
        if (stateInfo.IsName("fight"))
        {
            animator.SetInteger("playerStatus", (int)agentScript.Attack());
        }
    }

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Agent agentScript = animator.GetComponent<Agent>();
        if (stateInfo.IsName("sleepy"))
        {
            agentScript.WakeUp();
        }
    }
}
