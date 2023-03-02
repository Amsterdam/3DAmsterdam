using UnityEngine;
using UnityEngine.UI;
using Netherlands3D.TileSystem;
using Netherlands3D.ObjectInteraction;
using Netherlands3D.Events;
using System.Collections.Generic;
using TMPro;

namespace Netherlands3D.Interface.SidePanel
{
	public class SelectionOutliner : MonoBehaviour
	{
		private string id = "";
		private string title = "";

		[SerializeField]
		private TextMeshProUGUI titleText;

		private GameObject linkedGameObject;

		[SerializeField]
		private StringListEvent onSelect;

		public GameObject LinkedGameObject { get => linkedGameObject; set => linkedGameObject = value; }
		public string Id { get => id; set => id = value; }
		public string Title {
			get => title;
			set
			{
				title = value;
				titleText.text = title;
			}
		}

		private void Update()
		{
			if (!LinkedGameObject) Close();
		}

		public void Link(GameObject targetGameObject, string title, string id = "")
		{
			linkedGameObject = targetGameObject;
			Title = title;
			Id = id;
		}

		public void Select()
		{	
			onSelect.InvokeStarted(new List<string> { Id });
		}

		public void Close()
		{
			//Did we close a SelectByID selection?
			var selectSubObjects = linkedGameObject.GetComponent<SelectSubObjects>();
			if (selectSubObjects)
			{
				selectSubObjects.DeselectSpecificID(Id);
				return;
			}

			//Maybe another simple interactable?
			var selectable = linkedGameObject.GetComponent<Interactable>();
			if (selectable)
			{
				selectable.Deselect();
			}

		}
	}
}