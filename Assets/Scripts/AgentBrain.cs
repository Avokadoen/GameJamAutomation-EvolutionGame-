using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct DecisionHeuristic
{
    //public float ThreatPriority;
    public float Threat;
    public float Fed;
    public float Sleep;
    public static DecisionHeuristic operator -(DecisionHeuristic d1, DecisionHeuristic d2)
    {
        DecisionHeuristic d = new DecisionHeuristic
        {
            Threat = d1.Threat - d2.Threat,
            Fed = d1.Fed - d2.Fed,
            Sleep = d1.Sleep - d2.Sleep,
        };
        return d;
    }

    public float TotalValue()
    {
        return Threat + Fed + Sleep;
    }
}

struct Node
{
    public List<Node> neighbours;
    public DecisionHeuristic attributes;
}

[RequireComponent(typeof(Agent))]
public class AgentBrain : MonoBehaviour {

    public Agent agent;
    public float decisionInterval;
    public bool completedDecision;

    private Node rootDecision;
   

    // Use this for initialization
    void Start () {
        agent = GetComponent<Agent>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    // TODO: Generer tre, for å så lage A*
    void CreateDecisionTree(DecisionHeuristic goal) {
        rootDecision.neighbours.Clear();

        DecisionHeuristic threatHeur = new DecisionHeuristic
        {
            Threat = 1f
        };
        Node threatNode = new Node
        {
            attributes = threatHeur
        };
        DecisionHeuristic fedHeur = new DecisionHeuristic
        {
            Fed = 1f
        };
        Node fedNode = new Node
        {
            attributes = fedHeur
        };
        DecisionHeuristic sleepHeur = new DecisionHeuristic
        {
            Sleep = 1f
        };
        Node sleepNode = new Node
        {
            attributes = sleepHeur
        };

        rootDecision.neighbours.Add(threatNode);
        rootDecision.neighbours.Add(fedNode);
        rootDecision.neighbours.Add(sleepNode);

        HashSet<Node> openSet = new HashSet<Node>();

        foreach(Node node in rootDecision.neighbours)
        {
            openSet.Add(node);
        }

        DecisionHeuristic bestCost = new DecisionHeuristic
        {
            Threat = 1f,
            Fed = 1f,
            Sleep = 1f,
        };

        Node currentNode = rootDecision;
        /*Node? bestNode = FindBestNode(currentNode, goal, bestCost);
        if (bestNode != null)
        {
            currentNode = (Node)bestNode;
            
        }*/

        // backtrack


    }

    /*Node? FindBestNode(Node currentNode, DecisionHeuristic goal, DecisionHeuristic bestCost)
    {
        Node? rtrNode = null; 
        foreach (Node node in currentNode.neighbours)
        {
            DecisionHeuristic cost = node.attributes - goal;
            if (cost.TotalValue() < bestCost.TotalValue())
            {
                bestCost = cost;
                rtrNode = node;
            }

        }
        return rtrNode;
    }*/
}
