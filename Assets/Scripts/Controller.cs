using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

    // sensors init
    public Vector3 frontRay = Vector3.forward.normalized;
    public Vector3 left45Ray = (Vector3.forward - Vector3.left).normalized;
    public Vector3 right45Ray = (Vector3.forward - Vector3.right).normalized;
    public Vector3 leftRay = Vector3.left.normalized;
    public Vector3 rightRay = Vector3.right.normalized;
    public RaycastHit hit;
    public int[] layerInfo = { 5, 4, 3, 2 };
    public float[] sensorDist = new float[5];
    public float lapTime = 0;

    public NeuralNet brain;
    Rigidbody rbody;

    public bool alive;
    public bool playerInput = false;
    public bool useNN = true;
    public int ID;

    // fitness value init
    public float fitness = 0;
    public float checkpointTime = 0;
    public int checkpoint = 1;
    public int generation = 0;
    private float inputSpeed = 35;
    private float steeringMultiplier = 10;

    // Use this for initialization
    void Start () {
        brain = new NeuralNet();
        alive = true;
        rbody = this.GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update () {
        if (useNN)
        {
            sensorDist[0] = CheckSensors(frontRay);
            sensorDist[1] = CheckSensors(left45Ray);
            sensorDist[2] = CheckSensors(right45Ray);
            sensorDist[3] = CheckSensors(leftRay);
            sensorDist[4] = CheckSensors(rightRay);

            // checkpoint timer
            checkpointTime += Time.deltaTime;

            float[] carMovement = brain.GenerateOutputs(sensorDist);
            float speed = carMovement[0];
            float steering = carMovement[1];

            if (alive)
            {
                KillCheck();
                lapTime += Time.deltaTime;
                if (!playerInput)
                {
                    rbody.velocity = transform.forward * (speed * 35);

                    if (steering < 0.49) transform.Rotate(new Vector3(0, -steering * steeringMultiplier, 0));
                    else if (steering > 0.51) transform.Rotate(new Vector3(0, steering * steeringMultiplier, 0));
                }
                else
                {
                    if (Input.GetKey(KeyCode.W))
                    {
                        rbody.velocity = transform.forward * inputSpeed;
                    }
                    else rbody.velocity = Vector3.zero;
                    if (Input.GetKey(KeyCode.A))
                    {
                        transform.Rotate(new Vector3(0, -1, 0));
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        transform.Rotate(new Vector3(0, 1, 0));
                    }
                }
            }
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

    public void StartBrain()
    {
        brain = new NeuralNet();
    }

    public void CreateChild(NeuralNet dad, NeuralNet mum)
    {
        brain = new NeuralNet(dad, mum);
    }

    public void Mutate()
    {
        brain.Mutate();
    }

    // collision detection for player hitting the track walls
    private void OnTriggerEnter(Collider hit)
    {
        if (hit.CompareTag("Walls"))
        {
            // run function to stop the player
            Stop();
        }

        //if (hit.CompareTag("Checkpoint"))
        //{
        //    checkpoint++;
        //    CalculateFitness();
        //    checkpointTime = 0;
        //}

        if (hit.CompareTag("Finish"))
        {
            Stop();
        }
    }

    // stops the car after it has hit a wall
    private void Stop()
    {
        rbody.velocity = new Vector3(0, 0, 0);
        alive = false;
    }

    public void CalculateFitness()
    {
        fitness = fitness + (checkpoint / checkpointTime);
    }

    private void KillCheck()
    {
        if (checkpointTime > 5)
        {
            Stop();
        }
    }
}
