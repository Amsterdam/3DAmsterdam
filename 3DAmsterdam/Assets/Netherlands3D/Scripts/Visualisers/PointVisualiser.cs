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
using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Visualisers
{
    public class PointVisualiser : MonoBehaviour
    {
        [SerializeField]
        private Vector3Event receiveDrawPointEvent;

        [SerializeField]
        private GameObject shape;

        [SerializeField]
        private Vector3 offset = Vector3.up;

        void Awake()
        {
            if (receiveDrawPointEvent) receiveDrawPointEvent.AddListenerStarted(DrawPointInWorld);
        }

        public void DrawPointInWorld(Vector3 position)
        {
            var newPoint = Instantiate(shape, this.transform);
            newPoint.transform.position = position + offset;
        }
    }
}