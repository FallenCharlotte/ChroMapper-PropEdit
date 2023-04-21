using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit.Components {

public class ScrollBox : MonoBehaviour {
	public RectTransform? content;
	
	public Scrollbar? scrollbar;
	
	public ScrollBox Init(RectTransform content) {
		this.content = content;
		return this;
	}
	
	public void ScrollToTop() {
		if (isActiveAndEnabled) {
			StartCoroutine(scroll_to_top());
		}
	}
	
	public void Start() {
		var mask = gameObject.AddComponent<RectMask2D>();
		var scrollrect = gameObject.AddComponent<ScrollRect>();
		scrollrect.vertical = true;
		scrollrect.horizontal = false;
		scrollrect.scrollSensitivity = 42.069f;
		scrollrect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
		scrollrect.content = content;
		
		var scroller = UI.AddChild(gameObject.transform.parent, "Scroll Bar", typeof(Scrollbar));
		UI.AttachTransform(scroller, new Vector2(10, 0), new Vector2(-5.5f, 0), new Vector2(1, 0), new Vector2(1, 1));
		scrollbar = scroller.GetComponent<Scrollbar>();
		scrollbar.transition = Selectable.Transition.ColorTint;
		scrollbar.direction = Scrollbar.Direction.BottomToTop;
		scrollbar.value = 1f;
		scrollrect.verticalScrollbar = scrollbar;
		
		var slide = UI.AddChild(scroller, "Slide");
		UI.AttachTransform(slide, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1));
		{
			var image = slide.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.24f, 0.24f, 0.24f, 1);
		}
		
		var handle = UI.AddChild(slide, "Handle", typeof(Canvas));
		UI.AttachTransform(handle, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1));
		{
			var image = handle.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.7f, 0.7f, 0.7f, 1);
			scrollbar.targetGraphic = image;
			scrollbar.handleRect = handle.GetComponent<RectTransform>();
		}
	}
	
	private IEnumerator scroll_to_top() {
		for (int i = 0; i < 3; ++i) {
			if (scrollbar != null) {
				scrollbar.value = 1f;
			}
			yield return new WaitForEndOfFrame();
		}
		yield return null;
	}
}

}
