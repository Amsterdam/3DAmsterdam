using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropUp : MonoBehaviour
{
    public DropUpTemplate Template;
    public GameObject ItemsContainer;
    public Color SelectedColor;
    public Color DefaultColor;

    List<DropUpTemplate> dropupTemplateItems = new List<DropUpTemplate>();

    private void Start()
    {
        var btn =GetComponent<Button>();

        //toggle show
        btn.onClick.AddListener(() => {            
            ItemsContainer.SetActive(!ItemsContainer.activeSelf);
        });

    }

    public bool ishovered;

    public void SetItems(List<string> items, int selectedIndex, Action<int> callback)
    {        

        for(int i = 0; i < items.Count ; i++)
        {
            var item = Instantiate(Template);
            dropupTemplateItems.Add(item);

            item.name = items[i];
            item.transform.SetParent(ItemsContainer.transform, false);
            item.transform.localScale = Vector3.one;
            item.Initialize(items[i]);

            var rect = item.GetComponent<RectTransform>();
            
            rect.localPosition = new Vector3(0, rect.rect.height * (items.Count - i), 0);

            var button = item.GetComponent<Button>();
            var month = i;

            button.onClick.AddListener(() => {                
                UpdateSelectedColor(month);
                callback(month);
                ishovered = false;
                ItemsContainer.SetActive(false);                
            });
            
        }
        UpdateSelectedColor(selectedIndex);

    }

    private void UpdateSelectedColor(int selectedindex)
    {
        for (int i = 0; i < dropupTemplateItems.Count; i++)
        {
            var color = i == selectedindex ? SelectedColor : DefaultColor;
            dropupTemplateItems[i].SetColor(color);
        }
        
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var curgam = EventSystem.current.currentSelectedGameObject;
            var ischild = curgam != null && curgam.transform != transform && curgam.transform.IsChildOf(transform);

            if (ischild == false)
            {
                ItemsContainer.SetActive(false);
            }

        }
        
    }


}
