using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ChroMapper_PropEdit.UserInterface {

public class PluginSettingsWindow : UIWindow {
	public Toggle? chroma_enable;
	public Toggle? noodle_enable;
	public Toggle? split_value;
	public Toggle? color_hex;
	public Toggle? tooltip_enable;
	public Toggle? force_events;
	TooltipStrings tooltip = TooltipStrings.Instance;
	
	public void Init(MapEditorUI mapEditorUI) {
		base.Init(mapEditorUI, "PropEdit Settings");
		
		{
			var button = UI.AddButton(window!.title!, UI.GetSprite("CloseIcon"), ToggleWindow);
			button.Image.color = Color.red;
			UI.AttachTransform(button.gameObject, pos: new Vector2(-25, -14), size: new Vector2(30, 30), anchor_min: new Vector2(1, 1), anchor_max: new Vector2(1, 1));
		}
		
		chroma_enable = AddCheckbox("Show Chroma", (v) => {
			Settings.Set(Settings.ShowChromaKey, v);
			Plugin.main?.UpdateSelection(false);
		}, tooltip.GetTooltip(TooltipStrings.Tooltip.ShowChroma));
		
		noodle_enable = AddCheckbox("Show Noodle Extensions", (v) => {
			Settings.Set(Settings.ShowNoodleKey, v);
			Plugin.main?.UpdateSelection(false);
		}, tooltip.GetTooltip(TooltipStrings.Tooltip.ShowNoodleExtensions));
		
		split_value = AddCheckbox("Split light values", (v) => {
			Settings.Set(Settings.SplitValue, v);
			Plugin.main?.UpdateSelection(false);
		}, tooltip.GetTooltip(TooltipStrings.Tooltip.SplitLightValues));
		
		color_hex = AddCheckbox("Colors as Hex", (v) => {
			Settings.Set(Settings.ColorHex, v);
			Plugin.main?.UpdateSelection(false);
		}, tooltip.GetTooltip(TooltipStrings.Tooltip.ColorsAsHex));
		
		tooltip_enable = AddCheckbox("Show Tooltips", (v) => {
			Settings.Set(Settings.ShowTooltips, v);
			Plugin.main?.UpdateSelection(false);
			UI.RefreshTooltips(Plugin.main?.panel);
			UI.RefreshTooltips(Plugin.map_settings?.panel);
			UI.RefreshTooltips(panel);
		}, tooltip.GetTooltip(TooltipStrings.Tooltip.ShowTooltips));
		
		Refresh();
		UI.RefreshTooltips(panel);
	}
	
	public void Refresh() {
		chroma_enable!.isOn = Settings.Get(Settings.ShowChromaKey, true);
		noodle_enable!.isOn = Settings.Get(Settings.ShowNoodleKey, true);
		split_value!.isOn = Settings.Get(Settings.SplitValue, true);
		color_hex!.isOn = Settings.Get(Settings.ColorHex, true);
		tooltip_enable!.isOn = Settings.Get(Settings.ShowTooltips, true);
	}
	
	public override void ToggleWindow() {
		Refresh();
		window!.Toggle();
	}
	
	private Toggle AddCheckbox(string label, UnityAction<bool> setter, string tooltip = "") {
		var container = UI.AddField(current_panel!, label, null, tooltip);
		return UI.AddCheckbox(container, false, setter);
	}
}

}
