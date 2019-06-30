using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerCode : MonoBehaviour
{
    // NOTATION: This script is used for controlling the game
    // hope that the speed of game rate and the start&end can be controlled by it
    private List<GameObject> students = new List<GameObject>();
    public int studentsLeft;
    public int studentsAlive;
    private float[][] totalQtable;
    private int statenum = 6;
    private int actionnum = 2;
    public float envWidth;
    public float envHeight;
    public int studentNumber = 0;
    public float timeStart;
    public float alpha = 0.1f;  // Learning rate
    public float epsilon = 0.8f;  // Greedy
    public float gamma = 0.5f;  // Reward discount
    public int studentsKilled;
    public int shootCounter;
    public float timeLimit;
    public bool timeUp;

    public int gameStatus; // 0, 1, -1 for ready, running, end
    void Start()
    {
        foreach (GameObject n in GameObject.FindGameObjectsWithTag("agent"))
        {
            students.Add(n);
            studentNumber++;
        }
        timeUp = false;
        timeLimit = 300.0f;
        shootCounter = 0;
        studentsLeft = studentNumber;
        studentsAlive = studentNumber;
        studentsKilled = 0;
        gameStatus = 0;
        envHeight = 27.0f;
        envWidth = 20.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // mouse click. game start
        {
            gameStatus = 1;
            timeStart = Time.time;
        }
        if (gameStatus == 1)
        {
            timeUp = checkTimeLimit();
        }
        if (studentsLeft == 0 || timeUp == true) // Game End. All students dead
        {
            gameStatus = -1;
            Debug.Log("Game End");
        }
    }

    private bool checkTimeLimit()
    {
        if (Time.time - timeStart >= timeLimit)
            return true;
        return false;
    }
    public float getHW()
    {
        return envHeight + envWidth;
    }
    private void collectQTable()
    {
        totalQtable = new float[statenum][];
        for (int i = 0; i < statenum; i++)
        {
            for (int j = 0; j < actionnum; j++)
                totalQtable[i][j] = students[0].GetComponent<StudentLearn>().getQTable()[i][j];
        }

        for (int s = 1; s < students.ToArray().Length; s++)
        {
            for (int i = 0; i < statenum; i++)
            {
                for (int j = 0; j < actionnum; j++)
                    totalQtable[i][j] += students[s].GetComponent<StudentLearn>().getQTable()[i][j];
            }
        }
        for (int i = 0; i < statenum; i++)
        {
            for (int j = 0; j < actionnum; j++)
                totalQtable[i][j] /= students.ToArray().Length;
        }
    }
    public void shootHeard()
    {
        foreach (GameObject s in students)
        {
            s.GetComponent<StudentLearn>().gunShotHeard += 1;
        }
    }

    public int getStatus()
    {
        return gameStatus;
    }
}
