using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Wander : MonoBehaviour
{

    Vector3 moveSpot;

    NavMeshAgent agent;
    float distance;

    public Transform plane;
    float minX;
    float maxX;
    float minZ;
    float maxZ;

    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();

        minX = plane.transform.position.x - plane.lossyScale.x * 4;
        maxX = plane.transform.position.x + plane.lossyScale.x * 4;

        minZ = plane.transform.position.z - plane.lossyScale.z * 4;
        maxZ = plane.transform.position.z + plane.lossyScale.z * 4;

        moveSpot = new Vector3(Random.Range(minX, maxX), plane.transform.position.y, Random.Range(minZ, maxZ));
        //Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), moveSpot, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        Wandering();
    }

    public void Wandering()
    {
        distance = Vector3.Distance(agent.transform.position, moveSpot);

        if(!(distance <= 2))
        {
            agent.SetDestination(moveSpot);
        }
        else if(distance <= 2f)
        {
            moveSpot = new Vector3(Random.Range(minX, maxX), plane.transform.position.y, Random.Range(minZ, maxZ));
            //Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), moveSpot, Quaternion.identity);
        }
    }
}
