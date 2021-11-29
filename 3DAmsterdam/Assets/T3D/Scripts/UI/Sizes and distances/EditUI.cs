using Netherlands3D.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw.BoundaryFeatures
{
    public class EditUI : WorldPointFollower
    {
        [SerializeField]
        private Button editButton, deleteButton;
        [SerializeField]
        private Image editButtonImage;
        [SerializeField]
        private Sprite editSprite, moveSprite;

        private BoundaryFeature activeFeature;

        //private RectTransform rectTransform;

        //public override void Awake()
        //{
        //    base.Awake();
        //    rectTransform = GetComponent<RectTransform>();
        //}

        public void UpdateSprites(EditMode newMode)
        {
            if (newMode == EditMode.Resize)
                editButtonImage.sprite = moveSprite;
            else
                editButtonImage.sprite = editSprite;
        }

        private void OnDestroy()
        {
            deleteButton.onClick.RemoveAllListeners();
        }

        public void SetFeature(BoundaryFeature feature)
        {
            activeFeature = feature;

            editButton.onClick.AddListener(activeFeature.EditFeature);
            deleteButton.onClick.AddListener(activeFeature.DeleteFeature);
        }

        //Vector3 offsetdirection = new Vector3(0, 1, 0).normalized;
        //float offsetMultiplier = -35f;
        //protected override void Update()
        void LateUpdate()
        {
            var labels = CoordinateNumbers.Instance.GetComponentsInChildren<NumberInputField>();
            //var addExtraHeightOffset = true;

            //var offsetdirection = new Vector3();
            foreach (var label in labels)
            {
                var otherRect = label.GetComponent<RectTransform>();
                
                if (rectTransform.Overlaps(otherRect))
                {
                    Debug.Log(otherRect, otherRect);
                    //var dir = otherRect.position - rectTransform.position;

                    var diff = (rectTransform.rect.height + otherRect.rect.height);
                    rectTransform.position += Vector3.up * diff;
                    rectTransform.ForceUpdateRectTransforms();

                    //if (offsetdirection == Vector3.zero)
                    //{
                    //    offsetdirection = new Vector3(0, dir.y, 0).normalized;
                    //}
                    //var diff = (rectTransform.rect.height + otherRect.rect.height) / 1.5f;

                    //if (offsetdirection.y > 0)
                    //{
                    //    print("up");
                    //    rectTransform.position += offsetdirection * (diff - dir.y);
                    //}
                    //else
                    //{
                    //    print("down");
                    //    rectTransform.position -= offsetdirection * (diff - dir.y);
                    //}
                }
            }

        }
    }
}
