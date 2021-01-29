using BruTile.Wms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
	public class MapTile : MonoBehaviour
	{
		[SerializeField]
		//private const string tilesUrl = "https://t1.data.amsterdam.nl/topo_rd/{zoom}/{x}/{y}.png";
		//utrecht
		private const string tilesUrl = "https://geodata.nationaalgeoregister.nl/tiles/service/tms/1.0.0/opentopo/EPSG:28992/{zoom}/{x}/{y}.png";
		
		private RawImage rawImage;
		public RawImage textureTargetRawImage { get => rawImage; private set => rawImage = value; }
		private Vector2 tileKey;

		private const float fadeSpeed = 3.0f;
		UnityWebRequest uwr;

		public void Initialize(Transform parentTo, int zoomLevel, int size, int xLocation, int yLocation, Vector2 key, bool rayCastTile)
		{
			tileKey = key;
			name = tileKey.x + "/" + tileKey.y;

			transform.SetParent(parentTo, false);

			//generate a new rawimage
			textureTargetRawImage = this.gameObject.AddComponent<RawImage>();
			textureTargetRawImage.raycastTarget = rayCastTile;
			textureTargetRawImage.rectTransform.pivot = Vector2.zero;
			textureTargetRawImage.rectTransform.sizeDelta = Vector2.one * size;
			textureTargetRawImage.enabled = false;

			//Posotion it in our parent according to x an y grid
			transform.localPosition = new Vector3(xLocation * size, yLocation * size, 0);
			StartCoroutine(LoadTexture(zoomLevel, (int)key.x, (int)key.y));
		}

		private IEnumerator LoadTexture(int zoom, int x, int y)
		{
			Debug.Log(zoom +"-"+x + "-" + y);
			var tileImageUrl = tilesUrl.Replace("{zoom}", zoom.ToString()).Replace("{x}", x.ToString()).Replace("{y}", y.ToString());
			
			using (uwr = UnityWebRequestTexture.GetTexture(tileImageUrl))
			{
				yield return uwr.SendWebRequest();

				if (uwr.isNetworkError || uwr.isHttpError)
				{
					Debug.Log("Could not find minimap tile :" + tileImageUrl);
				}
				else
				{
					var texture = DownloadHandlerTexture.GetContent(uwr);
					textureTargetRawImage.texture = texture;
					textureTargetRawImage.enabled = true;
					textureTargetRawImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
					StartCoroutine(FadeInRawImage());
				}
			}
		}

		IEnumerator FadeInRawImage(){
			while(textureTargetRawImage.color.a < 1.0f)
			{
				textureTargetRawImage.color = new Color(textureTargetRawImage.color.r, textureTargetRawImage.color.g, textureTargetRawImage.color.b, textureTargetRawImage.color.a + fadeSpeed*Time.deltaTime);
				yield return new WaitForEndOfFrame();
			}
		}

		private void OnDestroy()
		{
			StopAllCoroutines();

			//Makes sure the UnityWebRequestTexture is disposed of
			//It holds an internal Texture2D that cant be GC'd
			if (uwr != null) uwr.Dispose();

			//Cleanup texture from memory
			Destroy(textureTargetRawImage.texture);
			textureTargetRawImage.texture = null;

			Destroy(textureTargetRawImage);
		}
	}
}
