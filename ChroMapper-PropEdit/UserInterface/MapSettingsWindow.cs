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

public class MapSettingsWindow : UIWindow {
	public GameObject? requirements_panel;
	public GameObject? settings_panel;
	public GameObject? pointdefinitions_panel;
	ArrayEditor? information_editor;
	ArrayEditor? warnings_editor;
	TooltipStrings tooltip = TooltipStrings.Instance;
	
	public List<string> custom_reqs = new List<string>();
	public Dictionary<string, UIDropdown> requirements = new Dictionary<string, UIDropdown>();
	public Dictionary<string, Toggle> forced = new Dictionary<string, Toggle>();
	public Dictionary<string, Type> default_reqchecks = new Dictionary<string, Type>();
	public HashSet<RequirementCheck>? requirementsAndSuggestions;
	
#if CHROMPER_11
	private JSONNode GetGustomData() {
		return BeatSaberSongContainer.Instance.DifficultyData.GetOrCreateCustomData();
	}
	
	private BeatSaberSong.DifficultyBeatmap DifficultyInfo() {
		return BeatSaberSongContainer.Instance.DifficultyData;
	}
#else
	private JSONNode GetGustomData() {
		return BeatSaberSongContainer.Instance.MapDifficultyInfo.CustomData;
	}
	
	private Beatmap.Info.InfoDifficulty DifficultyInfo() {
		return BeatSaberSongContainer.Instance.MapDifficultyInfo;
	}
#endif
	
	public void Init(MapEditorUI mapEditorUI) {
		base.Init(mapEditorUI, "Map Settings");
		
		{
			var button = UI.AddButton(window!.title!, UI.GetSprite("CloseIcon"), ToggleWindow);
			button.Image.color = Color.red;
			UI.AttachTransform(button.gameObject, pos: new Vector2(-25, -14), size: new Vector2(30, 30), anchor_min: new Vector2(1, 1), anchor_max: new Vector2(1, 1));
		}
		
		{
			var collapsible = Collapsible.Create(panel!, "Requirements", "Requirements", true, tooltip.GetTooltip(TooltipStrings.Tooltip.Requirement));
			requirements_panel = collapsible.panel;
			
			RefreshRequirements();
		}
		
		information_editor = ArrayEditor.Create(panel!, "Information", ArrayEditor.NodePathGetSet(GetGustomData(), "_information"), false, tooltip.GetTooltip(TooltipStrings.Tooltip.Information));
		warnings_editor = ArrayEditor.Create(panel!, "Warnings", ArrayEditor.NodePathGetSet(GetGustomData(), "_warnings"), false, tooltip.GetTooltip(TooltipStrings.Tooltip.Warning));
		
		{
			AddExpando("Settings Override", "Map Options", true, tooltip.GetTooltip(TooltipStrings.Tooltip.MapOptions));
			{
				AddExpando("Player Options", "Player Options", true);
				prefix = "_playerOptions";
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
				panels.Pop();
			}
			{
				AddExpando("Modifiers", "Modifiers", true);
				prefix = "_modifiers";
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
				panels.Pop();
			}
			{
				AddExpando("Environments", "Environments", true);
				prefix = "_environments";
				AddDropdown("Override Environments", "_overrideEnvironments", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.OverrideEnvironments));
				panels.Pop();
			}
			{
				AddExpando("Colors", "Colors", true);
				prefix = "_colors";
				AddDropdown("Override Colors", "_overrideDefaultColors", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.OverrideColors));
				panels.Pop();
			}
			{
				AddExpando("Graphics", "Graphics", true);
				prefix = "_graphics";
				AddParsed<int>("Mirror Quality", "_mirrorGraphicsSettings", tooltip.GetTooltip(TooltipStrings.Tooltip.MirrorQuality));
				AddParsed<int>("Bloom Post Process", "_mainEffectGraphicsSettings", tooltip.GetTooltip(TooltipStrings.Tooltip.BloomPostProcess));
				AddParsed<int>("Smoke", "_smokeGraphicsSettings", tooltip.GetTooltip(TooltipStrings.Tooltip.Smoke));
				AddDropdown("Burn Mark Trails", "_burnMarkTrailsEnabled", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.BurnMarkTrails));
				AddDropdown("Screen Displacement", "_screenDisplacementEffectsEnabled", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.Information));
				AddParsed<int>("Max Shockwave Particles", "_maxShockwaveParticles", tooltip.GetTooltip(TooltipStrings.Tooltip.MaxShockwaveParticles));
				panels.Pop();
			}
			{
				AddExpando("Chroma", "Chroma", true);
				prefix = "_chroma";
				AddDropdown("Disable Chroma Events", "_disableChromaEvents", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.DisableChromaEvents));
				AddDropdown("Disable Environment Enhancements", "_disableEnvironmentEnhancements", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.DisableEnvironmentEnhancements));
				AddDropdown("Disable Note Coloring", "_disableNoteColoring", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.DisableNoteColoring));
				AddDropdown("Force Zen Mode Walls", "_forceZenModeWalls", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.ForceZenModeWalls));
				panels.Pop();
			}
			panels.Pop();
		}
		
		if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
			pointdefinitions_panel = Collapsible.Create(panel!, "Point Definitions", "Point Definitions", false).panel;
		}
		
		Refresh();
		UI.RefreshTooltips(panel);
	}
	
	private void AddDropdown<T>(string name, string path, Map<T?> options, string tooltip = "") {
		path = $"_settings.{prefix}.{path}";
		var container = UI.AddField(current_panel!, name, null, tooltip);
		var node = Data.GetNode(GetGustomData(), path);
		var value = (node == null)
			? default(T)!
			: Data.CreateConvertFunc<JSONNode, T>()(node);
		UI.AddDropdown<T>(container, value, (v) => {
			if (v == null) {
				Data.RemoveNode(GetGustomData(), path);
			}
			else {
				Data.SetNode(GetGustomData(), path, Data.CreateConvertFunc<T, SimpleJSON.JSONNode>()(v));
			}
		}, options, true);
	}
	
	
	private void AddParsed<T>(string name, string path, string tooltip = "") where T : struct {
		path = $"_settings.{prefix}.{path}";
		var container = UI.AddField(current_panel!, name, null, tooltip);
		var node = Data.GetNode(GetGustomData(), path);
		T? value = (node == null)
			? null
			: Data.CreateConvertFunc<JSONNode, T>()(node);
		UI.AddParsed<T>(container, value, (v) => {
			if (v == null) {
				Data.RemoveNode(GetGustomData(), path);
			}
			else {
				Data.SetNode(GetGustomData(), path, Data.CreateConvertFunc<T, SimpleJSON.JSONNode>()((T)v));
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
			if (GetGustomData()[req_status.Key] is JSONArray reqs) {
				foreach (var req in reqs.Children) {
					var reqcheck = GetReqCheck(req);
					if (reqcheck == null) {
						RequirementCheck.RegisterRequirement(new CustomRequirement(req, req_status.Value, DifficultyInfo()));
					}
					else {
						if (reqcheck.IsRequiredOrSuggested(DifficultyInfo(), BeatSaberSongContainer.Instance.Map) != req_status.Value) {
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
				
				RequirementCheck.RegisterRequirement(new CustomRequirement(s!, RequirementCheck.RequirementType.Requirement, DifficultyInfo()));
				
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
#if !CHROMPER_11
		{ "VivifyReq", "Vivify"},
#endif
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
#if !CHROMPER_11
		RequirementCheck.RegisterRequirement(new VivifyReq());
#endif
	}
	
	private void SetForced(string name, bool force) {
		// TODO: Update instead of removing, currently unable to change multiple maps in the same set
		requirementsAndSuggestions!.Remove(GetReqCheck(name)!);
		RequirementCheck.RegisterRequirement(force
			? (new CustomRequirement(name, (RequirementCheck.RequirementType)requirements[name].Dropdown.value, DifficultyInfo()))
			: ((RequirementCheck)Activator.CreateInstance(default_reqchecks[name])));
		if (forced.ContainsKey(name))
			forced[name].isOn = force;
		Refresh();
	}
	
	private RequirementCheck? GetReqCheck(string name) {
		return requirementsAndSuggestions.FirstOrDefault((r) => r.Name == name);
	}
	
	public MapSettingsWindow() {
		// Break into ChroMapper's house and grab the requirement check list via reflection
		var req_type = typeof(RequirementCheck);
		var ras = req_type.GetField("requirementsAndSuggestions", BindingFlags.Static | BindingFlags.NonPublic);
		requirementsAndSuggestions = (HashSet<RequirementCheck>)ras.GetValue(null);
		foreach (var rc in requirementsAndSuggestions) {
			default_reqchecks[rc.Name] = rc.GetType();
		}
	}
	
	public void Refresh() {
		foreach (var r in requirements) {
			r.Value.Dropdown.SetValueWithoutNotify((int)(GetReqCheck(r.Key)!.IsRequiredOrSuggested(DifficultyInfo(), BeatSaberSongContainer.Instance.Map)));
		}
		information_editor?.Refresh();
		warnings_editor?.Refresh();
		if (pointdefinitions_panel != null) {
			foreach (Transform child in pointdefinitions_panel.transform) {
				GameObject.Destroy(child.gameObject);
			}
			
			var pds = BeatSaberSongContainer.Instance.Map.PointDefinitions;
			
			foreach (var pd in pds) {
				ArrayEditor.Getter getter = () => pds.ContainsKey(pd.Key)
					? pds[pd.Key]
					: new JSONArray();
				ArrayEditor.Setter setter = (JSONArray v) => {
					if (v.Count == 0) {
						pds.Remove(pd.Key);
						Refresh();
					}
					else {
						pds[pd.Key] = v;
					}
				};
				ArrayEditor.Create(pointdefinitions_panel, pd.Key, (getter, setter), true).Refresh();
			}
			
			var new_input = UI.AddTextbox(pointdefinitions_panel, "", (v) => {
				if (!string.IsNullOrEmpty(v)) {
					pds.Add(v!, new JSONArray());
					Refresh();
				}
			});
			UI.MoveTransform((RectTransform)new_input.transform, new Vector2(0, 20), new Vector2(0, 0));
		}
	}
	
	protected override void OnResize() {
		var layout = panel!.GetComponent<LayoutElement>();
		layout!.minHeight = window!.GetComponent<RectTransform>().sizeDelta.y - 40 - 15;
	}
	
	public override void ToggleWindow() {
		Refresh();
		window!.Toggle();
		scrollbox!.scrollbar!.value = 1;
	}
}

}
