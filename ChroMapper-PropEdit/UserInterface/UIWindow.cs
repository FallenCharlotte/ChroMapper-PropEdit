using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ChroMapper_PropEdit.Components;
using ChroMapper_PropEdit.Utils;

namespace ChroMapper_PropEdit.UserInterface {

// Base class for Main and Settings windows
public abstract class UIWindow : MonoBehaviour {
	public Window? window;
	public ScrollBox? scrollbox;
	public GameObject? panel;
	public Stack<GameObject> panels = new Stack<GameObject>();
	
	public GameObject? current_panel {
		get {
			return (panels.Count > 0)
				? panels.Peek()
				: null;
		}
	}
	
	public static T Create<T>(MapEditorUI mapEditorUI) where T : UIWindow {
		var obj = new GameObject();
		var uiw = obj.AddComponent<T>();
		uiw.Init(mapEditorUI);
		return uiw;
	}
	
	public abstract void Init(MapEditorUI mapEditorUI);
	
	public virtual void Init(MapEditorUI mapEditorUI, string title) {
		var parent = mapEditorUI.MainUIGroup[5].gameObject;
		
		gameObject.name = $"{title} Window";
		
		window = gameObject.AddComponent<Window>().Init(title, title, parent, new Vector2(220, 256));
		window.onShow += OnResize;
		window.onResize += OnResize;
		
		var container = UI.AddChild(window.gameObject, "Scroll Container");
		UI.AttachTransform(container, new Vector2(-10, -40), new Vector2(0, -15), new Vector2(0, 0), new Vector2(1, 1));
		{
			var image = container.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.1f, 0.1f, 0.1f, 1);
		}
		
		scrollbox = ScrollBox.Create(container);
		panel = scrollbox.content;
		panels.Push(panel!);
	}
	
	public virtual Toggle EditCheckbox(string label, IAccessor<bool> accessor, string tooltip = "") {
		var container = UI.AddField(current_panel!, label, null, tooltip);
		return UI.AddCheckbox(container, accessor.Get(), accessor.Set);
	}
	
	public virtual UIDropdown EditDropdown<T>(string label, IAccessor<T?> accessor, Enums.Map<T?> options, bool nullable = false, string tooltip = "") {
		var container = UI.AddField(current_panel!, label, null, tooltip);
		return UI.AddDropdown<T>(container, accessor.Get(), accessor.Set, options, nullable);
	}
	
	public virtual Textbox EditTextbox(string label, IAccessor<string?> accessor, bool tall = false, string tooltip = "") {
		var container = UI.AddField(current_panel!, label, null, tooltip);
		return UI.AddTextbox(container, accessor.Get(), accessor.Set, tall);
	}
	
	public virtual Textbox EditParsed<T>(string label, IAccessor<T?> accessor, string tooltip = "") where T : struct
		=> EditTextbox(label, accessor + Data.TextParser<T>(), false, tooltip);
	
	public virtual Collapsible Expando(string name, string label, bool expanded, string tooltip = "", bool background = true) {
		var c = Collapsible.Singleton(current_panel ?? panel!, name, label, expanded, tooltip, background);
		panels.Push(c.panel!);
		return c;
	}
	
	public abstract void ToggleWindow();
	
	protected virtual void OnResize() {
		var layout = panel!.GetComponent<LayoutElement>();
		layout!.minHeight = window!.GetComponent<RectTransform>().sizeDelta.y - 40 - 15;
	}
}

}
