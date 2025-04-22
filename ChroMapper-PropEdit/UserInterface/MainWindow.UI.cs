using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
				.With("Button", "<Keyboard>/n")
				.With("Modifier", "<Keyboard>/shift");
			keybind.performed += (_) => {
				if (   (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				    && !CMInputCallbackInstaller.IsActionMapDisabled(typeof(CMInput.INodeEditorActions))
				    && !NodeEditorController.IsActive) {
					ToggleWindow();
				}
				else {
					//Debug.Log("Bullshit still required ;-;");
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
		old_type = null;
		UpdateSelection();
	}
	
	public void Disable() {
		keybind?.Disable();
	}
	
#if CHROMPER_11
	private IEnumerator WaitUpdate() {
		yield return 1;
		UpdateSelection();
		yield break;
	}
#endif
	
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
		
		UpdateSelection();
		
		SelectionController.SelectionChangedEvent += () => UpdateSelection();
#if CHROMPER_11
		BeatmapActionContainer.ActionCreatedEvent += (_) => {
			if (window.isActiveAndEnabled) {
				window.StartCoroutine(WaitUpdate());
			}
		};
#else
		BeatmapActionContainer.ActionCreatedEvent += (_) => UpdateSelection();
#endif
		BeatmapActionContainer.ActionUndoEvent += (_) => UpdateSelection();
		BeatmapActionContainer.ActionRedoEvent += (_) => UpdateSelection();
		
		keybind?.Enable();
		
		bundleInfo = new BundleInfo();
	}
	
#region Form Fields
	
	private GameObject AddLine(string title, Vector2? size = null, string tooltip = "") {
		return (full_rebuild)
			? UI.AddField(current_panel!, title, size, tooltip)
			: current_panel!.transform.Find(title).gameObject;
	}
	
	public override void AddExpando(string name, string label, bool expanded, string tooltip = "") {
		var expando = (full_rebuild)
			? Collapsible.Create(current_panel ?? panel!, name, label, expanded, tooltip)
			: current_panel!.transform.Find(name).GetComponent<Collapsible>();
		panels.Push(expando.panel!);
	}
	
	// CustomData node gets removed when value = default
	private Toggle AddCheckbox(string title, (Data.Getter<bool?>, Data.Setter<bool?>) get_set, bool? _default, string tooltip = "") {
		var container = AddLine(title, null, tooltip);
		var staged = editing!;
		var (value_or, _) = Data.GetAllOrNothing<bool?>(editing!, get_set.Item1);
		var value = value_or ?? _default ?? false;
		UnityAction<bool> setter = (v) => {
			if (v == _default) {
				Data.UpdateObjects<bool?>(staged, get_set.Item2, null);
			}
			else {
				Data.UpdateObjects<bool?>(staged, get_set.Item2, v);
			}
		};
		
		if (full_rebuild) {
			return UI.AddCheckbox(container, value!, setter);
		}
		else {
			var toggle = container.GetComponentInChildren<Toggle>();
			return UI.UpdateCheckbox(toggle!, value!, setter);
		}
	}
	
	private UIDropdown AddDropdown<T>(string? title, (Data.Getter<T?>, Data.Setter<T?>) get_set, Map<T?> type, bool nullable = false, string tooltip = "") {
		var container = (title != null)
			? AddLine(title, null, tooltip)
			: current_panel!;
		var staged = editing!;
		var (value, _) = Data.GetAllOrNothing<T?>(editing!, get_set.Item1);
		UnityAction<T?> setter = (v) => {
			Data.UpdateObjects<T?>(staged, get_set.Item2, v);
		};
		
		if (full_rebuild) {
			return UI.AddDropdown(container, value, setter, type, nullable);
		}
		else {
			var dd = container.GetComponentInChildren<UIDropdown>();
			return UI.UpdateDropdown(dd, value, setter, type, nullable);
		}
	}
	
	private Textbox AddParsed<T>(string title, (Data.Getter<T?>, Data.Setter<T?>) get_set, bool time = false, string tooltip = "") where T : struct {
		var container = AddLine(title, null, tooltip);
		var staged = editing!;
		var (value, mixed) = Data.GetAllOrNothing<T?>(editing!, get_set.Item1);
		
		UnityAction<T?> setter = (v) => {
			if (!(v == null && value == null)) {
				Data.UpdateObjects<T?>(staged, get_set.Item2, v, time);
			}
		};
		
		if (full_rebuild) {
			return UI.SetMixed(UI.CreateParsed<T>(container, value, setter), mixed);
		}
		else {
			var input = container.GetComponentInChildren<Textbox>();
			UI.UpdateParsed<T>(input, value, mixed, setter);
			return input;
		}
	}
	
	private Textbox AddTextbox(string? title, (Data.Getter<string?>, Data.Setter<string?>) get_set, bool tall = false, string tooltip = "") {
		var container = (title != null)
			? AddLine(title, tall ? (new Vector2(0, 22)) : null, tooltip)
			: current_panel!;
		var staged = editing!;
		var (value, mixed) = Data.GetAllOrNothing<string>(editing!, get_set.Item1);
		
		Textbox.Setter setter = (v) => {
			if (v == "") {
				v = null;
			}
			if (v != value) {
				Data.UpdateObjects<string?>(staged, get_set.Item2, v);
			}
		};
		
		if (full_rebuild) {
			return UI.SetMixed(UI.AddTextbox(container, value, setter), mixed);
		}
		else {
			var input = container.GetComponentInChildren<Textbox>();
			return input.Set(value, mixed, setter);
		}
	}
	
	private void AddPointDefinition(string title, (Data.Getter<string?>, Data.Setter<string?>) get_set, string tooltip = "") {
		var (value, mixed) = Data.GetAllOrNothing(editing!, get_set.Item1);
		
		PointDefinitionEditor
			.Singleton(current_panel!, title, tooltip)
			.Set(
				value,
				mixed,
				(v) => {
					if (v == "") {
						v = null;
					}
					if (v != value) {
						Data.UpdateObjects<string?>(editing!, get_set.Item2, v);
					}
				}
			);
	}
	
#endregion
	
}

}
