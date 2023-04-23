using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit.Components {

public class ScrollBox : MonoBehaviour {
	public GameObject? content;
	
	public Scrollbar? scrollbar;
	
	public ScrollBox Init(Transform parent) {
		content = UI.AddChild(parent, "Scroll Content");
		var target = content.AddComponent<RectTransform>();
		
		var mask = gameObject.AddComponent<RectMask2D>();
		var scrollrect = gameObject.AddComponent<ScrollRect>();
		scrollrect.vertical = true;
		scrollrect.horizontal = false;
		scrollrect.scrollSensitivity = 42.069f;
		scrollrect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
		scrollrect.content = target;
		
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
		
		{
			var layout = content.AddComponent<VerticalLayoutGroup>();
			layout.padding = new RectOffset(5, 10, 0, 0);
			layout.spacing = 0;
			layout.childControlHeight = false;
			layout.childControlWidth = true;
			layout.childForceExpandHeight = false;
			layout.childForceExpandWidth = true;
			layout.childAlignment = TextAnchor.UpperCenter;
		}
		{
			var layout = content.AddComponent<LayoutElement>();
		}
		{
			var fitter = content.AddComponent<ContentSizeFitter>();
			fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		}
		
		return this;
	}
	
	public void Awake() {
		StartCoroutine(DirtyPanel(false));
	}
	
	public void ScrollToTop() {
		if (isActiveAndEnabled) {
			StartCoroutine(scroll_to_top());
		}
	}
	
	// From CM's StrobeGeneratorControllerUI, leaving the original comment because it needs repeating
	// Unity is a fantastic game engine with no flaws whatsoever.
	// Just kidding. It's shit. This shouldn't be necessary. Why am I being forced to go this route so that Unity UI can update the way that it's supposed to god fucking damnit i have lost all hope in the unity engine by spending one hour of my life just to waste a frame (and get a flickering effect) by having to write this ienumerator god dufkcinhjslkajdfklwa
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members",
		Justification = "This is called indirectly via Unity Message.")]
	private IEnumerator DirtyPanel(bool first)
	{
		var layout = content!.GetComponent<VerticalLayoutGroup>();
		if (first) {
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
		}
		layout.enabled = false;
		yield return new WaitForEndOfFrame();
		layout.enabled = true;
		yield return new WaitForEndOfFrame();
		scrollbar!.value = 1f;
		yield return new WaitForEndOfFrame();
		scrollbar!.value = 1f;
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