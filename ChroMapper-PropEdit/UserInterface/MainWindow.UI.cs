using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using SimpleJSON;

using Beatmap.Base;

using ChroMapper_PropEdit.Components;
using ChroMapper_PropEdit.Enums;
using ChroMapper_PropEdit.Utils;

namespace ChroMapper_PropEdit.UserInterface {

public partial class MainWindow : UIWindow {
	public ExtensionButton main_button;
	public InputAction? keybind;
	public List<BaseObject>? editing;
	
	public MainWindow() {
		main_button = ExtensionButtons.AddButton(
			UI.LoadSprite("ChroMapper_PropEdit.Resources.Icon.png"),
			"Prop Edit",
			ToggleWindow);
		panels = new Stack<GameObject>();
		try {
			var map = CMInputCallbackInstaller.InputInstance.asset.actionMaps
				.Where(x => x.name == "Node Editor")
				.FirstOrDefault();
			CMInputCallbackInstaller.InputInstance.Disable();
			keybind = map.AddAction("Prop Editor", type: InputActionType.Button);
			keybind.AddCompositeBinding("ButtonWithOneModifier")
				.With("Modifier", "<Keyboard>/shift")
				.With("Button", "<Keyboard>/n");
			keybind.performed += (_) => {
				if (   (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				    && !CMInputCallbackInstaller.IsActionMapDisabled(typeof(CMInput.INodeEditorActions))
				    && !NodeEditorController.IsActive) {
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
	
	public override void ToggleWindow() {
		if (window == null) return;
		window!.Toggle();
		UpdateSelection(window!.gameObject.activeSelf);
	}
	
	public void Disable() {
		keybind?.Disable();
	}
	
	public void Init(MapEditorUI mapEditorUI) {
		base.Init(mapEditorUI, "Prop Editor");
		
		{
			var button = UI.AddButton(window!.title!, UI.LoadSprite("ChroMapper_PropEdit.Resources.Settings.png"), () => Plugin.plugin_settings!.ToggleWindow());
			UI.AttachTransform(button.gameObject, pos: new Vector2(-25, -14), size: new Vector2(30, 30), anchor_min: new Vector2(1, 1), anchor_max: new Vector2(1, 1));
			var tooltip = button.gameObject.AddComponent<Tooltip>();
			tooltip.TooltipOverride = "PropEdit Settings";
		}
		{
			// Sprite yoinked from ChroMapper/Assets/_Graphics/Textures And Sprites/UI/BeatsaberSpriteSheet.png
			var button = UI.AddButton(window!.title!, UI.LoadSprite("ChroMapper_PropEdit.Resources.EditorIcon.png"), () => Plugin.map_settings!.ToggleWindow());
			UI.AttachTransform(button.gameObject, pos: new Vector2(-60, -14), size: new Vector2(30, 30), anchor_min: new Vector2(1, 1), anchor_max: new Vector2(1, 1));
			var tooltip = button.gameObject.AddComponent<Tooltip>();
			tooltip.TooltipOverride = "Map Settings";
		}
		
		UpdateSelection(true);
		
		SelectionController.SelectionChangedEvent += () => UpdateSelection(true);
		BeatmapActionContainer.ActionCreatedEvent += (_) => UpdateSelection(false);
		BeatmapActionContainer.ActionUndoEvent += (_) => UpdateSelection(false);
		BeatmapActionContainer.ActionRedoEvent += (_) => UpdateSelection(false);
		
		keybind?.Enable();
		
		bundleInfo = new BundleInfo();
	}
	
#region Form Fields
	
	private GameObject AddLine(string title, Vector2? size = null, string tooltip = "") {
		return UI.AddField(current_panel!, title, size, tooltip);
	}
	
	// CustomData node gets removed when value = default
	private Toggle AddCheckbox(string title, System.ValueTuple<Data.Getter<bool?>, Data.Setter<bool?>> get_set, bool? _default, string tooltip = "") {
		var container = AddLine(title, null, tooltip);
		var staged = editing!;
		var (value_or, _) = Data.GetAllOrNothing<bool?>(editing!, get_set.Item1);
		var value = value_or ?? _default ?? false;
		
		return UI.AddCheckbox(container, value!, (v) => {
			if (v == _default) {
				Data.UpdateObjects<bool?>(staged, get_set.Item2, null);
			}
			else {
				Data.UpdateObjects<bool?>(staged, get_set.Item2, v);
			}
		});
	}
	
	private UIDropdown AddDropdown<T>(string? title, System.ValueTuple<Data.Getter<T?>, Data.Setter<T?>> get_set, Map<T?> type, bool nullable = false, string tooltip = "") {
		var container = (title != null)
			? AddLine(title, null, tooltip)
			: current_panel!;
		var staged = editing!;
		var (value, _) = Data.GetAllOrNothing<T?>(editing!, get_set.Item1);
		
		return UI.AddDropdown(container, value, (v) => {
			Data.UpdateObjects<T?>(staged, get_set.Item2, v);
		}, type, nullable);
	}
	
	private UITextInput SetMixed(UITextInput input, bool mixed) {
		if (mixed) {
			var placeholder = input.gameObject.GetComponentInChildren<TMPro.TMP_Text>();
			placeholder.text = "Mixed";
		}
		
		return input;
	}
	
	private UITextInput AddParsed<T>(string title, System.ValueTuple<Data.Getter<T?>, Data.Setter<T?>> get_set, bool time = false, string tooltip = "") where T : struct {
		var container = AddLine(title, null, tooltip);
		var staged = editing!;
		var (value, mixed) = Data.GetAllOrNothing<T?>(editing!, get_set.Item1);
		
		return SetMixed(UI.AddParsed<T>(container, value, (v) => {
			if (!(v == null && value == null)) {
				Data.UpdateObjects<T?>(staged, get_set.Item2, v, time);
			}
		}), mixed);
	}
	
	private UITextInput AddTextbox(string? title, System.ValueTuple<Data.Getter<string?>, Data.Setter<string?>> get_set, bool tall = false, string tooltip = "") {
		var container = (title != null)
			? AddLine(title, tall ? (new Vector2(0, 22)) : null, tooltip)
			: current_panel!;
		var staged = editing!;
		var (value, mixed) = Data.GetAllOrNothing<string>(editing!, get_set.Item1);
		
		return SetMixed(UI.AddTextbox(container, value, (v) => {
			if (v == "") {
				v = null;
			}
			if (v != value) {
				Data.UpdateObjects<string?>(staged, get_set.Item2, v);
			}
		}, tall), mixed);
	}
	
	private void AddPointDefinition(string title, System.ValueTuple<Data.Getter<string?>, Data.Setter<string?>> get_set, string tooltip = "") {
		var container = AddLine(title, new Vector2(0, 22), tooltip);
		var staged = editing!;
		var (value, mixed) = Data.GetAllOrNothing<string>(editing!, get_set.Item1);
		
		var textbox = SetMixed(UI.AddTextbox(container, value, (v) => {
			if (v == "") {
				v = null;
			}
			if (v != value) {
				Data.UpdateObjects<string?>(staged, get_set.Item2, v);
			}
		}, true), mixed);
		
		ArrayEditor.Getter arr_get = () => {
			return (Data.RawToJson(value ?? "") as JSONArray) ?? new JSONArray();
		};
		
		ArrayEditor.Setter arr_set = (JSONArray node) => {
			Data.UpdateObjects<string?>(staged, get_set.Item2, node.ToString());
		};
		
		var array = ArrayEditor.Create(current_panel!, title, (arr_get, arr_set), true);
		
		var pds = new Map<string?>();
		
		foreach (var pd in BeatSaberSongContainer.Instance.Map.PointDefinitions.Keys) {
			pds.Add($"\"{pd}\"", pd);
		}
		
		var dd_container = UI.AddChild(current_panel!, title + " PD Dropdown");
		UI.AttachTransform(dd_container, new Vector2(0, 20), new Vector2(0, 0));
		panels.Push(dd_container);
		var dropdown = AddDropdown(null, get_set, pds, true);
		UI.AttachTransform(dropdown.gameObject, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1));
		panels.Pop();
		
		System.Action show_array = () => {
			array.gameObject.SetActive(true);
			array.Refresh();
			dd_container.gameObject.SetActive(false);
		};
		System.Action show_dropdown = () => {
			array.gameObject.SetActive(false);
			dd_container.gameObject.SetActive(true);
		};
		
		if ((value?.StartsWith("[") ?? false)) {
			show_array();
		}
		else if ((value?.StartsWith("\"") ?? false)) {
			show_dropdown();
		}
		else {
			array.gameObject.SetActive(false);
			dd_container.gameObject.SetActive(false);
		}
		
		var type_changer = DropdownButton.Create(container, new List<string>() {
			"Point Definition Type:",
			"Array",
			"Named"
		}, (v) => {
			switch (v) {
			case "Array":
				show_array();
				break;
			case "Named":
				show_dropdown();
				break;
			default:
				break;
			}
		}, UI.LoadSprite("ChroMapper_PropEdit.Resources.Settings.png"));
	}
	
#endregion
	
}

}
