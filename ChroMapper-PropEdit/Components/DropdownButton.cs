using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using ChroMapper_PropEdit.UserInterface;
using ChroMapper_PropEdit.Utils;
using SimpleJSON;

namespace ChroMapper_PropEdit.Components {

public class DropdownButton : MonoBehaviour {
	public delegate void Callback(string v);
	
	public static DropdownButton Create(GameObject parent, List<string> values, Callback onChange, Sprite? sprite = null) {
		var dropdown = Instantiate(GetBase(), parent.transform);
		return dropdown.AddComponent<DropdownButton>().Init(values, onChange, sprite);
	}
	
	private DropdownButton Init(List<string> values, Callback onChange, Sprite? sprite = null) {
		var dropdown = transform.Find("Arrow").GetComponent<TMP_Dropdown>();
		
		if (sprite != null) {
			transform.Find("Arrow").GetComponent<Image>().sprite = sprite;
		}
		
		dropdown.ClearOptions();
		dropdown.AddOptions(values);
		dropdown.value = 0;
		dropdown.onValueChanged.AddListener((v) => {
			if (v > 0) {
				string value = values[v];
				onChange(value);
				dropdown.value = 0;
			}
		});
		
		return this;
	}
	
	private static GameObject GetBase() {
		if (_base == null) {
			_base = Instantiate(PersistentUI.Instance.DropdownPrefab).gameObject;
			Destroy(_base.GetComponent<Image>());
			_base.transform.Find("Arrow").AddComponent(_base.GetComponent<TMP_Dropdown>());
			Destroy(_base.GetComponent<TMP_Dropdown>());
			Destroy(_base.transform.Find("Label").gameObject);
		}
		return _base;
	}
	
	private static GameObject? _base = null;
}

}
