using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Netherlands3D.Interface.Minimap
{
	public class MapTile : MonoBehaviour
	{
		private RawImage rawImage;
		public RawImage TextureTargetRawImage { get => rawImage; private set => rawImage = value; }
		private Vector2 tileKey;

		private const float fadeSpeed = 3.0f;
		private UnityWebRequest uwr;

		public void Initialize(Transform parentTo, int zoomLevel, int size, int xLocation, int yLocation, Vector2 key, bool rayCastTile)
		{
			tileKey = key;
			name = tileKey.x + "/" + tileKey.y;

			transform.SetParent(parentTo, false);

			//generate a new rawimage
			TextureTargetRawImage = this.gameObject.AddComponent<RawImage>();
			TextureTargetRawImage.raycastTarget = rayCastTile;
			TextureTargetRawImage.rectTransform.pivot = Vector2.zero;
			TextureTargetRawImage.rectTransform.sizeDelta = Vector2.one * size;
			TextureTargetRawImage.enabled = false;

			//Posotion it in our parent according to x an y grid
			transform.localPosition = new Vector3(xLocation * size, yLocation * size, 0);
			StartCoroutine(LoadTexture(zoomLevel, (int)key.x, (int)key.y));
		}

		private IEnumerator LoadTexture(int zoom, int x, int y)
		{
			var tileImageUrl = Config.activeConfiguration.minimapServiceUrl.Replace("{zoom}", zoom.ToString()).Replace("{x}", x.ToString()).Replace("{y}", y.ToString());

			using (uwr = UnityWebRequestTexture.GetTexture(tileImageUrl))
			{
				yield return uwr.SendWebRequest();

				if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
				{
					Debug.Log("Could not find minimap tile :" + tileImageUrl);
				}
				else
				{
					var texture = DownloadHandlerTexture.GetContent(uwr);
					TextureTargetRawImage.texture = texture;
					TextureTargetRawImage.enabled = true;
					TextureTargetRawImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
					StartCoroutine(FadeInRawImage());
				}
			}
		}

		IEnumerator FadeInRawImage(){
			while(TextureTargetRawImage.color.a < 1.0f)
			{
				TextureTargetRawImage.color = new Color(TextureTargetRawImage.color.r, TextureTargetRawImage.color.g, TextureTargetRawImage.color.b, TextureTargetRawImage.color.a + fadeSpeed*Time.deltaTime);
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
			Destroy(TextureTargetRawImage.texture);
			TextureTargetRawImage.texture = null;

			Destroy(TextureTargetRawImage);
		}
	}
}
