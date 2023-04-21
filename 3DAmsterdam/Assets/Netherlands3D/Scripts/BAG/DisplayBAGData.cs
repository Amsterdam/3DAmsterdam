using Netherlands3D.Interface;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.BAG
{
    public class DisplayBAGData : MonoBehaviour
    {
        /// <summary>
        /// Generates all premises buttons. If there is only 1 premises it will show just that premises
        /// </summary>
        /// <param name="pandData"></param>
        public void ShowBuildingData(string bagId)
        {
            if (bagId.Length < 5) return;

            StopAllCoroutines(); //Make sure all delayed Api coroutines are stopped before running this one again
            if (this.gameObject.activeInHierarchy)
            {
                if (Config.activeConfiguration.BagApiType == BagApyType.Amsterdam)
                {

                    StartCoroutine(ImportBAG.GetBuildingDataAmsterdam(bagId, (buildingData) =>
                    {
                        EstimateBuildingThumbnailFrame(buildingData.bbox);
                        PropertiesPanel.Instance.AddTitle("Pand " + bagId, true);
                        CheckAddDataField("BAG ID", buildingData._display);
                        CheckAddDataField("Stadsdeel", buildingData._stadsdeel.naam);
                        CheckAddDataField("Wijk", buildingData._buurtcombinatie.naam);
                        CheckAddDataField("Buurt", buildingData._buurt.naam);
                        CheckAddDataField("Bouwjaar", buildingData.oorspronkelijk_bouwjaar);
                        CheckAddDataField("Bouwlagen", buildingData.bouwlagen);
                        CheckAddDataField("Verblijfsobjecten", buildingData.verblijfsobjecten.count);
                        PropertiesPanel.Instance.AddLink("Meer pand informatie", Config.activeConfiguration.moreBuildingInfoUrl.Replace("{bagid}", buildingData._display));
                        PropertiesPanel.Instance.AddSeperatorLine();

                        //Load up the list of addresses tied to this building (in a Seperate API call)
                        PropertiesPanel.Instance.AddTitle("Adressen");
                        StartCoroutine(ImportBAG.GetBuildingAdressesAmsterdam(bagId, (addressList) =>
                        {
                            foreach (var address in addressList.results)
                            {
                                //We create a field and make it clickable, so addresses cant contain more data
                                var dataKeyAndValue = PropertiesPanel.Instance.AddDataField(address._display, "");
                                var button = dataKeyAndValue.GetComponent<Button>();
                                button.onClick.AddListener((() => ShowAddressData(address.landelijk_id, button)));
                            }
                            PropertiesPanel.Instance.AddSpacer(20);
                        }));
                    }));
                }
                else if (Config.activeConfiguration.BagApiType == BagApyType.Kadaster)
                {
                    //Pdok requires a key requested from https://www.kadaster.nl/zakelijk/producten/adressen-en-gebouwen/bag-api-individuele-bevragingen
#if UNITY_EDITOR
                    var key = Config.activeConfiguration.developmentKey;
#else
                    var key = Config.activeConfiguration.productionKey;
#endif

                    StartCoroutine(ImportBAG.GetBuildingData(bagId, key, (buildingData) =>
                    {
                        if (buildingData == null) return;

                        Debug.Log("YESSS " + buildingData.pand.identificatie);
                        var geometry = buildingData.pand.geometrie;
                        if (geometry != null && geometry.coordinates.Length > 0)
                        {
                            List<Vector3> geometryPoints = new List<Vector3>();
                            for (int i = 0; i < geometry.coordinates.Length; i+=3)
                            {
                                var unityCoordinate = CoordConvert.RDtoUnity(geometry.coordinates[i], geometry.coordinates[i + 1], geometry.coordinates[i + 2]);
                                geometryPoints.Add(unityCoordinate);
                            }
                            PropertiesPanel.Instance.RenderThumbnailContaining(geometryPoints.ToArray(), PropertiesPanel.ThumbnailRenderMethod.HIGHLIGHTED_BUILDINGS);
                        }

                        PropertiesPanel.Instance.AddTitle("Pand " + bagId, true);

                        CheckAddDataField("Naam", buildingData.pand.identificatie);
                        CheckAddDataField("Bouwjaar", buildingData.pand.oorspronkelijkBouwjaar);
                        CheckAddDataField("Status", buildingData.pand.status);
                        PropertiesPanel.Instance.AddLink("Meer pand informatie", Config.activeConfiguration.moreBuildingInfoUrl.Replace("{bagid}", bagId));
                        PropertiesPanel.Instance.AddSeperatorLine();

                        //Load up the list of addresses tied to this building (in a Seperate API call)
                        PropertiesPanel.Instance.AddTitle("Adressen");
                        StartCoroutine(ImportBAG.GetBuildingAdresses(bagId, key, (addressList) =>
                        {
                            foreach (var address in addressList._embedded.adressen)
                            {
                                //We can make this clickable later like the one from amsterdam does ( to get more data )
                                var dataKeyAndValue = PropertiesPanel.Instance.AddDataField(address.adresregel5, address.adresregel6);
                            }
                            PropertiesPanel.Instance.AddSpacer(20);
                        }));
                    }));
                }
                else
                {
                    PropertiesPanel.Instance.AddLabel($"ApiType {Config.activeConfiguration.BagApiType} is niet geimplementeerd..");
                }
            }
        }

        private void CheckAddDataField(string label, object data)
        {
            var result = $"{data}";
            if (result.Trim() == "") return;
            PropertiesPanel.Instance.AddDataField(label, $"{data}");           
        }

		private static void EstimateBuildingThumbnailFrame(float[] bbox)
		{
            //Create our building area using bbox coming from the building data
            List<Vector3> points = new List<Vector3>();
			var rdA = CoordConvert.RDtoUnity(new Vector3RD(bbox[0], bbox[1], 0.0));
			var rdB = CoordConvert.RDtoUnity(new Vector3RD(bbox[2], bbox[3], 0.0));
           
            //Estimate height using a raycast shot from above at the center of the bounding box
            float estimatedHeight = 100.0f;
            RaycastHit hit;
            if (Physics.BoxCast(Vector3.Lerp(rdA,rdB,0.5f) + Vector3.up*300.0f,Vector3.one*10.0f, Vector3.down, out hit))
            {
                estimatedHeight = hit.point.y;
            }

            //Add extra points giving our points shape a height
            var rdC = CoordConvert.RDtoUnity(new Vector3RD(bbox[0], bbox[1], 0));
			var rdD = CoordConvert.RDtoUnity(new Vector3RD(bbox[2], bbox[3], 0));
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

				PropertiesPanel.Instance.AddSeperatorLine();
				PropertiesPanel.Instance.AddDataField("BAG ID", addressData.nummeraanduidingidentificatie);
				PropertiesPanel.Instance.AddDataField("Adres", addressData.adres + addressData.huisletter);
				PropertiesPanel.Instance.AddDataField("", addressData.postcode + ", " + addressData.woonplaats._display);
				PropertiesPanel.Instance.AddLink("Meer adres informatie", Config.activeConfiguration.moreAddressInfoUrl.Replace("{bagid}", addressData.nummeraanduidingidentificatie));
				PropertiesPanel.Instance.AddSeperatorLine();
				PropertiesPanel.Instance.CloseGroup();
            }));
        }
    }
}