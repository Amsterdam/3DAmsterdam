using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw.BoundaryFeatures
{
    public static class LibraryComponentSelectedEvent
    {
        public class LibraryComponentSelectedEventArgs : EventArgs
        {
            public Image Image;
            public bool IsTopComponent;
            public float ComponentWidth;
            public float ComponentHeight;
            public BoundaryFeature ComponentObject;
            public SelectComponent SelectComponent;
        }

        private static event EventHandler<LibraryComponentSelectedEventArgs> OnEvent = delegate { };

        public static void Raise(object sender, Image image, bool isTopComponent, float width, float height, BoundaryFeature componentObject, SelectComponent selectComponent)
        {
            OnEvent(sender, new LibraryComponentSelectedEventArgs()
            {
                Image = image,
                IsTopComponent = isTopComponent,
                ComponentWidth = width,
                ComponentHeight = height,
                ComponentObject = componentObject,
                SelectComponent = selectComponent
            });
        }

        public static void Subscribe(EventHandler<LibraryComponentSelectedEventArgs> f)
        {
            OnEvent += f;
        }

        public static void Unsubscribe(EventHandler<LibraryComponentSelectedEventArgs> f)
        {
            OnEvent -= f;
        }
    }
}
