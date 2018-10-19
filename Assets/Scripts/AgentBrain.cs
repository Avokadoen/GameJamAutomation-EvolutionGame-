using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Agent))]
public class AgentBrain : MonoBehaviour {

    Agent agent;

	// Use this for initialization
	void Start () {
        agent = GetComponent<Agent>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
