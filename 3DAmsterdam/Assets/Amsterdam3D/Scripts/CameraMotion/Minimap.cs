using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ConvertCoordinates;

namespace Amsterdam3D.Interface
{
    public class Minimap : MonoBehaviour
    {
        public GameObject kompas;

        Vector3 linksOnder, rechtsBoven, midden, startPos;
        float breedteAdam, lengteAdam, breedtePlaatje, lengtePlaatje, ratioBreedte, ratioLengte;

        private void Start()
        {
            startPos = transform.localPosition;

            linksOnder = CoordConvert.WGS84toUnity(4.727386, 52.261480); // omgerekende coordinaat (van WGS naar Unity)
            rechtsBoven = CoordConvert.WGS84toUnity(5.108260, 52.454227); // omgerekende coordinaat (van WGS naar Unity)
            midden = (linksOnder + rechtsBoven) / 2; // middenpunt van Adam in Unity coordinaten

            breedteAdam = rechtsBoven.x - linksOnder.x;
            lengteAdam = rechtsBoven.z - linksOnder.z;
            breedtePlaatje = transform.parent.GetComponent<RectTransform>().sizeDelta.x;
            lengtePlaatje = transform.parent.GetComponent<RectTransform>().sizeDelta.y;

            ratioBreedte = breedtePlaatje / breedteAdam; // verhouding tussen breedte van Adam en plaatje van de minimap
            ratioLengte = lengtePlaatje / lengteAdam;    // verhouding tussen lengte van Adam en plaatje van de minimap
        }

        void Update()
        {
            float posX = (Camera.main.transform.position.x - midden.x) * ratioBreedte; // xpos van minimaplocatie
            float posY = (Camera.main.transform.position.z - midden.z) * ratioLengte;  // ypos van minimaplocatie

            transform.localPosition = new Vector3(posX, posY, 0);

            transform.localRotation = Quaternion.Euler(kompas.transform.rotation.eulerAngles * -1);

            if (transform.localPosition.x >= (startPos.x + breedtePlaatje / 2) || transform.localPosition.x <= (startPos.x - breedtePlaatje / 2) ||
                transform.localPosition.y >= (startPos.y + lengtePlaatje / 2) || transform.localPosition.y <= (startPos.y - lengtePlaatje / 2))
            {
                GetComponent<Image>().enabled = false;
            }
            else
            {
                GetComponent<Image>().enabled = true;
            }
        }
    }
}