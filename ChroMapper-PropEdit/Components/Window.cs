using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;

using ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit.Components {

public class Window : MonoBehaviour {
	public event Action? onShow;
	public event Action? onResize;
	
	public GameObject? title;
	
	public string? settings_key;
	
	public static Window Create(string name, string title, Transform parent, Vector2 size) {
		var obj = new GameObject($"name Window");
		return obj.AddComponent<Window>().Init(name, title, parent, size);
	}
	
	public Window Init(string name, string title, Transform parent, Vector2 size) {
		var canvas = parent.GetComponent<Canvas>();
		gameObject.transform.parent = parent;
		settings_key = $"{name}_window";
		// Window Drag
		gameObject.AddComponent<DragWindowController>().Init(canvas, PosSave);
		
		UI.AttachTransform(gameObject, size, new Vector2(0, 0), new Vector2(0.5f, 0.5f));
		{
			var image = gameObject.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.24f, 0.24f, 0.24f, 1);
		}
		
		{
			var handle = UI.AddChild(gameObject, "Resize Height");
			UI.AttachTransform(handle, new Vector2(0, 12), new Vector2(0, 4), new Vector2(0, 0), new Vector2(1, 0));
			handle.AddComponent<ResizeWindowController>().Init(canvas, PosSave, new Vector2(0, 1));
		}
		{
			var handle = UI.AddChild(gameObject, "Resize Width");
			UI.AttachTransform(handle, new Vector2(12, 0), new Vector2(-4, 0), new Vector2(1, 0), new Vector2(1, 1));
			handle.AddComponent<ResizeWindowController>().Init(canvas, PosSave, new Vector2(1, 0));
		}
		{
			var handle = UI.AddChild(gameObject, "Resize Corner");
			UI.AttachTransform(handle, new Vector2(12, 12), new Vector2(-4, 4), new Vector2(1, 0), new Vector2(1, 0));
			handle.AddComponent<ResizeWindowController>().Init(canvas, PosSave, new Vector2(1, 1));
		}
		
		this.title = UI.AddLabel(transform, "Title", title, pos: new Vector2(10, -20), size: new Vector2(-10, 30), font_size: 28, anchor_min: new Vector2(0, 1), anchor_max: new Vector2(1, 1), align: TextAlignmentOptions.Left);
		
		gameObject.SetActive(false);
		
		return this;
	}
	
	public void Toggle() {
		gameObject.SetActive(!gameObject.activeSelf);
		if (gameObject.activeSelf) {
			PosLoad();
			onShow?.Invoke();
		}
	}
	
	public void SetTitle(string text) {
		title!.GetComponent<TextMeshProUGUI>().text = text;
	}
	
	private void PosLoad() {
		Settings.Reload();
		var settings = Settings.Get(settings_key!)?.AsObject;
		if (settings == null) {
			return;
		}
		GetComponent<RectTransform>().anchoredPosition =
			new Vector2(settings["x"].AsFloat, settings["y"].AsFloat);
		GetComponent<RectTransform>().sizeDelta =
			new Vector2(settings["w"].AsInt,   settings["h"].AsInt);
	}
	
	private void PosSave() {
		var pos = GetComponent<RectTransform>().anchoredPosition;
		var settings = new JSONObject();
		settings["x"] = pos.x;
		settings["y"] = pos.y;
		settings["w"] = GetComponent<RectTransform>().sizeDelta.x;
		settings["h"] = GetComponent<RectTransform>().sizeDelta.y;
		Settings.Set(settings_key!, settings);
		onResize?.Invoke();
	}
}

}
