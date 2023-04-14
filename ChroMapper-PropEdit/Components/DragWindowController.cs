// From ChroMapper_RhythmMarker

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ChroMapper_PropEdit.Components {

public class DragWindowController : MonoBehaviour, IDragHandler, IEndDragHandler
{
	public Canvas canvas { set; get; }
	public event Action OnDragWindow;

	public void OnDrag(PointerEventData eventData)
	{
		gameObject.GetComponent<RectTransform>().anchoredPosition += eventData.delta / canvas.scaleFactor;
	}
	
	public void OnEndDrag(PointerEventData eventData) {
		OnDragWindow?.Invoke();
	}
}

}
