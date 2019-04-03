using UnityEngine.AI;
using UnityEngine;

public class NavPathFollow : MonoBehaviour
{

    public Transform[] positions; //An Array to save all the nodes in
    public NavMeshAgent agent; //Reference to the NavMeshAgent
    public int currentPos; //an int variable to navigate through the array

    float distance; //a distance variable to calculate distance between two points

    // Update is called once per frame
    void Update()
    {
        FollowRoute();
    }

    public virtual void FollowRoute()
    {
        //Calculate the distance between the AI and his current selected point in his path
        distance = Vector3.Distance(agent.transform.position, positions[currentPos].position);

        //If the last node in the array is selected, select the first one again to create a cycle.
        if (currentPos >= positions.Length - 1)
        {
            currentPos = 0;
        }

        //If the distance is not smaller than three floats
        if (!(distance <= 2f))
        {
            agent.SetDestination(positions[currentPos].transform.position); //Move towards the node
        }
        else
        {
            currentPos = (currentPos + 1) % positions.Length; //else select the next point in the cycle
        }
    }
}
