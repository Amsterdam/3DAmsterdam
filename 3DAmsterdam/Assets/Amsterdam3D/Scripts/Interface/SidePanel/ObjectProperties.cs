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

        public DisplayBAGData displayBagData;

        public static ObjectProperties Instance = null;

        [SerializeField]
        private Text titleText;

        [Header("Generated field prefabs:")]
        [SerializeField]
        private GameObject subtitlePrefab;
        [SerializeField]
        private DataKeyAndValue dataFieldPrefab;
        [SerializeField]
        private GameObject seperatorLinePrefab;
        [SerializeField]
        private NameAndURL urlPrefab;

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
            titleText.text = title;
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
        public void AddURLText(string urlText, string urlPath)
        {
            Instantiate(urlPrefab, generatedFieldsContainer).SetURL(urlText,urlPath);
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