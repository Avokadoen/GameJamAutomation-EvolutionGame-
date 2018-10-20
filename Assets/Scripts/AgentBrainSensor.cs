using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentBrainSensor : StateMachineBehaviour {

    private int agentStatus;
    private int moveTowardsStatus;
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
            agentStatus = agentScript.FindBestPlantNearby();
            moveTowardsStatus = 404;
            animator.SetInteger("playerStatus", agentStatus);
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        Agent agentScript = animator.GetComponent<Agent>();
        if (stateInfo.IsName("findPlant") && agentStatus == 200)
        {
            if (agentScript.LineOfSightToTarget() && moveTowardsStatus > 299)
            {
                moveTowardsStatus = agentScript.MoveTowardsTarget();
                if(moveTowardsStatus == 200)
                {
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
