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

        Vector3 offset = new Vector3(-35, -35, 0);
        float moveSpeed = 1f;
        //protected override void Update()
        void LateUpdate()
        {
            //base.Update();

            if (Input.GetKey(KeyCode.A))
            {
                moveSpeed -= 1;
                print(moveSpeed);
            }

            if (Input.GetKey(KeyCode.S))
            {
                moveSpeed += 1;
                print(moveSpeed);
            }

            var labels = CoordinateNumbers.Instance.GetComponentsInChildren<NumberInputField>();
            foreach (var label in labels)
            {
                if (label == rectTransform)
                {
                    continue;
                }
                var otherRect = label.GetComponent<RectTransform>();
                if (rectTransform.Overlaps(otherRect))
                {
                    var dir = otherRect.position - rectTransform.position;
                    print(dir);
                    //rectTransform.position -= offset * moveSpeed;
                    //break;
                }
            }

        }
    }
}
