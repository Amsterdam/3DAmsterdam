using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisablePeople : MonoBehaviour
{
    public GameObject[] people;
    int switcher;

    void Start()
    {
        people = GameObject.FindGameObjectsWithTag("Person");
        switcher = 1;
    }

    public void PeopleDisable()
    {
        if (switcher == 1)
        {
            foreach (GameObject person in people)
            {
                person.active = false;
            }

            switcher *= -1;

        }
        else if (switcher == -1)
        {
            foreach (GameObject person in people)
            {
                person.active = true;

            }
            switcher *= -1;
        }



        Debug.Log(switcher);
    }
}
