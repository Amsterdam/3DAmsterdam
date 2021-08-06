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

namespace Netherlands3D.AssetGeneration
{
    public class ProgressPreviewMap : MonoBehaviour
    {
        public static ProgressPreviewMap Instance;

        private Texture2D backDropTexture;
        private Texture2D drawIntoPixels;

        [Header("Progress preview")]
        [SerializeField]
        private int backgroundSize = 500;

        [SerializeField]
        private RawImage backgroundRawImage;

        [SerializeField]
        private RawImage gridPixelsRawImage;

        void Awake()
        {
            Instance = this;
        }

        public IEnumerator Initialize(int xTiles, int yTiles)
        {
            //Show a previewmap
            backDropTexture = new Texture2D(backgroundSize, backgroundSize, TextureFormat.RGBA32, false);
            drawIntoPixels = new Texture2D(xTiles, yTiles, TextureFormat.RGBA32, false);
            drawIntoPixels.filterMode = FilterMode.Point;

            gridPixelsRawImage.texture = drawIntoPixels;

            //Download background preview image
            var downloadUrl = Config.activeConfiguration.previewBackdropImage
                .Replace("{xmin}", Config.activeConfiguration.BottomLeftRD.x.ToString())
                .Replace("{ymin}", Config.activeConfiguration.BottomLeftRD.y.ToString())
                .Replace("{xmax}", Config.activeConfiguration.TopRightRD.x.ToString())
                .Replace("{ymax}", Config.activeConfiguration.TopRightRD.y.ToString())
                .Replace("{w}", backgroundSize.ToString())
                .Replace("{h}", backgroundSize.ToString());
            print(downloadUrl);
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(downloadUrl);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                backDropTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            }
            backgroundRawImage.texture = backDropTexture;
        }

        public void ColorTile(int x, int y, TilePreviewState state)
        {
            switch(state)
            {
                case TilePreviewState.DONE:
                    drawIntoPixels.SetPixel(x, y, Color.clear);
                    break;
                case TilePreviewState.SKIPPED:
                    drawIntoPixels.SetPixel(x, y, Color.grey);
                    break;
                case TilePreviewState.EMPTY:
                    drawIntoPixels.SetPixel(x, y, Color.black);
                    break;
            }
            drawIntoPixels.Apply();
        }
    }

    public enum TilePreviewState
    {
        DONE,
        SKIPPED,
        EMPTY
	}
}