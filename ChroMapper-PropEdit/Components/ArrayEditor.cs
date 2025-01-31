using System.Collections.Generic;
using UnityEngine;

using ChroMapper_PropEdit.UserInterface;
using ChroMapper_PropEdit.Utils;
using SimpleJSON;

namespace ChroMapper_PropEdit.Components {

// TODO: More generic getter/setter, the raw toggle is jank and not flexible enough

public class ArrayEditor : MonoBehaviour {
	public JSONNode? root;
	public string? path;
	public bool raw = false;
	
	//public System.Func<string?>? Getter;
	//public System.Action<string?>? Setter;
	
	public ArrayEditor() {
		inputs = new List<UITextInput>();
	}
	
	public static ArrayEditor Create(GameObject parent, JSONNode root, string path, string title, string tooltip = "") {
		return UI.AddChild(parent, title).AddComponent<ArrayEditor>().Init(root, path, title, false, tooltip);
	}
	
	public static ArrayEditor Create(GameObject parent, JSONNode root, string path, string title, bool raw) {
		return UI.AddChild(parent, title).AddComponent<ArrayEditor>().Init(root, path, title, raw);
	}
	
	public ArrayEditor Init(JSONNode root, string path, string title, bool raw = false, string tooltip = "") {
		this.root = root;
		this.path = path;
		this.raw = raw;
		container = gameObject.AddComponent<Collapsible>().Init(title, true, tooltip);
		return this;
	}
	
	public void Refresh() {
		inputs = new List<UITextInput>();
		foreach (Transform child in container!.panel!.transform) {
			GameObject.Destroy(child.gameObject);
		}
		
		var node = Data.GetNode(root!, path!)?.AsArray ?? new JSONArray();
		
		foreach (var value in node) {
			AddLine(raw ? value.Value.ToString() : value.Value as JSONString);
		}
		
		AddLine("");
		
		SendMessageUpwards("DirtyPanel");
	}
	
	public void Submit() {
		var node = new JSONArray();
		
		foreach (var input in inputs) {
			if (input.InputField.text != "") {
				node.Add("", raw ? Data.RawToJson(input.InputField.text) : input.InputField.text);
			}
		}
		
		if (node.Count == 0) {
			Data.RemoveNode(root!, path!);
		}
		else {
			Data.SetNode(root!, path!, node);
		}
		
		Refresh();
	}
	
	private void AddLine(string value) {
		var input = UI.AddTextbox(container!.panel!, value, (_) => Submit() );
		
		UI.MoveTransform((RectTransform)input.transform, new Vector2(0, 20), new Vector2(0, 0));
		
		inputs.Add(input);
	}
	
	public Collapsible? container;
	public List<UITextInput> inputs;
};

}
