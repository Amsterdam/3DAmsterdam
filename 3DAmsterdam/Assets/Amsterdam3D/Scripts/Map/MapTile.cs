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

		public static List<MapTile> currentZoomLevelMapTiles;

		private RectTransform visibleMaskedArea;

		private void Awake()
		{
			if (currentZoomLevelMapTiles == null) currentZoomLevelMapTiles = new List<MapTile>();
			currentZoomLevelMapTiles.Add(this);
		}

		public void Initialize(Transform parentTo, RectTransform maskedArea, int zoomLevel, int size, int xLocation, int yLocation, Vector2 key)
		{
			visibleMaskedArea = maskedArea;

			transform.SetParent(parentTo, false);

			name = key.x + "/" + key.y;

			//generate a new rawimage
			textureTargetRawImage = this.gameObject.AddComponent<RawImage>();
			textureTargetRawImage.rectTransform.pivot = Vector2.zero;
			textureTargetRawImage.rectTransform.sizeDelta = Vector2.one * size;
			textureTargetRawImage.enabled = false;

			//Posotion it in our parent according to x an y grid
			transform.localPosition = new Vector3(xLocation * size, yLocation * size, 0);
			StartCoroutine(LoadTexture(zoomLevel, (int)key.x, (int)key.y));
		}

		private void Update()
		{
			if (!textureTargetRawImage.rectTransform.rect.Overlaps(visibleMaskedArea.rect))
			{
				textureTargetRawImage.color = Color.red;
			}
			else{
				textureTargetRawImage.color = Color.white;
			}
		}

		private IEnumerator LoadTexture(int zoom, int x, int y)
		{
			var solvedUrl = tilesUrl.Replace("{zoom}", zoom.ToString()).Replace("{x}", x.ToString()).Replace("{y}", y.ToString());
			UnityWebRequest www = UnityWebRequestTexture.GetTexture(solvedUrl);
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
			}
		}

		private void OnDestroy()
		{
			StopAllCoroutines();
			//Cleanup texture from memory
			if(textureTargetRawImage.texture)
				Destroy(textureTargetRawImage.texture);
			Destroy(textureTargetRawImage);
		}
	}
}
