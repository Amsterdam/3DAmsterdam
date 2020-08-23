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
		private const string tilesUrl = "https://t1.data.amsterdam.nl/topo_rd/{zoom}/{x}/{y}.png";

		private RawImage rawImage;
		public RawImage textureTargetRawImage { get => rawImage; private set => rawImage = value; }

		private RectTransform visibleMaskedArea;
		private Vector2 tileKey;

		private const float fadeSpeed = 0.5f;

		public void Initialize(Transform parentTo, RectTransform maskedArea, int zoomLevel, int size, int xLocation, int yLocation, Vector2 key)
		{
			tileKey = key;
			name = tileKey.x + "/" + tileKey.y;

			visibleMaskedArea = maskedArea;

			transform.SetParent(parentTo, false);

			//generate a new rawimage
			textureTargetRawImage = this.gameObject.AddComponent<RawImage>();
			textureTargetRawImage.rectTransform.pivot = Vector2.zero;
			textureTargetRawImage.rectTransform.sizeDelta = Vector2.one * size;
			textureTargetRawImage.enabled = false;

			//Posotion it in our parent according to x an y grid
			transform.localPosition = new Vector3(xLocation * size, yLocation * size, 0);
			StartCoroutine(LoadTexture(zoomLevel, (int)key.x, (int)key.y));
		}

		private IEnumerator LoadTexture(int zoom, int x, int y)
		{
			var tileImageUrl = tilesUrl.Replace("{zoom}", zoom.ToString()).Replace("{x}", x.ToString()).Replace("{y}", y.ToString());
			UnityWebRequest www = UnityWebRequestTexture.GetTexture(tileImageUrl);
			yield return www.SendWebRequest();

			if (www.isNetworkError || www.isHttpError)
			{
				Debug.Log(www.error);
			}
			else
			{
				Texture texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
				textureTargetRawImage.texture = texture;
				textureTargetRawImage.enabled = true;
				textureTargetRawImage.color = Color.clear;
				StartCoroutine(FadeInRawImage());
			}
		}

		IEnumerator FadeInRawImage(){
			while(textureTargetRawImage.color.a < 1.0f)
			{
				textureTargetRawImage.color = new Color(textureTargetRawImage.color.r, textureTargetRawImage.color.g, textureTargetRawImage.color.b, textureTargetRawImage.color.a + fadeSpeed);
				yield return new WaitForEndOfFrame();
			}
		}

		private void OnDestroy()
		{
			StopAllCoroutines();

			//Cleanup texture from memory
			if (textureTargetRawImage.texture)
				Destroy(textureTargetRawImage.texture);
			Destroy(textureTargetRawImage);
		}
	}
}
