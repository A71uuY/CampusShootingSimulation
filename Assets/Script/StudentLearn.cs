using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
//TODO
//Check reward.
//Check state.
public class StudentLearn : MonoBehaviour
{

    // Use this for initialization
    private float gamma;  // reward discount
    private float epsilon;  // greedy
    public float eMin = 0.1f;
    private float alpha;  // learning rate
    private int states = 6; // 012 if running. 345 if hiding
    private float[][] qtable;
    private float reward = 0.0f;
    private int action;  // 0 if run; 1 if hide
    private int stateNow;
    private int stateNext;
    private float episodeReward = 0.0f;
    public int ID;
    public int life; // 1 if alive, 0 if dead
    public int gunShotHeard;
    private bool hiding;
    private GameObject targetHideout;
    private GameObject ignoreHideout;  // This hideout should be ignored to avoid triggered decision making too many times.
    public string statusString;
    private NavMeshAgent agent;
    private GameObject GameController;
    private List<GameObject> hideouts = new List<GameObject>();
    private List<GameObject> exits = new List<GameObject>();



    void Start()
    {
        life = 1;
        gunShotHeard = 0;
        hiding = false;
        statusString = "Init";
        GameController = GameObject.FindGameObjectWithTag("GameController");
        agent = GetComponent<NavMeshAgent>();
        foreach (GameObject n in GameObject.FindGameObjectsWithTag("hideout"))
        {
            hideouts.Add(n);
        }
        foreach (GameObject n in GameObject.FindGameObjectsWithTag("exit"))
        {
            exits.Add(n);
        }
        // Init part for q-learning parameters
        alpha = GameController.GetComponent<GameControllerCode>().alpha;
        epsilon = GameController.GetComponent<GameControllerCode>().epsilon;
        gamma = GameController.GetComponent<GameControllerCode>().gamma;
        // Initialize the Q_table to all zero
        qtable = new float[states][];
        for (int i = 0; i < states; i++)
        {
            qtable[i] = new float[2];
            for (int j = 0; j < 2; i++)
                qtable[i][j] = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.GetComponent<GameControllerCode>().getStatus() == 1 && (life == 1)) // Gamestart. agent in environment
        {
            if (statusString == "Init")
                RunAction();
            if (statusString == "Running")
                CheckEscaped(); // Already reached exit 
            CheckStateNow(); // Get State
            if (statusString == "Running")
            { // Strategy while running
                if ((GetNearestHideout() != null) && targetHideout != ignoreHideout) // Found a hideout nearby. Make a decision
                {
                    MakeDecision();
                    ignoreHideout = targetHideout;
                }
            }
            else if (statusString == "hiding") // Heard a gunshot. Make a decision
            { // Strategy while hiding
                if (gunShotHeard > 0)
                {
                    gunShotHeard = 0;
                    MakeDecision();
                }
            }
        }
    }

    void CheckStateNow()
    {  // Check state by some strategy
        if (statusString == "Running")
        {
        }
        else if (statusString == "Hiding")
        {
        }
    }
    GameObject GetNearestHideout()
    {
        NavMeshHit hit;
        GameObject result = null;
        float dis = 99999;
        foreach (GameObject h in hideouts)
        {
            if (!agent.Raycast(h.transform.position, out hit))  // Hideout in view and around
            {
                if (Vector3.Distance(agent.transform.position, h.transform.position) < dis)
                {
                    dis = Vector3.Distance(agent.transform.position, h.transform.position);
                    result = h;
                }
            }
        }
        if (result != null)
            targetHideout = result;
        return result;
    }
    void MakeDecision()
    { // Q-learning decision
        ChooseAction();
        ChooseState();
        UpdateQtable();
        stateNow = stateNext;
    }

    void ChooseState()
    {
        if (statusString == "Running" && action == 1) // Running choose to hide
        {
            stateNext = stateNow + 3;
        }
        if (statusString == "Hiding" && action == 0)  // Hiding choose to run
        {
            stateNext = stateNow - 3;
        }
    }
    void ChooseAction()
    {
        if (Random.Range(0.0f, 1.0f) > epsilon)  // Random Action
        {
            action = Random.Range(0, 2);
        }
        else  // Greedy
        {
            action = qtable[stateNow].ToList().IndexOf(qtable[stateNow].Max());
        }
    }

    void UpdateQtable()
    {
        qtable[stateNow][action] += alpha * (reward + gamma * qtable[stateNext].Max() - qtable[stateNow][action]);
    }
    void CheckEscaped()
    {
        if (!agent.pathPending && (agent.remainingDistance - agent.stoppingDistance) < 0.1f)
        { // reach destination
            Escaped();
        }
    }

    void RunAction()
    {
        int index = 0;
        int i = 0;
        float dis = 9999;
        foreach (GameObject e in exits)
        {
            if (Vector3.Distance(e.transform.position, agent.transform.position) < dis)  // Nearest exit
            {
                index = i;
            }
            i++;
        }
        agent.SetDestination(exits[index].transform.position);
        statusString = "Running";
    }

    void HideAction()
    {
        agent.SetDestination(targetHideout.transform.position);
        statusString = "hiding";
    }
    public bool isHiding()
    {
        return hiding;
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
