using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    [Header("Spawn Setup")]
    [SerializeField] private FlockUnit flockUnitPrefab;
    [SerializeField] private int flockSize; //Number of units
    [SerializeField] private Vector3 spawnBounds; //Vector to define spawn bounds
    
    // Speed for units. Each will have a min and max
    [Header("Speed Setup")]
    [Range(0, 10)]
    [SerializeField] private float _minSpeed;
    public float minSpeed { get { return _minSpeed; } }

    [Range(0, 10)]
    [SerializeField] private float _maxSpeed;
    public float maxSpeed { get { return maxSpeed; } }


    // Distances used in FlockUnitAlt to check for distances between other units.
    [Header("Detection Distances")]
    [Range(0, 10)]
    [SerializeField] private float _cohesionDistance; 

    public float cohesianDistance { get { return _cohesionDistance; } }

    [Range(0, 10)]
    [SerializeField] private float _avoidanceDistance;

    public float avoidanceDistance { get { return _avoidanceDistance; } }

    [Range(0, 10)]
    [SerializeField] private float _alignDistance;

    public float alignmentDistance { get { return _alignDistance; } }


    [Range(0, 10)]
    [SerializeField] private float _obstacleDistance;
    public float obstacleDistance { get { return _obstacleDistance; } }

    [Range(0, 100)]
    [SerializeField] private float _boundsDistance;
    public float boundsDistance { get { return _boundsDistance; } }

    // Add weights to every behavior type. This way we can build different flock behaviors, that will favor different behavior.
    [Header("Behavior Weights")]
    [Range(0, 10)]
    [SerializeField] private float _cohesionWeight;

    public float cohesionWeight { get { return _cohesionWeight; } }

    [Range(0, 10)]
    [SerializeField] private float _avoidanceWeight;

    public float avoidanceWeight { get { return _avoidanceWeight; } }

    [Range(0, 10)]
    [SerializeField] private float _alignmentWeight;

    public float alignmentWeight { get { return _alignmentWeight; } }

    [Range(0, 10)]
    [SerializeField] private float _boundsWeight;
    public float boundsWeight { get { return _boundsWeight; } }

    [Range(0, 100)]
    [SerializeField] private float _obstacleWeight;
    public float obstacleWeight { get { return _obstacleWeight; } }


    //Keep all instatiated units in an array 
    public FlockUnit[] allUnits { get; set; }




    // Start is called before the first frame update
    void Start()
    {
        GenerateUnits();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < allUnits.Length; i++)
        {
            allUnits[i].MoveUnit();
        }
        
    }

    //Handles the spawning of units
    private void GenerateUnits()
    {
        allUnits = new FlockUnit[flockSize];
        for (int i = 0; i < flockSize; i++)
        {
            var randomVector = UnityEngine.Random.insideUnitSphere;
            randomVector = new Vector3(randomVector.x * spawnBounds.x, randomVector.y * spawnBounds.y, randomVector.z * spawnBounds.z); //multiply the insideUnitSphere with our randomVector
            var spawnPosition = transform.position + randomVector;      //Add random vector to flock position
            var rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            allUnits[i] = Instantiate(flockUnitPrefab, spawnPosition, rotation);
            allUnits[i].AssignFlock(this);
            allUnits[i].InitializeSpeed(UnityEngine.Random.Range(minSpeed, maxSpeed));       // After assigning a unit to a flock, lets also assign it a random min and max speed
        }
    }
}
