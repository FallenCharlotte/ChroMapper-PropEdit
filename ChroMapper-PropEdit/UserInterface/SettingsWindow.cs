using UnityEngine;
using UnityEngine.UI;

using ChroMapper_PropEdit.Components;

namespace ChroMapper_PropEdit.UserInterface {

public class SettingsController {
	public Window? window;
	public GameObject? panel;
	public Toggle? chroma_toggle;
	public Toggle? noodle_toggle;
	
	public void Init(MapEditorUI mapEditorUI) {
		var parent = mapEditorUI.MainUIGroup[5];
		
		window = Window.Create("Settings", "Settings", parent.transform, size: new Vector2(200, 80));
		panel = UI.AddChild(window.gameObject, "Settings Panel");
		UI.AttachTransform(panel, new Vector2(-10, -40), new Vector2(0, -15), new Vector2(0, 0), new Vector2(1, 1));
		{
			var layout = panel.AddComponent<VerticalLayoutGroup>();
			layout.padding = new RectOffset(10, 15, 0, 0);
			layout.spacing = 0;
			layout.childControlHeight = false;
			layout.childForceExpandHeight = false;
			layout.childForceExpandWidth = true;
			layout.childAlignment = TextAnchor.UpperCenter;
		}
		
		{
			var container = UI.AddField(panel, "Chroma");
			chroma_toggle = UI.AddCheckbox(container, false, (v) => {
				Settings.Set("Chroma", v);
				Plugin.main?.UpdateSelection(false);
			});
		}
		{
			var container = UI.AddField(panel, "Noodle Extensions");
			noodle_toggle = UI.AddCheckbox(container, false, (v) => {
				Settings.Set("Noodle", v);
				Plugin.main?.UpdateSelection(false);
			});
		}
		
		Refresh();
	}
	
	public void Refresh() {
		chroma_toggle!.isOn = Settings.Get("Chroma", true);
		noodle_toggle!.isOn = Settings.Get("Noodle", true);
	}
	
	public void ToggleWindow() {
		Refresh();
		window!.Toggle();
	}
}

}
