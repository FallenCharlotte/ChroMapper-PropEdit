// From ChroMapper_RhythmMarker

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ChroMapper_PropEdit.Components {

public class DragWindowController : MonoBehaviour, IDragHandler, IEndDragHandler
{
	public float canvas_scale = 1f;
	public event Action? onDragWindow;
	
	public DragWindowController Init(Canvas canvas, Action onDragWindow) {
		this.canvas_scale = canvas.scaleFactor;
		this.onDragWindow = onDragWindow;
		
		return this;
	}
	
	public void OnDrag(PointerEventData eventData) {
		var target = GetComponentInParent<Window>();
		target.GetComponent<RectTransform>().anchoredPosition += eventData.delta / canvas_scale;
	}
	
	public void OnEndDrag(PointerEventData eventData) {
		onDragWindow?.Invoke();
	}
}

}
