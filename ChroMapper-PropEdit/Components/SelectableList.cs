using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

using ChroMapper_PropEdit.UserInterface;

using static System.Math;

namespace ChroMapper_PropEdit.Components {

public class SelectableList : MonoBehaviour {
	public delegate void SelectionCallback(IList? slected);
	public delegate void CreateItem();
	public delegate void ItemRemoved(object item);
	
	public SelectionCallback? OnSelectionChanged;
	public CreateItem? OnCreateItem;
	public ItemRemoved? OnItemRemoved;
	
	public static SelectableList Create(GameObject parent) {
		var go = UI.AddChild(parent, "Selection List");
		UI.AttachTransform(go, new Vector2(0, 20), pos: new Vector2(0, 0));
		var sl = go.AddComponent<SelectableList>()!;
		
		{
			var layout = go.AddComponent<VerticalLayoutGroup>();
			layout.padding = new RectOffset(2, 2, 2, 2);
			layout.spacing = 0;
			layout.childControlHeight = false;
			layout.childControlWidth = true;
			layout.childForceExpandHeight = false;
			layout.childForceExpandWidth = true;
			layout.childAlignment = TextAnchor.UpperCenter;
		}
		{
			var csf = go.AddComponent<ContentSizeFitter>();
			csf.verticalFit = ContentSizeFitter.FitMode.MinSize;
		}
		
		return sl;
	}
	
	public void SetItems<T>(List<T> items, System.Func<int, T, string>? name_func = null) {
		while (transform.childCount > 0) {
			GameObject.DestroyImmediate(transform.GetChild(0).gameObject);
		}
		toggles.Clear();
		
		_items = items;
		
		for (int i = 0; i < items.Count; ++i) {
			AddItem<T>(name_func?.Invoke(i, items[i]) ?? $"[{i}]");
		}
		
		if (OnCreateItem != null) {
			var container = UI.AddField(gameObject, "", null);
			var new_item = UI.AddButton(container, "Add", () => OnCreateItem());
			UI.AttachTransform(new_item.gameObject, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, 1));
		}
	}
	
	public void SetSelection(IList? items) {
		refreshing = true;
		
		foreach (var toggle in toggles) {
			toggle.isOn = false;
		}
		
		if (items == null) {
			Plugin.Trace("Deselecting all");
			selectionStart = selectionEnd = -1;
			refreshing = false;
			return;
		}
		
		selectionStart = _items!.Count;
		selectionEnd = -1;
		
		foreach (var item in items!) {
			var i = _items.IndexOf(item);
			if (i == -1) {
				Debug.LogError($"Trying to select an item that isn't in the collection! {item}");
				continue;
			}
			
			Plugin.Trace($"Selecting item {i}");
			
			toggles[i].isOn = true;
			
			selectionStart = Min(selectionStart, i);
			selectionEnd = Max(selectionEnd, i);
		}
		
		if (selectionStart == _items.Count) {
			selectionStart = -1;
		}
		
		Plugin.Trace($"{selectionStart} - {selectionEnd}");
		
		refreshing = false;
		
		OnSelectionChanged?.Invoke(items);
	}
	
	protected virtual void AddItem<T>(string label) {
		var line = UI.AddChild(gameObject, label);
		UI.AttachTransform(line, new Vector2(0, 20), pos: new Vector2(0, 0));
		
		var bg = UI.AddChild(line, "Background");
		var image = bg.AddComponent<Image>();
		UI.AttachTransform(bg, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1));
		
		var m = UI.AddChild(line, "Mark");
		var mi = m.AddComponent<Image>();
		UI.AttachTransform(m, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1));
		mi.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		
		var text = UI.AddChild(line, label + " Label", typeof(TextMeshProUGUI));
		UI.AttachTransform(text, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1));
		
		var textComponent = text.GetComponent<TextMeshProUGUI>();
		textComponent.font = PersistentUI.Instance.ButtonPrefab.Text.font;
		textComponent.alignment = (TMPro.TextAlignmentOptions) 0x1000;
		textComponent.enableAutoSizing = true;
		textComponent.fontSizeMin = 8;
		textComponent.fontSizeMax = 16;
		textComponent.text = label;
		
		var index = transform.childCount - 1;
		var toggle = line.AddComponent<Toggle>();
		toggle.transition = Selectable.Transition.ColorTint;
		toggle.targetGraphic = image;
		toggle.graphic = mi;
		var colors = toggle.colors;
		colors.normalColor = new Color(0.157f, 0.157f, 0.157f);
		colors.highlightedColor = colors.selectedColor = new Color(0.435f, 0.435f, 0.435f); // 6F6F6F
		colors.pressedColor = new Color(0.35f, 0.35f, 0.35f);
		toggle.colors = colors;
		toggle.onValueChanged.AddListener((v) => OnClick<T>(index));
		
		toggles.Add(toggle);
		
		// TODO: Delete button
	}
	
	private void OnClick<T>(int index) {
		if (refreshing) return;
		
		if (selectionStart >= 0 && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) {
			selectionStart = Min(selectionStart, index);
			selectionEnd = Max(selectionEnd, index);
		}
		else {
			selectionStart = index;
			selectionEnd = index;
		}
		
		Plugin.Trace($"{selectionStart} - {selectionEnd}");
		
		refreshing = true;
		for (var i = 0; i < toggles.Count; ++i) {
			toggles[i].isOn = (selectionStart <= i && i <= selectionEnd);
		}
		refreshing = false;
		
		OnSelectionChanged?.Invoke((_items as List<T>)!.GetRange(selectionStart, selectionEnd - selectionStart + 1));
	}
	
	private bool refreshing = false;
	
	private int selectionStart = -1;
	private int selectionEnd = -1;
	
	private IList? _items;
	private List<Toggle> toggles = new();
};

}
