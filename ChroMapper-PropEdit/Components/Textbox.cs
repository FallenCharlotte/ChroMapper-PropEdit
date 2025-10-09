using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit.Components {

public class Textbox : MonoBehaviour {
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
		if (tab_next == null) {
			var map = CMInputCallbackInstaller.InputInstance.asset.actionMaps
				.Where(x => x.name == "Dialog Box")
				.FirstOrDefault();
			tab_next = map.FindAction("Navigate Down");
			tab_back = map.FindAction("Navigate Up");
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
			StartEditing();
		});
		TextInput.InputField.onEndEdit.AddListener((string s) => {
			if (s != _value) {
				OnChange!(s);
			}
			else {
				Plugin.Trace($"No change! {_value} => {s}");
			}
		});
		TextInput.InputField.onDeselect.AddListener((_) => {
			EndEditing();
		});
		
		return this;
	}
	
	public Textbox Set(string? value, bool mixed, Setter setter) {
		Value = value ?? "";
		OnChange = setter;
		Placeholder = (mixed) ? "Mixed" : "Empty";
		
		return this;
	}
	
	public static MessageReceiver AddTabListener(GameObject go, UnityAction<(Textbox, int)> action) {
		return go.AddComponent<MessageReceiver>()
			.AddEvent("TabDir", (args) => action(((Textbox, int))args));
	}
	
	public void Select() {
		TextInput?.InputField.ActivateInputField();
	}
	
	private void StartEditing() {
		last_selected = this;
		CMInputCallbackInstaller.DisableActionMaps(typeof(UI), new[] { typeof(CMInput.INodeEditorActions) });
		CMInputCallbackInstaller.DisableActionMaps(typeof(UI), ActionMapsDisabled);
		Plugin.Trace("StartEditing");
		tab_next!.performed += onTabNext;
		tab_back!.performed += onTabBack;
		Plugin.array_insert!.performed += onInsert;
	}
	
	private void EndEditing() {
		if (last_selected == this) {
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(UI), new[] { typeof(CMInput.INodeEditorActions) });
			CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(UI), ActionMapsDisabled);
		}
		Plugin.Trace($"EndEditing: {last_selected == this}");
		tab_next!.performed -= onTabNext;
		tab_back!.performed -= onTabBack;
		Plugin.array_insert!.performed -= onInsert;
	}
	
	private void OnDestroy() {
		EndEditing();
	}
	
	private void onTabNext(InputAction.CallbackContext _) {
		if (disabledCheck()) return;
		gameObject.NotifyUpOnce("TabDir", (this, 1));
	}
	private void onTabBack(InputAction.CallbackContext _) {
		if (disabledCheck()) return;
		gameObject.NotifyUpOnce("TabDir", (this, -1));
	}
	private void onInsert(InputAction.CallbackContext _) {
		if (disabledCheck()) return;
		gameObject.NotifyUpOnce("Insert", this);
	}
	
	// This can happen sometimes, not sure why
	private bool disabledCheck() {
		// AAAAAAAAAAAAAAAAAAAAAAAA
		if (this && isActiveAndEnabled) {
			return false;
		}
		Plugin.Trace($"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA {gameObject.name}");
		EndEditing();
		return true;
	}
	
	private string _value = "";
	
	private static InputAction? tab_next = null;
	private static InputAction? tab_back = null;
	
	private static Textbox? last_selected = null;
	
	// Stop textbox input from triggering actions, copied from the node editor
	
	private static readonly System.Type[] actionMapsEnabledWhenNodeEditing = {
		typeof(CMInput.ICameraActions), typeof(CMInput.IBeatmapObjectsActions), typeof(CMInput.INodeEditorActions),
		typeof(CMInput.ISavingActions), typeof(CMInput.ITimelineActions)
	};
	
	private static System.Type[] ActionMapsDisabled => typeof(CMInput).GetNestedTypes()
		.Where(x => x.IsInterface && !actionMapsEnabledWhenNodeEditing.Contains(x)).ToArray();
};

}
