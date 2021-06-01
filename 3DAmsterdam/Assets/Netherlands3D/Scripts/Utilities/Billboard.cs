using Netherlands3D.InputHandler;
using Netherlands3D.ObjectInteraction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D
{
    

    public class Billboard : Interactable
    {
        public string[] Row;
        public int Index;
        public Action<int> ClickAction;

        GameObject textmeshGameObject;

        private bool isFiltered;

        Renderer ren;
        SphereCollider col;

        public void FilterOn(int columnIndex, string filter)
        {
            isFiltered = Row[columnIndex] != filter;
            var show = isFiltered == false;
            ren.enabled = show;
            col.enabled = show;
            textmeshGameObject.SetActive(show);
        }

        public void Show()
        {
            isFiltered = false;
            ren.enabled = true;
            col.enabled = true;
            textmeshGameObject.SetActive(true);
        }

        private void Start()
        {
            textmeshGameObject = GetComponentInChildren<TextMesh>().gameObject;
            ren = GetComponent<Renderer>();
            col = GetComponent<SphereCollider>();
        }

        public override void Select()
        {
            ClickAction.Invoke(Index);           
        }

        void LateUpdate()
        {
            if (isFiltered) return;

            var lookPos = Camera.main.transform.position - transform.position;
            var distance = Vector3.Distance(Camera.main.transform.position, transform.position);
            
            textmeshGameObject.gameObject.SetActive(distance < 1200);

            lookPos.y = 0;
            transform.rotation = Quaternion.LookRotation(lookPos);

        }
    }

}