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
	public Window? window;
	public GameObject? panel;
	public ScrollBox? scrollbox;
	public List<GameObject> elements = new List<GameObject>();
	public List<BaseObject>? editing;
	
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
		keybind.performed += (_) => {
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
				ToggleWindow();
			}
		};
		keybind.Disable();
		map.Enable();
	}
	
	public void ToggleWindow() {
		window!.Toggle();
		UpdateSelection(false);
	}
	
	public void Disable() {
		keybind.Disable();
	}
	
	public void Init(MapEditorUI mapEditorUI) {
		var parent = mapEditorUI.MainUIGroup[5];
		
		window = Window.Create("Main", "Prop Editor", parent.transform, new Vector2(220, 256));
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
		
		var scroll_area = UI.AddChild(container, "Scroll Area");
		UI.AttachTransform(scroll_area, new Vector2(0, -10), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1));
		panel = UI.AddChild(scroll_area, "Prop Panel");
		scrollbox = scroll_area.AddComponent<ScrollBox>().Init(UI.AttachTransform(panel, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1)));
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
		
		UpdateSelection(true);
		
		SelectionController.SelectionChangedEvent += () => UpdateSelection(true);
		BeatmapActionContainer.ActionCreatedEvent += (_) => UpdateSelection(false);
		BeatmapActionContainer.ActionUndoEvent += (_) => UpdateSelection(false);
		BeatmapActionContainer.ActionRedoEvent += (_) => UpdateSelection(false);
		
		keybind.Enable();
	}
	
#region Form Fields
	
	private GameObject AddLine(string title, Vector2? size = null) {
		var container = UI.AddField(panel!, title, size);
		elements.Add(container);
		return container;
	}
	
	// CustomData node gets removed when value = default
	private Toggle AddCheckbox(string title, System.ValueTuple<System.Func<BaseObject, bool?>, System.Action<BaseObject, bool?>> get_set, bool _default) {
		var container = AddLine(title);
		
		bool value = Data.GetAllOrNothing<bool?>(editing!, get_set.Item1) ?? _default;
		
		return UI.AddCheckbox(container, value, (v) => {
			if (v == _default) {
				UpdateObjects<bool?>(get_set.Item2, null);
			}
			else {
				UpdateObjects<bool?>(get_set.Item2, v);
			}
		});
	}
	
	private UIDropdown AddDropdown<T>(string title, System.ValueTuple<System.Func<BaseObject, T?>, System.Action<BaseObject, T?>> get_set, Map<T?> type, bool nullable = false) {
		var container = AddLine(title);
		
		T? value = Data.GetAllOrNothing<T>(editing!, get_set.Item1);
		
		return UI.AddDropdown(container, value, (v) => {
			UpdateObjects<T?>(get_set.Item2, v);
		}, type, nullable);
	}
	
	private UITextInput AddParsed<T>(string title, System.ValueTuple<System.Func<BaseObject, T?>, System.Action<BaseObject, T?>> get_set) where T : struct {
		var container = AddLine(title);
		
		(var getter, var setter) = get_set;
		
		T? value = Data.GetAllOrNothing<T?>(editing!, getter);
		
		var input = Object.Instantiate(PersistentUI.Instance.TextInputPrefab, container.transform);
		UI.MoveTransform((RectTransform)input.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, 1));
		input.InputField.text = (value != null) ? (string)Convert.ChangeType(value, typeof(string)) : "";
		input.InputField.onEndEdit.AddListener((s) => {
			// No IParsable in mono ;_;
			var methods = typeof(T).GetMethods();
			System.Reflection.MethodInfo? parse = null;
			foreach (var method in methods) {
				if (method.Name == "TryParse") {
					parse = method;
					break;
				}
			}
			if (parse == null) {
				Debug.LogError("Tried to parse a non-parsable type!");
				return;
			}
			object?[] parameters = new object?[]{s, null};
			bool res = (bool)parse.Invoke(null, parameters);
			if (!res) {
				UpdateObjects<T?>(setter, null);
			}
			else {
				UpdateObjects<T?>(setter, (T)parameters[1]!);
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
	
	private UITextInput AddTextbox(string title, System.ValueTuple<System.Func<BaseObject, string?>, System.Action<BaseObject, string?>> get_set, bool tall = false) {
		var container = AddLine(title, tall ? (new Vector2(0, 22)) : null);
		
		(var getter, var setter) = get_set;
		
		var value = Data.GetAllOrNothing<string>(editing!, getter);
		
		var input = Object.Instantiate(PersistentUI.Instance.TextInputPrefab, container.transform);
		input.InputField.pointSize = tall ? 12 : 14;
		UI.MoveTransform((RectTransform)input.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, 1));
		input.InputField.text = value ?? "";
		input.InputField.onEndEdit.AddListener((string? s) => {
			if (s == "") {
				s = null;
			}
			UpdateObjects<string?>(setter, s);
			
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
		var layout = panel!.GetComponent<LayoutElement>();
		layout!.minHeight = window!.GetComponent<RectTransform>().sizeDelta.y - 40 - 15;
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
