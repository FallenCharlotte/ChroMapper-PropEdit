using System.Collections.Generic;
using System.Reflection;
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
	public Toggle? force_lanes;
	TooltipStrings tooltip = TooltipStrings.Instance;
	
	public override void Init(MapEditorUI mapEditorUI) {
		base.Init(mapEditorUI, "PropEdit Settings");
		
		{
			var button = UI.AddButton(window!.title!, UI.GetSprite("CloseIcon"), ToggleWindow);
			button.Image.color = Color.red;
			UI.AttachTransform(button.gameObject, pos: new Vector2(-25, -14), size: new Vector2(30, 30), anchor_min: new Vector2(1, 1), anchor_max: new Vector2(1, 1));
		}
		
		chroma_enable = EditCheckbox("Show Chroma",
			SettingAccessor(Settings.ShowChromaKey, true),
			tooltip.GetTooltip(TooltipStrings.Tooltip.ShowChroma));
		
		noodle_enable = EditCheckbox("Show Noodle Extensions",
			SettingAccessor(Settings.ShowNoodleKey, true),
			tooltip.GetTooltip(TooltipStrings.Tooltip.ShowNoodleExtensions));
		
		split_value = EditCheckbox("Split light values",
			SettingAccessor(Settings.SplitValue, true),
			tooltip.GetTooltip(TooltipStrings.Tooltip.SplitLightValues));
		
		color_hex = EditCheckbox("Colors as Hex", 
			SettingAccessor(Settings.SplitValue, true),
			tooltip.GetTooltip(TooltipStrings.Tooltip.ColorsAsHex));
		
		tooltip_enable = EditCheckbox("Show Tooltips",
			SettingAccessor(Settings.ShowTooltips, true, (_) => {
				UI.RefreshTooltips(Plugin.main?.panel);
				UI.RefreshTooltips(Plugin.map_settings?.panel);
				UI.RefreshTooltips(panel);
			}),
			tooltip.GetTooltip(TooltipStrings.Tooltip.ShowTooltips));
		
		force_lanes = EditCheckbox("Force Custom Event Lanes",
			SettingAccessor(Settings.ForceLanes, false, (v) => {
				if (v) ShowDefaultLanes();
			}),
			tooltip.GetTooltip(TooltipStrings.Tooltip.ForceLanes));
		
		if (Settings.Get(Settings.ForceLanes, false)) {
			ShowDefaultLanes();
		}
		
		UI.RefreshTooltips(panel);
	}
	
	private Utils.Accessor<bool> SettingAccessor(string key, bool _default, UnityAction<bool>? extra = null) {
		return new Utils.Accessor<bool>(
			() => Settings.Get(key, _default),
			(v) => {
				Settings.Set(key, v);
				Plugin.main?.TriggerFullRefresh();
				if (extra != null) extra(v);
			}
		);
	}
	
	public override void ToggleWindow() {
		window!.Toggle();
	}
	
	private static string[] Lanes = new string[] {
		"AnimateTrack",
		"AssignPathAnimation",
		"AssignTrackParent",
		"AssignPlayerToTrack",
		"AnimateComponent"
	};
	
	private void ShowDefaultLanes() {
		var collection = BeatmapObjectContainerCollection.GetCollectionForType(Beatmap.Enums.ObjectType.CustomEvent) as CustomEventGridContainer;
		var ceg_type = typeof(CustomEventGridContainer);
		var cets = ceg_type.GetField("customEventTypes", BindingFlags.Instance | BindingFlags.NonPublic);
		var customEventTypes = cets.GetValue(collection) as List<string>;
		foreach (var lane in Lanes) {
			if (!customEventTypes!.Contains(lane)) {
				customEventTypes.Add(lane);
			}
		}
		ceg_type.GetMethod("RefreshTrack", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(collection, null);
	}
}

}
