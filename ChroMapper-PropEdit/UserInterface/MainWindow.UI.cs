using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using Beatmap.Base;

using ChroMapper_PropEdit.Components;
using ChroMapper_PropEdit.Enums;
using ChroMapper_PropEdit.Utils;

using Convert = System.Convert;

namespace ChroMapper_PropEdit.UserInterface {

public partial class MainWindow {
	public ExtensionButton main_button;
	public InputAction? keybind;
	public Window? window;
	public GameObject? panel;
	public GameObject? current_panel;
	public ScrollBox? scrollbox;
	public List<BaseObject>? editing;
	
	public MainWindow() {
		main_button = ExtensionButtons.AddButton(
			UI.LoadSprite("ChroMapper_PropEdit.Resources.Icon.png"),
			"Prop Edit",
			ToggleWindow);
		try {
			var map = CMInputCallbackInstaller.InputInstance.asset.actionMaps
				.Where(x => x.name == "Node Editor")
				.FirstOrDefault();
			CMInputCallbackInstaller.InputInstance.Disable();
			keybind = map.AddAction("Prop Editor", type: InputActionType.Button);
			// Dynamic to support dev and stable with one build
			((dynamic)keybind.AddCompositeBinding("ButtonWithOneModifier"))
				.With("Modifier", "<Keyboard>/shift")
				.With("Button", "<Keyboard>/n");
			keybind.performed += (_) => {
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
					ToggleWindow();
				}
			};
			keybind.Disable();
			CMInputCallbackInstaller.InputInstance.Enable();
		}
		catch (System.Exception e) {
			Debug.LogWarning("PropEdit couldn't register a keybind!");
			Debug.LogException(e);
		}
	}
	
	public void ToggleWindow() {
		if (window == null) return;
		window!.Toggle();
		UpdateSelection(window!.gameObject.activeSelf);
	}
	
	public void Disable() {
		keybind?.Disable();
	}
	
	public void Init(MapEditorUI mapEditorUI) {
		var parent = mapEditorUI.MainUIGroup[5].gameObject;
		
		window = Window.Create("Main", "Prop Editor", parent, new Vector2(220, 256));
		window.onShow += Resize;
		window.onResize += Resize;
		
		{
			var button = UI.AddButton(window.title!, UI.LoadSprite("ChroMapper_PropEdit.Resources.Settings.png"), () => Plugin.settings!.ToggleWindow());
			UI.AttachTransform(button.gameObject, pos: new Vector2(-25, -14), size: new Vector2(30, 30), anchor_min: new Vector2(1, 1), anchor_max: new Vector2(1, 1));
		}
		
		var container = UI.AddChild(window.gameObject, "Prop Scroll Container");
		UI.AttachTransform(container, new Vector2(-10, -40), new Vector2(0, -15), new Vector2(0, 0), new Vector2(1, 1));
		{
			var image = container.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.1f, 0.1f, 0.1f, 1);
		}
		
		scrollbox = ScrollBox.Create(container);
		panel = scrollbox.content;
		
		UpdateSelection(true);
		
		SelectionController.SelectionChangedEvent += () => UpdateSelection(true);
		BeatmapActionContainer.ActionCreatedEvent += (_) => UpdateSelection(false);
		BeatmapActionContainer.ActionUndoEvent += (_) => UpdateSelection(false);
		BeatmapActionContainer.ActionRedoEvent += (_) => UpdateSelection(false);
		
		keybind?.Enable();
	}
	
#region Form Fields
	
	private GameObject AddLine(string title, Vector2? size = null) {
		return UI.AddField(current_panel!, title, size);
	}
	
	// CustomData node gets removed when value = default
	private Toggle AddCheckbox(string title, System.ValueTuple<System.Func<BaseObject, bool?>, System.Action<BaseObject, bool?>> get_set, bool? _default) {
		var container = AddLine(title);
		var staged = editing!;
		var value = Data.GetAllOrNothing<bool?>(editing!, get_set.Item1) ?? _default ?? false;
		
		return UI.AddCheckbox(container, value, (v) => {
			if (v == _default) {
				Data.UpdateObjects<bool?>(staged, get_set.Item2, null);
			}
			else {
				Data.UpdateObjects<bool?>(staged, get_set.Item2, v);
			}
		});
	}
	
	private UIDropdown AddDropdown<T>(string title, System.ValueTuple<System.Func<BaseObject, T?>, System.Action<BaseObject, T?>> get_set, Map<T?> type, bool nullable = false) {
		var container = AddLine(title);
		var staged = editing!;
		var value = Data.GetAllOrNothing<T?>(editing!, get_set.Item1);
		
		return UI.AddDropdown(container, value, (v) => {
			Data.UpdateObjects<T?>(staged, get_set.Item2, v);
		}, type, nullable);
	}
	
	private UITextInput AddParsed<T>(string title, System.ValueTuple<System.Func<BaseObject, T?>, System.Action<BaseObject, T?>> get_set) where T : struct {
		var container = AddLine(title);
		var staged = editing!;
		var value = Data.GetAllOrNothing<T?>(editing!, get_set.Item1);
		
		return UI.AddParsed<T>(container, value, (v) => {
			if (!(v == null && value == null)) {
				Data.UpdateObjects<T?>(staged, get_set.Item2, v);
			}
		});
	}
	
	private UITextInput AddTextbox(string title, System.ValueTuple<System.Func<BaseObject, string?>, System.Action<BaseObject, string?>> get_set, bool tall = false) {
		var container = AddLine(title, tall ? (new Vector2(0, 22)) : null);
		var staged = editing!;
		var value = Data.GetAllOrNothing<string>(editing!, get_set.Item1);
		
		return UI.AddTextbox(container, value, (v) => {
			if (v == "") {
				v = null;
			}
			if (v != value) {
				Data.UpdateObjects<string?>(staged, get_set.Item2, v);
			}
		}, tall);
	}
	
#endregion
	
	private void Resize() {
		var layout = panel!.GetComponent<LayoutElement>();
		layout!.minHeight = window!.GetComponent<RectTransform>().sizeDelta.y - 40 - 15;
	}
}

}
