using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Events;

public class ImageAccordionController : MonoBehaviour
{

	[SerializeField] private RawImage rawImage;

	[SerializeField] private Transform imageContainer;
	[SerializeField] private string urlImage;

	void Start()
	{
		StartCoroutine(LoadImage(urlImage));
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
			//Get texture
			var texture = ((DownloadHandlerTexture)textureWebRequest.downloadHandler).texture;

			//Instantiate as raw image
			rawImage.texture = texture;

			//Edit height
			var newHeight = rawImage.rectTransform.sizeDelta.x / texture.width;
			rawImage.rectTransform.sizeDelta = new Vector2(rawImage.rectTransform.sizeDelta.x, texture.height * newHeight);
			imageContainer.GetComponent<RectTransform>().sizeDelta = rawImage.rectTransform.sizeDelta;
		}
	}

}
