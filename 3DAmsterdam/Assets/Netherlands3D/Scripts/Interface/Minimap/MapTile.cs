﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Netherlands3D.Interface.Minimap
{
	public class MapTile : MonoBehaviour
	{
		private RawImage rawImage;
		public RawImage TextureTargetRawImage { get => rawImage; private set => rawImage = value; }
		private Vector2 tileKey;

		private const float fadeSpeed = 3.0f;
		private UnityWebRequest uwr;

		private float pixelSize = 256;

		private int zoomLevel = 0;

		public void Initialize(Transform parentTo, int zoom, float size, float xLocation, float yLocation, Vector2 key, bool rootTile)
		{
			zoomLevel = zoom;
			pixelSize = size;
			tileKey = key;
			name = tileKey.x + "/" + tileKey.y;

			transform.SetParent(parentTo, false);

			//generate a new rawimage
			TextureTargetRawImage = this.gameObject.AddComponent<RawImage>();
			TextureTargetRawImage.raycastTarget = rootTile;
			TextureTargetRawImage.rectTransform.pivot = new Vector2(0, 1);
			TextureTargetRawImage.rectTransform.anchorMin = TextureTargetRawImage.rectTransform.anchorMax = new Vector2(0, 1);
			TextureTargetRawImage.rectTransform.sizeDelta = Vector2.one * size;
			ClearTextureImage();

			//Position it in our parent according to x an y grid
			transform.localPosition = new Vector3(xLocation, yLocation, 0);

			StartCoroutine(LoadTexture(zoomLevel, (int)tileKey.x, (int)tileKey.y));
		}

		private void ClearTextureImage()
		{
			TextureTargetRawImage.enabled = false;
			TextureTargetRawImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
		}

		private IEnumerator LoadTexture(int zoom, int x, int y)
		{
			var tileImageUrl = Config.activeConfiguration.minimapServiceUrl.Replace("{zoom}", zoom.ToString()).Replace("{x}", x.ToString()).Replace("{y}", y.ToString());

			using (uwr = UnityWebRequestTexture.GetTexture(tileImageUrl))
			{
				yield return uwr.SendWebRequest();

				if (uwr.result != UnityWebRequest.Result.Success)
				{
					Debug.Log("Could not find minimap tile :" + tileImageUrl);
				}
				else
				{
					var texture = DownloadHandlerTexture.GetContent(uwr);
					texture.wrapMode = TextureWrapMode.Clamp;
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
