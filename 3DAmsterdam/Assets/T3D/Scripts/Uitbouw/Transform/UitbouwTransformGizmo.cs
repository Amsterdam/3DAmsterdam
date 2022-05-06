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
        Measure,
    }

    public class UitbouwTransformGizmo : DragableAxis
    {
        public GizmoMode Mode { get; private set; } = GizmoMode.None;
        public Vector3 GizmoPoint => transform.position + RestrictionChecker.ActiveUitbouw.transform.rotation * RestrictionChecker.ActiveUitbouw.Extents;
        public float Radius => transform.localScale.x / 2;

        private SpriteRenderer spriteRenderer;
        [SerializeField]
        private Sprite moveSprite, rotateSprite, measureSprite;

        public UitbouwTransformGizmoButtons GizmoButtons { get; private set; }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        protected override void Update()
        {
            base.Update();
            if (GizmoButtons)
                GizmoButtons.AlignWithWorldPosition(GizmoPoint);
        }

        public void SetDiameter(float d)
        {
            transform.localScale = new Vector3(d, d, d);
        }

        public void SetActive(bool active, bool allowMove = true, bool allowRotate = true, bool allowMeasure = true)
        {
            if (!active)
                SetMode(GizmoMode.None);
            else
                SetMode(GizmoMode.Move);

            if (!GizmoButtons)
            {
                GizmoButtons = ServiceLocator.GetService<CoordinateNumbers>().CreateUitbouwTransformGizmoButtons();
                GizmoButtons.SubscribeListeners(this);
            }

            GizmoButtons.SetActive(active, allowMove, allowRotate, allowMeasure);
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
            else if (mode == GizmoMode.Rotate)
                spriteRenderer.sprite = rotateSprite;
            else
                spriteRenderer.sprite = measureSprite;
        }
    }
}