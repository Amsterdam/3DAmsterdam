using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
    public class ObjectProperties : MonoBehaviour
    {
        [SerializeField]
        private GameObject objectPropertiesPanel;

        [SerializeField]
        private Transform generatedFieldsContainer;

        public static ObjectProperties Instance = null;

        [SerializeField]
        private string pandTitlePrefix = "Pand: ";

        [Header("Generated field prefabs:")]
        [SerializeField]
        private GameObject subtitlePrefab;
        [SerializeField]
        private DataKeyAndValue dataFieldPrefab;
        [SerializeField]
        private GameObject seperatorLinePrefab;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            //Properties panel is disabled at startup
            objectPropertiesPanel.SetActive(false);
        }

        public void OpenPanel(string title)
        {
            ClearGeneratedFields();

            objectPropertiesPanel.SetActive(true);
            pandTitlePrefix = title;
        }
        public void ClosePanel()
        {
            objectPropertiesPanel.SetActive(false);
        }

        public void AddSubtitle(string titleText)
        {
            Instantiate(subtitlePrefab, generatedFieldsContainer).GetComponent<Text>().text = titleText;
        }
        public void AddDataField(string keyTitle, string valueText)
        {
            Instantiate(dataFieldPrefab, generatedFieldsContainer).SetTexts(keyTitle, valueText);
        }
        public void AddSeperatorLine()
        {
            Instantiate(seperatorLinePrefab, generatedFieldsContainer);
        }

        public void ClearGeneratedFields()
        {
            foreach (Transform field in generatedFieldsContainer)
            {
                Destroy(field.gameObject);
            }
        }
    }
}