using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Schaal : MonoBehaviour
{
    public float Refreshrate = 0.025f;
    public int Lengte, Breedte, Hoogte, RotatieY;
    public float Schaalfactor = 4;
    public GameObject PijlenPrefab;

    private GameObject AddPrefab;

    [HideInInspector]
    public GameObject MenuVerschaler;

    private void Start()
    {
        MenuVerschaler = GameObject.Find("MenuLengteBreedteHoogte");
        StartCoroutine(Menucheck());
        
    }

    IEnumerator Menucheck()
    {       
        for (; ; )
        {
            if (MenuVerschaler.GetComponent<SchaalBestuurMenu>().Objectnaam == gameObject.name)
            {
                gameObject.GetComponentInChildren<Renderer>().material.SetColor("_Color", Color.red);

                LengteBreedteHoogteRotatie();

                if (MenuVerschaler.GetComponent<SchaalBestuurMenu>().PijlenAanUit == true)
                {
                    if (GameObject.Find("PijlenPrefab(Clone)") == null)
                    {
                        AddPrefab = Instantiate(PijlenPrefab);
                        AddPrefab.transform.SetParent(gameObject.transform);
                        AddPrefab.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 10, gameObject.transform.position.z);
                        AddPrefab.transform.localScale = new Vector3(Schaalfactor,Schaalfactor,Schaalfactor);
                        gameObject.AddComponent<MeshCollider>();
                    }
                    else if (GameObject.Find("PijlenPrefab(Clone)") != null)
                    {

                    }
                }
            }

            else
            {
                gameObject.GetComponentInChildren<Renderer>().material.SetColor("_Color", Color.white);
                Destroy(AddPrefab);
            }
            yield return new WaitForSeconds(Refreshrate);
        }
    }

    void LengteBreedteHoogteRotatie()
    {
        Lengte = MenuVerschaler.GetComponent<SchaalBestuurMenu>().MenuLengte;
        Breedte = MenuVerschaler.GetComponent<SchaalBestuurMenu>().MenuBreedte;
        Hoogte = MenuVerschaler.GetComponent<SchaalBestuurMenu>().MenuHoogte;

        RotatieY = MenuVerschaler.GetComponent<SchaalBestuurMenu>().MenuRotatie;

    }

    void Update()
    {
        Verschalen();
        Rotaties();
    }

    public void Verschalen()
    {
        gameObject.transform.localScale = new Vector3(Lengte,Hoogte,Breedte);
    }

    public void Rotaties()
    {
        if (GameObject.Find("PijlenPrefab(Clone)") == null)
        {
            gameObject.transform.localEulerAngles = new Vector3(0, RotatieY, 0);
        }
    }
    
}
