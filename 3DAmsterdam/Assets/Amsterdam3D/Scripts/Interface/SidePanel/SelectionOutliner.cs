using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface {
	public class SelectionOutliner : MonoBehaviour
	{
		private string id = "";
		private string title = "";

		[SerializeField]
		private Text titleText;

		private GameObject linkedGameObject;

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
			var selectByID = linkedGameObject.GetComponent<SelectByID>();
			if (selectByID)
			{
				selectByID.ShowBAGDataForSelectedID(Id);
				return;
			}
		}

		public void Close()
		{
			//Did we close a SelectByID selection?
			var selectByID = linkedGameObject.GetComponent<SelectByID>();
			if (selectByID)
			{
				selectByID.DeselectSpecificID(Id);
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