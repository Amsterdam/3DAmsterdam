using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.LayerSystem
{
    public static class ObjectIDMapping
    {
        public static Vector2 GetUV(int itemNumber, Vector2Int textureSize)
        {
            Vector2 uv = new Vector2();
            int itemsPerRow = textureSize.x / 2;
            int columnNumber = itemNumber % itemsPerRow;
            int rowNumber = (itemNumber - columnNumber) / itemsPerRow;

            float pixelnummerX = 0.5f + (columnNumber * 2);
            float pixelnummerY = 0.5f + (rowNumber * 2);
            uv.x = (pixelnummerX / (textureSize.x - 1));
            uv.y = (pixelnummerY / (textureSize.y - 1));
            return uv;
        }

        public static Vector2Int GetTextureSize(int TotalItems)
        {
            Vector2Int textureSize = new Vector2Int();

            int minimumPowerOfTwo = 2;
            int maximumPowerOfTwo = 11;

            int SurfaceRequiered = 4 * TotalItems;
            int surfaceFound = int.MaxValue;

            int width = 0;
            int height = 0;
            int surface = 0;

            for (int x = minimumPowerOfTwo; x < maximumPowerOfTwo; x++)
            {
                for (int y = minimumPowerOfTwo; y < maximumPowerOfTwo; y++)
                {
                    width = (int)Math.Pow(2, x) * 2;
                    height = (int)Math.Pow(2, y) * 2;
                    surface = width * height;
                    if (surface >= SurfaceRequiered)
                    {
                        if (surface < surfaceFound)
                        {
                            textureSize.x = width;
                            textureSize.y = height;
                            surfaceFound = surface;
                        }
                        continue;
                    }
                }
            }

            return textureSize;
        }

        public static Vector2Int GetBottomLeftPixel(Vector2Int textureSize, int itemNumber)
        {
            int itemsPerRow = textureSize.x / 2;
            int columnNumber = itemNumber % itemsPerRow;
            int rowNumber = (itemNumber - columnNumber) / itemsPerRow;

            return new Vector2Int(columnNumber * 2, rowNumber * 2);
        }
    }
}