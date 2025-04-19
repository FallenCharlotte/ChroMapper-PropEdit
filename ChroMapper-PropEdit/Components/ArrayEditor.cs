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
	
	public static ArrayEditor Singleton(GameObject parent, string title, ValueTuple<Getter, Setter> get_set, bool raw = false, string tooltip = "") {
		var go_name = $"{title} Array";
		var ae = parent.transform.Find(go_name)?.GetComponent<ArrayEditor>();
		if (ae == null) {
			ae = UI.AddChild(parent, go_name).AddComponent<ArrayEditor>().Init(title, raw, tooltip);
		}
		(ae._getter, ae._setter) = get_set;
		ae.Refresh();
		return ae;
	}
	
	public static ArrayEditor Create(GameObject parent, string title, ValueTuple<Getter, Setter> get_set, bool raw = false, string tooltip = "") {
		return Singleton(parent, title, get_set, raw, tooltip);
	}
	
	public ArrayEditor Init(string title, bool raw, string tooltip = "") {
		this.raw = raw;
		container = gameObject.AddComponent<Collapsible>().Init(title, true, tooltip, false);
		{
			var layout = container.panel!.GetComponent<VerticalLayoutGroup>();
			layout.padding = new RectOffset(5, 5, 0, 5);
		}
		return this;
	}
	
	private int linenum = 0;
	
	public void Refresh() {
		inputs = new List<Textbox>();
		foreach (Transform child in container!.panel!.transform) {
			GameObject.Destroy(child.gameObject);
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
		
		if (isActiveAndEnabled) {
			SendMessageUpwards("DirtyPanel");
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
		
		Refresh();
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
		var input = UI.AddTextbox(container!.panel!, value, (_) => Submit(i), true);
		input.gameObject.name = $"Input {i}";
		
		UI.MoveTransform((RectTransform)input.transform, new Vector2(0, raw ? 22 : 20), new Vector2(0, 0));
		UI.SetMixed(input, mixed);
		
		inputs.Add(input);
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
	
	public Collapsible? container;
	public List<Textbox> inputs;
};

}
