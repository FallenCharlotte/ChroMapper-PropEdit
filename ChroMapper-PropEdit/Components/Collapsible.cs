// Derived (very far) from CM's StrobeGeneratorPassUIController
using System.Collections;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit.Components {

public class Collapsible : MonoBehaviour
{
	public Toggle? expandToggle;
	public GameObject? panel;
	
	public UnityAction<bool>? OnAnimationComplete;
	
	private string? settings_key;
	private float? height_cache;
	
	public bool Expanded {
		get { return expandToggle!.isOn && anim == null; }
		set { SetExpanded(value); }
	}
	
	public static Collapsible Create(GameObject parent, string name, string label, bool expanded) {
		return Create(parent, name, label, expanded, "");
	}
	public static Collapsible Create(GameObject parent, string name, string label, bool expanded, string tooltip) {
		return UI.AddChild(parent, name).AddComponent<Collapsible>().Init(label, expanded, tooltip);
	}
	public static Collapsible Singleton(GameObject parent, string name, string label, bool expanded, string tooltip) {
		var go = parent.transform.Find(name)?.GetComponent<Collapsible>();
		if (go == null) {
			go = UI.AddChild(parent, name).AddComponent<Collapsible>().Init(label, expanded, tooltip);
		}
		return go;
	}
	
	public Collapsible Init(string label, bool expanded, string tooltip = "", bool background = true) {
		if (!gameObject.name.StartsWith("_")) {
			settings_key = $"ExpanderStates.{gameObject.name}";
		}
		UI.AttachTransform(gameObject, new Vector2(0, 20), pos: new Vector2(0, 0));
		{
			var image = gameObject.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.5f, 0.5f, 0.5f, 1);
		}
		{
			var layout = gameObject.AddComponent<VerticalLayoutGroup>();
			layout.padding = new RectOffset(2, 2, 2, 2);
			layout.spacing = 0;
			layout.childControlHeight = false;
			layout.childControlWidth = true;
			layout.childForceExpandHeight = false;
			layout.childForceExpandWidth = true;
			layout.childAlignment = TextAnchor.UpperCenter;
		}
		{
			var csf = gameObject.AddComponent<ContentSizeFitter>();
			csf.verticalFit = ContentSizeFitter.FitMode.MinSize;
		}
		
		var header = UI.AddField(gameObject, label, null, tooltip);
		// Invisible image to make it clickable... Unity is a truly magical engine
		{
			var image = header.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0, 0, 0, 0);
		}
		header.AddComponent<Clickable>().onClick += () => {
			expandToggle!.isOn = !expandToggle!.isOn;
		};
		{
			var _label = header.transform.GetChild(0).gameObject;
			_label.GetComponent<RectTransform>().anchoredPosition += new Vector2(5, 0);
		}
		if (settings_key != null) {
			expanded = Settings.Get(settings_key, expanded);
		}
		expandToggle = UI.AddCheckbox(header, expanded, SetExpanded);
		Sprite arrow = UI.GetSprite("ArrowIcon");
		
		Image[] images = expandToggle.gameObject.GetComponentsInChildren<Image>();
		foreach (var image in images) {
			image.sprite = arrow;
			((RectTransform)image.gameObject.transform).sizeDelta = new Vector2(12, 9);
		}
		((RectTransform)expandToggle.transform).anchoredPosition = new Vector2(-15, 0);
		
		
		panel = UI.AddChild(gameObject, "Panel");
		UI.AttachTransform(panel, new Vector2(0, 50), pos: Vector2.zero);
		panel.AddComponent<RectMask2D>();
		if (background)
		{
			var image = panel.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.2f, 0.2f, 0.2f, 1);
		}
		{
			var layout = panel.AddComponent<LayoutElement>();
		}
		{
			var csf = panel.AddComponent<ContentSizeFitter>();
			csf.verticalFit = ContentSizeFitter.FitMode.MinSize;
		}
		{
			var layout = panel.AddComponent<VerticalLayoutGroup>();
			layout.padding = new RectOffset(5, 5, 5, 5);
			layout.spacing = 0;
			layout.childControlHeight = false;
			layout.childControlWidth = true;
			layout.childForceExpandHeight = false;
			layout.childForceExpandWidth = true;
			layout.childAlignment = TextAnchor.UpperCenter;
		}
		
		SetExpandedNow(expanded, true);
		
		return this;
	}
	
	public void Awake() {
		SendMessageUpwards("DirtyPanel");
	}
	
	private const float anim_dur = 0.25f;
	
	private IEnumerator? anim = null;
	private IEnumerator AnimExpanded(bool expanded) {
		var rect = (panel!.transform as RectTransform)!;
		var size = rect.sizeDelta;
		SetExpandedNow(expanded, false);
		float time_start = Time.time;
		if (expanded) {
			panel!.SetActive(expanded);
			if (height_cache == null) {
				yield return new WaitForEndOfFrame();
				height_cache = rect.sizeDelta.y;
			}
			do {
				size.y = (Easing.Cubic.Out((Time.time - time_start) / anim_dur)) * height_cache ?? 0;
				rect.sizeDelta = size;
				LayoutRebuilder.MarkLayoutForRebuild(transform.parent as RectTransform);
				yield return new WaitForEndOfFrame();
			} while ((Time.time - time_start) < anim_dur);
			{
				var csf = panel.GetComponent<ContentSizeFitter>();
				csf.enabled = true;
			}
		}
		else {
			height_cache = size.y;
			{
				var csf = panel.GetComponent<ContentSizeFitter>();
				csf.enabled = false;
			}
			while ((Time.time - time_start) < anim_dur) {
				size.y = (1 - Easing.Cubic.Out((Time.time - time_start) / anim_dur)) * height_cache ?? 0;
				rect.sizeDelta = size;
				LayoutRebuilder.MarkLayoutForRebuild(transform.parent as RectTransform);
				yield return new WaitForEndOfFrame();
			}
			panel!.SetActive(expanded);
		}
		SendMessageUpwards("DirtyPanel");
		anim = null;
		OnAnimationComplete?.Invoke(expanded);
		yield break;
	}
	
	public void SetExpanded(bool expanded)
	{
		if (isActiveAndEnabled) {
			if (anim != null) {
				StopCoroutine(anim);
			}
			anim = AnimExpanded(expanded);
			StartCoroutine(anim);
		}
		else {
			SetExpandedNow(expanded, true);
		}
	}
	
	private void SetExpandedNow(bool expanded, bool set_active) {
		if (set_active) panel!.SetActive(expanded);
		expandToggle!.transform.localEulerAngles = (expanded ? 1 : 0) * 180f * Vector3.forward;
		if (settings_key != null) {
			Settings.Set(settings_key, expanded);
		}
	}
}

}
