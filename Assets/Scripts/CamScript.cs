using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScript : MonoBehaviour {

    public GameObject car;
    private Camera mainCam;
    public bool camMode;
    private Vector3 camVelo = new Vector3(0, 0, 1f);
    private Vector3 offset;
    private Vector3 carPos;
    Vector3 newPos, carPastPos, carDirection;

    float offset_;
    // Use this for initialization
    void Start () {
        mainCam = gameObject.GetComponent<Camera>();
        carPastPos = car.transform.position;
        camMode = true;
	}
	
	// Update is called once per frame
	void Update () {
		switch (camMode)
        {
            case true:
                Top();
                break;
            case false:
                Back();
                break;
        }
	}

    // back cam
    void Back()
    {
        offset = new Vector3(0, 3.5f, -6);
        offset_ = offset.magnitude;
        carDirection = (car.transform.position - carPastPos);
        carDirection.Normalize();
        carPos = car.transform.position;

        newPos = carPos - carDirection * offset_;

        transform.position = newPos + new Vector3(0, 3.5f, 0);
        transform.LookAt(carPos);
        carPastPos = carPos;
    }

    // top cam
    void Top()
    {
        offset = new Vector3(0, 40f, 0);
        carPos = new Bounds(car.transform.position, Vector3.zero).center;

        newPos = carPos + offset;

        transform.position = newPos;
        transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    // camera switcher
    public void ChangeCamera()
    {
        if (camMode)
        {
            camMode = false;
        }
        else camMode = true;
    }
}
