// Static UI helper functions

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using ChroMapper_PropEdit.Enums;

namespace ChroMapper_PropEdit.UserInterface {

public class UI {
	public static GameObject AddChild(GameObject parent, string name, params System.Type[] components) {
		var obj = new GameObject(name, components);
		obj.transform.SetParent(parent.transform);
		return obj;
	}
	public static GameObject AddChild(Transform parent, string name, params System.Type[] components) {
		var obj = new GameObject(name, components);
		obj.transform.SetParent(parent);
		return obj;
	}
	
	public static GameObject AddLabel(Transform parent, string title, string text, Vector2 pos, Vector2? anchor_min = null, Vector2? anchor_max = null, int font_size = 14, Vector2? size = null, TextAlignmentOptions align = TextAlignmentOptions.Center) {
		var entryLabel = AddChild(parent, title + " Label");
		AttachTransform(entryLabel, size ?? new Vector2(110, 24), pos, anchor_min ?? new Vector2(0.5f, 1), anchor_max ?? new Vector2(0.5f, 1));
		
		var textComponent = entryLabel.AddComponent<TextMeshProUGUI>();
		
		textComponent.name = title;
		textComponent.font = PersistentUI.Instance.ButtonPrefab.Text.font;
		textComponent.alignment = align;
		textComponent.fontSize = font_size;
		textComponent.text = text;
		
		return entryLabel;
	}
	
	// A container for an input element with a label
	public static GameObject AddField(GameObject parent, string title, Vector2? size = null) {
		var container = UI.AddChild(parent, title + " Container");
		UI.AttachTransform(container, size ?? new Vector2(0, 20), pos: new Vector2(0, 0));
		
		var label = UI.AddChild(container, title + " Label", typeof(TextMeshProUGUI));
		UI.AttachTransform(label, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 1));
		
		var textComponent = label.GetComponent<TextMeshProUGUI>();
		textComponent.font = PersistentUI.Instance.ButtonPrefab.Text.font;
		textComponent.alignment = TextAlignmentOptions.Left;
		textComponent.enableAutoSizing = true;
		textComponent.fontSizeMin = 8;
		textComponent.fontSizeMax = 16;
		textComponent.text = title;
		
		return container;
	}
	
	public static UIButton AddButton(GameObject parent, string text, UnityAction on_press) {
		var button = Object.Instantiate(PersistentUI.Instance.ButtonPrefab, parent.transform);
		button.SetText(text);
		button.Button.onClick.AddListener(on_press);
		return button;
	}
	public static UIButton AddButton(GameObject parent, Sprite sprite, UnityAction on_press) {
		var button = Object.Instantiate(PersistentUI.Instance.ButtonPrefab, parent.transform);
		button.SetImage(sprite);
		button.Button.onClick.AddListener(on_press);
		return button;
	}
	
#region Input Fields
	
	public static Toggle AddCheckbox(GameObject parent, bool value, UnityAction<bool> setter) {
		var original = GameObject.Find("Strobe Generator").GetComponentInChildren<Toggle>(true);
		var toggleObject = UnityEngine.Object.Instantiate(original, parent.transform);
		var toggleComponent = toggleObject.GetComponent<Toggle>();
		var colorBlock = toggleComponent.colors;
		colorBlock.normalColor = Color.white;
		toggleComponent.colors = colorBlock;
		toggleComponent.isOn = value;
		toggleComponent.onValueChanged.AddListener(setter);
		return toggleComponent;
	}
	
	public static UIDropdown AddDropdown<T>(GameObject parent, T? value, UnityAction<T?> setter, Map<T?> type, bool nullable = false) {
		// Get values from selected items
		var options = new List<string>();
		int i = 0;
		if (value == null) {
			options.Add("--");
		}
		else {
			i = type.dict.Keys.ToList().IndexOf((T)value);
			if (nullable) {
				options.Add("Unset");
				i += 1;
			}
		}
		options.AddRange(type.dict.Values.ToList());
		
		var dropdown = Object.Instantiate(PersistentUI.Instance.DropdownPrefab, parent.transform);
		UI.MoveTransform((RectTransform)dropdown.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, 1));
		dropdown.SetOptions(options);
		dropdown.Dropdown.value = i;
		dropdown.Dropdown.onValueChanged.AddListener((i) => {
			T? value = type.Backward(options[i]);
			setter(value);
		});
		
		return dropdown;
	}
	
#endregion
	
	public static RectTransform AttachTransform(GameObject obj,    Vector2 size, Vector2 pos, Vector2? anchor_min = null, Vector2? anchor_max = null, Vector2? pivot = null) {
		var rectTransform = obj.GetComponent<RectTransform>();
		if (rectTransform == null) {
			rectTransform = obj.AddComponent<RectTransform>();
		}
		return MoveTransform(rectTransform, size, pos, anchor_min, anchor_max, pivot);
	}
	
	public static RectTransform MoveTransform(RectTransform trans, Vector2 size, Vector2 pos, Vector2? anchor_min = null, Vector2? anchor_max = null, Vector2? pivot = null) {
		trans.localScale = new Vector3(1, 1, 1);
		trans.sizeDelta = size;
		trans.pivot = pivot ?? new Vector2(0.5f, 0.5f);
		trans.anchorMin = anchor_min ?? new Vector2(0, 0);
		trans.anchorMax = anchor_max ?? anchor_min ?? new Vector2(1, 1);
		trans.anchoredPosition = new Vector3(pos.x, pos.y, 0);
		
		return trans;
	}
	
	public static Sprite LoadSprite(string asset) {
		Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(asset);
		byte[] data = new byte[stream.Length];
		stream.Read(data, 0, (int)stream.Length);
		
		Texture2D texture2D = new Texture2D(256, 256);
		texture2D.LoadImage(data);
		
		return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0, 0), 100.0f);
	}
}

}
