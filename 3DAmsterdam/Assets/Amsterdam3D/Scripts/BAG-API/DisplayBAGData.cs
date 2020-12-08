using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
    public class DisplayBAGData : MonoBehaviour
    {
        [SerializeField] private Text objectPanelTitle = default;

        private const string moreInfoUrl = "https://data.amsterdam.nl/data/bag/nummeraanduiding/id{bagid}/";

        [SerializeField]
        private string pandTitlePrefix = "Pand: ";

        /// <summary>
        /// Generates all premises buttons. If there is only 1 premises it will show just that premises
        /// </summary>
        /// <param name="pandData"></param>
        public void ShowData(Pand.Rootobject pandData)
        {
            if (pandData.results.Length > 0)
            {
                // starts ui en cleans up all previous buttons
                objectPanelTitle.text = pandTitlePrefix + pandData._display;
                // check if there's more than one adress
                if (pandData.results.Length > 1)
                {
                    // creates a button for each adress
                    for (int i = 0; i < pandData.results.Length; i++)
                    {
                        ObjectProperties.Instance.AddDataField("BAG id", pandData._display);
                        ObjectProperties.Instance.AddDataField("Stadsdeel", pandData._stadsdeel.naam);
                        ObjectProperties.Instance.AddDataField("Wijk", pandData._buurtcombinatie.naam);
                        ObjectProperties.Instance.AddDataField("Buurt", pandData._buurt.naam);
                        ObjectProperties.Instance.AddDataField("Bouwjaar", pandData.oorspronkelijk_bouwjaar);
                        ObjectProperties.Instance.AddDataField("Bouwlagen", pandData.bouwlagen);
                        ObjectProperties.Instance.AddDataField("Verblijfsobjecten", pandData.verblijfsobjecten.count.ToString());
                        ObjectProperties.Instance.AddURLText("Meer informatie", moreInfoUrl.Replace("{bagid}", pandData._display));
                    }
                }
            }
        }
        /// <summary>
        /// Places premises but doesn't wait for it to be completed due to the data loading fast
        /// </summary>
        /// <param name="pandData"></param>
        /// <param name="index"></param>
        public void PlacePremises(Pand.Rootobject pandData, int index)
        {
            StartCoroutine(ImportBAG.Instance.CallAPI("https://api.data.amsterdam.nl/bag/v1.1/nummeraanduiding/", ImportBAG.Instance.hoofdData.results[index].landelijk_id, RetrieveType.NummeraanduidingInstance));
        }
    }
}