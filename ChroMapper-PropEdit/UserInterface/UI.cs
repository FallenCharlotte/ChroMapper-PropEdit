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

using System.Linq.Expressions;

using ChroMapper_PropEdit.Component;
using ChroMapper_PropEdit.Enums;

namespace ChroMapper_PropEdit.UserInterface {

public class UI {
	public ExtensionButton main_button;
	public GameObject window;
	public GameObject title;
	public GameObject panel;
	//public UIDropdown test;
	public List<GameObject> elements = new List<GameObject>();
	public HashSet<BeatmapObject> editing;
	
	public UI() {
		main_button = ExtensionButtons.AddButton(
			LoadSprite("ChroMapper_PropEdit.Resources.Icon.png"),
			"Prop Edit",
			ToggleWindow);
	}
	
	public void AddWindow(MapEditorUI mapEditorUI) {
		var parent = mapEditorUI.MainUIGroup[5];
		
		window = new GameObject("PropEdit Window");
		window.transform.parent = parent.transform;
		// Window Drag
		window.AddComponent<DragWindowController>();
		window.GetComponent<DragWindowController>().canvas = parent.GetComponent<Canvas>();
		//window.GetComponent<DragWindowController>().OnDragWindow += AnchoredPosSave;

		AttachTransform(window, new Vector2(220, 256), new Vector2(0, 0), new Vector2(0.5f, 0.5f));
		{
			var image = window.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.24f, 0.24f, 0.24f, 1);
		}
		
		//window.SetActive(false);
		
		title = AddLabel(window.transform, "Title", "Prop Editor", new Vector2(10, -20), size: new Vector2(-10, 30), font_size: 28, anchor_min: new Vector2(0, 1), anchor_max: new Vector2(1, 1), align: TextAlignmentOptions.Left);
		
		var container = AddChild(window, "Prop Scroll Container");
		AttachTransform(container, new Vector2(-10, -40), new Vector2(0, -15), new Vector2(0, 0), new Vector2(1, 1));
		{
			var image = container.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.1f, 0.1f, 0.1f, 1);
		}
		
		var scroll_area = AddChild(container, "Scroll Area", typeof(ScrollRect));
		AttachTransform(scroll_area, new Vector2(0, -10), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1));
		var mask = scroll_area.AddComponent<RectMask2D>();
		var srect = scroll_area.GetComponent<ScrollRect>();
		srect.vertical = true;
		srect.horizontal = false;
		srect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
		
		panel = AddChild(scroll_area, "Prop Panel");
		srect.content = AttachTransform(panel, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1));
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
		
		var scroller = AddChild(container, "Scroll Bar", typeof(Scrollbar));
		AttachTransform(scroller, new Vector2(10, 0), new Vector2(-5.5f, 0), new Vector2(1, 0), new Vector2(1, 1));
		var scrollbar = scroller.GetComponent<Scrollbar>();
		scrollbar.transition = Selectable.Transition.ColorTint;
		scrollbar.direction = Scrollbar.Direction.BottomToTop;
		srect.verticalScrollbar = scrollbar;
		
		var slide = AddChild(scroller, "Slide");
		AttachTransform(slide, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1));
		{
			var image = slide.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.24f, 0.24f, 0.24f, 1);
		}
		
		var handle = AddChild(slide, "Handle", typeof(Canvas));
		AttachTransform(handle, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1));
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
				elements.Add(AddLabel(panel.transform, "Unsupported", "Multi-Type Unsupported!", new Vector2(0, 0)));
				return;
			}
			
			var type = editing.First().BeatmapType;
			
			switch (type) {
				case BeatmapObject.ObjectType.Note:
					AddDropdown("Type", typeof(NoteTypes), BaseGetSet<int>(typeof(BeatmapNote), "Type"));
					break;
				case BeatmapObject.ObjectType.Event:
					var events = editing.Select(o => (MapEvent)o);
					// Light
					if (events.Where(e => !e.IsUtilityEvent).Count() == editing.Count()) {
						AddDropdown("Value", typeof(LightValues), BaseGetSet<int>(typeof(MapEvent), "Value"));
					}
					// Laser Speeds
					if (events.Where(e => e.IsLaserSpeedEvent).Count() == editing.Count()) {
						AddCheckbox("Lock Rotation", "_lockPosition");
						AddTextbox("Speed", BaseGetSet<int>(typeof(MapEvent), "Value"));
						AddDropdown("Direction", typeof(LaserDirection), CustomGetSet<int>("_direction"));
						AddTextbox("Chroma Speed", CustomGetSet<float>("_speed"));
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
		var container = new GameObject(title + " Container");
		container.transform.SetParent(panel.transform);
		AttachTransform(container, new Vector2(0, 20), new Vector2(0, 0));
		
		elements.Add(container);
		
		var label = AddChild(container, title + " Label", typeof(TextMeshProUGUI));
		AttachTransform(label, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 1));
		
		var textComponent = label.GetComponent<TextMeshProUGUI>();
		textComponent.font = PersistentUI.Instance.ButtonPrefab.Text.font;
		textComponent.alignment = TextAlignmentOptions.Left;
		textComponent.enableAutoSizing = true;
		textComponent.fontSizeMin = 8;
		textComponent.fontSizeMax = 16;
		textComponent.text = title;
		
		return container;
	}
	
	private Toggle AddCheckbox(string title, string custom_field) {
		var container = AddField(title);
		
		// Get value from selected items, false unless all true
		bool val = true;
		foreach (var o in editing) {
			if (!(o.GetOrCreateCustomData()[custom_field] ?? false)) {
				val = false;
				break;
			}
		}
		
		var original = GameObject.Find("Strobe Generator").GetComponentInChildren<Toggle>(true);
		var toggleObject = UnityEngine.Object.Instantiate(original, container.transform);
		var toggleComponent = toggleObject.GetComponent<Toggle>();
		var colorBlock = toggleComponent.colors;
		colorBlock.normalColor = Color.white;
		toggleComponent.colors = colorBlock;
		toggleComponent.isOn = val;
		toggleComponent.onValueChanged.AddListener((value) => {
			UpdateCustomBool(custom_field, value);
		});
		return toggleComponent;
	}
	private void UpdateCustomBool(string custom_field, bool val) {
		var beatmapActions = new List<BeatmapObjectModifiedAction>();
		
		foreach (var o in editing) {
			// WTF THIS SHOULDN'T BE REQUIRED WHYYYYYYYYYYYYYY
			o.GetOrCreateCustomData();
			var clone = System.Activator.CreateInstance(o.GetType(), new object[] { o.ConvertToJson() }) as BeatmapObject;
			
			clone.GetOrCreateCustomData()[custom_field] = val;
			
			beatmapActions.Add(new BeatmapObjectModifiedAction(clone, o, o, $"Edited a {o.BeatmapType} with Prop Edit.", true));
		}
		
		BeatmapActionContainer.AddAction(
			new ActionCollectionAction(beatmapActions, true, true,
				$"Edited ({SelectionController.SelectedObjects.Count()}) objects with Prop Edit."), false);
		
		// Prevent selecting "--"
		UpdateSelection();
	}
	
	private UIDropdown AddDropdown(string title, System.Type type, System.ValueTuple<System.Func<BeatmapObject, int>, System.Action<BeatmapObject, int>> get_set) {
		var container = AddField(title);
		
		(var getter, var setter) = get_set;
		
		// Get values from selected items
		var options = new List<string>();
		int last = getter(editing.First());
		foreach (var o in editing) {
			
			int val = getter(o);
			
			if (last != val) {
				options.Add("--");
				last = 0;
				break;
			}
		}
		options.AddRange(System.Enum.GetNames(type).ToList());
		
		var dropdown = Object.Instantiate(PersistentUI.Instance.DropdownPrefab, container.transform);
		MoveTransform((RectTransform)dropdown.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, 1));
		dropdown.SetOptions(options);
		dropdown.Dropdown.value = last;
		dropdown.Dropdown.onValueChanged.AddListener((val) => {
			UpdateDropdown(setter, type, options[val]);
		});
		
		return dropdown;
	}
	private void UpdateDropdown(System.Action<BeatmapObject, int> setter, System.Type type, string strval) {
		ushort val = (ushort)System.Enum.GetValues(type).GetValue(System.Enum.GetNames(type).ToList().IndexOf(strval));
		
		var beatmapActions = new List<BeatmapObjectModifiedAction>();
		foreach (var o in editing) {
			var clone = System.Activator.CreateInstance(o.GetType(), new object[] { o.ConvertToJson() }) as BeatmapObject;
			
			setter(clone, val);
			
			beatmapActions.Add(new BeatmapObjectModifiedAction(clone, o, o, $"Edited a {o.BeatmapType} with Prop Edit.", true));
		}
		
		BeatmapActionContainer.AddAction(
			new ActionCollectionAction(beatmapActions, true, false, $"Edited ({SelectionController.SelectedObjects.Count()}) objects with Prop Edit."),
			true);
		
		// Prevent selecting "--"
		UpdateSelection();
	}
	
	private UITextInput AddTextbox<T>(string title, System.ValueTuple<System.Func<BeatmapObject, T>, System.Action<BeatmapObject, T>> get_set) {
		var container = AddField(title);
		
		(var getter, var setter) = get_set;
		
		// Get values from selected items
		var it = editing.GetEnumerator();
		it.MoveNext();
		T val = getter(it.Current);
		bool same = true;
		while (it.MoveNext()) {
			if (!EqualityComparer<T>.Default.Equals(getter(it.Current), val)) {
				same = false;
				break;
			}
		}
		
		var input = Object.Instantiate(PersistentUI.Instance.TextInputPrefab, container.transform);
		MoveTransform((RectTransform)input.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, 1));
		input.InputField.text = same ? (string)Convert.ChangeType(val, typeof(string)) : "";
		input.InputField.onEndEdit.AddListener(delegate {
			//Plugin.rhythmMarkerController.InputEnable();
		});
		input.InputField.onSelect.AddListener(delegate {
			//Plugin.rhythmMarkerController.InputDisable();
		});
		input.InputField.onValueChanged.AddListener((v) => {
			UpdateTextbox(setter, v);
		});
		
		return input;
	}
	private void UpdateTextbox<T>(System.Action<BeatmapObject, T> setter, string strval) {
		var methods = typeof(T).GetMethods();
		System.Reflection.MethodInfo parse = null;
		foreach (var method in methods) {
			if (method.Name == "TryParse") {
				parse = method;
				break;
			}
		}
		T val;
		object[] parameters = new object[]{strval, null};
		bool res = (bool)parse.Invoke(null, parameters);
		
		if (!res) {
			UpdateSelection();
			return;
		}
		
		val = (T)parameters[1];
		
		var beatmapActions = new List<BeatmapObjectModifiedAction>();
		foreach (var o in editing) {
			var clone = System.Activator.CreateInstance(o.GetType(), new object[] { o.ConvertToJson() }) as BeatmapObject;
			
			setter(clone, val);
			
			beatmapActions.Add(new BeatmapObjectModifiedAction(clone, o, o, $"Edited a {o.BeatmapType} with Prop Edit.", true));
		}
		
		BeatmapActionContainer.AddAction(
			new ActionCollectionAction(beatmapActions, true, false, $"Edited ({SelectionController.SelectedObjects.Count()}) objects with Prop Edit."),
			true);
		
		// Prevent selecting "--"
		UpdateSelection();
	}
	
	// Form Data Helpers
	
	private (System.Func<BeatmapObject, T>, System.Action<BeatmapObject, T>) BaseGetSet<T>(System.Type type, string field_name) {
		var field = type.GetField(field_name);
		System.Func<BeatmapObject, T> getter = (o) => (T)field.GetValue(o);
		System.Action<BeatmapObject, T> setter = (o, v) => field.SetValue(o, v);
		return (getter, setter);
	}
	
	private (System.Func<BeatmapObject, T>, System.Action<BeatmapObject, T>) CustomGetSet<T>(string field_name) {
		System.Func<BeatmapObject, T> getter = (o) => CreateConvertFunc<SimpleJSON.JSONNode, T>()(o.GetOrCreateCustomData()[field_name]);
		System.Action<BeatmapObject, T> setter = (o, v) => o.GetOrCreateCustomData()[field_name] = CreateConvertFunc<T, SimpleJSON.JSONNode>()(v);
		return (getter, setter);
	}
	
	// https://stackoverflow.com/a/32037899
	static System.Func<TInput, TOutput> CreateConvertFunc<TInput, TOutput>()
	{
		var source = Expression.Parameter(typeof(TInput), "source");
		// the next will throw if no conversion exists
		var convert = Expression.Convert(source, typeof(TOutput));
		var method = convert.Method;
		return Expression.Lambda<System.Func<TInput, TOutput>>(convert, source).Compile();
	}
	
	// General UI Helpers
	
	private GameObject AddChild(GameObject parent, string name, params System.Type[] components) {
		var obj = new GameObject(name, components);
		obj.transform.SetParent(parent.transform);
		return obj;
	}
	
	private GameObject AddLabel(Transform parent, string title, string text, Vector2 pos, Vector2? anchor_min = null, Vector2? anchor_max = null, int font_size = 14, Vector2? size = null, TextAlignmentOptions align = TextAlignmentOptions.Center) {
		var entryLabel = new GameObject(title + " Label", typeof(TextMeshProUGUI));
		var rectTransform = ((RectTransform)entryLabel.transform);
		rectTransform.SetParent(parent);
		
		MoveTransform(rectTransform, size ?? new Vector2(110, 24), pos, anchor_min ?? new Vector2(0.5f, 1), anchor_max ?? new Vector2(0.5f, 1));
		var textComponent = entryLabel.GetComponent<TextMeshProUGUI>();
		
		textComponent.name = title;
		textComponent.font = PersistentUI.Instance.ButtonPrefab.Text.font;
		textComponent.alignment = align;
		textComponent.fontSize = font_size;
		textComponent.text = text;
		
		return entryLabel;
	}
	
	private RectTransform AttachTransform(GameObject obj,    Vector2 size, Vector2 pos, Vector2? anchor_min = null, Vector2? anchor_max = null, Vector2? pivot = null) {
		var rectTransform = obj.GetComponent<RectTransform>();
		if (rectTransform == null) {
			rectTransform = obj.AddComponent<RectTransform>();
		}
		return MoveTransform(rectTransform, size, pos, anchor_min, anchor_max, pivot);
	}
	
	private RectTransform MoveTransform(RectTransform trans, Vector2 size, Vector2 pos, Vector2? anchor_min = null, Vector2? anchor_max = null, Vector2? pivot = null) {
		trans.localScale = new Vector3(1, 1, 1);
		trans.sizeDelta = size;
		trans.pivot = pivot ?? new Vector2(0.5f, 0.5f);
		trans.anchorMin = anchor_min ?? new Vector2(0, 0);
		trans.anchorMax = anchor_max ?? anchor_min ?? new Vector2(1, 1);
		trans.anchoredPosition = new Vector3(pos.x, pos.y, 0);
		
		return trans;
	}
	
	private Sprite LoadSprite(string asset) {
		Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(asset);
		byte[] data = new byte[stream.Length];
		stream.Read(data, 0, (int)stream.Length);
		
		Texture2D texture2D = new Texture2D(256, 256);
		texture2D.LoadImage(data);
		
		return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0, 0), 100.0f);
	}
}

}
