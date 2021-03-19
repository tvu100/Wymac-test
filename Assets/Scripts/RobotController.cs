using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    private CoroutineQueue queue;
    private List<Coroutine> coroutines = new List<Coroutine>();
    void Start()
    {
        queue = new CoroutineQueue(this);
        queue.StartLoop();
    }

    //Set the robot to a position
    public IEnumerator START(float x, float y, float z)
    {
        transform.position = new Vector3(x, y, z);
        yield return null;
    }

    //Rotate the robot around the Y axis D degrees at DS degrees per second.
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
                deltaAngle += degreeSpeed * Time.deltaTime;
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
