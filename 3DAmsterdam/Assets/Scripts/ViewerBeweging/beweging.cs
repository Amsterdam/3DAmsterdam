using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class beweging : MonoBehaviour
{
    public Camera cam;

    private const float zoomSpeed = 0.3f;
    private const float maxZoomOut = 100f;
    private const float maxZoomIn = 1f;
    private const float rotationSpeed = 1f;
    private const float dragSpeed = 4f;
    private const float maxRotate = 50f;
    private const float minAngle = -10f;
    private const float maxAngle = 89f;

    private float scroll;
    private float zoomDistance;

    private bool canMove = true;
    private bool canUseFunction = true;

    private Quaternion startRotation = Quaternion.Euler(90f, 0, 0);
    private Vector3 zoomPoint;
    private Vector3 zoomDirection;
    private Vector3 zoom;
    private Vector3 direction;
    private Vector3 dragOrigin;
    private Vector3 upDirection;
    private Vector3 movDir;

    void Start()
    {
        cam.transform.rotation = startRotation;

        // de juiste directie om omhoog en omlaag te bewegen (ook voor als de cam geroteerd wordt)
        upDirection = cam.transform.up;
    }

    void Update()
    {
        // checkt of de muis oven een UI element zit (zo ja, dan kunnen bepaalde functies niet gebruikt worden)
        if (EventSystem.current.IsPointerOverGameObject())
        {
            canUseFunction = false;
        }
        else
        {
            canUseFunction = true;
        }

        // als spatie wordt ingedrukt wordt de huidige actie gestopt
        if (!(Input.GetKey(KeyCode.Space)))
        {
            StandardMovement();
            Rotation();

            if (canUseFunction)
            {
                Zooming();
                Dragging();
                FocusPoint();
            }
        }
    }

    void StandardMovement()
    {
        if (canMove)
        {
            // de directie wordt gelijk gezet aan de juiste directie plus hoeveel de camera gedraaid is
            movDir = upDirection;
            movDir = Quaternion.AngleAxis(cam.transform.eulerAngles.y, Vector3.up) * movDir;

            // vooruit/achteruit bewegen (gebaseerd op rotatie van camera)
            if (Input.GetKey(KeyCode.UpArrow)) cam.transform.position += movDir;
            if (Input.GetKey(KeyCode.DownArrow)) cam.transform.position -= movDir;

            // zijwaarts bewegen (gebaseerd op rotatie van camera)
            if (Input.GetKey(KeyCode.LeftArrow)) cam.transform.position -= cam.transform.right;    
            if (Input.GetKey(KeyCode.RightArrow)) cam.transform.position += cam.transform.right;     
        }
    }

    void Rotation()
    {
        // rotatie met control knop
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            canMove = false;

            // roteren naar links/rechts met control knop
            if (Input.GetKey(KeyCode.LeftArrow)) cam.transform.RotateAround(cam.transform.position, Vector3.up, -rotationSpeed);
            if (Input.GetKey(KeyCode.RightArrow)) cam.transform.RotateAround(cam.transform.position, Vector3.up, rotationSpeed);

            // de camera kan niet verder geroteerd worden dan de min en max angle
            cam.transform.rotation = Quaternion.Euler(new Vector3(ClampAngle(cam.transform.eulerAngles.x, minAngle, maxAngle), 
                                                                             cam.transform.eulerAngles.y, cam.transform.eulerAngles.z));

            // roteren omhoog/omlaag
            if (Input.GetKey(KeyCode.UpArrow)) cam.transform.RotateAround(cam.transform.position, cam.transform.right, -rotationSpeed);
            if (Input.GetKey(KeyCode.DownArrow)) cam.transform.RotateAround(cam.transform.position, cam.transform.right, rotationSpeed);
        }

        // rotatie met shift knop
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            canMove = false;

            // roteren naar links/recht met shift knop.
            if (Input.GetKey(KeyCode.LeftArrow)) cam.transform.RotateAround(cam.transform.position, Vector3.up, rotationSpeed);
            if (Input.GetKey(KeyCode.RightArrow)) cam.transform.RotateAround(cam.transform.position, Vector3.up, -rotationSpeed);
        }

        // bewegingsfunctionaliteit wordt weer actief als control/shift losgelaten wordt.
        if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl) ||
            Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
        {
            canMove = true;
        }
    }

    void Zooming()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        scroll = Input.GetAxis("Mouse ScrollWheel");
        zoomPoint = cam.transform.position;

        Vector3 maxZoomPosition = new Vector3(cam.transform.position.x, maxZoomOut, cam.transform.position.z);
        Vector3 minZoomPosition = new Vector3(cam.transform.position.x, maxZoomIn, cam.transform.position.z);

        // raycast wordt afgevuurd naar de positie van de muis. als er iets wordt gedecteerd wordt dat opgeslagen in een variabel.
        if (Physics.Raycast(ray, out hit))
        {
            zoomPoint = hit.point;
        }

        // de afstand tussen de camera en het zoompunt wordt berekend.
        zoomDistance = Vector3.Distance(zoomPoint, cam.transform.position);

        // de richting waar in gezoomd moet worden wordt berekend.
        zoomDirection = Vector3.Normalize(zoomPoint - cam.transform.position);

        zoom = zoomDirection * zoomDistance * scroll * zoomSpeed;

        // er kan niet verder worden uitgezoomd dan de maximale range.
        if (cam.transform.position.y > maxZoomOut)
        {
            cam.transform.position = maxZoomPosition;
        }
        // er kan niet verder worden ingezoomd dan de minimale range.
        else if (cam.transform.position.y < maxZoomIn)
        {
            cam.transform.position = minZoomPosition;
        }
        else
        {
            // als de maximale uitzoom range bereikt is kan er alleen ingezoomd worden.
            if (cam.transform.position.y == maxZoomOut)
            {
                if (scroll > 0) cam.transform.position += zoom;
            }
            // als de maximale inzoom range bereikt is kan er alleen uitgezoomd worden.
            else if (cam.transform.position.y == maxZoomIn)
            {
                if (scroll < 0) cam.transform.position += zoom;
            }
            // de positie van de camera wordt aangepast.
            else
            {
                cam.transform.position += zoom;
            }
        }
    }

    void Dragging()
    {
        if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            // het punt vanaf waar gesleept wordt, wordt opgeslagen als de muis ingedrukt wordt
            if (Input.GetMouseButtonDown(0)) dragOrigin = Input.mousePosition;

            // als de muis niet ingedrukt wordt, wordt de methode verlaten
            if (!Input.GetMouseButton(0)) return;

            // de positie waar de muis heen beweegt wordt bijgehouden
            Vector2 updatedPos = cam.ScreenToViewportPoint(Input.mousePosition - dragOrigin);

            // de bewegingsfactor voor de camera wordt berekent
            Vector3 camMove = new Vector3(updatedPos.x * dragSpeed, 0, updatedPos.y * dragSpeed);

            // de bewegingsfactor verandert gebaseerd op hoeveel de camera gedraaid is
            camMove = Quaternion.AngleAxis(cam.transform.eulerAngles.y, Vector3.up) * camMove;

            // de bewegingfactor wordt van de positie afgetrokken zodat de camera de andere kant op beweegt
            cam.transform.position -= camMove;
        }
    }

    void FocusPoint()
    {
        Vector3 rotatePoint = new Vector3();

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetMouseButtonDown(0)) rotatePoint = Camera.main.ScreenToViewportPoint(Input.mousePosition);

            if (Input.GetMouseButton(0))
            {
                cam.transform.RotateAround(rotatePoint, Vector3.up, 1f);
                //transform.Rotate(Vector3(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * Time.deltaTime * speed);
            }
            //if (!Input.GetMouseButton(0)) return;

        }
    }

    // buggy functie
    private float ClampAngle(float angle, float from, float to)
    {
        if (angle < 0f) angle = 360 + angle;
        if (angle > 180f) return Mathf.Max(angle, 360 + from);
        return Mathf.Min(angle, to);
    }
}
