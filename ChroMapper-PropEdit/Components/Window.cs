using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SimpleJSON;

using ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit.Components {

public class Window : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	public event Action? onShow;
	public event Action? onResize;
	
	public GameObject? title;
	
	public string? settings_key;
	
	public static Window Create(string name, string title, GameObject parent, Vector2 size) {
		var obj = new GameObject($"{name} Window");
		return obj.AddComponent<Window>().Init(name, title, parent, size);
	}
	
	public Window Init(string name, string title, GameObject parent, Vector2 size) {
		var canvas = parent.GetComponent<Canvas>();
		transform.parent = parent.transform;
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
			handle.AddComponent<ResizeWindowController>().Init(canvas, SizeSave, new Vector2(0, 1));
		}
		{
			var handle = UI.AddChild(gameObject, "Resize Width");
			UI.AttachTransform(handle, new Vector2(12, 0), new Vector2(-4, 0), new Vector2(1, 0), new Vector2(1, 1));
			handle.AddComponent<ResizeWindowController>().Init(canvas, SizeSave, new Vector2(1, 0));
		}
		{
			var handle = UI.AddChild(gameObject, "Resize Corner");
			UI.AttachTransform(handle, new Vector2(12, 12), new Vector2(-4, 4), new Vector2(1, 0), new Vector2(1, 0));
			handle.AddComponent<ResizeWindowController>().Init(canvas, SizeSave, new Vector2(1, 1));
		}
		
		this.title = UI.AddLabel(gameObject, "Title", title, pos: new Vector2(10, -20), size: new Vector2(-10, 30), font_size: 28, anchor_min: new Vector2(0, 1), anchor_max: new Vector2(1, 1), align: TextAlignmentOptions.Left);
		
		// Tab Receiver
		Textbox.AddTabListener(gameObject, TabDir);
		
		gameObject.SetActive(false);
		
		return this;
	}
	
	public void TabDir((Textbox, int) args) {
		var (input, dir) = args;
		var textboxes = GetComponentsInChildren<Textbox>()
			.Where(it => it.isActiveAndEnabled)
			.ToList();
		
		if (textboxes.Count == 0) return;
		
		var i = (input != null)
			? textboxes.IndexOf(input)
			: 0;
		
		if (i == -1) {
			Debug.LogWarning($"Couldn't find {input!.gameObject.GetPath()}");
		}
		
		var target = textboxes[(i + dir + textboxes.Count) % textboxes.Count];
		target.Select();
		
		GetComponentInChildren<ScrollBox>()?.ScrollTo(target.gameObject);
	}
	
	public void Toggle() {
		gameObject.SetActive(!gameObject.activeSelf);
		if (gameObject.activeSelf) {
			PosLoad();
			onShow?.Invoke();
		}
		else {
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(Window), ActionMapsDisabled);
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
		float x = settings["x"].AsFloat,
		      y = settings["y"].AsFloat;
		int w = settings["w"].AsInt,
		    h = settings["h"].AsInt;
		
		var screen_size = (transform.parent as RectTransform)!.sizeDelta;
		
		float max_x = (screen_size.x - w) / 2.0f;
		float max_y = (screen_size.y - h) / 2.0f;
		
		Plugin.Trace($"Clamp x: {x} {max_x}");
		Plugin.Trace($"Clamp y: {y} {max_y}");
		
		x = Mathf.Clamp(x, -max_x, max_x);
		y = Mathf.Clamp(y, -max_y, max_y);
		
		var t = (transform as RectTransform)!;
		t.anchoredPosition = new Vector2(x, y);
		t.sizeDelta = new Vector2(w, h);
	}
	
	private void PosSave() {
		var pos = GetComponent<RectTransform>().anchoredPosition;
		var settings = new JSONObject();
		settings["x"] = pos.x;
		settings["y"] = pos.y;
		settings["w"] = GetComponent<RectTransform>().sizeDelta.x;
		settings["h"] = GetComponent<RectTransform>().sizeDelta.y;
		Settings.Set(settings_key!, settings);
	}
	
	private void SizeSave() {
		PosSave();
		onResize?.Invoke();
	}
	
	public void OnPointerEnter(PointerEventData _) {
		CMInputCallbackInstaller.DisableActionMaps(typeof(Window), ActionMapsDisabled);
	}
	
	public void OnPointerExit(PointerEventData _) {
		CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(Window), ActionMapsDisabled);
	}
	
	private List<Selectable> tabIndexs = new List<Selectable>();
	
	private static System.Type[] ActionMapsDisabled = {
		typeof(CMInput.ICameraActions), typeof(CMInput.IPlacementControllersActions)
	};
}

}
