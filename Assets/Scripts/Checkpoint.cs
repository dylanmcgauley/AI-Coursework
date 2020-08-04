using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {

    List<int> carIDs;
    Controller controller;
    bool alreadyHit;
    int previousGeneration = 0;

	// Use this for initialization
	void Start () {
        carIDs = new List<int>();
        alreadyHit = false;
	}

    private void OnTriggerEnter(Collider hit)
    {
        if(hit.CompareTag("Player"))
        {
            DoCheckpoint(hit);
        }
    }

    private void DoCheckpoint(Collider hit)
    {
        controller = hit.GetComponent<Controller>();

        if (previousGeneration != controller.generation)
        {
            carIDs.Clear();
        }

        for (int x = 0; x < carIDs.Count; x++)
        {
            if (controller.ID == carIDs[x])
            {
                alreadyHit = true;
            }
        }

        if (!alreadyHit)
        {
            controller.checkpoint++;
            controller.CalculateFitness();
            controller.checkpointTime = 0;
            previousGeneration = controller.generation;
            carIDs.Add(controller.ID);
        }

        alreadyHit = false;
    }

    }
