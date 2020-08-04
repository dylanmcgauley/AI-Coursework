using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RuleBased : MonoBehaviour {

    // sensors init
    public Vector3 frontRay = Vector3.forward.normalized;
    public Vector3 left45Ray = (Vector3.forward - Vector3.left).normalized;
    public Vector3 right45Ray = (Vector3.forward - Vector3.right).normalized;
    public Vector3 leftRay = Vector3.left.normalized;
    public Vector3 rightRay = Vector3.right.normalized;
    public RaycastHit hit;
    private float frontDist = 0;
    private float left45Dist = 0;
    private float right45Dist = 0;
    private float leftDist = 0;
    private float rightDist = 0;
    private float past45Left = 0;
    private float past45Right = 0;
    private float speed = 35;
    public bool ruleBased = false;
    float lapTime = 0;
    float lapTime2 = 0;
    public Text lapText;
    public Text lapText2;

    Rigidbody rbody;


    // Use this for initialization
    void Start () {
        rbody = this.GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        if (ruleBased)
        {
            lapTime += Time.deltaTime;
            lapText.text = "Lap Time: " + lapTime;
            lapText2.text = "Past Lap Time: " + lapTime2;
            // check sensor distances
            frontDist = CheckSensors(frontRay);
            right45Dist = CheckSensors(left45Ray);
            left45Dist = CheckSensors(right45Ray);
            leftDist = CheckSensors(leftRay);
            rightDist = CheckSensors(rightRay);

            // check/ run rules
            Forward();
            Turn();
            TurnCheck();
        }
	}

    // Rule 1
    // Speed Rule
    // Move forward if nothing is in front
    void Forward()
    {
        // if nothing is in front drive forward
        if (frontDist > 15)
        {
            rbody.velocity = transform.forward * 35;
        }
        // stop if too close to wall
        else if (frontDist < 6) rbody.velocity = Vector3.zero;
        // slow down if getting close to a wall
        else rbody.velocity = transform.forward * (2 * frontDist);
    }

    // Rule 2
    // Turning Rule
    void Turn()
    {
        // if getting close to wall decide which direction to turn
        if (frontDist < 15)
        {
            if (left45Dist > right45Dist)
            {
                transform.Rotate(new Vector3(0, -1, 0));
            }
            else transform.Rotate(new Vector3(0, 1, 0));
        }
        // if dangerously close to wall do a sharp turn
        else if (frontDist < 8)
        {
                if (leftDist > rightDist)
                {
                    transform.Rotate(new Vector3(0, -10, 0));
                }
                else transform.Rotate(new Vector3(0, 10, 0));
        }
    }

    // Rule 3 
    // Turn Check
    void TurnCheck()
    {
        // check if sensor has detected a corner in the track and turn towards it
        if (left45Dist - past45Left > 6)
        {
            transform.Rotate(new Vector3(0, -20, 0));
        }
        else if (right45Dist - past45Right > 6)
        {
            transform.Rotate(new Vector3(0, 20, 0));
        }
        past45Left = left45Dist;
        past45Right = right45Dist;
    }

    // collision detection for player hitting the track walls
    private void OnTriggerEnter(Collider hit)
    {
        if (hit.CompareTag("Finish"))
        {
            lapTime2 = lapTime;
            lapTime = 0;
        }
    }

    // check if sensor is in distance of the wall and hitting
    float CheckSensors(Vector3 sensorDir)
    {
        // gives the ray an extra bit of height
        Vector3 carPos = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);

        if (Physics.Raycast(carPos, transform.TransformDirection(sensorDir), out hit))
        {
            // draw the ray and return the distance
            Debug.DrawRay(carPos, transform.TransformDirection(sensorDir) * hit.distance, Color.green, 0, true);
            return hit.distance;
        }
        return 0;
    }
}
