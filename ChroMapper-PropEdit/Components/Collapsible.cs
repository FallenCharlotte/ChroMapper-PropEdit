// Derived (very far) from CM's StrobeGeneratorPassUIController
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit.Components {

public class Collapsible : MonoBehaviour
{
	public Toggle? expandToggle;
	public GameObject? panel;
	private string? settings_key;
	
	public static Collapsible Create(GameObject parent, string name, string label, bool expanded) {
		return Create(parent, name, label, expanded, "");
	}
	public static Collapsible Create(GameObject parent, string name, string label, bool expanded, string tooltip) {
		return UI.AddChild(parent, name).AddComponent<Collapsible>().Init(label, expanded, tooltip);
	}
	
	public Collapsible Init(string label, bool expanded, string tooltip = "") {
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
		Sprite[] sprites = (Sprite[])Resources.FindObjectsOfTypeAll(typeof(Sprite));
		Sprite arrow = sprites.Single(s => s.name == "ArrowIcon");
		
		Image[] images = expandToggle.gameObject.GetComponentsInChildren<Image>();
		foreach (var image in images) {
			image.sprite = arrow;
			((RectTransform)image.gameObject.transform).sizeDelta = new Vector2(20, 15);
		}
		((RectTransform)expandToggle.transform).anchoredPosition = new Vector2(-15, 0);
		
		
		panel = UI.AddChild(gameObject, "Panel");
		UI.AttachTransform(panel, new Vector2(0, 50), pos: Vector2.zero);
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
		
		SetExpanded(expanded);
		
		return this;
	}
	
	public void Awake() {
		SendMessageUpwards("DirtyPanel");
	}
	
	public void SetExpanded(bool expanded)
	{
		panel!.SetActive(expanded);
		expandToggle!.transform.localEulerAngles = (expanded ? 1 : 0) * 180f * Vector3.forward;
		SendMessageUpwards("DirtyPanel");
		if (settings_key != null) {
			Settings.Set(settings_key, expanded);
		}
	}
}

}
