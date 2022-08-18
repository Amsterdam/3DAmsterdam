using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.Interface
{
	public class OnFocusEffect : MonoBehaviour, ISelectHandler, IDeselectHandler
	{
		[Tooltip("Can be any type of MonoBehaviour. A UI Outline, an Image, your own script etc.")]
		[SerializeField]
		private MonoBehaviour effect;

		public void OnSelect(BaseEventData eventData)
		{
			effect.enabled = true;
		}

		public void OnDeselect(BaseEventData eventData)
		{
			effect.enabled = false;
		}
	}
}