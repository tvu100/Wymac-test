using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RobotController : MonoBehaviour
{
    public TextAsset robotData;

    private CoroutineQueue _queue;
    private List<Coroutine> _coroutines = new List<Coroutine>();
    private List<string> _commandsText = new List<string>();

    //movement
    private bool _canMove;
    private float _vel;
    //rotation
    private bool _canRot;
    private float _rotSpeed;
    private float _rotTarget;
    private Quaternion _startRot;
    private float deltaAngle = 0;

    void Start()
    {
        string tempData = robotData.text;
        List<string> tempCommands = new List<string>();
        //add every line from text to list
        tempCommands.AddRange(tempData.Trim().Split("\n"[0]));
        //sort the list for any strings that doesnt start with "//" or white spaces and then add it to the commands list.
        foreach (string s in tempCommands)
            if (!s.StartsWith("//") && !char.IsWhiteSpace(s[0]))
                _commandsText.Add(s.Trim());

        _queue = new CoroutineQueue(this);
        //start queue
        _queue.StartLoop();

        //go through each command and add it to the queue.
        foreach (string command in _commandsText)
        {
            string[] method = command.Split(" "[0]);
            //get the meothod name
            string methodName = method[0];
            //get the parameters from data, if none set it to null. 
            string methodParams = method.Length > 1 ? method[1] : null;

            //ensure that there should only be 2 or 1 strings. One for method name and another for parameters(if any).
            if (method.Length > 2) throw new ArgumentException($"Invalid Command at {method[1]}");

            switch (methodName)
            {
                case "START":
                    //get the values in the param data
                    string[] posValue = methodParams.Split(","[0]);

                    #region throw exceptions
                    //make sure that it has only 3 params
                    if (posValue.Length != 3) throw new ArgumentException("Please make sure there is only 3 parameters in the data");

                    //make sure params are all a number
                    foreach (string s in posValue)
                    {
                        float number;
                        if (!float.TryParse(s, out number))
                            throw new ArgumentException($"Please make sure <color=red>{s}</color> parameter is a number", nameof(posValue));
                    }
                    #endregion

                    _queue.EnqueueAction(START(float.Parse(posValue[0]), float.Parse(posValue[1]), float.Parse(posValue[2])));
                    break;
                case "ROTATE":
                    //split the parameters in the string up into individual values.
                    string[] rotValues = methodParams.Split(","[0]);

                    #region throw exceptions
                    //make sure that it has only 2 params
                    if (rotValues.Length != 2) throw new ArgumentException("Please make sure there is only 2 parameters in the data");

                    //make sure params are all a number
                    foreach (string s in rotValues)
                    {
                        float number;
                        if (!float.TryParse(s, out number))
                            throw new ArgumentException($"Please make sure <color=red>{s}</color> parameter is a number", nameof(posValue));
                    }
                    #endregion

                    //add rotation to the queue 
                    _queue.EnqueueAction(ROTATE(float.Parse(rotValues[0]), float.Parse(rotValues[1])));
                    break;
                case "MOVE":

                    #region throw exceptions
                    //make sure that it has only 1 params
                    if (method[1].Trim().Contains(",")) throw new ArgumentException("Please make sure there is only 1 parameter in the data");

                    //make sure params are all a number and if is it'll pass it onto the MOVE method
                    float vel;
                    if (!float.TryParse(methodParams, out vel))
                        throw new ArgumentException($"Please make sure <color=red>{methodParams}</color> parameter is a number", nameof(vel));
                    #endregion
                    //add the movement to queue
                    _queue.EnqueueAction(MOVE(vel));
                    break;
                case "WAIT":
                    #region throw exceptions
                    //make sure that it has only 1 params
                    if (method[1].Trim().Contains(",")) throw new ArgumentException("Please make sure there is only 1 parameter in the data");

                    //make sure params are all a number
                    float waitTime;
                    if (!float.TryParse(methodParams, out waitTime))
                        throw new ArgumentException($"Please make sure <color=red>{methodParams}</color> parameter is a number", nameof(waitTime));
                    #endregion
                    //add wait to the queue
                    _queue.EnqueueWait(WAIT(float.Parse(methodParams)));
                    break;
                case "STOP":
                    //add stop to the queue
                    _queue.EnqueueAction(STOP());
                    break;
                case "DESTROY":
                    //add destory to the queue
                    _queue.EnqueueAction(DESTROY());
                    break;
            };
        };
    }

    void Update()
    {
        //  doing this in the Update() method will ensure if the robot is moving or rotating it will continue to move/rotate, 
        //  whilst the command processor is 'waiting'.
        if (_canMove)
            transform.position += transform.forward * Time.deltaTime * _vel;

        if (_canRot)
        {
            // Zero degreeSpeed means to rotate immediatley.
            if (_rotSpeed == 0)
            {
                //snap to angle
                transform.rotation *= Quaternion.AngleAxis(_rotTarget, Vector3.up);
                //stop rotating
                _canRot = false;
            }
            //check if _rotTarget is positive or negative and then determine the direction to rotate, as well as get the smallest or largest angle so it doesn't overshoot and set it to the target rotation when reached.
            deltaAngle = _rotTarget > 0 ? Mathf.Min(deltaAngle += _rotSpeed * Time.deltaTime, _rotTarget) :
                                          Mathf.Max(deltaAngle -= _rotSpeed * Time.deltaTime, _rotTarget);

            //rotate until target angle is reached;
            if ((_rotTarget > 0 && deltaAngle < _rotTarget) || (_rotTarget < 0 && deltaAngle > _rotTarget))
                // The rotation is relative from the robot's current rotation.
                transform.rotation = _startRot * Quaternion.AngleAxis(deltaAngle, Vector3.up);
            else
                _canRot = false;
        }
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
        deltaAngle = 0;
        // save starting rotation position
        _startRot = transform.rotation;
        _rotTarget = degree;
        _rotSpeed = degreeSpeed;
        _canRot = true;
        yield break;
    }

    //Set the robot moving in the robot's current forward direction at vel
    public IEnumerator MOVE(float vel)
    {
        _canMove = true;
        _vel = vel;
        yield break;
    }


    //Pauses the execution of the command processor for waitTime seconds
    public IEnumerator WAIT(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
    }

    //Stop Moving and or Rotating the robot.
    public IEnumerator STOP()
    {
        _canRot = false;
        _canMove = false;
        yield break;
    }

    //Destroy and remove the robot from the scene.
    public IEnumerator DESTROY()
    {
        Destroy(this.gameObject);
        yield break;
    }

}
