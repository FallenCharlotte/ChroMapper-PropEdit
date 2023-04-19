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
	public InputAction keybind;
	public Window window;
	public GameObject panel;
	public Scrollbar scrollbar;
	public ScrollToTop scroll_to_top;
	public List<GameObject> elements = new List<GameObject>();
	public List<BaseObject> editing;
	
	public MainWindow() {
		main_button = ExtensionButtons.AddButton(
			UI.LoadSprite("ChroMapper_PropEdit.Resources.Icon.png"),
			"Prop Edit",
			ToggleWindow);
		
		var map = CMInputCallbackInstaller.InputInstance.asset.actionMaps
			.Where(x => x.name == "Node Editor")
			.FirstOrDefault();
		map.Disable();
		keybind = map.AddAction("Prop Editor", type: InputActionType.Button);
		keybind.AddCompositeBinding("ButtonWithOneModifier")
			.With("Modifier", "<Keyboard>/shift")
			.With("Button", "<Keyboard>/n");
		keybind.performed += (_) => ToggleWindow();
		keybind.Disable();
		map.Enable();
	}
	
	public void ToggleWindow() {
		window.Toggle();
		UpdateSelection(false);
	}
	
	public void Disable() {
		keybind.Disable();
	}
	
	public void Init(MapEditorUI mapEditorUI) {
		var parent = mapEditorUI.MainUIGroup[5];
		
		window = new Window("Main", "Prop Editor", parent, new Vector2(220, 256));
		window.OnShow += Resize;
		
		{
			var button = UI.AddButton(window.title, UI.LoadSprite("ChroMapper_PropEdit.Resources.Settings.png"), () => Plugin.settings.ToggleWindow());
			UI.AttachTransform(button.gameObject, pos: new Vector2(-25, -14), size: new Vector2(30, 30), anchor_min: new Vector2(1, 1), anchor_max: new Vector2(1, 1));
		}
		
		var container = UI.AddChild(window.game_object, "Prop Scroll Container");
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
		srect.scrollSensitivity = 42.069f;
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
		scroll_to_top = window.game_object.AddComponent<ScrollToTop>();
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
		
		UpdateSelection(true);
		
		SelectionController.SelectionChangedEvent += () => UpdateSelection(true);
		BeatmapActionContainer.ActionCreatedEvent += (_) => UpdateSelection(false);
		BeatmapActionContainer.ActionUndoEvent += (_) => UpdateSelection(false);
		BeatmapActionContainer.ActionRedoEvent += (_) => UpdateSelection(false);
		
		keybind.Enable();
	}
	
#region Form Fields
	
	private GameObject AddLine(string title, Vector2? size = null) {
		var container = UI.AddField(panel, title, size);
		elements.Add(container);
		return container;
	}
	
	// CustomData node gets removed when value = default
	private Toggle AddCheckbox(string title, System.ValueTuple<System.Func<BaseObject, bool?>, System.Action<BaseObject, bool?>> get_set, bool _default) {
		var container = AddLine(title);
		
		var value = Data.GetAllOrNothing<bool>(editing, get_set.Item1) ?? _default;
		
		return UI.AddCheckbox(container, value, (v) => {
			if (v == _default) {
				UpdateObjects<bool>(get_set.Item2, null);
			}
			else {
				UpdateObjects<bool>(get_set.Item2, v);
			}
		});
	}
	
	private UIDropdown AddDropdownI(string title, System.ValueTuple<System.Func<BaseObject, int?>, System.Action<BaseObject, int?>> get_set, Map<int> type, bool nullable = false) {
		var container = AddLine(title);
		
		(var getter, var setter) = get_set;
		
		// Get values from selected items
		var options = new List<string>();
		var value = Data.GetAllOrNothing<int>(editing, getter);
		int i = 0;
		if (!value.HasValue) {
			options.Add("--");
		}
		else {
			i = type.dict.Keys.ToList().IndexOf((int)value);
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
			int? value = type.Backward(options[i]);
			UpdateObjects<int>(setter, value);
		});
		
		return dropdown;
	}
	
	// I hate C#
#nullable enable
	private UIDropdown AddDropdownS(string title, System.ValueTuple<System.Func<BaseObject, string?>, System.Action<BaseObject, string?>> get_set, Map type, bool nullable = false) {
		var container = AddLine(title);
		
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
	
	private UITextInput AddParsed<T>(string title, System.ValueTuple<System.Func<BaseObject, T?>, System.Action<BaseObject, T?>> get_set) where T : struct {
		var container = AddLine(title);
		
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
			if (!res) {
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
		var container = AddLine(title, tall ? (new Vector2(0, 22)) : null);
		
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
	
	private void Resize() {
		var layout = panel.GetComponent<LayoutElement>();
		layout.minHeight = window.game_object.GetComponent<RectTransform>().sizeDelta.y - 40 - 15;
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
