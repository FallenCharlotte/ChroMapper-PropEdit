using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit.Components {

public class DropdownButton : MonoBehaviour {
	public delegate void Callback(string v);
	
	public static DropdownButton Create(GameObject parent) {
		var dropdown = Instantiate(GetBase(), parent.transform);
		return dropdown.AddComponent<DropdownButton>();
	}
	
	public DropdownButton Init(List<string> values, Callback onChange, Sprite? sprite = null) {
		var dropdown = transform.Find("Arrow").GetComponent<TMP_Dropdown>();
		
		if (sprite != null) {
			transform.Find("Arrow").GetComponent<Image>().sprite = sprite;
		}
		
		dropdown.onValueChanged.RemoveAllListeners();
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
			{
				var arrow = _base.transform.Find("Arrow")!;
				((RectTransform)arrow.transform).anchoredPosition = new Vector2(-6, 0);
				arrow.AddComponent(_base.GetComponent<TMP_Dropdown>());
			}
			Destroy(_base.GetComponent<TMP_Dropdown>());
			Destroy(_base.transform.Find("Label").gameObject);
		}
		return _base;
	}
	
	private static GameObject? _base = null;
}

}
