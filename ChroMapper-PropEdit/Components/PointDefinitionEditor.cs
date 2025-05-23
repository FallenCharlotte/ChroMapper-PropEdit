using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using ChroMapper_PropEdit.Enums;
using ChroMapper_PropEdit.UserInterface;
using ChroMapper_PropEdit.Utils;
using SimpleJSON;

namespace ChroMapper_PropEdit.Components {

public class PointDefinitionEditor : MonoBehaviour {
	public static PointDefinitionEditor Singleton(GameObject parent, string title, string? tooltip = null) {
		var pde = parent.transform.Find(title)?.GetComponent<PointDefinitionEditor>();
		if (pde == null) {
			pde = UI.AddField(parent, title, new Vector2(0, 22), tooltip ?? "")
				.AddComponent<PointDefinitionEditor>()
				.Init(title);
		}
		return pde;
	}
	
	public PointDefinitionEditor Set(string? value, bool mixed, Textbox.Setter setter, UnityAction<bool>? default_setter = null) {
		full_value!.gameObject.SetActive(true);
		helper_selector!.gameObject.SetActive(true);
		
		var types = new List<string>() {
			"Point Definition Type:",
			"Array",
			"Named"
		};
		
		if (default_setter != null) {
			types.Add("Example");
		}
		
		helper_selector.Init(types, (v) => {
			switch (v) {
			case "Array":
				ShowArray();
				StartCoroutine(DelayFocus());
				break;
			case "Named":
				ShowDropdown();
				break;
			case "Example":
				default_setter!(true);
				break;
			case "None":
				break;
			default:
				break;
			}
		}, UI.LoadSprite("ChroMapper_PropEdit.Resources.Settings.png"));
		
		full_value!.Set(value, mixed, setter);
		
		ArrayEditor.Getter arr_get = () => {
			return (Data.RawToJson(value ?? "") as JSONArray) ?? new JSONArray();
		};
		ArrayEditor.Setter arr_set = (JSONArray node) => setter(node.ToString());
		array_helper!.Set((arr_get, arr_set), true);
		
		var pds = new Map<string?>();
		foreach (var pd in BeatSaberSongContainer.Instance.Map.PointDefinitions.Keys) {
			pds.Add($"\"{pd}\"", pd);
		}
		
		UI.UpdateDropdown(dropdown_helper!, value, (v) => setter(v), pds, true);
		
		if ((value?.StartsWith("[") ?? false && array_helper!.gameObject.activeInHierarchy == false)) {
			ShowArray();
		}
		else if ((value?.StartsWith("\"") ?? false)) {
			ShowDropdown();
		}
		else {
			ShowNone();
		}
		
		return this;
	}
	
	// One-time object creation and layout stuff
	private PointDefinitionEditor Init(string title) {
		//make_default = UI.AddCheckbox(gameObject, false, (v) => {} );
		full_value = UI.AddTextbox(gameObject, "", (v) => {}, true);
		((RectTransform)full_value.transform).offsetMax = new Vector2(-14, 0);
		
		array_helper = ArrayEditor.Singleton(transform.parent.gameObject, title);
		
		var dd_container = UI.AddChild(transform.parent.gameObject, title + " PD Dropdown");
		UI.AttachTransform(dd_container, new Vector2(0, 20), new Vector2(0, 0));
		dropdown_helper = UI.CreateDropdown(dd_container);
		UI.AttachTransform(dropdown_helper.gameObject, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1));
		
		helper_selector = DropdownButton.Create(gameObject);
		
		// Tab Receiver
		Textbox.AddTabListener(gameObject, TabDir);
		
		return this;
	}
	
	private void LateUpdate() {
		if (tab_dir != 0) {
			if (tab_delay > 0) {
				--tab_delay;
				return;
			}
			gameObject.NotifyUpOnce("TabDir", (full_value, tab_dir));
			tab_dir = 0;
		}
	}
	
	// Redo the tab after changes are processed
	public void TabDir((Textbox, int) args) {
		var (textbox, dir) = args;
		if (textbox.Modified) {
			if (textbox == full_value) {
				tab_dir = dir;
				tab_delay = 1;
			}
			textbox.TextInput!.InputField.DeactivateInputField();
		}
		else {
			gameObject.NotifyUpOnce("TabDir", (textbox, dir));
		}
	}
	
	void ShowArray() {
		array_helper!.gameObject.SetActive(true);
		array_helper.RefreshNow();
		dropdown_helper!.transform.parent.gameObject.SetActive(false);
		SendMessageUpwards("DirtyPanel", SendMessageOptions.DontRequireReceiver);
	}
	void ShowDropdown() {
		array_helper!.gameObject.SetActive(false);
		dropdown_helper!.transform.parent.gameObject.SetActive(true);
		SendMessageUpwards("DirtyPanel", SendMessageOptions.DontRequireReceiver);
	}
	
	void ShowNone() {
		array_helper!.gameObject.SetActive(false);
		dropdown_helper!.transform.parent.gameObject.SetActive(false);
		SendMessageUpwards("DirtyPanel", SendMessageOptions.DontRequireReceiver);
	}
	
	// Used to select the array editor after selecting it in the dropdown
	private IEnumerator DelayFocus() {
		for (var i = 0; i < 6; ++i) {
			yield return 0;
		}
		gameObject.NotifyUpOnce("TabDir", (full_value!, 1));
		
		yield break;
	}
	
	int tab_dir = 0;
	int tab_delay = 0;
	Textbox? full_value;
	DropdownButton? helper_selector;
	ArrayEditor? array_helper;
	UIDropdown? dropdown_helper;
};

}
