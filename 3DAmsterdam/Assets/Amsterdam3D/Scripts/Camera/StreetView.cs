using UnityEngine;

namespace Amsterdam3D.CameraMotion
{
    public class StreetView : MonoBehaviour
    {
        private bool moveToStreet = false;
        private bool canClick = false;
        private bool instanCam = false;
        public GameObject streetcamButton;
        public GameObject GodviewButton;

        private Vector3 endPos;
        private Vector3 mousePos;

        public GameObject FPSCam, tileManager;
        public Camera cam;
        public CameraControls camcontroller;

        private float stopHeight = 60f;
        private float lerpSpeed = 3f;
        private Quaternion Endrotation;

        //private void Start()
        //{
        //    tileScript = tileManager.GetComponent<TileLoader>();
        //}

        //private void Update()
        //{
        //    if (tileScript.pendingQueue.Count != 0)
        //    {
        //        FPSCam.transform.position = Vector3.zero;
        //    }
        //}

        private void LateUpdate()
        {
            // als de linkermuis knop ergens op het scherm wordt ingedrukt vlieg je daarheen
            if (Input.GetMouseButtonDown(0) && canClick)
            {
                RaycastHit hit;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                // raycast wordt afgevuurd naar de positie van de muis. als er iets wordt gedecteerd wordt dat opgeslagen in een variabel.
                if (Physics.Raycast(ray, out hit))
                {
                    mousePos = hit.point;
                }

                // de stoppositie is waar geklikt is met een bepaalde afstand van de grond (y-axis)
                endPos = new Vector3(mousePos.x, mousePos.y + 1.8f, mousePos.z);
                Endrotation = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0);
                canClick = false;
                moveToStreet = true;
                streetcamButton.SetActive(false);
                GodviewButton.SetActive(true);
            }

            if (moveToStreet)
            {
                // camera beweegt langzaam naar aangewezen plaats
                cam.transform.position = Vector3.Lerp(cam.transform.position, endPos, lerpSpeed * Time.deltaTime);
                cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, Endrotation, lerpSpeed * Time.deltaTime);
            }

            // camera stopt met bewegen als die is aangekomen op locatie
            if ((cam.transform.position.y <= (stopHeight + 0.01f)) && instanCam)
            {
                moveToStreet = false;
                instanCam = false;
            }
        }

        // de knop wordt ingedrukt dus er kan nu een plek geselecteerd worden om heen te vliegen
        public void StreetCam()
        {

            canClick = true;
            instanCam = true;
        }
    }
}