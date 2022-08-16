using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Netherlands3D.T3D.Uitbouw
{
    public class ShapeableUitbouw : UitbouwBase
    {
        //public CityObject uitbouwCityObject; //todo

        private UitbouwMuur left => GetWall(WallSide.Left);
        private UitbouwMuur right => GetWall(WallSide.Right);
        private UitbouwMuur bottom => GetWall(WallSide.Bottom);
        private UitbouwMuur top => GetWall(WallSide.Top);
        private UitbouwMuur front => GetWall(WallSide.Front);
        private UitbouwMuur back => GetWall(WallSide.Back);

        [SerializeField]
        private UitbouwMuur[] walls;

        public override Vector3 LeftCenter => left.transform.position;

        public override Vector3 RightCenter => right.transform.position;

        public override Vector3 TopCenter => top.transform.position;

        public override Vector3 BottomCenter => bottom.transform.position;

        public override Vector3 FrontCenter => front.transform.position;

        public override Vector3 BackCenter => back.transform.position;

        //[SerializeField]
        //private Color highlightColor;
        //[SerializeField]
        //private Color normalColor;

        //public Color HighlightColor => highlightColor;
        //public Color NormalColor => normalColor;

        public override void UpdateDimensions()
        {
            SetDimensions(left, right, bottom, top, front, back);
        }

        private void SetDimensions(UitbouwMuur left, UitbouwMuur right, UitbouwMuur bottom, UitbouwMuur top, UitbouwMuur front, UitbouwMuur back)
        {
            var widthVector = Vector3.Project(right.transform.position - left.transform.position, left.transform.forward);
            var heightVector = Vector3.Project(top.transform.position - bottom.transform.position, bottom.transform.forward);
            var depthVector = Vector3.Project(back.transform.position - front.transform.position, front.transform.forward);

            SetDimensions(widthVector.magnitude, depthVector.magnitude, heightVector.magnitude);
        }

        protected override void Awake()
        {
            base.Awake();
            UpdateDimensions(); //set the dimensions before these are used to calculate the wall offsets of the saved data
        }

        protected override void Update()
        {
            UpdateDimensions();
            base.Update();
        }

        public void MoveWall(WallSide side, float delta)
        {
            //safety deactivation
            foreach (var wall in walls)
            {
                wall.SetActive(false);
            }

            UitbouwMuur activeWall = GetWall(side);
            activeWall.MoveWall(delta);
        }

        public UitbouwMuur GetWall(WallSide side)
        {
            var muur = walls.FirstOrDefault(x => x.Side == side);
            return muur;
        }

        public void SetWallHighlightActive(WallSide side, bool enable)
        {
            var wall = GetWall(side);
            wall.SetHighlightActive(enable);
        }
    }
}