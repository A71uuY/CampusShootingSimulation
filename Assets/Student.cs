using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// TODO: Fix bug in node selection part.
public class Student : MonoBehaviour
{
    // NOTATION:
    // These variables are the properties of the student
    // The student try to survive from the killers' hunting
    // Suppose that reaching exit will ensure the student's life
    // END(3.26)
    public int ID;
    public int life; // 1 if alive, 0 if dead

    private NavMeshAgent agent;
    private GameObject target;
    private GameObject targetExit;
    private GameObject GameController;
    private int exitIndex;
    private bool haveTarget;
    private bool hiding;
    private List<GameObject> nodes = new List<GameObject>();
    private List<GameObject> nodesInSight = new List<GameObject>();
    private List<GameObject> exits = new List<GameObject>();
    private List<int> visited = new List<int>();

    // Use this for initialization
    void Start()
    {
        life = 1;
        hiding = false;
        GameController = GameObject.FindGameObjectWithTag("GameController");
        agent = GetComponent<NavMeshAgent>();
        targetExit = null;
        exitIndex = -1;
        foreach (GameObject n in GameObject.FindGameObjectsWithTag("node"))
        {
            nodes.Add(n);
            visited.Add(0);
        }
        foreach (GameObject n in GameObject.FindGameObjectsWithTag("exit"))
        {
            exits.Add(n);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (GameController.GetComponent<GameControllerCode>().getStatus() == 1 && (life == 1)) // Gamestart. agent in environment
        {
            if (targetExit != null) // Exit found
            {
                if (!agent.pathPending && (agent.remainingDistance - agent.stoppingDistance) < 0.1f)
                { // reach destination
                    Escaped();
                }
            }
            else // No exit found
            {
                exitIndex = CanReachExit();
                if (exitIndex > -1) // have a path to exit
                {
                    agent.SetDestination(exits[exitIndex].transform.position);
                    targetExit = exits[exitIndex];
                }
                else
                {
                    if (!haveTarget && !hiding) // not hiding, no destination yet. find the furthest node
                    {
                        target = SelectNode();
                        if (null != target)
                        {
                            target.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0);
                            agent.SetDestination(target.transform.position);
                            haveTarget = true;
                        }
                    }
                    else if (!hiding)
                    {
                        MoveToTarget();
                    }
                }
            }
        }
    }
    void MoveToTarget()
    {
        if (!agent.pathPending && (agent.remainingDistance - agent.stoppingDistance) < 0.1f)
        { // reach destination
            haveTarget = false;
            visited[target.GetComponent<Node>().index] += 1;
            target.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 1);
            if(target.GetComponent<Node>().hideout)
                hide();
        }
    }
    void GetNodesInView()
    {
        // get the nodes in agent's view
        NavMeshHit hit;
        foreach (GameObject node in nodes)
        {
            if (!agent.Raycast(node.transform.position, out hit)) // no obstacle
            {
                nodesInSight.Add(node);
            }
        }
    }

    int CanReachExit()
    {
        int rv = -1;
        NavMeshHit hit;
        foreach (GameObject e in exits)
        {
            rv++;
            if (!agent.Raycast(e.transform.position, out hit)) // no obstacle
            {
                return rv;
            }
        }
        return -1;
    }

    GameObject SelectNode()
    {
        // Last edit: 3.10
        nodesInSight.Clear();
        float distance = 0;
        GameObject selectedNode = null;
        float maxdistance = 0;
        int visitedTime = -1;
        GetNodesInView(); // look around
        while (selectedNode == null)
        {
            visitedTime++;
            foreach (GameObject node in nodesInSight)
            {
                if(node.GetComponent<Node>().hideout) // hdieout has higest priority
                    return node;
                if (visitedTime >= visited[node.GetComponent<Node>().index]) // least visited time
                {
                    distance = Vector3.Distance(node.transform.position, agent.transform.position);
                    if (distance > maxdistance) // select the fartest node.
                    {
                        maxdistance = distance;
                        selectedNode = node;
                    }
                }
            }
        }
        return selectedNode;
    }

    void hide()
    {
        // hide in a room to avoid the killer
        // choose other behaviors if activated
        haveTarget = false;
        hiding = true;
        Debug.Log("Agent" + ID.ToString() + "Hiding");
        agent.GetComponent<MeshRenderer>().material.color = new Color32(138,43,226,1);
    }

    void activated()
    {
        // TODO: need triggers
        hiding = false;
    }
    public void Dead()
    {
        // called when killed
        life = 0;
        agent.GetComponent<MeshRenderer>().material.color = Color.yellow;
        agent.GetComponent<CapsuleCollider>().enabled = false;
        agent.GetComponent<NavMeshAgent>().enabled = false;
        //agent.SetDestination(agent.transform.position);
        GameController.GetComponent<GameControllerCode>().studentsLeft--;
        GameController.GetComponent<GameControllerCode>().studentsKilled++;
    }

    void Escaped()
    {
        agent.GetComponent<MeshRenderer>().material.color = Color.green;
        GameController.GetComponent<GameControllerCode>().studentsLeft--;
        agent.GetComponent<CapsuleCollider>().enabled = false;
        agent.GetComponent<NavMeshAgent>().enabled = false;
        life--;
    }
}
