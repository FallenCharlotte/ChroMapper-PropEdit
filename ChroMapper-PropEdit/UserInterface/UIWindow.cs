using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ChroMapper_PropEdit.Components;

namespace ChroMapper_PropEdit.UserInterface {

// Base class for Main and Settings windows
public abstract class UIWindow {
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
	
	public virtual void Init(MapEditorUI mapEditorUI, string title) {
		var parent = mapEditorUI.MainUIGroup[5].gameObject;
		
		window = Window.Create(title, title, parent, new Vector2(220, 256));
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
	}
	
	public void AddExpando(string name, string label, bool expanded, string tooltip = "") {
		panels.Push(Collapsible.Create(current_panel ?? panel!, name, label, expanded, tooltip).panel!);
	}
	
	public abstract void ToggleWindow();
	
	protected abstract void OnResize();
}

}
