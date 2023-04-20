// From ChroMapper_RhythmMarker

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ChroMapper_PropEdit.Components {

public class DragWindowController : MonoBehaviour, IDragHandler, IEndDragHandler
{
	public Canvas canvas { set; get; }
	public event Action onDragWindow;
	
	public DragWindowController Init(Canvas canvas, Action onDragWindow) {
		this.canvas = canvas;
		this.onDragWindow = onDragWindow;
		
		return this;
	}
	
	public void OnDrag(PointerEventData eventData) {
		var target = GetComponentInParent<Window>();
		target.GetComponent<RectTransform>().anchoredPosition += eventData.delta / canvas.scaleFactor;
	}
	
	public void OnEndDrag(PointerEventData eventData) {
		onDragWindow?.Invoke();
	}
}

}
