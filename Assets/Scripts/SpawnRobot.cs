using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpawnRobot : MonoBehaviour
{
    public GameObject robot;
    public TextMeshProUGUI countText;
    private int count;
    // Update is called once per frame
    void Update()
    {
        //check player for correct input then spawn a robot.
        //Robot follow the commands in sequence until finished.
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            GameObject newRobot = Instantiate(robot, transform.position, transform.rotation);
            count++;
            countText.text = $"Robots Spawned: {count}";
        }
    }
}
