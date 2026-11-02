using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using SimpleJSON;

using ChroMapper_PropEdit.Utils;

namespace ChroMapper_PropEdit.UserInterface {

public class PluginSettingsWindow : UIWindow {
	public UIDropdown? force_reqs = null;
	TooltipStrings tooltip = TooltipStrings.Instance;
	
	public override void Init(MapEditorUI mapEditorUI) {
		base.Init(mapEditorUI, "PropEdit Settings");
		
		{
			var button = UI.AddButton(window!.title!, UI.GetSprite("CloseIcon"), ToggleWindow);
			button.Image.color = Color.red;
			UI.AttachTransform(button.gameObject, pos: new Vector2(-25, -14), size: new Vector2(30, 30), anchor_min: new Vector2(1, 1), anchor_max: new Vector2(1, 1));
		}
		
		EditCheckbox("Show Chroma",
			SettingAccessor<bool>(Settings.ShowChroma),
			tooltip.GetTooltip(TooltipStrings.Tooltip.ShowChroma));
		
		EditCheckbox("Show Noodle Extensions",
			SettingAccessor<bool>(Settings.ShowNoodle),
			tooltip.GetTooltip(TooltipStrings.Tooltip.ShowNoodleExtensions));
		
		EditCheckbox("Split light values",
			SettingAccessor<bool>(Settings.SplitValue),
			tooltip.GetTooltip(TooltipStrings.Tooltip.SplitLightValues));
		
		EditCheckbox("Colors as Hex", 
			SettingAccessor<bool>(Settings.SplitValue),
			tooltip.GetTooltip(TooltipStrings.Tooltip.ColorsAsHex));
		
		EditCheckbox("Show Tooltips",
			SettingAccessor<bool>(Settings.ShowTooltips, (_) => {
				UI.RefreshTooltips(Plugin.main?.panel);
				UI.RefreshTooltips(Plugin.map_settings?.panel);
				UI.RefreshTooltips(panel);
			}),
			tooltip.GetTooltip(TooltipStrings.Tooltip.ShowTooltips));
		
		EditCheckbox("Force Custom Event Lanes",
			SettingAccessor<bool>(Settings.ForceLanes, (v) => {
				if (v) ShowDefaultLanes();
			}),
			tooltip.GetTooltip(TooltipStrings.Tooltip.ForceLanes));
		
		force_reqs = EditDropdown("Keep Forced Requirements",
			SettingAccessor<string>(Settings.KeepForcedReqs, (_) => { Plugin.map_settings?.RefreshRequirements(); }),
			Enums.MapSettings.KeepCustomRequirements
		);
		
		if (Settings.ForceLanes.Get()) {
			ShowDefaultLanes();
		}
		
		UI.RefreshTooltips(panel);
	}
	
	private Utils.Accessor<T?> SettingAccessor<T>(Settings.Setting setting, UnityAction<T>? extra = null) {
		var conv = Data.JSONValue<T?>();
		return new Accessor<JSONNode?>(
			() => setting.Get(),
			(v) => {
				setting.Set(v!);
				Plugin.main?.TriggerFullRefresh();
				if (extra != null) extra(conv.Forwards(v)!);
			}
		) + conv;
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
