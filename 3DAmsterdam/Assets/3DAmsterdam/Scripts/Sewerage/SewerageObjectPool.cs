using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3D.Sewerage
{
    public class SewerageObjectPool : MonoBehaviour
    {
        [SerializeField]
        private GameObject prefab;
        private List<GameObject> pooledObjects;
        private Queue<GameObject> objectQueue;

        [SerializeField]
        private int amountToPool;
        // Start is called before the first frame update
        void Start()
        {

            objectQueue = new Queue<GameObject>();
            pooledObjects = new List<GameObject>();
            GameObject tmp;
            for (int i = 0; i < amountToPool; i++)
            {
                tmp = Instantiate(prefab, transform);
                tmp.SetActive(false);
                // pooledObjects.Add(tmp);
                objectQueue.Enqueue(tmp);
            }
        }



        public GameObject GetPooledObject()
        {
            if (objectQueue.Count == 0)
            {
                GameObject tmp;
                tmp = Instantiate(prefab, transform);
                tmp.SetActive(false);
                // pooledObjects.Add(tmp);
                amountToPool++;
                return tmp;
            }
            return objectQueue.Dequeue();


        }
        public void ReturnObject(GameObject returnedObject)
        {
            returnedObject.transform.parent = transform;
            returnedObject.transform.position = prefab.transform.position;
            returnedObject.transform.localScale = prefab.transform.localScale;
            returnedObject.transform.rotation = prefab.transform.rotation;
            returnedObject.SetActive(false);
            objectQueue.Enqueue(returnedObject);
        }
    }
}
