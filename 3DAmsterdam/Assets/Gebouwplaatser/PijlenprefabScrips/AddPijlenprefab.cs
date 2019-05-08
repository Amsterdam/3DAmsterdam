using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddPijlenprefab : MonoBehaviour {

    //private GameObject pijlenprefab;
    //private bool klik = false;
    //private Renderer renderer;
    //public GameObject Pijlenprefab;

    //private float arrowPositioningY = 4f;
    //private float arrowScaling = 2f;

    //private void OnMouseDown()
    //{
    //    GameObject oldprefab = GameObject.Find("PijlenPrefab(Clone)");
    //    renderer = gameObject.GetComponent<Renderer>();

    //    Vector3 positionUnderObject = new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z);
    //    float distanceToObject;
    //    float scalingX, scalingY, scalingZ;

    //    Destroy(oldprefab);

    //    if (klik == false)
    //    {
    //        //pijlenprefab = Instantiate(Resources.Load("Pijlenprefab/PijlenPrefab", typeof(GameObject))) as GameObject;
    //        pijlenprefab = Instantiate<GameObject>(Pijlenprefab, transform.position, transform.rotation);

    //        pijlenprefab.transform.parent = gameObject.transform;

    //        // de afstand tussen het object en de grond wordt berekend.
    //        distanceToObject = Vector3.Distance(positionUnderObject, gameObject.transform.position);

    //        // de positie van de pijlen worden op de juiste positie neergezet. Deze afstand is de afstand tot het object
    //        // plus de 1/4de van het object zelf.
    //        pijlenprefab.transform.position = new Vector3(renderer.bounds.center.x, distanceToObject + (renderer.bounds.size.y
    //                                                     / arrowPositioningY), renderer.bounds.center.z);

    //        // de juiste scaling factoren worden berekend voor x, y en z.
    //        scalingX = renderer.bounds.size.x / (gameObject.transform.localScale.x / 1f);
    //        scalingY = renderer.bounds.size.y / (gameObject.transform.localScale.y / 1f);
    //        scalingZ = renderer.bounds.size.z / (gameObject.transform.localScale.z / 1f);

    //        // de scale van de pijlen wordt aangepast met een scaling factor.
    //        pijlenprefab.transform.localScale = new Vector3(scalingX, scalingY, scalingZ) * arrowScaling;

    //        klik = true;
    //    }
    //    else
    //    {
    //        Destroy(pijlenprefab);
    //        klik = false;
    //    }
    //}
  

    //void Start () {
    //    klik = false;
    //}
}
