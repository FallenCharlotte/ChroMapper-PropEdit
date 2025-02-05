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
	// Return raw json
	public Func<string[]>? Getter;
	public Action<string[]>? Setter;
	
	public ArrayEditor() {
		inputs = new List<UITextInput>();
	}
	
	public static ArrayEditor Create(GameObject parent, JSONNode root, string path, string title, string tooltip = "") {
		return UI.AddChild(parent, title).AddComponent<ArrayEditor>().Init(root, path, title, false, tooltip);
	}
	
	public static ArrayEditor Create(GameObject parent, string title, ValueTuple<Func<string[]>, Action<string[]>> get_set) {
		return UI.AddChild(parent, title).AddComponent<ArrayEditor>().Init(title, get_set);
	}
	
	public ArrayEditor Init(JSONNode root, string path, string title, bool raw = false, string tooltip = "") {
		(Getter, Setter) = NodePathGetSet(root, path, raw);
		container = gameObject.AddComponent<Collapsible>().Init(title, true, tooltip, false);
		return this;
	}
	public ArrayEditor Init(string title, ValueTuple<Func<string[]>, Action<string[]>> get_set, string tooltip = "") {
		(Getter, Setter) = get_set;
		container = gameObject.AddComponent<Collapsible>().Init(title, true, tooltip, false);
		return this;
	}
	
	public void Refresh() {
		inputs = new List<UITextInput>();
		foreach (Transform child in container!.panel!.transform) {
			GameObject.Destroy(child.gameObject);
		}
		
		var lines = Getter!();
		
		foreach (var line in lines) {
			AddLine(line);
		}
		
		AddLine("");
		
		SendMessageUpwards("DirtyPanel");
	}
	
	public void Submit() {
		Setter!(inputs.Select(it => it.InputField.text).ToArray());
		
		Refresh();
	}
	
	private void AddLine(string value) {
		var input = UI.AddTextbox(container!.panel!, value, (_) => Submit(), true);
		
		UI.MoveTransform((RectTransform)input.transform, new Vector2(0, 20), new Vector2(0, 0));
		
		inputs.Add(input);
	}
	
	public static (System.Func<string[]>, System.Action<string[]>) NodePathGetSet(JSONNode root, string path, bool raw) {
		System.Func<string[]> getter = () => {
			var node = Data.GetNode(root, path)?.AsArray ?? new JSONArray();
			var items = new string[node.Count];
			for (var i = 0; i < node.Count; ++i) {
				items[i] = (raw)
					? node[i].ToString()
					: (string)node[i];
			}
			return items;
		};
		System.Action<string[]> setter = (string[] inputs) => {
			var node = new JSONArray();
			
			foreach (var input in inputs) {
				if (input != "") {
					node.Add("", raw ? Data.RawToJson(input) : input);
				}
			}
			
			if (node.Count == 0) {
				Data.RemoveNode(root, path);
			}
			else {
				Data.SetNode(root, path, node);
			}
		};
		
		return (getter, setter);
	}
	
	public Collapsible? container;
	public List<UITextInput> inputs;
};

}
