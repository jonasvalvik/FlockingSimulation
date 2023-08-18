using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is for the flocking behaviors
public class FlockUnit : MonoBehaviour
{
    // Units should not detect other units behind them, so this variable is for the FOV
    [SerializeField] private float FOVAngle;
    [SerializeField] private float smoothDamp;      // The lower this value is, the closer to the move vector we can get in a frame - therefore it will rotate faster.
    [SerializeField] private LayerMask obstacleMask;        // Only detect obstacles for certain layers
    [SerializeField] private Vector3[] directionsToCheckWhenAvoidingObstacles;      // Array of directions for obstacle detection
    // List of cohesion neighbours
    private List<FlockUnit> cohesionNeighbours = new List<FlockUnit>();
    private List<FlockUnit> avoidanceNeighbours = new List<FlockUnit>();
    private List<FlockUnit> alignmentNeighbours = new List<FlockUnit>();
    private Flock assignedFlock; // Reference to the Flock component that spawned this unit
    private Vector3 currentVelocity;
    private Vector3 currentObstacleAvoidanceVector;
    private float speed;

    // Store transform component as a property because we will use it a lot. Faster to use.
    public Transform myTransform { get; set; }  

    private void Awake()
    {
        myTransform = transform;
    }
    // Passed the flock object in the parameter and assigns it to the flock field.
    public void AssignFlock(Flock flock)
    {
        assignedFlock = flock;
    }
    // Initializing the units speed by simply passing it in the parameter
    public void InitializeSpeed(float speed) 
    {
        this.speed = speed;
    }
    //      We multiply each vector by its weights. All behavior calculated method return normalized vectors
    public void MoveUnit() 
    {
        FindNeighbours();
        CalculateSpeed();
        
        var cohesionVector = CalculateCohesionVector() * assignedFlock.cohesionWeight;
        var avoidanceVector = CalculateAvoidanceVector() * assignedFlock.avoidanceWeight;
        var alignmentVector = CalculateAlignmentVector() * assignedFlock.alignmentWeight;
        var boundsVector = CalculateBoundsVector() * assignedFlock.boundsWeight;
        var obstacleVector = CalculateObstacleVector() * assignedFlock.obstacleWeight;

        var moveVector = cohesionVector + avoidanceVector + alignmentVector + boundsVector + obstacleVector;        // Sum of all distance vectors
        moveVector = Vector3.SmoothDamp(myTransform.forward, moveVector, ref currentVelocity, smoothDamp);        // Gradually changes a vector towards a desired goal over time. The vector is smoothed by some spring-damper like function, which will never overshoot.The most common use is for smoothing a follow camera.
        moveVector = moveVector.normalized * speed;
        if (moveVector == Vector3.zero)     // Avoid fish standing still when dividing by zero
        {
            moveVector = transform.forward;
        }
        myTransform.forward = moveVector;
        myTransform.position += moveVector * Time.deltaTime;
    }









    // Before any calculations we need to find the neighbouring units. Will invoke it first in the MoveUnits method.
    private void FindNeighbours()
    {
        cohesionNeighbours.Clear();
        avoidanceNeighbours.Clear();
        alignmentNeighbours.Clear();
        var allUnits = assignedFlock.allUnits; // We will have to iterate through all units that are part of the same flock

        // Iterate through all units in the same flock
        for (int i = 0; i < allUnits.Length; i++)
        {
            var currentUnit = allUnits[i];   // Store current unit in a variable
            if (currentUnit != this)
            {
                float currentNeighbourDistanceSqr = Vector3.SqrMagnitude(currentUnit.myTransform.position - myTransform.position);    // Calculate distance between ourselves and the current unit. Squaredistance is a faster method, calculating distance requires a square root calculations
                if (currentNeighbourDistanceSqr <= assignedFlock.cohesianDistance * assignedFlock.cohesianDistance) // Check if calculated distance is shorter than cohesion distance
                {
                    cohesionNeighbours.Add(currentUnit);    // If distance is shorter, then add current unit to cohesion neighbours.
                }
                if (currentNeighbourDistanceSqr <= assignedFlock.avoidanceDistance * assignedFlock.avoidanceDistance) // Check if calculated distance is shorter than avoidance distance
                {
                    avoidanceNeighbours.Add(currentUnit);    // If distance is shorter, then add current unit to avoidance neighbours.
                }
                if (currentNeighbourDistanceSqr <= assignedFlock.alignmentDistance * assignedFlock.alignmentDistance) // Check if calculated distance is shorter than alignment distance
                {
                    alignmentNeighbours.Add(currentUnit);    // If distance is shorter, then add current unit to alignment neighbours.
                }
            }
        }
    }

    // Calculate speed based on neighbours. Set speed value as an average of all neighbouring units
    private void CalculateSpeed()
    {
        if (cohesionNeighbours.Count == 0)
        {
            return;
        }
        speed = 0;
        for (int i = 0; i < cohesionNeighbours.Count; i++)
        {
            speed += cohesionNeighbours[i].speed;        // Add neighbour speed to our speed
        }
        speed /= cohesionNeighbours.Count;      // After iterating, divide the speed by the neighbour count
        speed = Mathf.Clamp(speed, assignedFlock.minSpeed, assignedFlock.maxSpeed);

    }


    // This method will initialize a vector 3 with a zero vector
    // Cohesion vector = average position of all units in a certain radius, when you take a collection of all neighboring units
    private Vector3 CalculateCohesionVector()
    {
        var cohesionVector = Vector3.zero;
        if (cohesionNeighbours.Count == 0)      //If no units in cohesion radius, return zero vector
        {    
            return cohesionVector;
        }
        int neighboursInPOV = 0;    // Number of units in cohesion distance AND in field of view
        for (int i = 0; i < cohesionNeighbours.Count; i++)      // Check if cohesion neighbours are in FOV
        {
            if (IsInFOV(cohesionNeighbours[i].myTransform.position)) 
            {
                neighboursInPOV++;
                cohesionVector += cohesionNeighbours[i].myTransform.position;   // Adding neighbours position to 'cohesionVector'
            }
        }

        cohesionVector /= neighboursInPOV;
        cohesionVector -= myTransform.position;     // Vector is an average world space position - we want to convert it to a local poisiton.
        cohesionVector = cohesionVector.normalized;
        return cohesionVector;
    }

    private Vector3 CalculateAlignmentVector()
    {
        var alignmentVector = myTransform.forward;     // In case of alignment, we want to return a forward vector when there are no
        if (alignmentNeighbours.Count == 0)
        {
            return alignmentVector;
        }
        int neighboursInFOV = 0;
        for (int i = 0; i < alignmentNeighbours.Count; i++)
        {
            if (IsInFOV(alignmentNeighbours[i].myTransform.position))        // If alignment neighbours are in the FOV, increment neighbours, and add that position to the alignment vector
            {
                neighboursInFOV++;
                alignmentVector += alignmentNeighbours[i].myTransform.forward;
            }
        }

        // Otherwise, divide by the number of neighbours in FOV and return it from the method.
        alignmentVector /= neighboursInFOV;
        alignmentVector = alignmentVector.normalized;
        return alignmentVector;
    }

    private Vector3 CalculateAvoidanceVector()
    {
        var avoidanceVector = Vector3.zero;     // In case of alignment, we want to initialize with a zero vector
        if (avoidanceNeighbours.Count == 0)
        {
            return Vector3.zero;
        }
        int neighboursInFOV = 0;
        for (int i = 0; i < avoidanceNeighbours.Count; i++)
        {
            if (IsInFOV(avoidanceNeighbours[i].myTransform.position))        // If alignment neighbours are in the FOV, increment neighbours, and add that position to the alignment vector
            {
                neighboursInFOV++;
                avoidanceVector += (myTransform.position - avoidanceNeighbours[i].myTransform.position);        // Instead of adding the neighbour position, We will add a vector that is opposite of the direction of that neighbour
            }
        }

        // Otherwise, divide by the number of neighbours in FOV and return it from the method.
        avoidanceVector /= neighboursInFOV;
        avoidanceVector = avoidanceVector.normalized;
        return avoidanceVector;
    }

    private Vector3 CalculateBoundsVector()
    {
        var offsetToCenter = assignedFlock.transform.position - myTransform.position;       // Calculate position between unit and center of flock
        bool isNearCenter = (offsetToCenter.magnitude >= assignedFlock.boundsDistance * 0.9);       // Bool is true when we are close to flock bounds. But first, we need to know the bounds radius. If distance to center is in 90% of the bounds distance, then we are close to bounds
        return isNearCenter ? offsetToCenter.normalized : Vector3.zero;
    }

    // Method for calculating vector, weight and a distance
    private Vector3 CalculateObstacleVector()
    {
        var obstacleVector = Vector3.zero;
        RaycastHit hit;
        if (Physics.Raycast(myTransform.position, myTransform.forward, out hit, assignedFlock.obstacleDistance, obstacleMask)) // Raycast to detects obstacles. Forward direction from the units position
        {
            obstacleVector = FindBestDirectionToAvoidObstacle();        // If we do hit obstacle, find direction to turn to
        }
        else
        {
            currentObstacleAvoidanceVector = Vector3.zero;      // Reset this direction everytime we dont have an obstacle in front.
        }
        return obstacleVector;
    }

    // Throw raycast in four different directions, then check which of them doesn't hit obstacles in the obstacleHitRange. If all hit -> pick on that has the longest distance
    private Vector3 FindBestDirectionToAvoidObstacle()
    {
        if (currentObstacleAvoidanceVector != Vector3.zero)     // Video: 24:35
        {
            RaycastHit hit;
            if (!Physics.Raycast(myTransform.position, myTransform.forward, out hit, assignedFlock.obstacleDistance, obstacleMask))
            {
                return currentObstacleAvoidanceVector;
            }
        }
        float maxDistance = int.MinValue;
        var selectedDirection = Vector3.zero;
        for (int i = 0; i < directionsToCheckWhenAvoidingObstacles.Length; i++)
        {

            RaycastHit hit;
            var currentDirection = myTransform.TransformDirection(directionsToCheckWhenAvoidingObstacles[i].normalized);        //Calculate world space direction from current direction. This way we transform the direction of the parameter in local space to the direction in world space
            if (Physics.Raycast(myTransform.position, currentDirection, out hit, assignedFlock.obstacleDistance, obstacleMask))         // Check the directions just calculated     
            {

                float currentDistance = (hit.point - myTransform.position).sqrMagnitude;
                if (currentDistance > maxDistance)      // Check if distance is longer than the current maxdistance. If so -> direction is better than previous one
                {
                    maxDistance = currentDistance;      // Assign current distance to the maxDistance variable
                    selectedDirection = currentDirection;       // Assign current direction to selected direction
                }
            }
            else
            {
                selectedDirection = currentDirection;
                currentObstacleAvoidanceVector = currentDirection.normalized;
                return selectedDirection.normalized;
            }
        }
        return selectedDirection.normalized;
    }



        private bool IsInFOV(Vector3 position)
    {
        return Vector3.Angle(myTransform.forward, position - myTransform.position) <= FOVAngle;
    }
}
