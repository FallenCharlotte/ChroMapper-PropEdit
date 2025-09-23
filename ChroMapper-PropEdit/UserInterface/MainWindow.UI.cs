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
	public IList? editing;
	
	public MainWindow() {
		panels = new Stack<GameObject>();
	}
	
	public void OnToggleWindow(InputAction.CallbackContext _) {
		if (   (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		    && !CMInputCallbackInstaller.IsActionMapDisabled(typeof(CMInput.INodeEditorActions))
		    && !NodeEditorController.IsActive) {
			ToggleWindow();
		}
		else {
			Plugin.Trace("Bullshit still required ;-;");
		}
	}
	
	public override void ToggleWindow() {
		if (window == null) return;
		window!.Toggle();
		TriggerFullRefresh();
	}
	
	public override void Init(MapEditorUI mapEditorUI) {
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
		
		old_otype = null;
		
		SelectionController.SelectionChangedEvent += Selection.OnObjectsSelected;
		Selection.OnSelectionChanged += UpdateFromSelection;
		BeatmapActionContainer.ActionCreatedEvent += UpdateFromAction;
		BeatmapActionContainer.ActionUndoEvent += UpdateFromAction;
		BeatmapActionContainer.ActionRedoEvent += UpdateFromAction;
		
		Plugin.toggle_window!.performed += OnToggleWindow;
		
		bundleInfo = new BundleInfo();
	}
	
	public void OnDestroy() {
		SelectionController.SelectionChangedEvent -= UpdateFromSelection;
		BeatmapActionContainer.ActionCreatedEvent -= UpdateFromAction;
		BeatmapActionContainer.ActionUndoEvent -= UpdateFromAction;
		BeatmapActionContainer.ActionRedoEvent -= UpdateFromAction;
		Plugin.toggle_window!.performed -= OnToggleWindow;
		Selection.OnDeselectAll();
	}
	
	private void UpdateFromSelection() {
		Plugin.Trace($"{Time.frameCount} UpdateFromSelection");
		TriggerRefresh();
	}
	
	private void UpdateFromAction(BeatmapAction? _) {
		Plugin.Trace($"{Time.frameCount} UpdateFromAction");
		Selection.OnObjectsSelected();
		TriggerRefresh();
	}
	
#region Form Fields
	
	private GameObject AddLine(string title, Vector2? size = null, string tooltip = "") {
		var existing = (!full_rebuild) ? current_panel!.transform.Find(title)?.gameObject : null;
		return existing ?? UI.AddField(current_panel!, title, size, tooltip);
	}
	
	public override Collapsible AddExpando(string name, string label, bool expanded, string tooltip = "", bool background = true) {
		var expando = ((!full_rebuild)
			? current_panel!.transform.Find(name)?.GetComponent<Collapsible>()
			: null) ?? Collapsible.Create(current_panel ?? panel!, name, label, expanded, tooltip, background);
		panels.Push(expando.panel!);
		return expando;
	}
	
	// CustomData node gets removed when value = default
	private Toggle AddCheckbox(string title, (Data.Getter<bool?>, Data.Setter<bool?>) get_set, bool? _default, string tooltip = "") {
		var container = AddLine(title, null, tooltip);
		var staged = editing!;
		var (value_or, mixed) = Data.GetAllOrNothing<bool?>(editing!, get_set.Item1);
		// Do some jank, mixed needs to be drawn as true but act like false
		var value = (value_or ?? _default ?? false) || mixed;
		UnityAction<bool> setter = (v) => {
			v ^= mixed;
			if (v == _default) {
				Data.UpdateObjects<bool?>(staged, get_set.Item2, null);
			}
			else {
				Data.UpdateObjects<bool?>(staged, get_set.Item2, v);
			}
		};
		
		Toggle toggle;
		
		if (full_rebuild) {
			toggle = UI.AddCheckbox(container, value!, setter);
		}
		else {
			toggle = container.GetComponentInChildren<Toggle>();
			toggle = UI.UpdateCheckbox(toggle!, value!, setter);
		}
		((Image)toggle.graphic).sprite = (mixed)
			// Another sprite ripped from ChroMapper because it's unused and gets optimized out ;-;
			? UI.LoadSprite("ChroMapper_PropEdit.Resources.Line.png")
			: UI.GetSprite("Checkmark");
		((Image)toggle.graphic).color = Color.black;
		return toggle;
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
