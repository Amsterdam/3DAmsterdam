using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueFamily : Families
{
    public float personRadius;

    public override void Start()
    {
        base.Start();

        agent.speed = 9f; 
        personRadius = 4f;
    }

    public override void Update()
    {
        base.Update();

        ScareFamilies();
        SlowDown();
    }

    public void ScareFamilies()
    {
        //Make two lists that contain everyone from the red and green family
        Families[] red; 
        Families[] green;

        red = GameObject.FindObjectsOfType<RedFamily>();
        green = GameObject.FindObjectsOfType<GreenFamily>();

        foreach (Families i in red) //Foreach red person
        {
            foreach (Families j in green) //And foreach green person
            {
                //Calculate the distance
                float distanceToI = Vector3.Distance(transform.position, i.transform.position);
                float distanceToJ = Vector3.Distance(transform.position, j.transform.position);

                //If the distance is smaller than the personradius 
                if (distanceToI <= personRadius)
                {
                    textBox.enabled = true; //Enable the image

                    Vector3 directionToI = (transform.position - i.transform.position).normalized; //Calculate the direction and normalize it zo we can do math with it
                    Quaternion rotationI = Quaternion.LookRotation(-transform.position + i.transform.position); //Save the rotation away from the agent
                    i.transform.rotation = Quaternion.Slerp(i.transform.rotation, rotationI, 2 * Time.deltaTime); //Apply this rotation to the red person
                    i.transform.position = Vector3.Lerp(i.transform.position, i.transform.position -= directionToI, i.agent.speed * Time.deltaTime); //And let the person move in the opposite direction from me
                }

                //Same applies to people from the green family
                if(distanceToJ <= personRadius)
                {
                    textBox.enabled = true;

                    Vector3 directionToJ = (transform.position - j.transform.position).normalized;
                    Quaternion rotationJ = Quaternion.LookRotation(-transform.position + j.transform.position);
                    j.transform.rotation = Quaternion.Slerp(j.transform.rotation, rotationJ, 2 * Time.deltaTime);
                    j.transform.position = Vector3.Lerp(j.transform.position, j.transform.position -= directionToJ, j.agent.speed * Time.deltaTime);
                }
            }
        }
    }

    public void SlowDown()
    {

        //Make an array that contains all cars in the scene
        GameObject[] cars;

        cars = GameObject.FindGameObjectsWithTag("Car");

        //For each car
        foreach(GameObject car in cars)
        {
            float carDistance = Vector3.Distance(car.transform.position, this.transform.position); //Calculate the distance

            if (carDistance <= 10f) //If the distance is smaller than 10 floats
            {
                agent.speed = 5; //Reduce the speed
            }
            else agent.speed = 9f; //Else return to normal
        }
    }
}
