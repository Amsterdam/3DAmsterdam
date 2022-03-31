using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Interface;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    public enum GizmoMode
    {
        None,
        Move,
        Rotate,
    }

    public class UitbouwTransformGizmo : DragableAxis
    {
        public GizmoMode Mode { get; private set; }
        public Vector3 GizmoPoint => transform.position + transform.right * Radius;
        public float Radius => transform.localScale.x / 2;

        private SpriteRenderer spriteRenderer;
        [SerializeField]
        private Sprite moveSprite, rotateSprite;

        public UitbouwTransformGizmoButtons GizmoButtons { get; private set; }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            GizmoButtons = CoordinateNumbers.Instance.CreateUitbouwTransformGizmoButtons();
            GizmoButtons.SubscribeListeners(this);
        }

        protected override void Update()
        {
            base.Update();
            GizmoButtons.AlignWithWorldPosition(GizmoPoint);
        }

        public void SetDiameter(float d)
        {
            transform.localScale = new Vector3(d, d, d);
        }

        public void SetActive(bool active)
        {
            if (!active)
                SetMode(GizmoMode.None);
            else
                SetMode(GizmoMode.Move);

            GizmoButtons.SetActive(active);
            gameObject.SetActive(active);
        }

        public void SetMode(GizmoMode newMode)
        {
            Mode = newMode;
            SetImage(newMode);
        }

        public void SetImage(GizmoMode mode)
        {
            if (mode == GizmoMode.Move)
                spriteRenderer.sprite = moveSprite;
            else
                spriteRenderer.sprite = rotateSprite;
        }
    }
}