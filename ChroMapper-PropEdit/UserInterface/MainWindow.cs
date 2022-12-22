using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using SimpleJSON;

using Convert = System.Convert;

using ChroMapper_PropEdit.Components;
using ChroMapper_PropEdit.Enums;

namespace ChroMapper_PropEdit.UserInterface {

public class MainWindow {
	public ExtensionButton main_button;
	public GameObject window;
	public GameObject title;
	public GameObject panel;
	public ScrollToTop scroll_to_top;
	public List<GameObject> elements = new List<GameObject>();
	public HashSet<BeatmapObject> editing;
	
	public MainWindow() {
		main_button = ExtensionButtons.AddButton(
			UI.LoadSprite("ChroMapper_PropEdit.Resources.Icon.png"),
			"Prop Edit",
			ToggleWindow);
	}
	
	public void Init(MapEditorUI mapEditorUI) {
		var parent = mapEditorUI.MainUIGroup[5];
		
		window = new GameObject("PropEdit Window");
		window.transform.parent = parent.transform;
		// Window Drag
		window.AddComponent<DragWindowController>();
		window.GetComponent<DragWindowController>().canvas = parent.GetComponent<Canvas>();
		//window.GetComponent<DragWindowController>().OnDragWindow += AnchoredPosSave;

		UI.AttachTransform(window, new Vector2(220, 256), new Vector2(0, 0), new Vector2(0.5f, 0.5f));
		{
			var image = window.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.24f, 0.24f, 0.24f, 1);
		}
		
		window.SetActive(false);
		
		title = UI.AddLabel(window.transform, "Title", "Prop Editor", new Vector2(10, -20), size: new Vector2(-10, 30), font_size: 28, anchor_min: new Vector2(0, 1), anchor_max: new Vector2(1, 1), align: TextAlignmentOptions.Left);
		
		var container = UI.AddChild(window, "Prop Scroll Container");
		UI.AttachTransform(container, new Vector2(-10, -40), new Vector2(0, -15), new Vector2(0, 0), new Vector2(1, 1));
		{
			var image = container.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.1f, 0.1f, 0.1f, 1);
		}
		
		var scroll_area = UI.AddChild(container, "Scroll Area", typeof(ScrollRect));
		UI.AttachTransform(scroll_area, new Vector2(0, -10), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1));
		var mask = scroll_area.AddComponent<RectMask2D>();
		var srect = scroll_area.GetComponent<ScrollRect>();
		srect.vertical = true;
		srect.horizontal = false;
		srect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
		
		panel = UI.AddChild(scroll_area, "Prop Panel");
		srect.content = UI.AttachTransform(panel, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1));
		{
			var layout = panel.AddComponent<VerticalLayoutGroup>();
			layout.padding = new RectOffset(10, 15, 0, 0);
			layout.spacing = 0;
			layout.childControlHeight = false;
			layout.childForceExpandHeight = false;
			layout.childForceExpandWidth = true;
			layout.childAlignment = TextAnchor.UpperCenter;
		}
		{
			var layout = panel.AddComponent<LayoutElement>();
			layout.minHeight = 256 - 40 - 15;
		}
		{
			var fitter = panel.AddComponent<ContentSizeFitter>();
			fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		}
		
		var scroller = UI.AddChild(container, "Scroll Bar", typeof(Scrollbar));
		UI.AttachTransform(scroller, new Vector2(10, 0), new Vector2(-5.5f, 0), new Vector2(1, 0), new Vector2(1, 1));
		var scrollbar = scroller.GetComponent<Scrollbar>();
		scrollbar.transition = Selectable.Transition.ColorTint;
		scrollbar.direction = Scrollbar.Direction.BottomToTop;
		srect.verticalScrollbar = scrollbar;
		scroll_to_top = window.AddComponent<ScrollToTop>();
		scroll_to_top.scrollbar = scrollbar;
		
		var slide = UI.AddChild(scroller, "Slide");
		UI.AttachTransform(slide, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1));
		{
			var image = slide.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.24f, 0.24f, 0.24f, 1);
		}
		
		var handle = UI.AddChild(slide, "Handle", typeof(Canvas));
		UI.AttachTransform(handle, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1));
		{
			var image = handle.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.7f, 0.7f, 0.7f, 1);
			scrollbar.targetGraphic = image;
			scrollbar.handleRect = handle.GetComponent<RectTransform>();
		}
		
		UpdateSelection();
	}
	
	public void ToggleWindow() {
		window.SetActive(!window.activeSelf);
	}
	
	public void UpdateSelection() {
		foreach (var e in elements) {
			Object.Destroy(e);
		}
		elements.Clear();
		
		editing = SelectionController.SelectedObjects;
		
		if (SelectionController.HasSelectedObjects()) {
			title.GetComponent<TextMeshProUGUI>().text = SelectionController.SelectedObjects.Count + " Items selected";
			
			if (editing.GroupBy(o => o.BeatmapType).Count() > 1) {
				elements.Add(UI.AddLabel(panel.transform, "Unsupported", "Multi-Type Unsupported!", new Vector2(0, 0)));
				return;
			}
			
			var type = editing.First().BeatmapType;
			
			AddParsed("Beat", Data.BaseGetSet<float>(typeof(BeatmapObject), "Time"));
			
			switch (type) {
				case BeatmapObject.ObjectType.Note:
					AddDropdown("Type", Data.BaseGetSet<int>(typeof(BeatmapNote), "Type"), typeof(NoteTypes));
					AddDropdown("Direction", Data.BaseGetSet<int>(typeof(BeatmapNote), "CutDirection"), typeof(CutDirections));
					AddField("");
					AddField("Chroma");
					// TODO: color
					AddCheckbox("Disable Spawn Effect", Data.CustomGetSet<bool>("_disableSpawnEffect"), false);
					
					AddField("");
					AddField("Noodle Extensions");
					// TODO: position, rotation
					AddParsed("NJS", Data.CustomGetSet<float>("_noteJumpMovementSpeed"), true);
					AddParsed("Spawn Offset", Data.CustomGetSet<float>("_noteJumpStartBeatOffset"), true);
					AddCheckbox("Fake", Data.CustomGetSet<bool>("_fake"), false);
					AddCheckbox("Interactable", Data.CustomGetSet<bool>("_interactable"), true);
					
					AddParsed("Direction", Data.CustomGetSet<float>("_cutDirection"), true);
					// TODO: flip
					AddCheckbox("Disable Gravity", Data.CustomGetSet<bool>("_disableNoteGravity"), false);
					AddCheckbox("Disable Look", Data.CustomGetSet<bool>("_disableNoteLook"), false);
					break;
				case BeatmapObject.ObjectType.Obstacle:
					AddParsed("Duration", Data.BaseGetSet<float>(typeof(BeatmapObstacle), "Duration"));
					AddDropdown("Height", Data.BaseGetSet<int>(typeof(BeatmapObstacle), "Type"), typeof(WallHeights));
					AddParsed("Width", Data.BaseGetSet<int>(typeof(BeatmapObstacle), "Width"));
					
					// TODO: Chroma color, size
					
					AddField("");
					AddField("Noodle Extensions");
					// TODO: position, rotation
					AddParsed("NJS", Data.CustomGetSet<float>("_noteJumpMovementSpeed"), true);
					AddParsed("Spawn Offset", Data.CustomGetSet<float>("_noteJumpStartBeatOffset"), true);
					AddCheckbox("Fake", Data.CustomGetSet<bool>("_fake"), false);
					AddCheckbox("Interactable", Data.CustomGetSet<bool>("_interactable"), true);
					// TODO: scale
					break;
				case BeatmapObject.ObjectType.Event:
					var events = editing.Select(o => (MapEvent)o);
					// Light
					if (events.Where(e => !e.IsUtilityEvent).Count() == editing.Count()) {
						AddDropdown("Value", Data.BaseGetSet<int>(typeof(MapEvent), "Value"), typeof(LightValues));
						// TODO: lightID, color
					}
					// Laser Speeds
					if (events.Where(e => e.IsLaserSpeedEvent).Count() == editing.Count()) {
						AddParsed("Speed", Data.BaseGetSet<int>(typeof(MapEvent), "Value"), true);
						AddField("");
						AddField("Chroma");
						AddCheckbox("Lock Rotation", Data.CustomGetSet<bool>("_lockPosition"), false);
						AddDropdown("Direction", Data.CustomGetSet<int>("_direction"), typeof(LaserDirection), true);
						AddParsed("Precise Speed", Data.CustomGetSet<float>("_speed"), true);
					}
					if (events.Where(e => e.Type == MapEvent.EventTypeRingsRotate).Count() == editing.Count()) {
						AddField("Chroma");
						AddTextbox("Filter", Data.CustomGetSetString("_nameFilter"));
						AddCheckbox("Reset", Data.CustomGetSet<bool>("_reset"), false);
						AddParsed("Rotation", Data.CustomGetSet<float>("_rotation"), true);
						AddParsed("Step", Data.CustomGetSet<float>("_step"), true);
						AddParsed("Propagation", Data.CustomGetSet<float>("_prop"), true);
						AddParsed("Speed", Data.CustomGetSet<float>("_speed"), true);
						AddDropdown("Direction", Data.CustomGetSet<int>("_direction"), typeof(RingDirection), true);
						AddCheckbox("Counter Spin", Data.CustomGetSet<bool>("_counterSpin"), false);
					}
					if (events.Where(e => e.Type == MapEvent.EventTypeRingsZoom).Count() == editing.Count()) {
						AddField("Chroma");
						AddParsed("Step", Data.CustomGetSet<float>("_step"), true);
						AddParsed("Speed", Data.CustomGetSet<float>("_speed"), true);
					}
					break;
			}
		}
		else {
			title.GetComponent<TextMeshProUGUI>().text = "No items selected";
		}
		scroll_to_top.Trigger();
	}
	
	// Form Fields
	
	private GameObject AddField(string title) {
		var container = UI.AddChild(panel, title + " Container");
		UI.AttachTransform(container, new Vector2(0, 20), new Vector2(0, 0));
		
		elements.Add(container);
		
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
	
	// CustomData node gets removed when value = default
	private Toggle AddCheckbox(string title, System.ValueTuple<System.Func<BeatmapObject, bool?>, System.Action<BeatmapObject, bool?>> get_set, bool _default) {
		var container = AddField(title);
		
		(var getter, var setter) = get_set;
		
		bool value = GetAllOrNothing<bool>(getter) ?? _default;
		
		var original = GameObject.Find("Strobe Generator").GetComponentInChildren<Toggle>(true);
		var toggleObject = UnityEngine.Object.Instantiate(original, container.transform);
		var toggleComponent = toggleObject.GetComponent<Toggle>();
		var colorBlock = toggleComponent.colors;
		colorBlock.normalColor = Color.white;
		toggleComponent.colors = colorBlock;
		toggleComponent.isOn = value;
		toggleComponent.onValueChanged.AddListener((v) => {
			if (v == _default) {
				UpdateObjects<bool>(setter, null);
			}
			else {
				UpdateObjects<bool>(setter, v);
			}
		});
		return toggleComponent;
	}
	
	private UIDropdown AddDropdown(string title, System.ValueTuple<System.Func<BeatmapObject, int?>, System.Action<BeatmapObject, int?>> get_set, System.Type type, bool nullable = false) {
		var container = AddField(title);
		
		(var getter, var setter) = get_set;
		
		// Get values from selected items
		var options = new List<string>();
		var value = GetAllOrNothing<int>(getter);
		if (!value.HasValue) {
			options.Add("--");
		}
		else if (nullable) {
			options.Add("Unset");
			value += 1;
		}
		options.AddRange(System.Enum.GetNames(type).ToList());
		
		var dropdown = Object.Instantiate(PersistentUI.Instance.DropdownPrefab, container.transform);
		UI.MoveTransform((RectTransform)dropdown.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, 1));
		dropdown.SetOptions(options);
		dropdown.Dropdown.value = value ?? 0;
		dropdown.Dropdown.onValueChanged.AddListener((i) => {
			int ei = System.Enum.GetNames(type).ToList().IndexOf(options[i]);
			ushort? value = (ei >= 0)
				? (ushort)System.Enum.GetValues(type).GetValue(ei)
				: null;
			UpdateObjects<int>(setter, value);
		});
		
		return dropdown;
	}
	
	private UITextInput AddParsed<T>(string title, System.ValueTuple<System.Func<BeatmapObject, T?>, System.Action<BeatmapObject, T?>> get_set, bool nullable = false) where T : struct {
		var container = AddField(title);
		
		(var getter, var setter) = get_set;
		
		var value = GetAllOrNothing<T>(getter);
		
		var input = Object.Instantiate(PersistentUI.Instance.TextInputPrefab, container.transform);
		UI.MoveTransform((RectTransform)input.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, 1));
		input.InputField.text = value.HasValue ? (string)Convert.ChangeType(value, typeof(string)) : "";
		input.InputField.onEndEdit.AddListener(delegate {
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(MainWindow), new[] { typeof(CMInput.INodeEditorActions) });
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(MainWindow), ActionMapsDisabled);
		});
		input.InputField.onSelect.AddListener(delegate {
			if (!CMInputCallbackInstaller.IsActionMapDisabled(ActionMapsDisabled[0])) {
				CMInputCallbackInstaller.DisableActionMaps(typeof(MainWindow), new[] { typeof(CMInput.INodeEditorActions) });
				CMInputCallbackInstaller.DisableActionMaps(typeof(MainWindow), ActionMapsDisabled);
			}
		});
		input.InputField.onValueChanged.AddListener((s) => {
			// No IParsable in mono ;_;
			var methods = typeof(T).GetMethods();
			System.Reflection.MethodInfo parse = null;
			foreach (var method in methods) {
				if (method.Name == "TryParse") {
					parse = method;
					break;
				}
			}
			object[] parameters = new object[]{s, null};
			bool res = (bool)parse.Invoke(null, parameters);
			if (!res && nullable) {
				UpdateObjects<T>(setter, null);
			}
			else {
				UpdateObjects<T>(setter, (T)parameters[1]);
			}
		});
		
		return input;
	}
	
	private UITextInput AddTextbox(string title, System.ValueTuple<System.Func<BeatmapObject, string>, System.Action<BeatmapObject, string>> get_set) {
		var container = AddField(title);
		
		(var getter, var setter) = get_set;
		
		var value = GetAllOrNothingString(getter);
		
		var input = Object.Instantiate(PersistentUI.Instance.TextInputPrefab, container.transform);
		UI.MoveTransform((RectTransform)input.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, 1));
		input.InputField.text = value ?? "";
		input.InputField.onEndEdit.AddListener(delegate {
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(MainWindow), new[] { typeof(CMInput.INodeEditorActions) });
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(MainWindow), ActionMapsDisabled);
		});
		input.InputField.onSelect.AddListener(delegate {
			if (!CMInputCallbackInstaller.IsActionMapDisabled(ActionMapsDisabled[0])) {
				CMInputCallbackInstaller.DisableActionMaps(typeof(MainWindow), new[] { typeof(CMInput.INodeEditorActions) });
				CMInputCallbackInstaller.DisableActionMaps(typeof(MainWindow), ActionMapsDisabled);
			}
		});
		input.InputField.onValueChanged.AddListener((s) => {
			if (s == "") {
				s = null;
			}
			UpdateObjectsString(setter, s);
		});
		
		return input;
	}
	
	private void UpdateObjects<T>(System.Action<BeatmapObject, T?> setter, T? value) where T : struct {
		var beatmapActions = new List<BeatmapObjectModifiedAction>();
		foreach (var o in editing) {
			var clone = System.Activator.CreateInstance(o.GetType(), new object[] { o.ConvertToJson() }) as BeatmapObject;
			
			setter(clone, value);
			
			beatmapActions.Add(new BeatmapObjectModifiedAction(clone, o, o, $"Edited a {o.BeatmapType} with Prop Edit.", true));
		}
		
		BeatmapActionContainer.AddAction(
			new ActionCollectionAction(beatmapActions, true, false, $"Edited ({SelectionController.SelectedObjects.Count()}) objects with Prop Edit."),
			true);
		
		// Prevent selecting "--"
		UpdateSelection();
	}
	
	// I hate c#
	private void UpdateObjectsString(System.Action<BeatmapObject, string> setter, string value) {
		var beatmapActions = new List<BeatmapObjectModifiedAction>();
		foreach (var o in editing) {
			var clone = System.Activator.CreateInstance(o.GetType(), new object[] { o.ConvertToJson() }) as BeatmapObject;
			
			setter(clone, value);
			
			beatmapActions.Add(new BeatmapObjectModifiedAction(clone, o, o, $"Edited a {o.BeatmapType} with Prop Edit.", true));
		}
		
		BeatmapActionContainer.AddAction(
			new ActionCollectionAction(beatmapActions, true, false, $"Edited ({SelectionController.SelectedObjects.Count()}) objects with Prop Edit."),
			true);
		
		// Prevent selecting "--"
		UpdateSelection();
	}
	
	private T? GetAllOrNothing<T>(System.Func<BeatmapObject, T?> getter) where T : struct {
		var it = editing.GetEnumerator();
		it.MoveNext();
		var last = getter(it.Current);
		// baby C# though null checks
		if (last is T l) {
			while (it.MoveNext()) {
				if (getter(it.Current) is T v) {
					if (!EqualityComparer<T>.Default.Equals(v, l)) {
						last = null;
						break;
					}
				}
			}
		}
		
		return last;
	}
	
	// I hate C#
	private string GetAllOrNothingString(System.Func<BeatmapObject, string> getter) {
		var it = editing.GetEnumerator();
		it.MoveNext();
		var last = getter(it.Current);
		while (last != null && it.MoveNext()) {
			if (last != getter(it.Current)) {
				last = null;
				break;
			}
		}
		
		return last;
	}
	
	// Stop textbox input from triggering actions, copied from the node editor
	
	private readonly System.Type[] actionMapsEnabledWhenNodeEditing = {
		typeof(CMInput.ICameraActions), typeof(CMInput.IBeatmapObjectsActions), typeof(CMInput.INodeEditorActions),
		typeof(CMInput.ISavingActions), typeof(CMInput.ITimelineActions)
	};
	
	private System.Type[] ActionMapsDisabled => typeof(CMInput).GetNestedTypes()
		.Where(x => x.IsInterface && !actionMapsEnabledWhenNodeEditing.Contains(x)).ToArray();
}

}
