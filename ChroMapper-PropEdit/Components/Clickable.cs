using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ChroMapper_PropEdit.Components {

public class Clickable : MonoBehaviour, IPointerClickHandler
{
	public Action? onClick;
	
	public void OnPointerClick(PointerEventData pointerEventData) {
		onClick?.Invoke();
	}
}

}
