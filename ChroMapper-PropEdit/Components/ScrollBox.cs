using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit.Components {

public class ScrollBox : MonoBehaviour {
	public GameObject? content;
	
	public Scrollbar? scrollbar;
	public float? TargetScroll;
	
	public static ScrollBox Create(GameObject parent) {
		return UI.AddChild(parent, "Scroll Box").AddComponent<ScrollBox>().Init();
	}
	
	public ScrollBox Init() {
		UI.AttachTransform(gameObject, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1));
		
		content = UI.AddChild(gameObject, "Scroll Content");
		var target = UI.AttachTransform(content!, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1));
		
		var mask = gameObject.AddComponent<RectMask2D>();
		var scrollrect = gameObject.AddComponent<ScrollRect>();
		scrollrect.vertical = true;
		scrollrect.horizontal = false;
		scrollrect.scrollSensitivity = 42.069f;
		scrollrect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
		scrollrect.content = target;
		
		var scroller = UI.AddChild(gameObject, "Scroll Bar", typeof(Scrollbar));
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
	
	private const float over_scoll = 10f;
	public void ScrollTo(GameObject target) {
		Vector3[] target_corners = new Vector3[4];
		Vector3[] mask_corners = new Vector3[4];
		Vector3[] content_corners = new Vector3[4];
		((RectTransform)target.transform).GetWorldCorners(target_corners);
		((RectTransform)transform).GetWorldCorners(mask_corners);
		((RectTransform)content!.transform).GetWorldCorners(content_corners);
		var content_height = content_corners[1].y - content_corners[0].y;
		var mask_height = mask_corners[1].y - mask_corners[0].y;
		var current_scroll = scrollbar!.value;
		if (target_corners[0].y - mask_corners[0].y < 0) {
			StartCoroutine(SmoothScroll((target_corners[0].y - mask_corners[0].y - over_scoll) / (content_height - mask_height)));
		}
		else if (mask_corners[1].y - target_corners[1].y < 0) {
			StartCoroutine(SmoothScroll((target_corners[1].y - mask_corners[1].y + over_scoll) / (content_height - mask_height)));
		}
	}
	
	public void ScrollTop() {
		if (!isActiveAndEnabled) {
			scrollbar!.value = 1f;
			return;
		}
		StartCoroutine(SmoothScroll(1 - scrollbar!.value));
	}
	
	private const float anim_dur = 0.25f;
	private IEnumerator SmoothScroll(float value_delta) {
		float time_start = Time.time;
		var value_start = scrollbar!.value;
		while ((Time.time - time_start) < anim_dur) {
			scrollbar!.value = value_start + ((Easing.Cubic.Out((Time.time - time_start) / anim_dur)) * value_delta);
			yield return new WaitForEndOfFrame();
		}
	}
	
	public void Awake() {
		StartCoroutine(DirtyPanel());
	}
	
	// From CM's StrobeGeneratorControllerUI, leaving the original comment because it needs repeating
	// Unity is a fantastic game engine with no flaws whatsoever.
	// Just kidding. It's shit. This shouldn't be necessary. Why am I being forced to go this route so that Unity UI can update the way that it's supposed to god fucking damnit i have lost all hope in the unity engine by spending one hour of my life just to waste a frame (and get a flickering effect) by having to write this ienumerator god dufkcinhjslkajdfklwa
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members",
		Justification = "This is called indirectly via Unity Message.")]
	private IEnumerator DirtyPanel()
	{
		// One at a time
		if (dirty) yield break;
		dirty = true;
		// Wait some frames then wiggle
		var target = GetComponentInParent<Window>();
		//yield return 5;
		
		for (var i = 0; i < 10; ++i) {
			target.GetComponent<RectTransform>().sizeDelta += new Vector2(0.25f, 0);
			if (TargetScroll != null) {
				scrollbar!.value = TargetScroll ?? 1;
			}
			yield return new WaitForEndOfFrame();
			target.GetComponent<RectTransform>().sizeDelta += new Vector2(-0.25f, 0);
			if (TargetScroll != null) {
				scrollbar!.value = TargetScroll ?? 1;
			}
			yield return 1;
			
		}
		if (TargetScroll != null) {
			scrollbar!.value = TargetScroll ?? 1;
		}
		TargetScroll = null;
		dirty = false;
	}
	
	private bool dirty = false;
}

}
