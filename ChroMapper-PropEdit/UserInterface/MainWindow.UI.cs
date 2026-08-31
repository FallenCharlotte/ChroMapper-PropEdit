using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using SimpleJSON;

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
		
		Selection.OnSelectionChanged += UpdateFromSelection;
#if CHROMPER_13
		SelectionController.SelectionChangedEvent += Selection.OnObjectsSelected;
		BeatmapActionContainer.ActionCreatedEvent += UpdateFromAction;
		BeatmapActionContainer.ActionUndoEvent += UpdateFromAction;
		BeatmapActionContainer.ActionRedoEvent += UpdateFromAction;
#else
		SelectionController.OnSelectionChanged += Selection.OnObjectsSelected;
		BeatmapActionContainer.OnActionCreated += UpdateFromAction;
		BeatmapActionContainer.OnActionUndo += UpdateFromAction;
		BeatmapActionContainer.OnActionRedo += UpdateFromAction;
#endif
		
		Plugin.toggle_window!.performed += OnToggleWindow;
		
		bundleInfo = new BundleInfo();
	}
	
	public void OnDestroy() {
		Selection.OnSelectionChanged -= UpdateFromSelection;
#if CHROMPER_13
		SelectionController.SelectionChangedEvent -= Selection.OnObjectsSelected;
		BeatmapActionContainer.ActionCreatedEvent -= UpdateFromAction;
		BeatmapActionContainer.ActionUndoEvent -= UpdateFromAction;
		BeatmapActionContainer.ActionRedoEvent -= UpdateFromAction;
#else
		SelectionController.OnSelectionChanged -= Selection.OnObjectsSelected;
		BeatmapActionContainer.OnActionCreated -= UpdateFromAction;
		BeatmapActionContainer.OnActionUndo -= UpdateFromAction;
		BeatmapActionContainer.OnActionRedo -= UpdateFromAction;
#endif
		Plugin.toggle_window!.performed -= OnToggleWindow;
		Selection.OnDeselectAll();
	}
	
	private void UpdateFromSelection() {
		Plugin.Trace($"{Time.frameCount} UpdateFromSelection");
		TriggerRefresh();
	}
	
	private void UpdateFromAction(BeatmapAction? _) {
		Plugin.Trace($"{Time.frameCount} UpdateFromAction");
		TriggerRefresh();
	}
	
#region Form Fields
	
	private GameObject Line(string title, Vector2? size = null, string tooltip = "") {
		var existing = (!full_rebuild) ? current_panel!.transform.Find(title)?.gameObject : null;
		return existing ?? UI.AddField(current_panel!, title, size, tooltip);
	}
	
	public override Collapsible Expando(string name, string label, bool expanded, string tooltip = "", bool background = true) {
		var expando = ((!full_rebuild)
			? current_panel!.transform.Find(name)?.GetComponent<Collapsible>()
			: null) ?? Collapsible.Create(current_panel ?? panel!, name, label, expanded, tooltip, background);
		panels.Push(expando.panel!);
		return expando;
	}
	
	public override Toggle EditCheckbox(string label, IAccessor<bool> accessor, string tooltip = "")
		=> EditCheckbox(label, (IAccessor<bool?>)accessor, null, tooltip);
	
	// CustomData node gets removed when value = default
	public Toggle EditCheckbox(string label, IAccessor<bool?> accessor, bool? _default, string tooltip = "") {
		var container = Line(label, null, tooltip);
		
		var value_or = accessor.Get();
		var mixed = accessor.IsMixed();
		
		// Do some jank, mixed needs to be drawn as true but act like false
		var value = (value_or ?? _default ?? false) || mixed;
		
		var conv = new Converter<bool?, bool>(
			(b) => b ?? _default ?? false,
			(v) => {
				v ^= mixed;
				return (v == _default)
					? null
					: v;
			}
		);
		var modded = (accessor as MultiAccessor<bool?>)!.Insert(conv);
		
		Toggle input;
		
		if (full_rebuild) {
			input = UI.AddCheckbox(container, value!, modded.Set);
		}
		else {
			input = container.GetComponentInChildren<Toggle>();
			input = UI.UpdateCheckbox(input!, value!, modded.Set);
		}
		((Image)input.graphic).sprite = (mixed)
			// Another sprite ripped from ChroMapper because it's unused and gets optimized out ;-;
			? UI.LoadSprite("ChroMapper_PropEdit.Resources.Line.png")
			: UI.GetSprite("Checkmark");
		((Image)input.graphic).color = Color.black;
		return input;
	}
	
	// I hate C# I hate C# I hate C#
	// Can't override because C# sucks and can't handle nullable correctly
	// Literally just says there's "no suitable method found to override" with copy-pasted signature
#pragma warning disable CS0114
	public virtual UIDropdown EditDropdown<T>(string? label, IAccessor<T?> accessor, Enums.Map<T?> options, bool nullable = false, string tooltip = "") {
#pragma warning restore CS0114
		var container = (label != null)
			? Line(label, null, tooltip)
			: current_panel!;
		
		if (!full_rebuild && container.GetComponentInChildren<UIDropdown>() is UIDropdown input) {
			return UI.UpdateDropdown(input, accessor.Get(), accessor.Set, options, nullable);
		}
		return UI.AddDropdown(container, accessor.Get(), accessor.Set, options, nullable);
	}
	
	public override Textbox EditParsed<T>(string title, IAccessor<T?> accessor, string tooltip = "") where T : struct
		=> EditTextbox(title, accessor + Data.TextParser<T>(), false, tooltip);
	
	public override Textbox EditTextbox(string? title, IAccessor<string?> accessor, bool tall = false, string tooltip = "") {
		var container = (title != null)
			? Line(title, tall ? (new Vector2(0, 22)) : null, tooltip)
			: current_panel!;
		
		var value = accessor.Get();
		var mixed = accessor.IsMixed();
		
		if (!full_rebuild && container.GetComponentInChildren<Textbox>() is Textbox input) {
			return input.Set(value, mixed, accessor.Set);
		}
		return UI.SetMixed(UI.AddTextbox(container, value, accessor.Set), mixed);
	}
	
	private void EditPointDefinition(string title, IAccessor<string?> accessor, string tooltip = "") {
		PointDefinitionEditor
			.Singleton(current_panel!, title, tooltip)
			.Set(accessor);
	}
	
#endregion
	
#region Custom helper fields
	
	private void EditAnimation(string name, string path, string default_json, string tooltip) {
		PointDefinitionEditor
			.Singleton(
				current_panel!,
				name,
				tooltip)
			.Set(
				CustomFieldRaw(path),
				CustomJSONNode(path, default_json).Set);
	}
	
	private void EditColor(string label, string key, string tooltip = "") {
		EditTextbox(label, CustomField(key, "CustomData").Insert(Data.JSONColor()), false, tooltip);
	}
	
	// Unarrayable track
	private void EditTrack(string? title, IAccessor<string?> accessor, string tooltip = "") {
		// TODO: Want a combined dropdown + custom textbox
		/*
		var collection = BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.CustomEvent) as CustomEventGridContainer;
		var tracks = new Map<string?>().AddRange(collection!.EventsByTrack.Keys);
		AddDropdown(title, get_set, tracks, true, tooltip);
		*/
		EditTextbox(title, accessor, false, tooltip);
	}
	
	// Arrayable tracks
	private void EditTracks(string title, MultiAccessor<JSONNode?> accessor, string tooltip = "") {
		ArrayEditor
			.Singleton(current_panel!, title, tooltip)
			.Set(accessor.Insert(ArrayEditor.JsonConverter()));
		
	}
	
	private void EditPrefab(string title, string prop, bool nullable = true) {
		if (bundleInfo?.Prefabs == null) {
			EditTextbox(title, DataField<string>(prop), false);
		}
		else {
			EditDropdown(title, DataField<string>(prop), bundleInfo.Prefabs, nullable);
		}
	}
	
	//[X, Y, Z] JSON Array
	private void EditVector3(string name, MultiAccessor<Vector3?> accessor) {
		var v3json = new Converter<Vector3?, JSONNode?>(
			(vec) => (vec != null)
				? (new JSONArray()).WriteVector3(vec ?? new Vector3()) // Holy fuck why can't the ! operator ever do its job
				: null,
			(node) => node?.ReadVector3()
		);
		
		EditTextbox(name, accessor.Insert(v3json).Insert(Data.JSONRaw()), true);
	}
	
	private void EditEEComponent(string name, IAccessor<bool?> accessor, System.Action editor) {
		var checkbox = EditCheckbox(name, accessor, null);
		
		var comp_container = Collapsible.Singleton(current_panel!, "_"+name, name, false);
		
		panels.Push(comp_container.panel!);
		editor();
		panels.Pop();
		
		checkbox.onValueChanged.AddListener((v) => {
			if (v) {
				comp_container.OnAnimationComplete = null;
				comp_container.SetExpanded(false);
				comp_container.gameObject.SetActive(true);
				comp_container.SetExpanded(true);
			}
			else {
				comp_container.OnAnimationComplete = (v) => {
					comp_container.gameObject.SetActive(v);
				};
				comp_container.SetExpanded(v);
			}
		});
		
		comp_container.gameObject.SetActive(checkbox.isOn);
		comp_container.SetExpanded(checkbox.isOn);
	}
	
#endregion
	
}

}
