using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentBrainSensor : StateMachineBehaviour {

    private int agentStatus;
    private int moveTowardsStatus;
    private float threat;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Agent agentScript = animator.GetComponent<Agent>();
        if (stateInfo.IsName("Sleepy"))
        {
            agentScript.Sleep();
        }
        if (stateInfo.IsName("findPlant"))
        {
            animator.SetInteger("playerStatus", 0);
            agentStatus = agentScript.FindBestPlantNearby();
            moveTowardsStatus = 404;
            if(agentStatus > 299)
                animator.SetInteger("playerStatus", agentStatus);
        }
        if (stateInfo.IsName("findPrey"))
        {
            Debug.Log("find prey start");
            float threat = agentScript.FindBestPreyNearby();
            animator.SetFloat("threat", threat);
            agentStatus = ((threat < 99999) ? 200 : 404);
            moveTowardsStatus = 404;
            if (agentStatus > 299)
                animator.SetInteger("playerStatus", agentStatus);
        }
        if (stateInfo.IsName("eat"))
        {
            agentScript.TryToPickUpTarget();
            agentScript.EatHeldObject();
            
            agentStatus = moveTowardsStatus = 0;
            animator.SetInteger("playerStatus", agentStatus);

        }
        
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        Agent agentScript = animator.GetComponent<Agent>();
        if (stateInfo.IsName("findPlant") && agentStatus >= 200 && agentStatus <= 299)
        {
            if (agentScript.LineOfSightToTarget() && moveTowardsStatus > 299)
            {
                moveTowardsStatus = agentScript.MoveTowardsTarget();
                Debug.Log("moving");
                if(moveTowardsStatus >= 200 && moveTowardsStatus <= 299)
                {
                    animator.SetInteger("playerStatus", agentStatus);
                }
            }
        }

        if (stateInfo.IsName("findPrey") && agentStatus >= 200 && agentStatus <= 299)
        {
            float threat = agentScript.FindBestPreyNearby();
            animator.SetFloat("threat", threat);
            Debug.Log("find prey");
            if (agentScript.LineOfSightToTarget() && moveTowardsStatus > 299)
            {
                Debug.Log("prey found");
                moveTowardsStatus = agentScript.MoveTowardsTarget();
                if (moveTowardsStatus >= 200 && moveTowardsStatus <= 299)
                {
                    Debug.Log("at prey");
                    animator.SetInteger("playerStatus", agentStatus);
                }
            }
        }
    }

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Agent agentScript = animator.GetComponent<Agent>();
        if (stateInfo.IsName("Sleepy"))
        {
            agentScript.WakeUp();
        }
    }
}
