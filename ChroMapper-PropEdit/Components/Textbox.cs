using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
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
	public bool Modified {
		get { return TextInput!.InputField.text != _value; }
	}
	
	public static Textbox Create(GameObject parent, bool tall = false) {
		return UI.AddChild(parent, "Textbox").AddComponent<Textbox>().Init(tall);
	}
	
	public Textbox Init(bool tall = false) {
		if (tab_action == null) {
			var map = CMInputCallbackInstaller.InputInstance.asset.actionMaps
				.Where(x => x.name == "Node Editor")
				.FirstOrDefault();
			CMInputCallbackInstaller.InputInstance.Disable();
			tab_action = map.AddAction("Tab Next", type: InputActionType.Button);
			tab_action.AddBinding()
				.WithPath("<Keyboard>/tab");
			CMInputCallbackInstaller.InputInstance.Enable();
		}
		
		TextInput = Object.Instantiate(PersistentUI.Instance.TextInputPrefab, transform);
		
		TextInput.InputField.pointSize = tall ? 12 : 14;
		foreach (var text in TextInput.gameObject.GetComponentsInChildren<TMPro.TMP_Text>()) {
			// Value derived from bullshit conversions of the Unity Editor:
			// 2nd Horizontal option, 5th Vertical option
			// 0x1 << 2 + 0x100 << 5
			text.alignment = (TMPro.TextAlignmentOptions) 0x1002;
		}
		UI.AttachTransform(TextInput.gameObject, new Vector2(0, 1), new Vector2(0, 0));
		
		TextInput.InputField.onSelect.AddListener((_) => {
			CMInputCallbackInstaller.DisableActionMaps(typeof(UI), new[] { typeof(CMInput.INodeEditorActions) });
			CMInputCallbackInstaller.DisableActionMaps(typeof(UI), ActionMapsDisabled);
			tab_action!.performed += onTab;
			tab_action!.Enable();
		});
		TextInput.InputField.onEndEdit.AddListener((string s) => {
			if (s != _value) {
				OnChange!(s);
			}
		});
		TextInput.InputField.onDeselect.AddListener((_) => {
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(UI), new[] { typeof(CMInput.INodeEditorActions) });
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(UI), ActionMapsDisabled);
			tab_action!.performed -= onTab;
		});
		
		return this;
	}
	
	public Textbox Set(string? value, bool mixed, Setter setter) {
		Value = value ?? "";
		OnChange = setter;
		Placeholder = (mixed) ? "Mixed" : "Empty";
		
		return this;
	}
	
	public static void AddTabListener(GameObject go, UnityAction<(Textbox, int)> action) {
		go.AddComponent<MessageReceiver>()
			.AddEvent("TabDir", (args) => action(((Textbox, int))args));
	}
	
	public override void Select() {
		TextInput?.InputField.ActivateInputField();
	}
	
	private void onTab(InputAction.CallbackContext _) {
		// This can happen sometimes, not sure why
		if (!isActiveAndEnabled) {
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(UI), new[] { typeof(CMInput.INodeEditorActions) });
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(UI), ActionMapsDisabled);
			tab_action!.performed -= onTab;
			return;
		}
		var dir = (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift))
			? -1
			: 1;
		gameObject.NotifyUpOnce("TabDir", (this, dir));
	}
	
	private string _value = "";
	
	private static InputAction? tab_action = null;
	
	// Stop textbox input from triggering actions, copied from the node editor
	
	private static readonly System.Type[] actionMapsEnabledWhenNodeEditing = {
		typeof(CMInput.ICameraActions), typeof(CMInput.IBeatmapObjectsActions), typeof(CMInput.INodeEditorActions),
		typeof(CMInput.ISavingActions), typeof(CMInput.ITimelineActions)
	};
	
	private static System.Type[] ActionMapsDisabled => typeof(CMInput).GetNestedTypes()
		.Where(x => x.IsInterface && !actionMapsEnabledWhenNodeEditing.Contains(x)).ToArray();
};

}
