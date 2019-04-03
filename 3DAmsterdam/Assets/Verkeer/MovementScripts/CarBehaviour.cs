using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CarBehaviour : MonoBehaviour
{
    public NavMeshAgent agent; //Reference to the navMeshAgent

    public float maxSpeed; //Variable for the maximum speed a car can go

    public float maxAngle; //Variable for the maximum angle of the Field of View (further explanation below)
    public float maxRadius; //Variable for the maxRadius of the circle
    public float priorityRadius;
    public float maxNodeAngle; //Angle for the collision course
    public Vector3 nodeForward; //Line directly towards the node currently selected

    private bool isInFoV; //A boolean that states if a person is in the Field of View of the car
    private bool lightStop = false; //A boolean that turns true or false whether the light is green or red

    private bool priority = false;

    public float carStopOffset = 2f; //offset that gets substracted off the maxRadius in cardetection, because I want cars to be able to get closer to one another

    GameObject[] overlappingPeople; //Array that holds all people within the boundaries of the maxRadius
    GameObject[] overlappingCars; //Array that holds all cars within the boundaries of the maxRadius

    NavPathFollow navPath;

    // Start is called before the first frame update
    void Start()
    {
      
        agent = GetComponent<NavMeshAgent>(); //Initialization of the NavMeshAgent
        navPath = GetComponent<NavPathFollow>();

        overlappingPeople = GameObject.FindGameObjectsWithTag("Person"); //All objects with this tag are saved in this array
        overlappingCars = GameObject.FindGameObjectsWithTag("Car"); //All objects with this tag are saved in this array
       
    }

    public void FixedUpdate()
    {

        agent.speed = maxSpeed; //the speed of the Agent is equivalent of the maxSpeed variable

        //Visualize 3 rays to calculate the collision course
        Vector3 nodeRay1 = Quaternion.AngleAxis((maxNodeAngle), transform.up) * (navPath.positions[navPath.currentPos].position - agent.transform.position);
        Vector3 nodeRay2 = Quaternion.AngleAxis((-maxNodeAngle), transform.up) * (navPath.positions[navPath.currentPos].position - agent.transform.position);
        nodeForward = (navPath.positions[navPath.currentPos].position - agent.transform.position);

        nodeRay1.y *= 0;
        nodeRay2.y *= 0;

        //Debug.DrawRay(agent.transform.position, nodeRay1, Color.cyan);
        //Debug.DrawRay(agent.transform.position, nodeRay2, Color.cyan);
        //Debug.DrawRay(agent.transform.position, nodeForward, Color.magenta);

        PriorityIntersection();


        if (lightStop) //If the traffic light is red
        {
            agent.speed = 0; //Stop the car
            lightStop = false; //turn off the boolean
        }

        for (int i = 0; i < overlappingPeople.Length; i++) //For statement to be able to set all people in the array as target for the constructorvalue thats necessary
        {
            isInFoV = inFoV(agent.transform, overlappingPeople[i], maxAngle, maxRadius); //Set the boolean equal to the boolean of the method
        }

        for (int i = 0; i < overlappingCars.Length; i++) //For statement to be able to set all cars in the array as target for the constructorvalue thats necessary
        {
            isInFoV = inFoV(agent.transform, overlappingCars[i], maxAngle, maxRadius); //Set the boolean equal to the boolean of the method
        }
    }

    private bool inFoV(Transform car, GameObject target, float maxAngle, float maxRadius)
    {
        foreach (GameObject t in overlappingPeople) //For each person in the array
        {
            //Calculate the distance
            float distanceP = Vector3.Distance(t.transform.position, car.transform.position);

            //If the distance between the person and the car is less than the maxRadius (so if they find themselves within the sphere)
            if (distanceP <= maxRadius)
            {
                Vector3 targetDirP = (t.transform.position - car.position).normalized; //Calculate the direction to that person
                targetDirP.y *= 0; //Mulitply the Y value with 0, because we're not using that axis and it can change the angle if we do use it anyways

                float angle = Vector3.Angle(car.forward, targetDirP); //Calculate the angle between the front of the car and the direction towards that person
                float nodeAngle = Vector3.Angle(nodeForward, targetDirP); //Calculate the angle between the node direction and the direction towards that person

                if (angle <= maxAngle) //If the angle is smaller than the Field of View area
                {
                    if (nodeAngle <= maxNodeAngle) //And the person is on a collision course with the car
                    {
                        Ray ray = new Ray(car.position, t.transform.position - car.position); //Make a ray towards that person
                        //Debug.DrawRay(car.position, t.transform.position - car.position, Color.red);

                        RaycastHit hit; //Create a RayCastHit to store data in about the hitobject

                        if (Physics.Raycast(ray, out hit, maxRadius)) //If the ray hits something
                        {
                            if (hit.transform == t.transform) //and the object is the person
                            {
                                agent.speed = 0f; //stop the car
                                return true; //and return true
                            }
                        }
                    }
                }
            }
        }
            foreach (GameObject c in overlappingCars) //And for each car in the array
            {
                float distanceC = Vector3.Distance(c.transform.position, car.transform.position);

                //All the code below is the same as the code above, but for cars, so I think no further explanation is needed for this part
                if (distanceC <= maxRadius + carStopOffset)
                {
                    Vector3 targetDirC = (c.transform.position - car.position).normalized;
                    targetDirC.y *= 0;

                    float angle = Vector3.Angle(car.forward, targetDirC);
                    float nodeAngle = Vector3.Angle(nodeForward, targetDirC);

                if (angle <= maxAngle)
                {
                    if (nodeAngle <= maxNodeAngle)
                    {
                        Ray ray = new Ray(car.position, c.transform.position - car.position);
                        RaycastHit hit;

                        if (Physics.Raycast(ray, out hit, maxRadius + carStopOffset))
                        {
                            if (hit.transform == c.transform)
                            {
                                agent.speed = 0;
                                return true;
                            }
                        }
                    }
                }
                }
            }
        
        return false;
    }

    public void PriorityIntersection()
    {
        overlappingCars = GameObject.FindGameObjectsWithTag("Car"); //All objects with this tag are saved in this array

        if (priority)
        {
            foreach (GameObject c in overlappingCars)
            {
                float distanceC = Vector3.Distance(c.transform.position, transform.position);

                if (distanceC <= priorityRadius)
                {
                    Vector3 targetDirC = (c.transform.position - transform.position).normalized;
                    targetDirC.y *= 0;

                    Debug.DrawRay(transform.position, c.transform.position - transform.position, Color.red);

                    Vector3 angle = transform.InverseTransformPoint(c.transform.position);

                    if (angle.x > 0f)
                    {
                        agent.speed = 0;

                    }
                    else if (angle.x < 0f)
                    {
                        agent.speed = maxSpeed;
                        priority = false;
                    }
                }
            }
        }
    }

    //If the car collides with the hitbox of the traffic light and thus is red
    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.tag == "Traffic Light")
        {
            lightStop = true; //Lightstop is true
        }
    }
    
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Intersection")
        {
            priority = true; //Priority is true
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Intersection")
        {
            priority = false;
        }
    }

    //Method to visualize lines and area's in the sceneview
    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(agent.transform.position, priorityRadius); //Visualize the priorityRadius

        ////Visualize the boundaries of the field of view
        //Vector3 line1 = Quaternion.AngleAxis(maxAngle, transform.up) * transform.forward * maxRadius;
        //Vector3 line2 = Quaternion.AngleAxis(-maxAngle, transform.up) * transform.forward * maxRadius;

        //Gizmos.color = Color.blue;
        //Gizmos.DrawRay(agent.transform.position, line1);
        //Gizmos.DrawRay(agent.transform.position, line2);

        ////Visualize the lookdirection
        //Gizmos.color = Color.black;
        //Gizmos.DrawRay(agent.transform.position, transform.forward * maxRadius);
    }
}
