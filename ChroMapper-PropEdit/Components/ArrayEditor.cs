using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using ChroMapper_PropEdit.UserInterface;
using ChroMapper_PropEdit.Utils;
using SimpleJSON;

namespace ChroMapper_PropEdit.Components {

// TODO: More generic getter/setter, the raw toggle is jank and not flexible enough

public class ArrayEditor : MonoBehaviour {
	
	public delegate JSONArray? Getter();
	public delegate void Setter(JSONArray v);
	// Return raw json
	public Getter? _getter = null;
	public Setter? _setter = null;
	bool raw = false;
	bool self_refresh = false;
	
	public ArrayEditor() {
		inputs = new List<Textbox>();
	}
	
	public static ArrayEditor Singleton(GameObject parent, string title, string tooltip = "") {
		var go_name = $"{title} Array";
		var ae = parent.transform.Find(go_name)?.GetComponent<ArrayEditor>();
		if (ae == null) {
			ae = UI.AddChild(parent, go_name).AddComponent<ArrayEditor>().Init(title, tooltip);
		}
		return ae;
	}
	
	public static ArrayEditor Create(GameObject parent, string title, (Getter, Setter) get_set, bool raw = false, string tooltip = "") {
		return Singleton(parent, title, tooltip).Set(get_set, raw, true);
	}
	
	public ArrayEditor Set((Getter, Setter) get_set, bool raw = false, bool self_refresh = false) {
		(_getter, _setter) = get_set;
		this.raw = raw;
		this.self_refresh = self_refresh;
		Refresh();
		return this;
	}
	
	private ArrayEditor Init(string title, string tooltip = "") {
		container = gameObject.AddComponent<Collapsible>().Init(title, true, tooltip, false);
		{
			var layout = container.panel!.GetComponent<VerticalLayoutGroup>();
			layout.padding = new RectOffset(1, 1, 0, 1);
		}
		// Tab Receiver
		Textbox.AddTabListener(gameObject, TabDir)
			.AddEvent("Insert", (arg) => OnInsert((Textbox)arg));
		return this;
	}
	
	private int linenum = 0;
	
	public void Refresh() {
		refresh_frame = true;
	}
	private void Update() {
		if (refresh_frame) RefreshNow();
	}
	
	public void RefreshNow() {
		refresh_frame = false;
		linenum = 0;
		
		var node = _getter!();
		
		if (node == null) {
			AddLine("", true);
		}
		else {
			foreach (var item in node) {
				if (linenum == insert) {
					AddLine("");
				}
				var line = (raw)
					? item.Value.ToString().Replace(",", ", ")
					: (string)item.Value;
				AddLine(line);
			}
			
			AddLine("");
		}
		while (linenum < inputs.Count) {
			GameObject.Destroy(inputs[inputs.Count - 1].gameObject);
			inputs.RemoveAt(inputs.Count - 1);
		}
		
		if (isActiveAndEnabled) {
			SendMessageUpwards("DirtyPanel");
		}
		
		insert = -1;
		
		// Redo the tab here
		if (tab_from >= 0) {
			var textbox = (tab_from < inputs.Count)
				? inputs[tab_from]
				: inputs[0];
			tab_from = -1;
			gameObject.NotifyUpOnce("TabDir", ((Textbox)textbox, tab_dir));
		}
	}
	
	public void Submit(int sender) {
		if (sender == linenum - 1) {
			if (inputs[sender].Value == "") {
				return;
			}
			tab_from = sender;
			if (tab_dir != -1) tab_dir = 1;
		}
		
		var lines = inputs.Select(it => it.Value).ToArray();
		
		var node = new JSONArray();
		
		foreach (var line in lines) {
			if (line != "") {
				node.Add("", raw ? Data.RawToJson(line) : line);
			}
		}
		
		_setter!(node);
		
		// This exists purely to make the info and warning field simpler
		if (self_refresh) {
			Refresh();
		}
	}
	
	public bool FocusLast() {
		if (container!.Expanded) {
			inputs[inputs.Count - 1].Select();
			return true;
		}
		else {
			container!.expandToggle!.isOn = true;
			return false;
		}
	}
	
	private void AddLine(string value, bool mixed = false) {
		var i = linenum++;
		if (i >= inputs.Count) {
			var input = Textbox.Create(container!.panel!, true);
			input.gameObject.name = $"Input {i}";
			
			UI.AttachTransform(input.gameObject, new Vector2(0, raw ? 22 : 20), new Vector2(0, 0));
			
			inputs.Add(input);
		}
		{
			var input = inputs[i];
			input.Set(value, mixed, (_) => Submit(i));
		}
	}
	
	public static (Getter, Setter) NodePathGetSet(JSONNode root, string path) {
		Getter getter = () => {
			return Data.GetNode(root, path)?.AsArray ?? new JSONArray();
		};
		Setter setter = (JSONArray v) => {
			if (v.Count == 0) {
				Data.RemoveNode(root, path);
			}
			else {
				Data.SetNode(root, path, v);
			}
		};
		
		return (getter, setter);
	}
	
	// Redo the tab after changes are processed
	public void TabDir((Textbox, int) args) {
		var (textbox, dir) = args;
		tab_dir = dir;
		tab_from = inputs.IndexOf(textbox!);
		gameObject.NotifyUpOnce("TabDir", (textbox, dir));
	}
	
	private void OnInsert(Textbox textbox) {
		// Textbox empty
		if (textbox.TextInput!.InputField.text.Length == 0) {
			return;
		}
		// Something is selected
		if (textbox.TextInput!.InputField.caretPosition != textbox.TextInput!.InputField.selectionAnchorPosition) {
			return;
		}
		if (textbox.TextInput!.InputField.caretPosition == 0) {
			var sender = inputs.IndexOf(textbox);
			insert = sender;
			tab_from = sender;
			tab_dir = 0;
			refresh_frame = true;
		}
		if (textbox.TextInput!.InputField.caretPosition == textbox.TextInput!.InputField.text.Length) {
			var sender = inputs.IndexOf(textbox);
			insert = sender + 1;
			tab_from = sender;
			tab_dir = 1;
			refresh_frame = true;
		}
	}
	
	private bool refresh_frame = false;
	
	private int tab_from = -1;
	private int tab_dir = 0;
	private int insert = -1;
	
	public Collapsible? container;
	public List<Textbox> inputs;
};

}
