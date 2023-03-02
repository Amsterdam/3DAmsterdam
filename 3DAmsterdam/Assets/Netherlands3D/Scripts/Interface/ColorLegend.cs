using Netherlands3D.Core.Colors;
using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

namespace Netherlands3D.Interface {
    public class ColorLegend : MonoBehaviour
    {
		[SerializeField]
		private Transform container;

		[Header("Listen to")]
		[SerializeField]
		private TriggerEvent closeLegend;

		[SerializeField]
		private StringEvent openLegendFromImageURL;

		[SerializeField]
		private ColorPaletteEvent openLegendFromColorPalette;

		[Header("Template references")]
		[SerializeField]
        private ColorPalette colorPalette;

        [SerializeField]
        private GameObject paletteColorPrefab;

        [SerializeField]
        private TextMeshProUGUI titleText;

		private Texture2D legendTexture;
		private RawImage legendRawImage;

		private void Awake()
		{
			if(closeLegend)
				closeLegend.AddListenerStarted(Close);

			if (openLegendFromImageURL)
				openLegendFromImageURL.AddListenerStarted(GenerateFromImage);

			if (openLegendFromColorPalette)
				openLegendFromColorPalette.AddListenerStarted(GenerateFromColorPalette);
		}

		/// <summary>
		/// Clears and closes legend
		/// </summary>
		public void Close()
		{
			Clear();
			gameObject.SetActive(false);
		}

		/// <summary>
		/// Cast an object to a ColorPalette object and generate its content
		/// </summary>
		/// <param name="colorPalette">ColorPalette object</param>
		public void GenerateFromColorPalette(object colorPalette)
		{
			var castedColorPalette = (ColorPalette)colorPalette;
			GenerateFromColorPalette(castedColorPalette);
		}

		/// <summary>
		/// Generate colors from a ColorPalette object
		/// </summary>
		/// <param name="colorPalette"></param>
		public void GenerateFromColorPalette(ColorPalette colorPalette)
        {
            this.colorPalette = colorPalette;
			this.gameObject.SetActive(true);
			GenerateColorsList();
        }

		/// <summary>
		/// Load an image url and display it as a ColorLegend
		/// </summary>
		/// <param name="imageUrl">The URL to the legend image</param>
        public void GenerateFromImage(string imageUrl)
        {
			Clear();
			
			this.gameObject.SetActive(true);
			StartCoroutine(LoadImage(imageUrl));	
		}

		private IEnumerator LoadImage(string imageUrl)
		{
			UnityWebRequest textureWebRequest = UnityWebRequestTexture.GetTexture(imageUrl);
			yield return textureWebRequest.SendWebRequest();

			if (textureWebRequest.result != UnityWebRequest.Result.Success)
			{
				Debug.Log($"Request returned error: {imageUrl}");
				Debug.Log(textureWebRequest.error);
			}
			else
			{
				if (legendTexture != null) Destroy(legendTexture);

				legendTexture = ((DownloadHandlerTexture)textureWebRequest.downloadHandler).texture;
				AddRawImage();
			}
		}

		private void AddRawImage()
		{
			legendRawImage = new GameObject().AddComponent<RawImage>();
			legendRawImage.transform.SetParent(container);
			legendRawImage.texture = legendTexture;
		}

		private void Update()
		{
			if (!legendRawImage) return;
			//Keep respecting aspect height of rawimage
			var newHeight = legendRawImage.rectTransform.sizeDelta.x / legendTexture.width;
			legendRawImage.rectTransform.sizeDelta = new Vector2(legendRawImage.rectTransform.sizeDelta.x, legendTexture.height * newHeight);
		}

		private void OnDisable()
		{
			if (legendRawImage) Destroy(legendRawImage);
		}

		private void GenerateColorsList()
		{
			Clear();
			if (!colorPalette) return;

			foreach (var namedColor in colorPalette.colors)
			{
				var newNamedColor = Instantiate(paletteColorPrefab, container);
				newNamedColor.name = namedColor.name;
				newNamedColor.GetComponentInChildren<Image>(true).color = namedColor.color;
				newNamedColor.GetComponentInChildren<TextMeshProUGUI>(true).text = namedColor.name;
				newNamedColor.AddComponent<TooltipTrigger>().TooltipText = namedColor.name;
			}
		}

		private void Clear()
		{
			foreach (Transform child in container)
			{
				Destroy(child.gameObject);
			}

			//Make sure scrollview is reset to up
			var scrollView = container.GetComponent<ScrollRect>();
			if(scrollView)
			{
				scrollView.verticalNormalizedPosition = 1;
			}
		}
	}
}