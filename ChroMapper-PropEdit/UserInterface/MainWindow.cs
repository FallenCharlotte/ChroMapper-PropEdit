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

using ChroMapper_PropEdit.Component;
using ChroMapper_PropEdit.Enums;

namespace ChroMapper_PropEdit.UserInterface {

public class MainWindow {
	public ExtensionButton main_button;
	public GameObject window;
	public GameObject title;
	public GameObject panel;
	//public UIDropdown test;
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
		
		//window.SetActive(false);
		
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
			layout.padding = new RectOffset(5, 5, 0, 0);
			layout.spacing = 0;
			layout.childControlHeight = false;
			layout.childForceExpandWidth = true;
			layout.childAlignment = TextAnchor.UpperCenter;
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
			
			switch (type) {
				case BeatmapObject.ObjectType.Note:
					AddDropdown("Type", Data.BaseGetSet<int>(typeof(BeatmapNote), "Type"), typeof(NoteTypes));
					break;
				case BeatmapObject.ObjectType.Event:
					var events = editing.Select(o => (MapEvent)o);
					// Light
					if (events.Where(e => !e.IsUtilityEvent).Count() == editing.Count()) {
						AddDropdown("Value", Data.BaseGetSet<int>(typeof(MapEvent), "Value"), typeof(LightValues));
					}
					// Laser Speeds
					if (events.Where(e => e.IsLaserSpeedEvent).Count() == editing.Count()) {
						AddTextbox( "Speed",         Data.BaseGetSet<int>(typeof(MapEvent), "Value"));
						AddCheckbox("Lock Rotation", Data.CustomGetSet<bool>("_lockPosition"));
						AddDropdown("Direction",     Data.CustomGetSet<int>("_direction"), typeof(LaserDirection));
						AddTextbox( "Precise Speed", Data.CustomGetSet<float>("_speed"));
					}
					break;
			}
		}
		else {
			title.GetComponent<TextMeshProUGUI>().text = "No items selected";
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
	
	private Toggle AddCheckbox(string title, System.ValueTuple<System.Func<BeatmapObject, bool?>, System.Action<BeatmapObject, bool?>> get_set) {
		var container = AddField(title);
		
		(var getter, var setter) = get_set;
		
		// Get value from selected items, false unless all true
		bool value = GetAllOrNothing<bool>(getter) ?? false;
		
		var original = GameObject.Find("Strobe Generator").GetComponentInChildren<Toggle>(true);
		var toggleObject = UnityEngine.Object.Instantiate(original, container.transform);
		var toggleComponent = toggleObject.GetComponent<Toggle>();
		var colorBlock = toggleComponent.colors;
		colorBlock.normalColor = Color.white;
		toggleComponent.colors = colorBlock;
		toggleComponent.isOn = value;
		toggleComponent.onValueChanged.AddListener((v) => {
			UpdateObjects<bool>(setter, v);
		});
		return toggleComponent;
	}
	
	private UIDropdown AddDropdown(string title, System.ValueTuple<System.Func<BeatmapObject, int?>, System.Action<BeatmapObject, int?>> get_set, System.Type type) {
		var container = AddField(title);
		
		(var getter, var setter) = get_set;
		
		// Get values from selected items
		var options = new List<string>();
		var value = GetAllOrNothing<int>(getter);
		if (!value.HasValue) {
			options.Add("--");
		}
		options.AddRange(System.Enum.GetNames(type).ToList());
		
		var dropdown = Object.Instantiate(PersistentUI.Instance.DropdownPrefab, container.transform);
		UI.MoveTransform((RectTransform)dropdown.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, 1));
		dropdown.SetOptions(options);
		dropdown.Dropdown.value = value ?? 0;
		dropdown.Dropdown.onValueChanged.AddListener((i) => {
			ushort value = (ushort)System.Enum.GetValues(type).GetValue(System.Enum.GetNames(type).ToList().IndexOf(options[i]));
			UpdateObjects<int>(setter, value);
		});
		
		return dropdown;
	}
	
	private UITextInput AddTextbox<T>(string title, System.ValueTuple<System.Func<BeatmapObject, T?>, System.Action<BeatmapObject, T?>> get_set) where T : struct {
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
			
			T? value = (res) ? (T)parameters[1] : null;
			UpdateObjects<T>(setter, value);
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
	
	// Stop textbox input from triggering actions, copied from the node editor
	
	private readonly System.Type[] actionMapsEnabledWhenNodeEditing = {
		typeof(CMInput.ICameraActions), typeof(CMInput.IBeatmapObjectsActions), typeof(CMInput.INodeEditorActions),
		typeof(CMInput.ISavingActions), typeof(CMInput.ITimelineActions)
	};
	
	private System.Type[] ActionMapsDisabled => typeof(CMInput).GetNestedTypes()
		.Where(x => x.IsInterface && !actionMapsEnabledWhenNodeEditing.Contains(x)).ToArray();
}

}
