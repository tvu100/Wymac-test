using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    public TextAsset robotData;

    private CoroutineQueue queue;
    private List<Coroutine> coroutines = new List<Coroutine>();
    private List<string> commands = new List<string>();

    void Start()
    {
        string tempData = robotData.text;
        List<string> tempCommands = new List<string>();
        //add every line from text to list
        tempCommands.AddRange(tempData.Trim().Split("\n"[0]));
        //sort the list for any strings that doesnt start with "//" or white spaces and then add it to the commands list.
        foreach (string s in tempCommands)
            if (!s.StartsWith("//") && !char.IsWhiteSpace(s[0]))
                commands.Add(s.Trim());

        queue = new CoroutineQueue(this);
        queue.StartLoop();
    }

    //Set the robot to a position
    public IEnumerator START(float x, float y, float z)
    {
        transform.position = new Vector3(x, y, z);
        yield return null;
    }

    //Rotate the robot around the Y axis degree at degreeSpeed per second.
    public IEnumerator ROTATE(float degree, float degreeSpeed)
    {

        // Zero degreeSpeed means to rotate immediatley.
        if (degreeSpeed == 0)
        {
            //snap to angle
            transform.rotation *= Quaternion.AngleAxis(degree, Vector3.up);
            //exit coroutine
            yield break;
        }

        float deltaAngle = 0;
        // save starting rotation position
        Quaternion startRot = transform.rotation;

        while (true)
        {
            //rotate until reached angle
            if (deltaAngle < degree)
            {
                //rotate speed;
                deltaAngle += degreeSpeed * Time.deltaTime;
                //returns the smallest angle so it doesn't overshoot
                deltaAngle = Mathf.Min(deltaAngle, degree);
                // The rotation is relative from the robot's current rotation.
                transform.rotation = startRot * Quaternion.AngleAxis(deltaAngle, Vector3.up);
                yield return null;
            }
            else
                yield break;

        }
    }

    //Set the robot moving in the robot's current forward direction at vel
    public IEnumerator MOVE(float vel)
    {
        while (true)
        {
            transform.position += transform.forward * Time.deltaTime * vel;
            yield return null;
        }
    }

    //Pauses the execution of the command processor for waitTime seconds
    public IEnumerator WAIT(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        //  If the robot is moving or rotating it will continue to move/rotate, whilst the command processor is 'waiting'.
    }

    //Stop Moving and or Rotating the robot.
    public IEnumerator STOP()
    {
        foreach (Coroutine c in coroutines)
        {
            StopCoroutine(c);
        }
        coroutines.Clear();
        yield return null;
    }

    //Destroy and remove the robot from the scene.
    public void DESTROY()
    {
        Destroy(this.gameObject);
    }

}
