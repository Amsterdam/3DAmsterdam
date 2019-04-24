using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SchaalBestuurMenu : MonoBehaviour
{
    public Text TekstObjectID;
    private GameObject VervormObject;

    public int test;

    public GameObject Gebouwplaatser1;

    [HideInInspector]
    public int MenuLengte = 25, MenuBreedte = 25, MenuHoogte = 25, MenuRotatie = 0;
    public Slider SLengte, SBreedte, SHoogte, SRotatie;

    public bool PijlenAanUit = false;

    public string Objectnaam;

    private void Start()
    {
        //MenuLengte = 25;
        //MenuBreedte = 25;
        //MenuHoogte = 25;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitMenu;
            int temp = 1 << 11;
            int layermask = ~(temp);
            var ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);

            Physics.Raycast(ray2, out hitMenu);

            if (hitMenu.collider.transform.parent.gameObject.layer == 11)
            {


                Objectnaam = hitMenu.collider.transform.parent.name;
                string SetSlider = hitMenu.collider.transform.parent.name;

                //TekstObjectID.text = SetSlider;

                VervormObject = hitMenu.collider.transform.parent.gameObject;

                int tempLengte = VervormObject.GetComponent<Schaal>().Lengte;
                MenuLengte = tempLengte;
                SLengte.value = tempLengte;

                int tempBreedte = VervormObject.GetComponent<Schaal>().Breedte;
                //TekstObjectID.text = Objectnaam;
                MenuBreedte = tempBreedte;
                SBreedte.value = tempBreedte;

                int tempHoogte = VervormObject.GetComponent<Schaal>().Hoogte;
                MenuHoogte = tempHoogte;
                SHoogte.value = tempHoogte;

                int tempRotatie = VervormObject.GetComponent<Schaal>().RotatieY;
                MenuRotatie = tempRotatie;
                SRotatie.value = tempRotatie;

                Objectnaam = SetSlider;

            }
            else
            {

            }
        }
    }

    public void Lengte(float Lengte)
    {
        MenuLengte = (int)(Lengte * 50);
    }

    public void Breedte(float Breedte)
    {
        MenuBreedte = (int)(Breedte * 50);
    }

    public void Hoogte(float Hoogte)
    {
        MenuHoogte = (int)(Hoogte * 50);
    }

    public void Rotatie(float Rotatie)
    {
        MenuRotatie = (int)(Rotatie * 360);
    }

    public void PlaatsGebouw()
    {
        Gebouwplaatser1.SetActive(true);

        string Objectnaam = "TijdelijkGebouw";
        TekstObjectID.text = Objectnaam;

        VervormObject = GameObject.Find(Objectnaam);
    }

    public void PijlenAan()
    {
        PijlenAanUit = !PijlenAanUit;
    }

    public void ZomaarEenKnop()
    {
        VervormObject.GetComponent<Schaal>().Lengte = 250;
    }

}
