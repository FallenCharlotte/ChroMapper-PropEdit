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

public partial class MainWindow {
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
	
	public void ToggleWindow() {
		LoadSettings();
		window.SetActive(!window.activeSelf);
		UpdateSelection(false);
	}
	
	public void Denit() {
		shift?.Disable();
		keybind?.Disable();
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
	
#region Form Fields
	
	private GameObject AddField(string title, bool tall = false) {
		var container = UI.AddChild(panel, title + " Container");
		UI.AttachTransform(container, new Vector2(0, tall ? 22 : 20), new Vector2(0, 0));
		
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
		
		bool value = Data.GetAllOrNothing<bool>(editing, getter) ?? _default;
		
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
	
	private UIDropdown AddDropdownI(string title, System.ValueTuple<System.Func<BaseObject, int?>, System.Action<BaseObject, int?>> get_set, Map<int> type, bool nullable = false) {
		var container = AddField(title);
		
		(var getter, var setter) = get_set;
		
		// Get values from selected items
		var options = new List<string>();
		var value = Data.GetAllOrNothing<int>(editing, getter);
		if (!value.HasValue) {
			options.Add("--");
		}
		else if (nullable) {
			options.Add("Unset");
			value += 1;
		}
		options.AddRange(type.dict.Values.ToList());
		
		var dropdown = Object.Instantiate(PersistentUI.Instance.DropdownPrefab, container.transform);
		UI.MoveTransform((RectTransform)dropdown.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, 1));
		dropdown.SetOptions(options);
		dropdown.Dropdown.value = value ?? 0;
		dropdown.Dropdown.onValueChanged.AddListener((i) => {
			int? value = type.Backward(options[i]);
			UpdateObjects<int>(setter, value);
		});
		
		return dropdown;
	}
	
	// I hate C#
#nullable enable
	private UIDropdown AddDropdownS(string title, System.ValueTuple<System.Func<BaseObject, string?>, System.Action<BaseObject, string?>> get_set, Map type, bool nullable = false) {
		var container = AddField(title);
		
		(var getter, var setter) = get_set;
		
		// Get values from selected items
		var options = new List<string>();
		var value = Data.GetAllOrNothing(editing, getter);
		int i = 0;
		if (value == null) {
			options.Add("--");
		}
		else {
			i = type.dict.Keys.ToList().IndexOf(value);
			if (nullable) {
				options.Add("Unset");
				i += 1;
			}
		}
		options.AddRange(type.dict.Values.ToList());
		
		var dropdown = Object.Instantiate(PersistentUI.Instance.DropdownPrefab, container.transform);
		UI.MoveTransform((RectTransform)dropdown.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, 1));
		dropdown.SetOptions(options);
		dropdown.Dropdown.value = i;
		dropdown.Dropdown.onValueChanged.AddListener((i) => {
			string? value = type.Backward(options[i]);
			UpdateObjects(setter, value);
		});
		
		return dropdown;
	}
#nullable disable
	
	private UITextInput AddParsed<T>(string title, System.ValueTuple<System.Func<BaseObject, T?>, System.Action<BaseObject, T?>> get_set, bool nullable = false) where T : struct {
		var container = AddField(title);
		
		(var getter, var setter) = get_set;
		
		var value = Data.GetAllOrNothing<T>(editing, getter);
		
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
	
	private UITextInput AddTextbox(string title, System.ValueTuple<System.Func<BaseObject, string>, System.Action<BaseObject, string>> get_set, bool tall = false) {
		var container = AddField(title, tall);
		
		(var getter, var setter) = get_set;
		
		var value = Data.GetAllOrNothing(editing, getter);
		
		var input = Object.Instantiate(PersistentUI.Instance.TextInputPrefab, container.transform);
		input.InputField.pointSize = tall ? 12 : 14;
		UI.MoveTransform((RectTransform)input.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, 1));
		input.InputField.text = value ?? "";
		input.InputField.onEndEdit.AddListener((s) => {
			if (s == "") {
				s = null;
			}
			UpdateObjects(setter, s);
			
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
	
#endregion
	
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
