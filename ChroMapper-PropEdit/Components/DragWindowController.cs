// From ChroMapper_RhythmMarker

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ChroMapper_PropEdit.Components {

public class DragWindowController : MonoBehaviour, IDragHandler
{
	public Canvas canvas { set; get; }
	public event Action OnDragWindow;

	public void OnDrag(PointerEventData eventData)
	{
		gameObject.GetComponent<RectTransform>().anchoredPosition += eventData.delta / canvas.scaleFactor;
		OnDragWindow?.Invoke();
	}
}

}
