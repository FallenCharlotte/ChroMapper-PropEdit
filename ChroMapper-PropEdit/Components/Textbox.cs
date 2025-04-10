//using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit.Components {

public class Textbox : Selectable {
	public delegate void Setter(string? s);
	
	public UITextInput? TextInput;
	public Setter? OnChange;
	public string Value {
		get { return TextInput!.InputField.text; }
		set { TextInput!.InputField.text = _value = value; }
	}
	public string Placeholder {
		set {
			var placeholder = gameObject.GetComponentInChildren<TMPro.TMP_Text>();
			placeholder.text = value;
		}
	}
	
	public static Textbox? Selected { get; private set; }
	public static int TabDir { get; private set; }
	
	public static Textbox Create(GameObject parent, bool tall = false) {
		return UI.AddChild(parent, "Textbox").AddComponent<Textbox>().Init(tall);
	}
	
	public Textbox Init(bool tall = false) {
		TextInput = Object.Instantiate(PersistentUI.Instance.TextInputPrefab, transform);
		
		TextInput.InputField.pointSize = tall ? 12 : 14;
		foreach (var text in TextInput.gameObject.GetComponentsInChildren<TMPro.TMP_Text>()) {
			// Value derived from bullshit conversions of the Unity Editor:
			// 2nd Horizontal option, 5th Vertical option
			// 0x1 << 2 + 0x100 << 5
			text.alignment = (TMPro.TextAlignmentOptions) 0x1002;
		}
		UI.AttachTransform(TextInput.gameObject, new Vector2(0, 1), new Vector2(0, 0));
		
		TextInput.InputField.onSelect.AddListener(delegate {
			Selected = this;
			TabDir = 0;
			CMInputCallbackInstaller.DisableActionMaps(typeof(UI), new[] { typeof(CMInput.INodeEditorActions) });
			CMInputCallbackInstaller.DisableActionMaps(typeof(UI), ActionMapsDisabled);
			StartCoroutine("WatchTabs");
		});
		TextInput.InputField.onEndEdit.AddListener((string s) => {
			StopCoroutine("WatchTabs");
			if (s != _value) {
				OnChange!(s);
			}
			
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(UI), new[] { typeof(CMInput.INodeEditorActions) });
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(UI), ActionMapsDisabled);
			Selected = null;
		});
		
		return this;
	}
	
	public override void Select() {
		TextInput?.InputField.ActivateInputField();
	}
	
	// TODO: This really should be an action
	private IEnumerator WatchTabs() {
		for (;;) {
			if (Input.GetKeyDown(KeyCode.Tab)) {
				TabDir = (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift))
					? -1
					: 1;
				GetComponentInParent<Window>()?.TabDir(this, TabDir);
			}
			yield return new WaitForEndOfFrame();
		}
	}
	
	private string _value = "";
	
	// Stop textbox input from triggering actions, copied from the node editor
	
	private static readonly System.Type[] actionMapsEnabledWhenNodeEditing = {
		typeof(CMInput.ICameraActions), typeof(CMInput.IBeatmapObjectsActions), typeof(CMInput.INodeEditorActions),
		typeof(CMInput.ISavingActions), typeof(CMInput.ITimelineActions)
	};
	
	private static System.Type[] ActionMapsDisabled => typeof(CMInput).GetNestedTypes()
		.Where(x => x.IsInterface && !actionMapsEnabledWhenNodeEditing.Contains(x)).ToArray();
};

}
