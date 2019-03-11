using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerCode : MonoBehaviour
{
    // NOTATION: This script is used for controlling the game
    // hope that the speed of game rate and the start&end can be controlled by it
    private List<GameObject> students = new List<GameObject>();
    public int studentsLeft;
    public int studentNumber = 0;
    public int studentsKilled;
    public int gameStatus; // 0, 1, -1 for ready, running, end
    void Start()
    {
        foreach (GameObject n in GameObject.FindGameObjectsWithTag("agent"))
        {
            students.Add(n);
            studentNumber++;
        }
        studentsLeft = studentNumber;
        studentsKilled = 0;
        gameStatus = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (studentsLeft == 0) // Game End. All students dead
            gameStatus = -1;
        else if (Input.GetMouseButtonDown(0)) // mouse click. game start
        {
            gameStatus = 1;
        }
    }

    public int getStatus()
    {
        return gameStatus;
    }
}
