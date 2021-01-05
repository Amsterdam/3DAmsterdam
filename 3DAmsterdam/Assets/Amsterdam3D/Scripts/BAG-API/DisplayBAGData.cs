using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
    public class DisplayBAGData : MonoBehaviour
    {
        private const string moreBuildingInfoUrl = "https://data.amsterdam.nl/data/bag/pand/id{bagid}/";
        private const string moreAddressInfoUrl = "https://data.amsterdam.nl/data/bag/nummeraanduiding/id{bagid}/";

        /// <summary>
        /// Generates all premises buttons. If there is only 1 premises it will show just that premises
        /// </summary>
        /// <param name="pandData"></param>
        public void ShowBuildingData(string bagId)
        {
            ObjectProperties.Instance.AddTitle("Pand " + bagId);
            if (bagId.Length < 5) return;

            StartCoroutine(ImportBAG.GetBuildingData(bagId, (buildingData) => {
                //Use the boundingbox coordinates to draw the thumbnail for this building position 
                double estimatedHeight = 100.0f;
                /*if(double.TryParse(buildingData.hoogste_bouwlaag, out estimatedHeight)){
                    estimatedHeight *= 2.5f; //Take an average layer height

                    Debug.Log("HEIGHT ESTIMATE: " + estimatedHeight);
                }*/

                List<Vector3> bbox = new List<Vector3>();
                var rdA = ConvertCoordinates.CoordConvert.RDtoUnity(new ConvertCoordinates.Vector3RD(buildingData.bbox[0], buildingData.bbox[1], 0.0));
                var rdB = ConvertCoordinates.CoordConvert.RDtoUnity(new ConvertCoordinates.Vector3RD(buildingData.bbox[2], buildingData.bbox[3], 0.0));
                var rdC = ConvertCoordinates.CoordConvert.RDtoUnity(new ConvertCoordinates.Vector3RD(buildingData.bbox[0], buildingData.bbox[1], estimatedHeight));
                var rdD = ConvertCoordinates.CoordConvert.RDtoUnity(new ConvertCoordinates.Vector3RD(buildingData.bbox[2], buildingData.bbox[3], estimatedHeight));
                bbox.Add(rdA);
                bbox.Add(rdB);
                bbox.Add(rdC);
                bbox.Add(rdD);
                ObjectProperties.Instance.RenderThumbnailContaining(bbox.ToArray());

                ObjectProperties.Instance.AddDataField("BAG ID", buildingData._display);
                ObjectProperties.Instance.AddDataField("Stadsdeel", buildingData._stadsdeel.naam);
                ObjectProperties.Instance.AddDataField("Wijk", buildingData._buurtcombinatie.naam);
                ObjectProperties.Instance.AddDataField("Buurt", buildingData._buurt.naam);
                ObjectProperties.Instance.AddDataField("Bouwjaar", buildingData.oorspronkelijk_bouwjaar);
                ObjectProperties.Instance.AddDataField("Bouwlagen", buildingData.bouwlagen.ToString());
                ObjectProperties.Instance.AddDataField("Verblijfsobjecten", buildingData.verblijfsobjecten.count.ToString());
                ObjectProperties.Instance.AddURLText("Meer pand informatie", moreBuildingInfoUrl.Replace("{bagid}", buildingData._display));

                ObjectProperties.Instance.AddSeperatorLine();

                //Load up the list of addresses tied to this building (in a Seperate API call)
                ObjectProperties.Instance.AddTitle("Adressen");
                StartCoroutine(ImportBAG.GetBuildingAdresses(bagId, (addressList) => {
                   foreach(var address in addressList.results)
                   {
                        //We create a field and make it clickable, so addresses cant contain more data
                        var dataKeyAndValue = ObjectProperties.Instance.AddDataField(address._display, "");
                        var button = dataKeyAndValue.GetComponent<Button>();
                        button.onClick.AddListener((() => ShowAddressData(address.landelijk_id, button)));
                   }
                }));
            }));
        }

        private void ShowAddressData(string addressId, Button button)
        {
            button.onClick.RemoveAllListeners();

            //TODO, add groupable dropdrown that shows/hides the following fields at below the selected object
            StartCoroutine(ImportBAG.GetAddressData(addressId, (addressData) =>
            {
                //Create group under button
                GameObject group = ObjectProperties.Instance.CreateGroup();
                group.transform.SetSiblingIndex(button.transform.GetSiblingIndex() + 1);

                //Next click closes (and removes) group again
                button.onClick.AddListener((() =>
                {
                    Destroy(group.gameObject);
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener((() => ShowAddressData(addressId, button)));
                }));

                ObjectProperties.Instance.AddSeperatorLine();
                ObjectProperties.Instance.AddDataField("BAG ID", addressData.nummeraanduidingidentificatie);
                ObjectProperties.Instance.AddDataField("Adres", addressData.adres + addressData.huisletter + " " + addressData.huisnummer_toevoeging);
                ObjectProperties.Instance.AddDataField("", addressData.postcode + ", " + addressData.woonplaats._display);
                ObjectProperties.Instance.AddURLText("Meer adres informatie", moreAddressInfoUrl.Replace("{bagid}", addressData.nummeraanduidingidentificatie));
                ObjectProperties.Instance.AddSeperatorLine();
                ObjectProperties.Instance.CloseGroup();
            }));
        }
    }
}