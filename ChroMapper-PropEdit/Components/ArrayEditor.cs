using System;
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
	
	public static ArrayEditor Create(GameObject parent, string title, ValueTuple<Getter, Setter> get_set, bool raw = false, string tooltip = "") {
		return Singleton(parent, title, tooltip).Set(get_set, raw);
	}
	
	public ArrayEditor Set((Getter, Setter) get_set, bool raw = false) {
		(_getter, _setter) = get_set;
		this.raw = raw;
		Refresh();
		return this;
	}
	
	private ArrayEditor Init(string title, string tooltip = "") {
		container = gameObject.AddComponent<Collapsible>().Init(title, true, tooltip, false);
		{
			var layout = container.panel!.GetComponent<VerticalLayoutGroup>();
			layout.padding = new RectOffset(1, 1, 0, 1);
		}
		return this;
	}
	
	private int linenum = 0;
	
	public void Refresh() {
		if (gameObject.name == "Color Array") {
			//Debug.Log("Array Reset!");
			//Debug.Log(Environment.StackTrace);
		}
		
		linenum = 0;
		
		var node = _getter!();
		
		if (node == null) {
			AddLine("", true);
		}
		else {
			foreach (var item in node) {
				var line = (raw)
					? item.Value.ToString().Replace(",", ", ")
					: (string)item.Value;
				AddLine(line);
			}
			
			AddLine("");
		}
		while (linenum < inputs.Count) {
			Debug.Log("Clearing extra lines");
			GameObject.Destroy(inputs[inputs.Count - 1].gameObject);
			inputs.RemoveAt(inputs.Count - 1);
		}
		
		if (isActiveAndEnabled) {
			SendMessageUpwards("DirtyPanel");
		}
		
		// Redo the tab here
		if (tab_from >= 0) {
			Debug.Log(tab_from);
			var textbox = (tab_from < inputs.Count)
				? inputs[tab_from]
				: inputs[0];
			Debug.Log(textbox.isActiveAndEnabled);
			tab_from = -1;
			GetComponentInParent<Window>().TabDir((textbox, tab_dir));
		}
	}
	
	public void Submit(int sender) {
		if (sender == linenum - 1) {
			if (inputs[sender].Value == "") {
				return;
			}
			Plugin.main!.Refocus = gameObject.GetPath();
		}
		
		var lines = inputs.Select(it => it.Value).ToArray();
		
		var node = new JSONArray();
		
		foreach (var line in lines) {
			if (line != "") {
				node.Add("", raw ? Data.RawToJson(line) : line);
			}
		}
		
		_setter!(node);
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
			var input = UI.AddTextbox(container!.panel!, value, (_) => Submit(i), true);
			input.gameObject.name = $"Input {i}";
			
			UI.MoveTransform((RectTransform)input.transform, new Vector2(0, raw ? 22 : 20), new Vector2(0, 0));
			UI.SetMixed(input, mixed);
			
			inputs.Add(input);
		}
		else {
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
	public void TabDir((Textbox?, int) args) {
		var (textbox, dir) = args;
		tab_dir = dir;
		tab_from = inputs.IndexOf(textbox!);
	}
	
	private int tab_from = -1;
	private int tab_dir = 0;
	
	public Collapsible? container;
	public List<Textbox> inputs;
};

}
