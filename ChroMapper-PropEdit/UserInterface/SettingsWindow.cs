using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

using ChroMapper_PropEdit.Components;
using ChroMapper_PropEdit.Enums;
using ChroMapper_PropEdit.Utils;
using SimpleJSON;

namespace ChroMapper_PropEdit.UserInterface {

public class SettingsWindow {
	public Window? window;
	public GameObject? panel;
	public GameObject? requirements_panel;
	public GameObject? settings_panel;
	public GameObject? current_panel;
	public Toggle? chroma_enable;
	public Toggle? noodle_enable;
	public Toggle? split_value;
	public Toggle? color_hex;
	public Toggle? tooltip_enable;
	public ScrollBox? scrollbox;
	ArrayEditor? information_editor;
	ArrayEditor? warnings_editor;
	TooltipStrings tooltip = TooltipStrings.Instance;
	
	public List<string> custom_reqs = new List<string>();
	public Dictionary<string, UIDropdown> requirements = new Dictionary<string, UIDropdown>();
	public Dictionary<string, Toggle> forced = new Dictionary<string, Toggle>();
	public Dictionary<string, Type> default_reqchecks = new Dictionary<string, Type>();
	public HashSet<RequirementCheck>? requirementsAndSuggestions;
	
	public void Init(MapEditorUI mapEditorUI) {
		var parent = mapEditorUI.MainUIGroup[5].gameObject;
		
		window = Window.Create("Settings", "Settings", parent, size: new Vector2(200, 80));
		window.onShow += OnResize;
		window.onResize += OnResize;
		
		{
			var button = UI.AddButton(window.title!, "Close", ToggleWindow);
			UI.AttachTransform(button.gameObject, pos: new Vector2(-35, -14), size: new Vector2(50, 30), anchor_min: new Vector2(1, 1), anchor_max: new Vector2(1, 1));
		}
		
		var window_content = UI.AddChild(window.gameObject, "Settings Window Content");
		UI.AttachTransform(window_content, new Vector2(-10, -40), new Vector2(0, -15), new Vector2(0, 0), new Vector2(1, 1));
		{
			var image = window_content.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.1f, 0.1f, 0.1f, 1);
		}
		scrollbox = ScrollBox.Create(window_content);
		panel = scrollbox.content;
		
		
		UI.AddLabel(panel!, "PropEdit", "PropEdit Settings", Vector2.zero);
		{
			var container = UI.AddField(panel!, "Show Chroma", null, tooltip.GetTooltip(TooltipStrings.Tooltip.ShowChroma));
			chroma_enable = UI.AddCheckbox(container, false, (v) => {
				Settings.Set(Settings.ShowChromaKey, v);
				Plugin.main?.UpdateSelection(false);
			});
		}
		{
			var container = UI.AddField(panel!, "Show Noodle Extensions", null, tooltip.GetTooltip(TooltipStrings.Tooltip.ShowNoodleExtensions));
			noodle_enable = UI.AddCheckbox(container, false, (v) => {
				Settings.Set(Settings.ShowNoodleKey, v);
				Plugin.main?.UpdateSelection(false);
			});
		}
		{
			var container = UI.AddField(panel!, "Split light values", null, tooltip.GetTooltip(TooltipStrings.Tooltip.SplitLightValues));
			split_value = UI.AddCheckbox(container, false, (v) => {
				Settings.Set(Settings.SplitValue, v);
				Plugin.main?.UpdateSelection(false);
			});
		}
		{
			var container = UI.AddField(panel!, "Colors as Hex", null, tooltip.GetTooltip(TooltipStrings.Tooltip.ColorsAsHex));
			color_hex = UI.AddCheckbox(container, false, (v) => {
				Settings.Set(Settings.ColorHex, v);
				Plugin.main?.UpdateSelection(false);
			});
		}
		{
			var container = UI.AddField(panel!, "Show Tooltips", null, tooltip.GetTooltip(TooltipStrings.Tooltip.ShowTooltips));
			tooltip_enable = UI.AddCheckbox(container, false, (v) => {
				Settings.Set(Settings.ShowTooltips, v);
				Plugin.main?.UpdateSelection(false);
				UI.RefreshTooltips(Plugin.main?.panel);
				UI.RefreshTooltips(panel);
			});
		}
		
		UI.AddField(panel!, "");
		UI.AddLabel(panel!, "Map", "Map Settings", Vector2.zero);
		{
			var collapsible = Collapsible.Create(panel!, "Requirements", "Requirements", true, tooltip.GetTooltip(TooltipStrings.Tooltip.Requirement));
			requirements_panel = collapsible.panel;
			
			RefreshRequirements();
		}
		
		information_editor = ArrayEditor.Create(panel!, BeatSaberSongContainer.Instance.MapDifficultyInfo.CustomData, "_information", "Information", tooltip.GetTooltip(TooltipStrings.Tooltip.Information));
		warnings_editor = ArrayEditor.Create(panel!, BeatSaberSongContainer.Instance.MapDifficultyInfo.CustomData, "_warnings", "Warnings", tooltip.GetTooltip(TooltipStrings.Tooltip.Warning));
		
		{
			var collapsible = Collapsible.Create(panel!, "Settings Override", "Map Options", true, tooltip.GetTooltip(TooltipStrings.Tooltip.MapOptions));
			{
				var c2 = Collapsible.Create(collapsible.panel!, "_playerOptions", "Player Options", true);
				prefix = "_playerOptions";
				current_panel = c2.panel;
				AddDropdown("Left Handed", "_leftHanded", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.LeftHanded));
				AddParsed<float>("Player Height", "_playerHeight", tooltip.GetTooltip(TooltipStrings.Tooltip.PlayerHeight));
				AddDropdown("Automatic Player Height", "_automaticPlayerHeight", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.AutomaticPlayerHeight));
				AddParsed<float>("Sfx Volume", "_sfxVolume", tooltip.GetTooltip(TooltipStrings.Tooltip.SFXVolume));
				AddDropdown("Reduce Debris", "_reduceDebris", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.ReduceDebris));
				AddDropdown("No Hud", "_noTextsAndHuds", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.NoHud));
				AddDropdown("Hide Miss Text", "_noFailEffects", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.HideMissText));
				AddDropdown("Advanced Hud", "_advancedHud", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.AdvancedHud));
				AddDropdown("Auto Restart", "_autoRestart", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.AutoRestart));
				AddParsed<float>("Saber Trail Intensity", "_saberTrailIntensity", tooltip.GetTooltip(TooltipStrings.Tooltip.SaberTrailIntensity));
				AddDropdown("Note Jump Duration Type", "_noteJumpDurationTypeSettings", MapSettings.JumpDurationTypes, tooltip.GetTooltip(TooltipStrings.Tooltip.NoteJumpDurationType));
				AddParsed<float>("Fixed Note Jump Duration", "_noteJumpFixedDuration", tooltip.GetTooltip(TooltipStrings.Tooltip.FixedNoteJumpDuration));
				AddParsed<float>("Note Jump Offset", "_noteJumpStartBeatOffset", tooltip.GetTooltip(TooltipStrings.Tooltip.NoteJumpOffset));
				AddDropdown("Hide Note Spawn Effect", "_hideNoteSpawnEffect", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.HideNoteSpawnEffect));
				AddDropdown("Adaptive Sfx", "_adaptiveSfx", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.AdaptiveSFX));
				AddDropdown("Expert- Effects Filter", "_environmentEffectsFilterDefaultPreset", MapSettings.EffectsFilters, tooltip.GetTooltip(TooltipStrings.Tooltip.ExpertEffectsFilter));
				AddDropdown("Expert+ Effects Filter", "_environmentEffectsFilterExpertPlusPreset", MapSettings.EffectsFilters, tooltip.GetTooltip(TooltipStrings.Tooltip.ExpertPlusEffectsFilter));
			}
			{
				var c2 = Collapsible.Create(collapsible.panel!, "_modifiers", "Modifiers", true);
				prefix = "_modifiers";
				current_panel = c2.panel;
				AddDropdown("Energy Type", "_energyType", MapSettings.EnergyTypes, tooltip.GetTooltip(TooltipStrings.Tooltip.EnergyType));
				AddDropdown("No Fail", "_noFailOn0Energy", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.NoFail));
				AddDropdown("Instant Fail", "_instaFail", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.InstantFail));
				AddDropdown("Fail When Sabers Touch", "_failOnSaberClash", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.FailWhenSabersTouch));
				AddDropdown("Enabled Obstacle Types", "_enabledObstacleType", MapSettings.ObstacleTypes, tooltip.GetTooltip(TooltipStrings.Tooltip.EnabledOstacleTypes));
				AddDropdown("Fast Notes", "_fastNotes", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.FastNotes));
				AddDropdown("Strict Angles", "_strictAngles", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.StrictAngles));
				AddDropdown("Disappearing Arrows", "_disappearingArrows", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.DisappearingArrows));
				AddDropdown("Ghost Notes", "_ghostNotes", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.GhostNotes));
				AddDropdown("No Bombs", "_noBombs", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.NoBombs));
				AddDropdown("Song Speed", "_songSpeed", MapSettings.SongSpeeds, tooltip.GetTooltip(TooltipStrings.Tooltip.SongSpeed));
				AddDropdown("No Arrows", "_noArrows", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.NoArrows));
				AddDropdown("Pro Mode", "_proMode", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.ProMode));
				AddDropdown("Zen Mode", "_zenMode", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.ZenMode));
				AddDropdown("Small Cubes", "_smallCubes", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.SmallCubes));
			}
			{
				var c2 = Collapsible.Create(collapsible.panel!, "_environments", "Environments", true);
				prefix = "_environments";
				current_panel = c2.panel;
				AddDropdown("Override Environments", "_overrideEnvironments", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.OverrideEnvironments));
			}
			{
				var c2 = Collapsible.Create(collapsible.panel!, "Colors", "Colors", true);
				prefix = "_colors";
				current_panel = c2.panel;
				AddDropdown("Override Colors", "_overrideDefaultColors", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.OverrideColors));
			}
			{
				var c2 = Collapsible.Create(collapsible.panel!, "_graphics", "Graphics", true);
				prefix = "_graphics";
				current_panel = c2.panel;
				AddParsed<int>("Mirror Quality", "_mirrorGraphicsSettings", tooltip.GetTooltip(TooltipStrings.Tooltip.MirrorQuality));
				AddParsed<int>("Bloom Post Process", "_mainEffectGraphicsSettings", tooltip.GetTooltip(TooltipStrings.Tooltip.BloomPostProcess));
				AddParsed<int>("Smoke", "_smokeGraphicsSettings", tooltip.GetTooltip(TooltipStrings.Tooltip.Smoke));
				AddDropdown("Burn Mark Trails", "_burnMarkTrailsEnabled", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.BurnMarkTrails));
				AddDropdown("Screen Displacement", "_screenDisplacementEffectsEnabled", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.Information));
				AddParsed<int>("Max Shockwave Particles", "_maxShockwaveParticles", tooltip.GetTooltip(TooltipStrings.Tooltip.MaxShockwaveParticles));
			}
			{
				var c2 = Collapsible.Create(collapsible.panel!, "_chroma", "Chroma", true);
				prefix = "_chroma";
				current_panel = c2.panel;
				AddDropdown("Disable Chroma Events", "_disableChromaEvents", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.DisableChromaEvents));
				AddDropdown("Disable Environment Enhancements", "_disableEnvironmentEnhancements", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.DisableEnvironmentEnhancements));
				AddDropdown("Disable Note Coloring", "_disableNoteColoring", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.DisableNoteColoring));
				AddDropdown("Force Zen Mode Walls", "_forceZenModeWalls", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.ForceZenModeWalls));
			}
		}
		
		Refresh();
		UI.RefreshTooltips(panel);
	}
	
	private void AddDropdown<T>(string name, string path, Map<T?> options, string tooltip = "") {
		path = $"_settings.{prefix}.{path}";
		var container = UI.AddField(current_panel!, name, null, tooltip);
		var node = Data.GetNode(BeatSaberSongContainer.Instance.MapDifficultyInfo.CustomData, path);
		var value = (node == null)
			? default(T)!
			: Data.CreateConvertFunc<JSONNode, T>()(node);
		UI.AddDropdown<T>(container, value, (v) => {
			if (v == null) {
				Data.RemoveNode(BeatSaberSongContainer.Instance.MapDifficultyInfo.CustomData, path);
			}
			else {
				Data.SetNode(BeatSaberSongContainer.Instance.MapDifficultyInfo.CustomData, path, Data.CreateConvertFunc<T, SimpleJSON.JSONNode>()(v));
			}
		}, options, true);
	}
	
	
	private void AddParsed<T>(string name, string path, string tooltip = "") where T : struct {
		path = $"_settings.{prefix}.{path}";
		var container = UI.AddField(current_panel!, name, null, tooltip);
		var node = Data.GetNode(BeatSaberSongContainer.Instance.MapDifficultyInfo.CustomData, path);
		T? value = (node == null)
			? null
			: Data.CreateConvertFunc<JSONNode, T>()(node);
		UI.AddParsed<T>(container, value, (v) => {
			if (v == null) {
				Data.RemoveNode(BeatSaberSongContainer.Instance.MapDifficultyInfo.CustomData, path);
			}
			else {
				Data.SetNode(BeatSaberSongContainer.Instance.MapDifficultyInfo.CustomData, path, Data.CreateConvertFunc<T, SimpleJSON.JSONNode>()((T)v));
			}
		});
	}
	
	private string prefix = "";
	
	private readonly Dictionary<string, RequirementCheck.RequirementType> req_statuses = new Dictionary<string, RequirementCheck.RequirementType>() {
		{"_requirements", RequirementCheck.RequirementType.Requirement},
		{"_suggestions", RequirementCheck.RequirementType.Suggestion}
	};
	
	private void RefreshRequirements() {
		foreach (Transform child in requirements_panel!.transform) {
			GameObject.Destroy(child.gameObject);
		}
		requirements = new Dictionary<string, UIDropdown>();
		forced = new Dictionary<string, Toggle>();
		
		foreach (var rc in default_reqchecks) {
			AddReqField(rc.Key, false, rc.Value.Name);
		}
		
		foreach (var req_status in req_statuses) {
			if (BeatSaberSongContainer.Instance.MapDifficultyInfo.CustomData?[req_status.Key] is JSONArray reqs) {
				foreach (var req in reqs.Children) {
					var reqcheck = GetReqCheck(req);
					if (reqcheck == null) {
						RequirementCheck.RegisterRequirement(new CustomRequirement(req, req_status.Value, BeatSaberSongContainer.Instance.MapDifficultyInfo));
					}
					else {
						if (reqcheck.IsRequiredOrSuggested(BeatSaberSongContainer.Instance.MapDifficultyInfo, BeatSaberSongContainer.Instance.Map) != req_status.Value) {
							// Triggers forced
							requirements[req].Dropdown.value = (int)req_status.Value;
						}
					}
				}
			}
		}
		
		foreach (var reqcheck in requirementsAndSuggestions!) {
			if (!default_reqchecks.ContainsKey(reqcheck.Name)) {
				AddReqField(reqcheck.Name, true, reqcheck.Name);
			}
		}
		
		{
			var input = UI.AddTextbox(requirements_panel!, "", (s) => {
				if (s == null || s == "") {
					return;
				}
				
				RequirementCheck.RegisterRequirement(new CustomRequirement(s!, RequirementCheck.RequirementType.Requirement, BeatSaberSongContainer.Instance.MapDifficultyInfo));
				
				RefreshRequirements();
				Refresh();
			});
			
			UI.MoveTransform((RectTransform)input.transform, new Vector2(0, 20), new Vector2(0, 0));
		}
	}
	
	private Dictionary<string, string> requirement_names = new Dictionary<string, string>() {
		{ "ChromaReq", "Chroma" },
		{ "LegacyChromaReq", "Legacy Chroma" },
		{ "MappingExtensionsReq", "Mapping Extensions" },
		{ "NoodleExtensionsReq", "Noodle Extensions" },
		{ "CinemaReq", "Cinema" },
		{ "SoundExtensionsReq", "Sound Extensions" },
	};
	
	private void AddReqField(string name, bool force, string reqcheck = "") {
		string tt_name = name;
		if (requirement_names.ContainsKey(reqcheck)) {
			tt_name = requirement_names[reqcheck];
		}
		var container = UI.AddField(requirements_panel!, tt_name, null, tooltip.GetTooltip(TooltipStrings.Tooltip.ModReq, tt_name));
		requirements[name] = UI.AddDropdown(container, 0, (v) => {
			SetForced(name, true);
		}, MapSettings.RequirementStatus);
		if (default_reqchecks.ContainsKey(name)) {
			var container2 = UI.AddField(requirements_panel!, "Override", null, tooltip.GetTooltip(TooltipStrings.Tooltip.OverrideModReq, tt_name));
			forced[name] = UI.AddCheckbox(container2, force, (v) => {
				SetForced(name, v);
			});
		}
	}
	
	public void Disable() {
		// Restore any requirement checks that were yoted
		requirementsAndSuggestions!.Clear();
		RequirementCheck.RegisterRequirement(new ChromaReq());
		RequirementCheck.RegisterRequirement(new LegacyChromaReq());
		RequirementCheck.RegisterRequirement(new MappingExtensionsReq());
		RequirementCheck.RegisterRequirement(new NoodleExtensionsReq());
		RequirementCheck.RegisterRequirement(new CinemaReq());
		RequirementCheck.RegisterRequirement(new SoundExtensionsReq());
	}
	
	private void SetForced(string name, bool force) {
		// TODO: Update instead of removing, currently unable to change multiple maps in the same set
		requirementsAndSuggestions!.Remove(GetReqCheck(name)!);
		RequirementCheck.RegisterRequirement(force
			? (new CustomRequirement(name, (RequirementCheck.RequirementType)requirements[name].Dropdown.value, BeatSaberSongContainer.Instance.MapDifficultyInfo))
			: ((RequirementCheck)Activator.CreateInstance(default_reqchecks[name])));
		if (forced.ContainsKey(name))
			forced[name].isOn = force;
		Refresh();
	}
	
	private RequirementCheck? GetReqCheck(string name) {
		return requirementsAndSuggestions.FirstOrDefault((r) => r.Name == name);
	}
	
	public SettingsWindow() {
		// Break into ChroMapper's house and grab the requirement check list via reflection
		var req_type = typeof(RequirementCheck);
		var ras = req_type.GetField("requirementsAndSuggestions", BindingFlags.Static | BindingFlags.NonPublic);
		requirementsAndSuggestions = (HashSet<RequirementCheck>)ras.GetValue(null);
		foreach (var rc in requirementsAndSuggestions) {
			default_reqchecks[rc.Name] = rc.GetType();
		}
	}
	
	public void Refresh() {
		chroma_enable!.isOn = Settings.Get(Settings.ShowChromaKey, true);
		noodle_enable!.isOn = Settings.Get(Settings.ShowNoodleKey, true);
		split_value!.isOn = Settings.Get(Settings.SplitValue, true);
		color_hex!.isOn = Settings.Get(Settings.ColorHex, true);
		tooltip_enable!.isOn = Settings.Get(Settings.ShowTooltips, true);
		foreach (var r in requirements) {
			r.Value.Dropdown.SetValueWithoutNotify((int)(GetReqCheck(r.Key)!.IsRequiredOrSuggested(BeatSaberSongContainer.Instance.MapDifficultyInfo, BeatSaberSongContainer.Instance.Map)));
		}
		information_editor?.Refresh();
		warnings_editor?.Refresh();
	}
	
	private void OnResize() {
		var layout = panel!.GetComponent<LayoutElement>();
		layout!.minHeight = window!.GetComponent<RectTransform>().sizeDelta.y - 40 - 15;
	}
	
	public void ToggleWindow() {
		Refresh();
		window!.Toggle();
	}
}

}
