using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
    public class DisplayBAGData : MonoBehaviour
    {
        private const string moreInfoUrl = "https://data.amsterdam.nl/data/bag/nummeraanduiding/id{bagid}/";

        /// <summary>
        /// Generates all premises buttons. If there is only 1 premises it will show just that premises
        /// </summary>
        /// <param name="pandData"></param>
        public void ShowBuildingData(string bagId)
        {
            ObjectProperties.Instance.AddTitle("Pand " + bagId);
            if (bagId.Length < 5) return;

            StartCoroutine(ImportBAG.GetBuildingData(bagId, (buildingData) => {
                ObjectProperties.Instance.AddDataField("BAG ID", buildingData._display);
                ObjectProperties.Instance.AddDataField("Stadsdeel", buildingData._stadsdeel.naam);
                ObjectProperties.Instance.AddDataField("Wijk", buildingData._buurtcombinatie.naam);
                ObjectProperties.Instance.AddDataField("Buurt", buildingData._buurt.naam);
                ObjectProperties.Instance.AddDataField("Bouwjaar", buildingData.oorspronkelijk_bouwjaar);
                ObjectProperties.Instance.AddDataField("Bouwlagen", buildingData.bouwlagen.ToString());
                ObjectProperties.Instance.AddDataField("Verblijfsobjecten", buildingData.verblijfsobjecten.count.ToString());
                ObjectProperties.Instance.AddURLText("Meer informatie", moreInfoUrl.Replace("{bagid}", buildingData._display));

                ObjectProperties.Instance.AddSeperatorLine();

                //Load up the list of addresses tied to this building (in a Seperate API call)
                ObjectProperties.Instance.AddTitle("Adressen");
                StartCoroutine(ImportBAG.GetBuildingAdresses(bagId, (addressList) => {
                   foreach(var address in addressList.results)
                   {
                        //We create a field and make it clickable, so addresses cant contain more data
                        var dataKeyAndValue = ObjectProperties.Instance.AddDataField(address._display, "( " + address.landelijk_id + " )");
                        dataKeyAndValue.GetComponent<Button>().onClick.AddListener((() => ShowAddressData(address.landelijk_id, dataKeyAndValue.transform)));
                   }
                }));
            }));
        }

        private void ShowAddressData(string addressId, Transform selected)
        {
            //TODO, add groupable dropdrown that shows/hides the following fields at below the selected object
            StartCoroutine(ImportBAG.GetAddressData(addressId, (addressData) =>
            {
                ObjectProperties.Instance.CreateGroup().transform.SetSiblingIndex(selected.GetSiblingIndex());
                ObjectProperties.Instance.AddDataField("BAG ID", addressData.nummeraanduidingidentificatie);
                ObjectProperties.Instance.AddDataField("Adres", addressData.adres + addressData.huisletter + " " + addressData.huisnummer_toevoeging);
                ObjectProperties.Instance.AddDataField("", addressData.postcode + ", " + addressData.woonplaats._display);
                ObjectProperties.Instance.AddURLText("Meer informatie", moreInfoUrl.Replace("{bagid}", addressData._display));
                ObjectProperties.Instance.CloseGroup();
            }));
        }
    }
}