using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Killer : MonoBehaviour
{

    // NOTATION:
    // These variables are the properties of the killer
    // The killer search for students and shoot them
    // Killers implement INDISCRIMINATE KILLING
    // END(3.26)
    public float KP; // Kill probability
    public int ID; // ID of the killer
    public int KillScore = 0; // Number of students killed
    public float viewAngle = 160;
    public float shootTime = 1; // Time to shoot
    private NavMeshAgent agent;
    private GameObject exit;
    private GameObject GameController;
    private GameObject killTarget;
    private List<GameObject> nodes = new List<GameObject>();
    private List<GameObject> nodesInSight = new List<GameObject>();
    private List<GameObject> studentsInSight = new List<GameObject>();
    private List<GameObject> studentsAlive = new List<GameObject>();
    private List<int> visited = new List<int>();

    // Use this for initialization
    void Start()
    {
        KP = 0.01f;
        exit = GameObject.FindGameObjectWithTag("exit");
        GameController = GameObject.FindGameObjectWithTag("GameController");

        agent = GetComponent<NavMeshAgent>();
        foreach (GameObject a in GameObject.FindGameObjectsWithTag("agent"))
        {
            studentsAlive.Add(a);
        }
        foreach (GameObject n in GameObject.FindGameObjectsWithTag("node"))
        {
            nodes.Add(n);
            visited.Add(0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.GetComponent<GameControllerCode>().getStatus() == 1)
        {
            killTarget = GetStudentInSight();
            if (killTarget != null)
            {
                // kill student
                Shoot();
            }
            else
            {
                if (!agent.pathPending && (agent.remainingDistance - agent.stoppingDistance) < 0.1f)
                    agent.SetDestination(RandomNode().transform.position);
            }
        }
        else if (GameController.GetComponent<GameControllerCode>().getStatus() == -1)
            agent.GetComponent<NavMeshAgent>().enabled = false;
    }

    IEnumerator StandBeforeShoot()
    {
        yield return new WaitForSeconds(shootTime);
    }
    void Shoot()
    {
        agent.enabled = false;
        StandBeforeShoot();
        if (Random.Range(0.0f, 1.0f) < KP) // Kill success
        {
            // kill success
            killTarget.GetComponent<Student>().Dead();
            KillScore++;
            studentsAlive.Remove(killTarget);
        }
        agent.enabled = true;
    }
    GameObject RandomNode()
    {
        // Get a node randomly
        GameObject node = null;
        node = nodes[Random.Range(0, nodes.Count)];
        return node;
    }

    GameObject GetStudentInSight()
    {
        // find a studnet in sight to kill
        NavMeshHit hit;
        float angle;
        studentsInSight.Clear();
        bool found = false;
        GameObject studentFound = null;
        foreach (GameObject student in studentsAlive)
        {
            if (student.GetComponent<Student>().life > 0)
            {
                if (student.GetComponent<Student>().isHiding())
                { // Hiding students are not easy to find
                    if (Random.Range(0, 2) == 1)
                    {
                        if (student != null && !agent.Raycast(student.transform.position, out hit)) // no obstacle. student is alive
                        {
                            angle = Vector3.Angle(student.transform.position - transform.position, transform.forward);
                            if (angle < viewAngle / 2)
                            {
                                found = true;
                                studentsInSight.Add(student);
                            }

                        }
                    }
                }
                else
                {
                    if (student != null && !agent.Raycast(student.transform.position, out hit)) // no obstacle. student is alive
                    {
                        angle = Vector3.Angle(student.transform.position - transform.position, transform.forward);
                        if (angle < viewAngle / 2)
                        {
                            found = true;
                            studentsInSight.Add(student);
                        }
                    }
                }
            }
        }
        if (!found)
            return null;
        studentFound = studentsInSight[Random.Range(0, studentsInSight.Count)];
        return studentFound;
    }
}
