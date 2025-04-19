using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using ChroMapper_PropEdit.UserInterface;
using ChroMapper_PropEdit.Utils;
using SimpleJSON;

namespace ChroMapper_PropEdit.Components {

public class PointDefinitionEditor : MonoBehaviour {
	Toggle? make_default;
	Textbox? full_value;
	DropdownButton? helper_selector;
	ArrayEditor? array_helper;
	UIDropdown? dropdown_helper;
	
	public static PointDefinitionEditor Singleton(
		GameObject parent,
		string title,
		string? tooltip = null
	) {
		var pde = parent.transform.Find(title)?.GetComponent<PointDefinitionEditor>();
		if (pde == null) {
			pde = UI.AddField(parent, title, new Vector2(0, 22), tooltip ?? "")
				.AddComponent<PointDefinitionEditor>()
				.Init();
		}
		return pde.Update();
	}
	
	PointDefinitionEditor Init() {
		make_default = UI.AddCheckbox(gameObject, false, (v) => {} );
		return this;
	}
	
	PointDefinitionEditor Update() {
		return this;
	}
};

}
