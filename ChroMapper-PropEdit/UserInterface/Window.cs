using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;

using ChroMapper_PropEdit.Components;

namespace ChroMapper_PropEdit.UserInterface {

public class Window {
	public event System.Action OnShow;
	
	public GameObject game_object;
	public GameObject title;
	
	public string name;
	public string settings_key;
	
	public Window(string name, string title, CanvasGroup parent, Vector2 size) {
		game_object = new GameObject($"{name} Window");
		game_object.transform.parent = parent.transform;
		this.name = name;
		settings_key = $"{name}_window";
		// Window Drag
		game_object.AddComponent<DragWindowController>();
		game_object.GetComponent<DragWindowController>().canvas = parent.GetComponent<Canvas>();
		game_object.GetComponent<DragWindowController>().OnDragWindow += PosSave;
		
		UI.AttachTransform(game_object, size, new Vector2(0, 0), new Vector2(0.5f, 0.5f));
		{
			var image = game_object.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.24f, 0.24f, 0.24f, 1);
		}
		
		game_object.SetActive(false);
		
		this.title = UI.AddLabel(game_object.transform, "Title", title, pos: new Vector2(10, -20), size: new Vector2(-10, 30), font_size: 28, anchor_min: new Vector2(0, 1), anchor_max: new Vector2(1, 1), align: TextAlignmentOptions.Left);
	}
	
	public void Toggle() {
		game_object.SetActive(!game_object.activeSelf);
		if (game_object.activeSelf) {
			PosLoad();
			OnShow?.Invoke();
		}
	}
	
	public void SetTitle(string text) {
		title.GetComponent<TextMeshProUGUI>().text = text;
	}
	
	private void PosLoad() {
		Settings.Reload();
		if (Settings.Get(settings_key) == null) {
			return;
		}
		var settings = Settings.Get(settings_key).AsObject;
		game_object.GetComponent<RectTransform>().anchoredPosition =
			new Vector2(settings["x"].AsFloat, settings["y"].AsFloat);
		game_object.GetComponent<RectTransform>().sizeDelta =
			new Vector2(settings["w"].AsInt,   settings["h"].AsInt);
	}
	
	private void PosSave() {
		var pos = game_object.GetComponent<RectTransform>().anchoredPosition;
		var settings = new JSONObject();
		settings["x"] = pos.x;
		settings["y"] = pos.y;
		settings["w"] = game_object.GetComponent<RectTransform>().sizeDelta.x;
		settings["h"] = game_object.GetComponent<RectTransform>().sizeDelta.y;
		Settings.Set(settings_key, settings);
	}
}

}
