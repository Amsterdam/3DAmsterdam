using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Netherlands3D.T3D.Uitbouw
{
    public class UitbouwMuurSaveData : SaveDataContainer
    {
        public UitbouwMuurSaveData(string instanceId) : base(instanceId)
        {
        }

        public int MaterialIndex = -1;
        public Vector2 TextureScale = Vector2.one;
    }

    public enum WallSide
    {
        Left,
        Right,
        Top,
        Bottom,
        Front,
        Back,
    }

    //[RequireComponent(typeof(CitySurface))]
    public class UitbouwMuur : SquareSurface
    {
        [SerializeField]
        private WallSide side;
        public WallSide Side => side;

        private UitbouwMuur left;
        private UitbouwMuur right;
        private UitbouwMuur top;
        private UitbouwMuur bottom;

        public override Vector3 LeftBoundPosition => WallPlane.ClosestPointOnPlane(leftBound.position);
        public override Vector3 RightBoundPosition => WallPlane.ClosestPointOnPlane(rightBound.position);
        public override Vector3 TopBoundPosition => WallPlane.ClosestPointOnPlane(topBound.position);
        public override Vector3 BottomBoundPosition => WallPlane.ClosestPointOnPlane(bottomBound.position);

        private Vector3 oldPosition;
        public Vector3 deltaPosition { get; private set; }

        private MeshFilter meshFilter;
        public MeshFilter MeshFilter => meshFilter;
        private MeshRenderer meshRenderer;

        public Plane WallPlane => new Plane(-transform.forward, transform.position);

        private Material normalMaterial;
        //public Material Material => normalMaterial;
        public Material Material
        {
            get
            {
                if (saveData.MaterialIndex < 0)
                    return normalMaterial;

                return MaterialLibrary.GetMaterial(saveData.MaterialIndex);
            }
        }
        public int MaterialIndex => saveData.MaterialIndex;
        [SerializeField]
        private Material highlightMaterial;

        //private Vector2 textureScale = new Vector2(0.3f, 0.3f);
        private UitbouwMuurSaveData saveData;
        public Vector2 TextureScale => saveData.TextureScale;

        protected override void Awake()
        {
            base.Awake();

            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = meshFilter.GetComponent<MeshRenderer>();
            oldPosition = transform.position;

            left = leftBound.GetComponent<UitbouwMuur>();
            right = rightBound.GetComponent<UitbouwMuur>();
            top = topBound.GetComponent<UitbouwMuur>();
            bottom = bottomBound.GetComponent<UitbouwMuur>();

            normalMaterial = meshRenderer.material;

            saveData = new UitbouwMuurSaveData(Side.ToString());
        }

        private void OnEnable()
        {
            MaterialLibrary.MaterialLibraryLoaded += MaterialLibrary_MaterialLibraryLoaded;
        }

        private void MaterialLibrary_MaterialLibraryLoaded(Material[] materials)
        {
            if (saveData.MaterialIndex >= 0 && saveData.MaterialIndex < materials.Length)
            {
                var savedMaterial = materials[saveData.MaterialIndex];
                SetMaterial(savedMaterial, saveData.TextureScale);
            }
        }

        private void OnDisable()
        {
            MaterialLibrary.MaterialLibraryLoaded -= MaterialLibrary_MaterialLibraryLoaded;
        }

        public void RecalculateSides(Vector3 newPosition)
        {
            deltaPosition = newPosition - oldPosition;
            oldPosition = transform.position;
            transform.position = newPosition;

            left.RecalculatePosition(deltaPosition / 2);
            right.RecalculatePosition(deltaPosition / 2);
            top.RecalculatePosition(deltaPosition / 2);
            bottom.RecalculatePosition(deltaPosition / 2);

            left.RecalculateScale();
            right.RecalculateScale();
            top.RecalculateScale();
            bottom.RecalculateScale();

            left.RecalculateMaterialTiling();
            right.RecalculateMaterialTiling();
            top.RecalculateMaterialTiling();
            bottom.RecalculateMaterialTiling();
        }

        public void RecalculateMaterialTiling()
        {
            normalMaterial.mainTextureScale = Size * saveData.TextureScale;
        }

        public void SetHighlightActive(bool enable)
        {
            meshRenderer.material = enable ? highlightMaterial : normalMaterial;
        }

        public void RecalculatePosition(Vector3 delta)
        {
            transform.position += delta;
        }

        public void SetActive(bool active)
        {
            oldPosition = transform.position;
            deltaPosition = Vector3.zero;
            //isActive = active;
        }

        public void MoveWall(float delta)
        {
            SetActive(true);
            var newPosition = transform.position + transform.forward * -delta;
            //transform.position += transform.forward * -delta;
            RecalculateSides(newPosition);
            SetActive(false);
        }

        public void SetMaterial(Material newMaterial, Vector2 scale)
        {
            var newMaterialInstance = new Material(newMaterial);
            //Instantiate(newMaterial);
            normalMaterial = newMaterialInstance;
            if (meshRenderer.material != highlightMaterial)
                meshRenderer.material = newMaterialInstance;

            saveData.MaterialIndex = MaterialLibrary.GetMaterialIndex(newMaterial);
            saveData.TextureScale = scale;
            RecalculateMaterialTiling();
        }
    }
}