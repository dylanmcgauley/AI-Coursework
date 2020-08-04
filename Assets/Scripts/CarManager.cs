using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class CarManager : MonoBehaviour {

    // variables
    [SerializeField] private GameObject carPrefab;
    public int carPopulation = 20;
    [SerializeField] private int[] layerInfo;
    public int topParentCars = 6;
    public int lastGenCars = 3;
    public int randParentCars = 7;
    public int generation = 0;
    public Camera cam;
    CamScript camScript;
    public Text generationText;
    public Text fitnessText;
    public Text lapTimeText;
    public Text lastLapTimeText;
    private float camTimer = 0;
    private Vector3 startPos;
    private Quaternion startRotation;
    bool playerInput = false;
    bool neuralNet = true;
    float lapTime = 0;
    float pastLapTime = 0;

    // NN cars
    public List<GameObject> cars;
    GameObject ruleCar;
    public List<Controller> controllers;
    private Controller bestCar, secondBestCar, previousBest;

    void Start()
    {
        // create empty list of cars and networks
        cars = new List<GameObject>();
        SpawnCars(carPopulation);
        ruleCar = Instantiate(carPrefab, carPrefab.transform.position, carPrefab.transform.rotation);
        ruleCar.SetActive(false);
        ruleCar.GetComponent<Controller>().useNN = false;
        ruleCar.GetComponent<RuleBased>().ruleBased = true;
        SetIDs();
        camScript = cam.GetComponent<CamScript>();
        camScript.car = cars[controllers[0].ID];
        startPos = carPrefab.transform.position;
        startRotation = carPrefab.transform.rotation;
    }

    void Update()
    {
        camTimer += Time.deltaTime;

        if (!playerInput && neuralNet)
        {
            if (camTimer > 2)
            {
                controllers.Sort(SortFitness);

                camScript.car = cars[controllers[0].ID];
                lapTime = controllers[0].lapTime;
            }
        }
        else if (!neuralNet) camScript.car = ruleCar;
        else camScript.car = carPrefab;

        generationText.text = "Generation: " + generation;
        fitnessText.text = "Fittest AI: " + controllers[0].fitness;
        lapTimeText.text = "Lap Time: " + lapTime;
        lastLapTimeText.text = "Last Lap Time: " + pastLapTime;

        for (int x = 0; x < cars.Count - 1; x++)
        {
            for (int y = 0; y < cars.Count -1; y++)
            {
                Physics.IgnoreCollision(cars[x].GetComponent<BoxCollider>(), cars[y].GetComponent<BoxCollider>());
            }
            if (playerInput)
            {
                Physics.IgnoreCollision(cars[x].GetComponent<BoxCollider>(), carPrefab.GetComponent<BoxCollider>());
            }
        }
        // check to see if all cars are dead
        if (AllDead())
        {
            // if all cars are dead start a new generation
            GenerationRestart();
        }
    }

    private void SetIDs()
    {
        for (int x = 0; x < controllers.Count; x++)
        {
            controllers[x].ID = x;
        }
    }

    // spawns user defined population of cars into the scene
    public void SpawnCars(int population)
    {
        // loops for ammount of cars needing added, creates a new car and adds it to list of NN Cars
        for (int x = 0; x < population; x++)
            {
                GameObject newCar = Instantiate(carPrefab, carPrefab.transform.position, carPrefab.transform.rotation);
                cars.Add(newCar);
                controllers.Insert(x, cars[x].GetComponent<Controller>());
            }
        carPrefab.SetActive(false);
    }

    // check to see if all the cars have died
    public bool AllDead()
    {
        // loop through each car and check its alive state
        for (int x = 0; x < controllers.Count; x++)
        { 
            if (controllers[x].alive)
            {
                return false;
            }
        }

        return true;
    }

    public void UsePlayerInput()
    {
        if (!playerInput)
        {
            playerInput = true;
            carPrefab.SetActive(true);
            carPrefab.transform.position = startPos;
            carPrefab.GetComponent<Controller>().playerInput = true;
        }
        else
        {
            playerInput = false;
            carPrefab.SetActive(false);
            carPrefab.GetComponent<Controller>().playerInput = false;
        }
    }

    public void UseNeuralNet()
    {
        if (!neuralNet)
        {
            neuralNet = true;
            foreach (GameObject car in cars)
            {
                car.SetActive(true);
            }
            ruleCar.SetActive(false);
        }
        else
        {
            neuralNet = false;
            foreach (GameObject car in cars)
            {
                car.SetActive(false);
            }
            ruleCar.SetActive(true);
            ruleCar.transform.position = startPos;
            ruleCar.transform.rotation = startRotation;
        }
    }

    // used to respawn the cars when a new generation is started
    public void GenerationRestart()
    {
        foreach (GameObject car in cars)
        {
            car.transform.position = startPos;
            car.transform.rotation = startRotation;
        }

        previousBest = controllers[0];

        controllers.Sort(SortFitness);

        bestCar = controllers[0];
        secondBestCar = controllers[1];

        controllers[0].checkpoint = 1;
        controllers[0].checkpointTime = 0;
        controllers[0].fitness = 0;
        controllers[0].alive = true;
        controllers[0].generation++;
        pastLapTime = controllers[0].lapTime;
        controllers[0].lapTime = 0;
        controllers[1].checkpoint = 1;
        controllers[1].checkpointTime = 0;
        controllers[1].fitness = 0;
        controllers[1].alive = true;
        controllers[1].generation++;
        controllers[1].lapTime = 0;
        carPrefab.GetComponent<Controller>().alive = true;
        carPrefab.GetComponent<Controller>().checkpointTime = 0;
        carPrefab.GetComponent<Controller>().checkpoint = 1;
        carPrefab.transform.position = startPos;
        carPrefab.transform.rotation = startRotation;

        for (int x = 2; x < controllers.Count; x++)
        {
            //int randCar1 = Random.Range(0, controllers.Count - 1);
            //int randCar2 = Random.Range(0, controllers.Count - 1);
            if (x < topParentCars + 2) controllers[x].CreateChild(bestCar.brain, secondBestCar.brain);
            else controllers[x].StartBrain();
            controllers[x].checkpoint = 1;
            controllers[x].checkpointTime = 0;
            controllers[x].fitness = 0;
            controllers[x].alive = true;
            controllers[x].generation++;
            controllers[x].lapTime = 0;
        }
        generation++;
    }

    static int SortFitness(Controller car1, Controller car2)
    {
        return car2.fitness.CompareTo(car1.fitness);
    }
}
