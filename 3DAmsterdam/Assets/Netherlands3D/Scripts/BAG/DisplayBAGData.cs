using Netherlands3D.Interface;
using Netherlands3D.Interface.SidePanel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.BAG
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
            if (bagId.Length < 5) return;

            StopAllCoroutines(); //Make sure all delayed Api coroutines are stopped before running this one again

			StartCoroutine(ImportBAG.GetBuildingData(bagId, (buildingData) =>
			{
                EstimateBuildingThumbnailFrame(buildingData);
				Interface.SidePanel.PropertiesPanel.Instance.AddTitle("Pand " + bagId);
				Interface.SidePanel.PropertiesPanel.Instance.AddDataField("BAG ID", buildingData._display);
				Interface.SidePanel.PropertiesPanel.Instance.AddDataField("Stadsdeel", buildingData._stadsdeel.naam);
				Interface.SidePanel.PropertiesPanel.Instance.AddDataField("Wijk", buildingData._buurtcombinatie.naam);
				Interface.SidePanel.PropertiesPanel.Instance.AddDataField("Buurt", buildingData._buurt.naam);
				Interface.SidePanel.PropertiesPanel.Instance.AddDataField("Bouwjaar", buildingData.oorspronkelijk_bouwjaar);
				Interface.SidePanel.PropertiesPanel.Instance.AddDataField("Bouwlagen", buildingData.bouwlagen.ToString());
				Interface.SidePanel.PropertiesPanel.Instance.AddDataField("Verblijfsobjecten", buildingData.verblijfsobjecten.count.ToString());
				Interface.SidePanel.PropertiesPanel.Instance.AddLink("Meer pand informatie", moreBuildingInfoUrl.Replace("{bagid}", buildingData._display));

				Interface.SidePanel.PropertiesPanel.Instance.AddSeperatorLine();

				//Load up the list of addresses tied to this building (in a Seperate API call)
				Interface.SidePanel.PropertiesPanel.Instance.AddTitle("Adressen");
				StartCoroutine(ImportBAG.GetBuildingAdresses(bagId, (addressList) =>
				{
					foreach (var address in addressList.results)
					{
						//We create a field and make it clickable, so addresses cant contain more data
						var dataKeyAndValue = Interface.SidePanel.PropertiesPanel.Instance.AddDataField(address._display, "");
						var button = dataKeyAndValue.GetComponent<Button>();
						button.onClick.AddListener((() => ShowAddressData(address.landelijk_id, button)));
					}
				}));
			}));
        }

		private static void EstimateBuildingThumbnailFrame(BagData.Rootobject buildingData)
		{
            //Create our building area using bbox coming from the building data
            List<Vector3> points = new List<Vector3>();
			var rdA = ConvertCoordinates.CoordConvert.RDtoUnity(new ConvertCoordinates.Vector3RD(buildingData.bbox[0], buildingData.bbox[1], 0.0));
			var rdB = ConvertCoordinates.CoordConvert.RDtoUnity(new ConvertCoordinates.Vector3RD(buildingData.bbox[2], buildingData.bbox[3], 0.0));
           
            //Estimate height using a raycast shot from above at the center of the bounding box
            float estimatedHeight = 100.0f;
            RaycastHit hit;
            if (Physics.BoxCast(Vector3.Lerp(rdA,rdB,0.5f) + Vector3.up*300.0f,Vector3.one*10.0f, Vector3.down, out hit))
            {
                estimatedHeight = hit.point.y;
            }

            //Add extra points giving our points shape a height
            var rdC = ConvertCoordinates.CoordConvert.RDtoUnity(new ConvertCoordinates.Vector3RD(buildingData.bbox[0], buildingData.bbox[1], 0));
			var rdD = ConvertCoordinates.CoordConvert.RDtoUnity(new ConvertCoordinates.Vector3RD(buildingData.bbox[2], buildingData.bbox[3], 0));
            rdC.y = estimatedHeight;
            rdD.y = estimatedHeight;

            points.Add(rdA);
			points.Add(rdB);
			points.Add(rdC);
			points.Add(rdD);
			PropertiesPanel.Instance.RenderThumbnailContaining(points.ToArray(), PropertiesPanel.ThumbnailRenderMethod.HIGHLIGHTED_BUILDINGS);
		}

		private void ShowAddressData(string addressId, Button button)
        {
            button.onClick.RemoveAllListeners();

            //TODO, add groupable dropdrown that shows/hides the following fields at below the selected object
            StartCoroutine(ImportBAG.GetAddressData(addressId, (addressData) =>
            {
				//Create group under button
				GameObject group = Interface.SidePanel.PropertiesPanel.Instance.CreateGroup();
                group.transform.SetSiblingIndex(button.transform.GetSiblingIndex() + 1);

                //Next click closes (and removes) group again
                button.onClick.AddListener((() =>
                {
                    Destroy(group.gameObject);
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener((() => ShowAddressData(addressId, button)));
                }));

				Interface.SidePanel.PropertiesPanel.Instance.AddSeperatorLine();
				Interface.SidePanel.PropertiesPanel.Instance.AddDataField("BAG ID", addressData.nummeraanduidingidentificatie);
				Interface.SidePanel.PropertiesPanel.Instance.AddDataField("Adres", addressData.adres + addressData.huisletter + " " + addressData.huisnummer_toevoeging);
				Interface.SidePanel.PropertiesPanel.Instance.AddDataField("", addressData.postcode + ", " + addressData.woonplaats._display);
				Interface.SidePanel.PropertiesPanel.Instance.AddLink("Meer adres informatie", moreAddressInfoUrl.Replace("{bagid}", addressData.nummeraanduidingidentificatie));
				Interface.SidePanel.PropertiesPanel.Instance.AddSeperatorLine();
				Interface.SidePanel.PropertiesPanel.Instance.CloseGroup();
            }));
        }
    }
}