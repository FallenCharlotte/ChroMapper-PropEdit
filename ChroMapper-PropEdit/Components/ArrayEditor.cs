using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ChroMapper_PropEdit.UserInterface;
using ChroMapper_PropEdit.Utils;
using SimpleJSON;

namespace ChroMapper_PropEdit.Components {

// TODO: More generic getter/setter, the raw toggle is jank and not flexible enough

public class ArrayEditor : MonoBehaviour {
	
	public delegate JSONArray? Getter();
	public delegate void Setter(JSONArray v);
	// Return raw json
	Getter? _getter = null;
	Setter? _setter = null;
	bool raw = false;
	
	public ArrayEditor() {
		inputs = new List<UITextInput>();
	}
	
	public static ArrayEditor Create(GameObject parent, string title, ValueTuple<Getter, Setter> get_set, bool raw = false, string tooltip = "") {
		return UI.AddChild(parent, title).AddComponent<ArrayEditor>().Init(title, raw, get_set, tooltip);
	}
	
	public ArrayEditor Init(string title, bool raw, ValueTuple<Getter, Setter> get_set, string tooltip = "") {
		(_getter, _setter) = get_set;
		this.raw = raw;
		container = gameObject.AddComponent<Collapsible>().Init(title, true, tooltip, false);
		return this;
	}
	
	private int linenum = 0;
	
	public void Refresh() {
		inputs = new List<UITextInput>();
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
			if (inputs[sender].InputField.text == "") {
				return;
			}
			var obj = transform;
			string path = "";
			while (obj != null) {
				path = "/" + obj.gameObject.name + path;
				obj = obj.parent;
			}
			Plugin.main!.Refocus = path;
		}
		
		var lines = inputs.Select(it => it.InputField.text).ToArray();
		
		var node = new JSONArray();
		
		foreach (var line in lines) {
			if (line != "") {
				node.Add("", raw ? Data.RawToJson(line) : line);
			}
		}
		
		_setter!(node);
		
		Refresh();
	}
	
	public void FocusLast() {
		inputs[inputs.Count - 1].InputField.ActivateInputField();
	}
	
	private void AddLine(string value, bool mixed = false) {
		var i = linenum++;
		var input = UI.AddTextbox(container!.panel!, value, (_) => Submit(i), true);
		
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
	public List<UITextInput> inputs;
};

}
