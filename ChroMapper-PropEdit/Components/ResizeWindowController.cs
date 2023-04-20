using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChroMapper_PropEdit.Components {

public class ResizeWindowController : MonoBehaviour, IDragHandler, IEndDragHandler
{
	public Canvas canvas { set; get; }
	public event Action onResizeWindow;
	
	public Vector2 mask = Vector2.zero;
	
	public ResizeWindowController Init(Canvas canvas, Action onResizeWindow, Vector2 mask) {
		this.canvas = canvas;
		this.onResizeWindow = onResizeWindow;
		this.mask = mask;
		
		return this;
	}
	
	public void Start() {
		var image = gameObject.AddComponent<Image>();
		image.color = new Color(0, 0, 0, 0);
	}
	
	public void OnDrag(PointerEventData eventData) {
		var target = GetComponentInParent<Window>();
		var delta = Vector2.Scale(mask, eventData.delta / canvas.scaleFactor);
		target.GetComponent<RectTransform>().sizeDelta += Vector2.Scale(delta, new Vector2(1, -1));
		target.GetComponent<RectTransform>().anchoredPosition += delta / 2;
	}
	
	public void OnEndDrag(PointerEventData eventData) {
		onResizeWindow?.Invoke();
	}
}

}
