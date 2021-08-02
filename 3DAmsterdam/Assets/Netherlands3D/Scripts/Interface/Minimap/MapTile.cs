using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Netherlands3D.Interface.Minimap
{
	public class MapTile : MonoBehaviour, IPointerDownHandler
	{
		private RawImage rawImage;
		public RawImage TextureTargetRawImage { get => rawImage; private set => rawImage = value; }
		private Vector2 tileKey;

		private const float fadeSpeed = 3.0f;
		private UnityWebRequest uwr;

		private int zoomLevel = 0;

		private bool loadedSubTiles = false;

		public void Initialize(Transform parentTo, int zoom, float size, float xLocation, float yLocation, Vector2 key, bool rayCastTile)
		{
			zoomLevel = zoom;

			tileKey = key;
			name = tileKey.x + "/" + tileKey.y;

			transform.SetParent(parentTo, false);
			//generate a new rawimage
			TextureTargetRawImage = this.gameObject.AddComponent<RawImage>();
			TextureTargetRawImage.raycastTarget = rayCastTile;
			TextureTargetRawImage.rectTransform.pivot = new Vector2(0, 1);
			TextureTargetRawImage.rectTransform.anchorMin = TextureTargetRawImage.rectTransform.anchorMax = new Vector2(0, 1);
			TextureTargetRawImage.rectTransform.sizeDelta = Vector2.one * size;
			TextureTargetRawImage.enabled = false;

			//Position it in our parent according to x an y grid
			transform.localPosition = new Vector3(xLocation * size, yLocation * size, 0);

			StartCoroutine(LoadTexture(zoom, (int)key.x, (int)key.y));
		}


		public void OnPointerDown(PointerEventData eventData)
		{
			LoadMoreDetail();
		}

		private void LoadMoreDetail()
		{
			if (loadedSubTiles) return;

			for (int x = 0; x < 2; x++)
			{
				for (int y = 0; y < 2; y++){ 
					var size = TextureTargetRawImage.rectTransform.sizeDelta.x / 2.0f;
					var subTile = new GameObject("ChildTile");
					subTile.AddComponent<MapTile>().Initialize(
						this.transform,
						zoomLevel + 1,
						size,
						x,
						-y,
						new Vector2((tileKey.x * 2) + x, (tileKey.y * 2) + y),
						true
					);
				}
			}

			loadedSubTiles = true;
		}

		private IEnumerator LoadTexture(int zoom, int x, int y)
		{
			var tileImageUrl = Config.activeConfiguration.minimapServiceUrl.Replace("{zoom}", zoom.ToString()).Replace("{x}", x.ToString()).Replace("{y}", y.ToString());

			using (uwr = UnityWebRequestTexture.GetTexture(tileImageUrl))
			{
				yield return uwr.SendWebRequest();

				if (uwr.isNetworkError || uwr.isHttpError)
				{
					Debug.Log("Could not find minimap tile :" + tileImageUrl);
				}
				else
				{
					Debug.Log("Minimap tile :" + tileImageUrl);
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
