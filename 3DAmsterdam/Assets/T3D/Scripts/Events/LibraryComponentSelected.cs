using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw.BoundaryFeatures;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw
{
    public static class LibraryComponentSelectedEvent
    {
        public enum LibraryEventArgsType
        {
            Undefined,
            BoundaryFeature,
            Material
        }

        public class LibraryEventArgs : EventArgs
        {
            public LibraryEventArgsType Type;
            public SelectableLibraryItem SelectableLibraryItem;
            public Sprite Sprite;
        }

        public class LibraryComponentSelectedEventargs : LibraryEventArgs
        {
            //public Sprite Sprite;
            public bool IsTopComponent;
            public float ComponentWidth;
            public float ComponentHeight;
            public BoundaryFeature ComponentObject;
        }

        public class LibraryMaterialSelectedEventargs : LibraryEventArgs
        {
            public bool IsTopComponent;
            //public float ComponentWidth;
            //public float ComponentHeight;
            public Material ComponentMaterial;
            public Vector2 TextureScale;
            //public SelectComponent SelectComponent;
        }

        public delegate void LibraryComponentEventHandler(object source, LibraryEventArgs args);
        public static event LibraryComponentEventHandler OnComponentSelectedEvent;
        public static event LibraryComponentEventHandler OnMaterialSelectedEvent;

        public static void RaiseComponentSelected(object sender, Sprite sprite, bool isTopComponent, float width, float height, BoundaryFeature componentObject, SelectComponent selectComponent)
        {
            OnComponentSelectedEvent?.Invoke(sender, new LibraryComponentSelectedEventargs()
            {
                Type = LibraryEventArgsType.BoundaryFeature,
                Sprite = sprite,
                IsTopComponent = isTopComponent,
                ComponentWidth = width,
                ComponentHeight = height,
                ComponentObject = componentObject,
                SelectableLibraryItem = selectComponent
            });
        }

        public static void RaiseMaterialSelected(object sender, Sprite sprite, bool isTopComponent, Material material, Vector2 textureScale,SelectMaterial selectMaterial)
        {
            OnMaterialSelectedEvent?.Invoke(sender, new LibraryMaterialSelectedEventargs()
            {
                Type = LibraryEventArgsType.Material,
                Sprite = sprite,
                IsTopComponent = isTopComponent,
                ComponentMaterial = material,
                TextureScale = textureScale,
                SelectableLibraryItem = selectMaterial
            });
        }
    }
}
