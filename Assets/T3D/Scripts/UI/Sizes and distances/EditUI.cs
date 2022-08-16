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

        void LateUpdate()
        {
            var labels = ServiceLocator.GetService<CoordinateNumbers>().GetComponentsInChildren<NumberInputField>();
            foreach (var label in labels)
            {
                var otherRect = label.GetComponent<RectTransform>();
                
                if (rectTransform.Overlaps(otherRect))
                {
                    var diff = rectTransform.rect.height + otherRect.rect.height;
                    rectTransform.position += Vector3.up * diff;
                    rectTransform.ForceUpdateRectTransforms();
                }
            }

        }
    }
}
