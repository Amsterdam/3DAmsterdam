using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlaatsGebouw : MonoBehaviour
{
    private Vector3 locatie;
    private Vector3 Templocatie;

    private GameObject instance;
    private bool Waarde;


    private string DefGebouwNaam = "Gebouw";
    private string TempGebouwNaam = "TijdelijkGebouw";
    private string MapNaam = "Gebouwen";

    public float Refreshrate = 0.025f;

    public GameObject TePlaatsenObject;


    public int Nummer;

    // Use this for initialization
    void Start()
    {
        MaakParentVoorGebouwen();
        Nummer = 1;
        Waarde = true;
    }

    IEnumerator PlaatsobjectD()
    {
         
        for (; ; )
        {
            Verplaatsobject();
         
      
            yield return new WaitForSeconds(Refreshrate);
        }
    }

    void Update()
    {
        
        if (Input.GetMouseButtonDown(0))

        if (Waarde == true)
            {
                Tempplaatsobject();
                
                StartCoroutine("PlaatsobjectD");

                Waarde = false;
            }
            else
            {
                if(!(GameObject.Find(DefGebouwNaam + Nummer) == null))
                {
                    Nummer++;
                }
                else
                {
                    Nummer++;
                }

                instance.name = DefGebouwNaam + Nummer;             
                GameObject Parent = GameObject.Find(MapNaam);
                instance.transform.parent = Parent.transform;
                instance.layer = 11;
                //instance.AddComponent<MeshCollider>();
                //instance.AddComponent<ActivateWhenClick>();

                this.gameObject.SetActive(false);

                Waarde = true;
            }

    }

    void Tempplaatsobject()
    {
        Raycast();
        {
            Verwijderobject();
            
            instance = Instantiate(TePlaatsenObject, transform.position, transform.rotation);

            //instance = PrefabUtility.InstantiatePrefab(TePlaatsenObject as GameObject) as GameObject;

            instance.transform.position = locatie;
                
            instance.name = TempGebouwNaam;
            instance.layer = 2;        
        }
    }

    void MaakParentVoorGebouwen()
    {
        if (GameObject.Find(MapNaam) == null)
        {
            GameObject Map = new GameObject(MapNaam);
        }
    }
    void Raycast()
    {
        int layermask = 1 << 9;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10000,layermask))
        {
            locatie = hit.point;
            Templocatie = hit.point;
        }
    }

    void Verplaatsobject()
    {
        Raycast();
        instance.transform.position = Templocatie;
    }


    void Verwijderobject()
        {
        GameObject VerwijderTempGebouw = GameObject.Find(TempGebouwNaam);
        Destroy(VerwijderTempGebouw);
        }
    }


 