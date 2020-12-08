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
            StartCoroutine(ImportBAG.GetBuildingData(bagId, (buildingData) => {
                ObjectProperties.Instance.AddSubtitle("Pand " + buildingData._display);

                ObjectProperties.Instance.AddDataField("BAG id", buildingData._display);
                ObjectProperties.Instance.AddDataField("Stadsdeel", buildingData._stadsdeel.naam);
                ObjectProperties.Instance.AddDataField("Wijk", buildingData._buurtcombinatie.naam);
                ObjectProperties.Instance.AddDataField("Buurt", buildingData._buurt.naam);
                ObjectProperties.Instance.AddDataField("Bouwjaar", buildingData.oorspronkelijk_bouwjaar);
                ObjectProperties.Instance.AddDataField("Bouwlagen", buildingData.bouwlagen.ToString());
                ObjectProperties.Instance.AddDataField("Verblijfsobjecten", buildingData.verblijfsobjecten.count.ToString());
                ObjectProperties.Instance.AddURLText("Meer informatie", moreInfoUrl.Replace("{bagid}", buildingData._display));

                ObjectProperties.Instance.AddSeperatorLine();
                //Load up the list of addresses tied to this building (in a Seperate API call)
                ObjectProperties.Instance.AddSubtitle("Verblijfsobjecten");
                StartCoroutine(ImportBAG.GetBuildingAdresses(bagId, (addressList) => {
                   foreach(var address in addressList.results)
                   {
                        ObjectProperties.Instance.AddDataField(address._display, "( " + address.landelijk_id + " )");
				   }
                }));
            }));
        }
    }
}