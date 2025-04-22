// Static UI helper functions

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using ChroMapper_PropEdit.Components;
using ChroMapper_PropEdit.Enums;

using Convert = System.Convert;

namespace ChroMapper_PropEdit.UserInterface {

public static class UI {
	public static GameObject AddChild(GameObject parent, string name, params System.Type[] components) {
		var obj = new GameObject(name, components);
		obj.transform.SetParent(parent.transform);
		return obj;
	}
	
	// You're welcome, PaulMapper
	public static GameObject AddLabel(GameObject parent, string title, string text, Vector2 pos, Vector2? anchor_min = null, Vector2? anchor_max = null, int font_size = 14, Vector2? size = null, TextAlignmentOptions align = TextAlignmentOptions.Center) {
		return AddLabel(parent, title, text, pos, anchor_min, anchor_max, font_size, size, align, "");
	}
	
	public static GameObject AddLabel(GameObject parent, string title, string text, Vector2 pos, Vector2? anchor_min, Vector2? anchor_max, int font_size, Vector2? size, TextAlignmentOptions align, string tooltip) {
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
	
	public static GameObject AddField(GameObject parent, string title, Vector2? size = null) {
		return AddField(parent, title, size, "");
	}
	
	// A container for an input element with a label
	public static GameObject AddField(GameObject parent, string title, Vector2? size, string tooltip) {
		var container = UI.AddChild(parent, title);
		UI.AttachTransform(container, size ?? new Vector2(0, 20), pos: new Vector2(0, 0));
		
		var label = UI.AddChild(container, title + " Label", typeof(TextMeshProUGUI));
		UI.LeftColumn(label);
		
		//main code that adds the tooltip to the label
		if (tooltip != "" && Settings.Get(Settings.ShowTooltips, true)!.AsBool == true) {
			var LINE_WIDTH = 40;
			var tooltip_wrapped = new System.Text.StringBuilder(tooltip);
			var i = 0;
			while (i + LINE_WIDTH < tooltip.Length) {
				var search_len = System.Math.Min(LINE_WIDTH, tooltip.Length - i - 1);
				// Reset count at premade newlines
				var j = tooltip.IndexOf("\n", i, search_len);
				if (j > 0) {
					i = j + 1;
					continue;
				}
				j = tooltip.LastIndexOf(" ", i + search_len, search_len);
				if (j == -1) {
					// Who tf has a 40-character word?
					break;
				}
				tooltip_wrapped[j] = '\n';
				i = j + 1;
			}
			var tooltipComp = label.AddComponent<Tooltip>();
			tooltipComp.TooltipOverride = tooltip_wrapped.ToString();
		}
		
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
		button.Image.transform.localRotation = Quaternion.identity;
		button.Button.onClick.AddListener(on_press);
		return button;
	}
	
#region Input Fields
	
	public static Toggle? _baseToggle = null;
	
	public static Toggle AddCheckbox(GameObject parent, bool value, UnityAction<bool> setter) {
		if (_baseToggle == null) {
			_baseToggle = GameObject.Find("Strobe Generator").GetComponentInChildren<Toggle>(true);
		}
		var toggleObject = UnityEngine.Object.Instantiate(_baseToggle!, parent.transform);
		var toggleComponent = toggleObject.GetComponent<Toggle>();
		var colorBlock = toggleComponent.colors;
		colorBlock.normalColor = Color.white;
		toggleComponent.colors = colorBlock;
		return UpdateCheckbox(toggleComponent, value, setter);
	}
	
	public static Toggle UpdateCheckbox(Toggle toggle, bool value, UnityAction<bool> setter) {
		toggle.onValueChanged.RemoveAllListeners();
		toggle.isOn = value;
		toggle.onValueChanged.AddListener(setter);
		return toggle;
	}
	
	public static UIDropdown AddDropdown<T>(GameObject parent, T? value, UnityAction<T?> setter, Map<T?> type, bool nullable = false) {
		var dropdown = CreateDropdown(parent);
		return UpdateDropdown<T>(dropdown, value, setter, type, nullable);
	}
	
	public static UIDropdown CreateDropdown(GameObject parent) {
		var dropdown = Object.Instantiate(PersistentUI.Instance.DropdownPrefab, parent.transform);
		UI.MoveTransform((RectTransform)dropdown.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, 1));
		return dropdown;
	}
	
	public static UIDropdown UpdateDropdown<T>(UIDropdown dropdown, T? value, UnityAction<T?> setter, Map<T?> type, bool nullable = false) {
		// Get values from selected items
		var options = new List<string>();
		int i = 0;
		if (value == null) {
			if (nullable) {
				options.Add("--");
			}
			else {
				options.Add("Mixed");
			}
		}
		else {
			i = type.dict.Keys.ToList().IndexOf((T)value);
			if (nullable) {
				options.Add("Unset");
				i += 1;
			}
		}
		options.AddRange(type.dict.Values.ToList());
		
		dropdown.Dropdown.onValueChanged.RemoveAllListeners();
		dropdown.SetOptions(options);
		dropdown.Dropdown.value = i;
		dropdown.Dropdown.onValueChanged.AddListener((i) => {
			T? value = type.Backward(options[i]);
			setter(value);
		});
		
		return dropdown;
	}
	
	public static UITextInput AddParsed<T>(GameObject parent, T? value, UnityAction<T?> setter) where T : struct {
		return CreateParsed<T>(parent, value, setter).TextInput!;
	}
	public static Textbox CreateParsed<T>(GameObject parent, T? value, UnityAction<T?> setter) where T : struct {
		var input = Textbox.Create(parent);
		UI.AttachTransform(input.gameObject, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, 1));
		
		return UpdateParsed<T>(input, value, false, setter);
	}
	
	public static Textbox UpdateParsed<T>(Textbox input, T? value, bool mixed, UnityAction<T?> setter) where T : struct {
		input.Value = (value != null) ? (string)Convert.ChangeType(value, typeof(string)) : "";
		input.OnChange = (s) => {
			var table = new System.Data.DataTable();
			var computed = table.Compute(s, "");
			T? converted = (computed == System.DBNull.Value)
				? null
				: (T)Convert.ChangeType(computed, typeof(T));
			setter(converted);
		};
		input.Placeholder = (mixed) ? "Mixed" : "Empty";
		
		return input;
	}
	
	public static Textbox AddTextbox(GameObject parent, string? value, Textbox.Setter setter, bool tall = false) {
		var input = Textbox.Create(parent, tall);
		UI.AttachTransform(input.gameObject, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, 1));
		
		return input.Set(value, false, setter);
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
	
	public static Textbox SetMixed(Textbox input, bool mixed) {
		if (mixed) {
			input.Placeholder = "Mixed";
		}
		
		return input;
	}
	
	public static void LeftColumn(GameObject obj) {
		UI.AttachTransform(obj, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 1));
	}
	
	public static Sprite LoadSprite(string asset) {
		Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(asset);
		byte[] data = new byte[stream.Length];
		stream.Read(data, 0, (int)stream.Length);
		
		Texture2D texture2D = new Texture2D(256, 256);
		texture2D.LoadImage(data);
		
		return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0, 0), 100.0f);
	}
	
	public static Sprite GetSprite(string name) {
		Sprite[] sprites = (Sprite[])Resources.FindObjectsOfTypeAll(typeof(Sprite));
		return sprites.Single(s => s.name == name);
	}
	
	public static void RefreshTooltips(GameObject? root) {
		if (root == null)
			return;
		
		var show_tooltips = Settings.Get(Settings.ShowTooltips, true)!.AsBool;
		
		foreach (var t in root.GetComponentsInChildren<Tooltip>(true)) {
			t.enabled = show_tooltips;
		}
	}
	
	// From https://discussions.unity.com/t/how-to-get-a-component-from-an-object-and-add-it-to-another-copy-components-at-runtime/80939/4
	public static T GetCopyOf<T>(this T comp, T other) where T : Component
	{
		System.Type type = comp.GetType();
		//if (type != other.GetType()) return null; // type mis-match
		BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
		PropertyInfo[] pinfos = type.GetProperties(flags);
		foreach (var pinfo in pinfos) {
			if (pinfo.CanWrite) {
				try {
					pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
				}
				catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
			}
		}
		FieldInfo[] finfos = type.GetFields(flags);
		foreach (var finfo in finfos) {
			finfo.SetValue(comp, finfo.GetValue(other));
		}
		return comp;
	}
	public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
	{
		return go.AddComponent<T>().GetCopyOf(toAdd);
	}
	public static T AddComponent<T>(this Transform t, T toAdd) where T : Component
	{
		return t.gameObject.AddComponent<T>().GetCopyOf(toAdd);
	}
	public static string GetPath(this GameObject go) {
		var trans = go.transform;
		string path = "";
		while (trans != null) {
			path = "/" + trans.gameObject.name + path;
			trans = trans.parent;
		}
		return path;
	}
}

}
