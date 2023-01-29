using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using SimpleJSON;

using Beatmap.Base;
using Beatmap.Enums;
using Beatmap.Helper;
using Beatmap.V2;

using ChroMapper_PropEdit.Components;
using ChroMapper_PropEdit.Enums;

using Convert = System.Convert;

namespace ChroMapper_PropEdit.UserInterface {

public class MainWindow {
	public readonly string SETTINGS_FILE = UnityEngine.Application.persistentDataPath + "/PropEdit.json";
	
	public ExtensionButton main_button;
	public InputAction keybind;
	public InputAction shift;
	public GameObject window;
	public GameObject title;
	public GameObject panel;
	public Scrollbar scrollbar;
	public ScrollToTop scroll_to_top;
	public List<GameObject> elements = new List<GameObject>();
	public IEnumerable<BaseObject> editing;
	
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
		window.GetComponent<DragWindowController>().OnDragWindow += AnchoredPosSave;
		
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
		}
		{
			var fitter = panel.AddComponent<ContentSizeFitter>();
			fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		}
		
		var scroller = UI.AddChild(container, "Scroll Bar", typeof(Scrollbar));
		UI.AttachTransform(scroller, new Vector2(10, 0), new Vector2(-5.5f, 0), new Vector2(1, 0), new Vector2(1, 1));
		scrollbar = scroller.GetComponent<Scrollbar>();
		scrollbar.transition = Selectable.Transition.ColorTint;
		scrollbar.direction = Scrollbar.Direction.BottomToTop;
		scrollbar.value = 1f;
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
		
		// C.U.M. required
		typeof(BeatmapActionContainer)
			.GetEvent("ActionUndoEvent")
			?.AddMethod
			?.Invoke(null, new object[] { (System.Action<BeatmapAction>) ((_) => {
				UpdateSelection(false);
			})});
		typeof(BeatmapActionContainer)
			.GetEvent("ActionRedoEvent")
			?.AddMethod
			?.Invoke(null, new object[] { (System.Action<BeatmapAction>) ((_) => {
				UpdateSelection(false);
			})});
		
		if (typeof(BeatmapActionContainer).GetEvent("ActionUndoEvent") == null) {
			Debug.LogWarning("Warning: Insufficient C.U.M., PropEdit won't update after undo/redo!");
		}
		
		UpdateSelection(true);
		
		keybind = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/n");
		keybind.performed += (ctx) => {
			ToggleWindow();
		};
		shift = new InputAction(binding: "<Keyboard>/shift");
		shift.started += (ctx) => {
			CMInputCallbackInstaller.InputInstance.NodeEditor.ToggleNodeEditor.Disable();
			keybind.Enable();
		};
		shift.canceled += (ctx) => {
			CMInputCallbackInstaller.InputInstance.NodeEditor.ToggleNodeEditor.Enable();
			keybind.Disable();
		};
		shift.Enable();
	}
	
	public void ToggleWindow() {
		LoadSettings();
		window.SetActive(!window.activeSelf);
		UpdateSelection(false);
	}
	
	public void Denit() {
		shift?.Disable();
		keybind?.Disable();
	}
	
	public void UpdateSelection(bool real) {
		foreach (var e in elements) {
			Object.Destroy(e);
		}
		elements.Clear();
		
		editing = SelectionController.SelectedObjects.Select(it => it);
		
		if (SelectionController.HasSelectedObjects()) {
			title.GetComponent<TextMeshProUGUI>().text = SelectionController.SelectedObjects.Count + " Items selected";
			
			if (editing.GroupBy(o => o.ObjectType).Count() > 1) {
				elements.Add(UI.AddLabel(panel.transform, "Unsupported", "Multi-Type Unsupported!", new Vector2(0, 0)));
				return;
			}
			
			var o = editing.First();
			var type = o.ObjectType;
			
			AddParsed("Beat", Data.GetSet<float>(typeof(BaseObject), "Time"));
			
			switch (type) {
				case ObjectType.Note:
					AddDropdown("Type", Data.GetSet<int>(typeof(BaseNote), "Type"), typeof(NoteTypes));
					AddDropdown("Direction", Data.GetSet<int>(typeof(BaseNote), "CutDirection"), typeof(CutDirections));
					AddField("");
					AddField("Chroma");
					AddTextbox("Color", Data.GetSetColor(o.CustomKeyColor));
					if (o is V2Note) {
						AddCheckbox("Disable Spawn Effect", Data.CustomGetSet<bool>("_disableSpawnEffect"), false);
					}
					else {
						AddCheckbox("Disable Spawn Effect", Data.CustomGetSet<bool>("spawnEffect"), true);
					}
					
					AddField("");
					AddField("Noodle Extensions");
					AddParsed("Direction", Data.GetSet<int>(typeof(BaseNote), "CustomDirection"), true);
					// TODO: position, rotation
					if (o is V2Note) {
						AddParsed("NJS", Data.CustomGetSet<float>("_noteJumpMovementSpeed"), true);
						AddParsed("Spawn Offset", Data.CustomGetSet<float>("_noteJumpStartBeatOffset"), true);
						AddCheckbox("Fake", Data.CustomGetSet<bool>("_fake"), false);
						AddCheckbox("Interactable", Data.CustomGetSet<bool>("_interactable"), true);
					}
					else {
						AddParsed("NJS", Data.CustomGetSet<float>("noteJumpMovementSpeed"), true);
						AddParsed("Spawn Offset", Data.CustomGetSet<float>("noteJumpStartBeatOffset"), true);
						AddCheckbox("Interactable", Data.CustomGetSet<bool>("uninteractable"), false);
						AddCheckbox("Disable Gravity", Data.CustomGetSet<bool>("disableNoteGravity"), false);
						AddCheckbox("Disable Look", Data.CustomGetSet<bool>("disableNoteLook"), false);
					}
					
					// TODO: flip
					
					break;
				case ObjectType.Obstacle:
					AddParsed("Duration", Data.GetSet<float>(typeof(BaseObstacle), "Duration"));
					AddDropdown("Height", Data.GetSet<int>(typeof(BaseObstacle), "Type"), typeof(WallHeights));
					AddParsed("Width", Data.GetSet<int>(typeof(BaseObstacle), "Width"));
					
					AddField("");
					AddField("Chroma");
					AddTextbox("Color", Data.GetSetColor(o.CustomKeyColor));
					
					AddField("");
					AddField("Noodle Extensions");
					// TODO: position, rotation
					if (o is V2Obstacle) {
						AddParsed("NJS", Data.CustomGetSet<float>("_noteJumpMovementSpeed"), true);
						AddParsed("Spawn Offset", Data.CustomGetSet<float>("_noteJumpStartBeatOffset"), true);
						AddCheckbox("Fake", Data.CustomGetSet<bool>("_fake"), false);
						AddCheckbox("Interactable", Data.CustomGetSet<bool>("_interactable"), true);
					}
					else {
						AddParsed("NJS", Data.CustomGetSet<float>("noteJumpMovementSpeed"), true);
						AddParsed("Spawn Offset", Data.CustomGetSet<float>("noteJumpStartBeatOffset"), true);
						AddCheckbox("Interactable", Data.CustomGetSet<bool>("uninteractable"), false);
					}
					
					// TODO: scale
					break;
				case ObjectType.Event:
					var env = BeatSaberSongContainer.Instance.Song.EnvironmentName;
					var events = editing.Select(o => (BaseEvent)o);
					var f = events.First();
					// Light
					if (events.Where(e => e.IsLightEvent(env)).Count() == editing.Count()) {
						AddDropdown("Value", Data.GetSet<int>(typeof(BaseEvent), "Value"), typeof(LightValues));
						// TODO: lightID
						AddField("");
						AddField("Chroma");
						AddTextbox("Color", Data.GetSetColor(o.CustomKeyColor));
					}
					// Laser Speeds
					if (events.Where(e => e.IsLaserRotationEvent(env)).Count() == editing.Count()) {
						AddParsed("Speed", Data.GetSet<int>(typeof(BaseEvent), "Value"), true);
						AddField("");
						AddField("Chroma");
						AddCheckbox("Lock Rotation", Data.CustomGetSet<bool> (f.CustomKeyLockRotation), false);
						AddDropdown("Direction",     Data.CustomGetSet<int>  (f.CustomKeyDirection), typeof(LaserDirection), true);
						AddParsed("Precise Speed",   Data.CustomGetSet<float>(f.CustomKeyPreciseSpeed), true);
					}
					if (events.Where(e => e.Type == (int)EventTypeValue.RingRotation).Count() == editing.Count()) {
						AddField("");
						AddField("Chroma");
						AddTextbox("Filter",     Data.GetSetString(typeof(BaseEvent), "CustomNameFilter"));
						if (o is V2Event) {
							AddCheckbox("Reset", Data.CustomGetSet<bool>("_reset"), false);
						}
						AddParsed("Rotation",    Data.CustomGetSet<int>  (f.CustomKeyLaneRotation), true);
						AddParsed("Step",        Data.CustomGetSet<float>(f.CustomKeyStep), true);
						AddParsed("Propagation", Data.CustomGetSet<float>(f.CustomKeyProp), true);
						AddParsed("Speed",       Data.CustomGetSet<float>(f.CustomKeySpeed), true);
						AddDropdown("Direction", Data.CustomGetSet<int>  (f.CustomKeyDirection), typeof(RingDirection), true);
						if (o is V2Event) {
							AddCheckbox("Counter Spin", Data.CustomGetSet<bool>("_counterSpin"), false);
						}
					}
					if (events.Where(e => e.Type == (int)EventTypeValue.RingZoom).Count() == editing.Count()) {
						AddField("");
						AddField("Chroma");
						AddParsed("Step",  Data.CustomGetSet<float>(f.CustomKeyStep), true);
						AddParsed("Speed", Data.CustomGetSet<float>(f.CustomKeySpeed), true);
					}
					break;
			}
		}
		else {
			title.GetComponent<TextMeshProUGUI>().text = "No items selected";
		}
		if (real) {
			scroll_to_top.Trigger();
		}
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
	private Toggle AddCheckbox(string title, System.ValueTuple<System.Func<BaseObject, bool?>, System.Action<BaseObject, bool?>> get_set, bool _default) {
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
	
	private UIDropdown AddDropdown(string title, System.ValueTuple<System.Func<BaseObject, int?>, System.Action<BaseObject, int?>> get_set, System.Type type, bool nullable = false) {
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
	
	private UITextInput AddParsed<T>(string title, System.ValueTuple<System.Func<BaseObject, T?>, System.Action<BaseObject, T?>> get_set, bool nullable = false) where T : struct {
		var container = AddField(title);
		
		(var getter, var setter) = get_set;
		
		var value = GetAllOrNothing<T>(getter);
		
		var input = Object.Instantiate(PersistentUI.Instance.TextInputPrefab, container.transform);
		UI.MoveTransform((RectTransform)input.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, 1));
		input.InputField.text = value.HasValue ? (string)Convert.ChangeType(value, typeof(string)) : "";
		input.InputField.onEndEdit.AddListener((s) => {
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
			
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(MainWindow), new[] { typeof(CMInput.INodeEditorActions) });
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(MainWindow), ActionMapsDisabled);
		});
		input.InputField.onSelect.AddListener(delegate {
			if (!CMInputCallbackInstaller.IsActionMapDisabled(ActionMapsDisabled[0])) {
				CMInputCallbackInstaller.DisableActionMaps(typeof(MainWindow), new[] { typeof(CMInput.INodeEditorActions) });
				CMInputCallbackInstaller.DisableActionMaps(typeof(MainWindow), ActionMapsDisabled);
			}
		});
		
		return input;
	}
	
	private UITextInput AddTextbox(string title, System.ValueTuple<System.Func<BaseObject, string>, System.Action<BaseObject, string>> get_set) {
		var container = AddField(title);
		
		(var getter, var setter) = get_set;
		
		var value = GetAllOrNothingString(getter);
		
		var input = Object.Instantiate(PersistentUI.Instance.TextInputPrefab, container.transform);
		UI.MoveTransform((RectTransform)input.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, 1));
		input.InputField.text = value ?? "";
		input.InputField.onEndEdit.AddListener((s) => {
			if (s == "") {
				s = null;
			}
			UpdateObjectsString(setter, s);
			
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(MainWindow), new[] { typeof(CMInput.INodeEditorActions) });
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(MainWindow), ActionMapsDisabled);
		});
		input.InputField.onSelect.AddListener(delegate {
			if (!CMInputCallbackInstaller.IsActionMapDisabled(ActionMapsDisabled[0])) {
				CMInputCallbackInstaller.DisableActionMaps(typeof(MainWindow), new[] { typeof(CMInput.INodeEditorActions) });
				CMInputCallbackInstaller.DisableActionMaps(typeof(MainWindow), ActionMapsDisabled);
			}
		});
		
		return input;
	}
	
	private void UpdateObjects<T>(System.Action<BaseObject, T?> setter, T? value) where T : struct {
		var beatmapActions = new List<BeatmapObjectModifiedAction>();
		foreach (var o in editing) {
			var clone = BeatmapFactory.Clone(o);
			
			setter(o, value);
			
			beatmapActions.Add(new BeatmapObjectModifiedAction(o, o, clone, $"Edited a {o.ObjectType} with Prop Edit.", true));
		}
		
		BeatmapActionContainer.AddAction(
			new ActionCollectionAction(beatmapActions, true, false, $"Edited ({SelectionController.SelectedObjects.Count()}) objects with Prop Edit."),
			true);
		
		// Prevent selecting "--"
		UpdateSelection(false);
	}
	
	// I hate c#
	private void UpdateObjectsString(System.Action<BaseObject, string> setter, string value) {
		var beatmapActions = new List<BeatmapObjectModifiedAction>();
		foreach (var o in editing) {
			var clone = BeatmapFactory.Clone(o);
			
			setter(o, value);
			o.RefreshCustom();
			
			beatmapActions.Add(new BeatmapObjectModifiedAction(o, o, clone, $"Edited a {o.ObjectType} with Prop Edit.", true));
		}
		
		BeatmapActionContainer.AddAction(
			new ActionCollectionAction(beatmapActions, true, false, $"Edited ({SelectionController.SelectedObjects.Count()}) objects with Prop Edit."),
			true);
		
		// Prevent selecting "--"
		UpdateSelection(false);
	}
	
	private T? GetAllOrNothing<T>(System.Func<BaseObject, T?> getter) where T : struct {
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
	private string GetAllOrNothingString(System.Func<BaseObject, string> getter) {
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
	
	private void LoadSettings() {
		if (File.Exists(SETTINGS_FILE)) {
			using (var reader = new StreamReader(SETTINGS_FILE)) {
				var settings = JSON.Parse(reader.ReadToEnd()).AsObject;
				window.GetComponent<RectTransform>().anchoredPosition = new Vector2(settings["x"].AsFloat, settings["y"].AsFloat);
				window.GetComponent<RectTransform>().sizeDelta = new Vector2(settings["w"].AsInt, settings["h"].AsInt);
			}
		}
		var layout = panel.GetComponent<LayoutElement>();
		layout.minHeight = window.GetComponent<RectTransform>().sizeDelta.y - 40 - 15;
	}
	
	private void AnchoredPosSave() {
		var pos = window.GetComponent<RectTransform>().anchoredPosition;
		var settings = new JSONObject();
		settings.Add("x", pos.x);
		settings.Add("y", pos.y);
		settings.Add("w", window.GetComponent<RectTransform>().sizeDelta.x);
		settings.Add("h", window.GetComponent<RectTransform>().sizeDelta.y);
		File.WriteAllText(SETTINGS_FILE, settings.ToString(4));
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
