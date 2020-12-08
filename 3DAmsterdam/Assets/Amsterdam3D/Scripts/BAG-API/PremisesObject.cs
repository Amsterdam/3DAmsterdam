using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
    public class PremisesObject : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button closeButton = default;
        [SerializeField] private Scrollbar scroll = default;
        [SerializeField] private Toggle streetName = default;
        [SerializeField] private Text streetText = default;

        private int adressIndex = 0;
        private Pand.Rootobject thisPremises = new Pand.Rootobject();
        public GameObject premisesGameObject = default;

        private void Start()
        {
            closeButton.onClick.AddListener(CloseObject);
        }

        /// <summary>
        /// Sets text for the premis
        /// </summary>
        /// <param name="premisesData"></param>
        /// <param name="Index"></param>
        public void SetText(Pand.Rootobject premisesData, int Index)
        { 
            //Display all premises data in the UI
            ObjectProperties.Instance.AddDataField("BAG ID", premisesData.results[adressIndex].nummeraanduiding.nummeraanduidingidentificatie);
            ObjectProperties.Instance.AddDataField("Adres", premisesData.results[adressIndex].nummeraanduiding.adres + " " + premisesData.results[adressIndex].nummeraanduiding.postcode + " " + "Amsterdam");
            ObjectProperties.Instance.AddDataField("Postcode", premisesData.results[adressIndex].nummeraanduiding.postcode);
            ObjectProperties.Instance.AddDataField("Woning type", premisesData.results[adressIndex].nummeraanduiding.type_adres);
            ObjectProperties.Instance.AddDataField("Bouwjaar", premisesData.oorspronkelijk_bouwjaar);
            ObjectProperties.Instance.AddDataField("Buurt", premisesData._buurt.naam);
            ObjectProperties.Instance.AddDataField("Buutcombinatie", premisesData._buurtcombinatie.naam);
            ObjectProperties.Instance.AddDataField("Stadsdeel", premisesData._stadsdeel.naam);
            ObjectProperties.Instance.AddDataField("Huur", premisesData.results[adressIndex].nummeraanduiding.type_adres);
            ObjectProperties.Instance.AddDataField("Gebruiks oppervlakte", premisesData.results[adressIndex].verblijfsobject.oppervlakte + " M²");
            ObjectProperties.Instance.AddDataField("Aantal kamers", string.IsNullOrEmpty(premisesData.results[adressIndex].verblijfsobject.aantal_kamers) ? "Onbekend" : premisesData.results[adressIndex].verblijfsobject.aantal_kamers);
            ObjectProperties.Instance.AddDataField("Aantal bouwlagen", string.IsNullOrEmpty(premisesData.bouwlagen) ? "Onbekend" : premisesData.bouwlagen);
            ObjectProperties.Instance.AddDataField("Hoogste bouwlaag", string.IsNullOrEmpty(premisesData.hoogste_bouwlaag) ? "Onbekend" : premisesData.hoogste_bouwlaag);
            ObjectProperties.Instance.AddDataField("Laagste bouwlaag", string.IsNullOrEmpty(premisesData.laagste_bouwlaag) ? "Onbekend" : premisesData.laagste_bouwlaag);
            ObjectProperties.Instance.AddDataField("Verdieping toegang", string.IsNullOrEmpty(premisesData.results[adressIndex].verblijfsobject.verdieping_toegang) ? "Onbekend" : premisesData.results[adressIndex].verblijfsobject.verdieping_toegang);
            ObjectProperties.Instance.AddDataField("Woning corporatie", premisesData.results[adressIndex].verblijfsobject.eigendomsverhouding);
        }

        private void OnDisable()
        {
            streetName.gameObject.SetActive(false);
        }
        /// <summary>
        /// Closes the premises object
        /// </summary>
        public void CloseObject()
        {
            premisesGameObject.SetActive(false);
            streetName.gameObject.SetActive(false);
        }
    }
}