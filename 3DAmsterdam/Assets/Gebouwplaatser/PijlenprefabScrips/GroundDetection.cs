//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using UnityEngine.UI;

//public class GroundDetection : MonoBehaviour
//{
//    GameObject ground;
//    private float distanceCheck = 1f;
//    Renderer renderer;

//    bool isHit = false;

//    public Button detectButton;

//    void Start()
//    {
//        renderer = GetComponent<Renderer>();
//    }

//    void Update()
//    {
//        RaycastHit hit;
//        if (Physics.Raycast(transform.position, Vector3.down, out hit, distanceCheck))
//        {
//            isHit = true;
//        }

//        if (isHit) {
//            FreezeScreen();
//            //isHit = false;
//        }

//        //Debug.Log(isHit);
//    }

//    void FreezeScreen()
//    {
//        detectButton.gameObject.SetActive(true);
//        Time.timeScale = 0;
//        //EditorUtility.DisplayDialog("Title", "Content", "Ok", "Cancel");
//    }

//    //private void OnCollisionEnter(Collision collision)
//    //{
//    //    Debug.Log(collision.collider.gameObject.name);
//    //}
//}
