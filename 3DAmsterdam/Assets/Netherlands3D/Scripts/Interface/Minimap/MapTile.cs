/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
using System.Collections;
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

		private int zoomLevel = 0;
		private MinimapConfig config;

		public void Initialize(Transform container, int zoom, float size, float xLocation, float yLocation, Vector2 key, MinimapConfig minimapConfig)
		{
			zoomLevel = zoom;
			tileKey = key;
			name = tileKey.x + "/" + tileKey.y;

			config = minimapConfig;

			transform.SetParent(container, false);

			//generate a new rawimage
			TextureTargetRawImage = this.gameObject.AddComponent<RawImage>();
			TextureTargetRawImage.raycastTarget = false;
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
			var tileImageUrl = config.ServiceUrl.Replace("{zoom}", zoom.ToString()).Replace("{x}", x.ToString()).Replace("{y}", y.ToString());

			using (uwr = UnityWebRequestTexture.GetTexture(tileImageUrl,true))
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
