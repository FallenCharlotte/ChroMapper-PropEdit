using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Enums;

using ChroMapper_PropEdit.Components;
using ChroMapper_PropEdit.Enums;
using ChroMapper_PropEdit.Utils;
using SimpleJSON;

namespace ChroMapper_PropEdit.UserInterface {

public class MapSettingsWindow : UIWindow {
	public GameObject? requirements_panel;
	public GameObject? settings_panel;
	public GameObject? pointdefinitions_panel;
	public SelectableList? environment_list;
	public SelectableList? materials_list;
	Textbox? new_pointdefinition_textbox;
	ArrayEditor? information_editor;
	ArrayEditor? warnings_editor;
	TooltipStrings tooltip = TooltipStrings.Instance;
	
	public List<string> custom_reqs = new List<string>();
	public Dictionary<string, UIDropdown> requirements = new Dictionary<string, UIDropdown>();
	public Dictionary<string, Toggle> forced = new Dictionary<string, Toggle>();
	public Dictionary<string, Type> default_reqchecks = new Dictionary<string, Type>();
	public HashSet<RequirementCheck>? requirementsAndSuggestions;
	
	private bool selection_lock = false;
	
	private JSONNode GetGustomData() {
		return BeatSaberSongContainer.Instance.MapDifficultyInfo.CustomData;
	}
	
	private Beatmap.Info.InfoDifficulty DifficultyInfo() {
		return BeatSaberSongContainer.Instance.MapDifficultyInfo;
	}
	
	public override void Init(MapEditorUI mapEditorUI) {
		base.Init(mapEditorUI, "Map Settings");
		scrollbox!.TargetScroll = 1;
		
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
		
		information_editor = ArrayEditor.Create(panel!, "Information", DifficultyField<List<string>>("CustomInformation"), tooltip.GetTooltip(TooltipStrings.Tooltip.Information));
		warnings_editor = ArrayEditor.Create(panel!, "Warnings", DifficultyField<List<string>>("CustomWarnings"), tooltip.GetTooltip(TooltipStrings.Tooltip.Warning));
		
		{
			Expando("Settings Override", "Map Options", true, tooltip.GetTooltip(TooltipStrings.Tooltip.MapOptions));
			{
				Expando("Player Options", "Player Options", true);
				prefix = "_settings._playerOptions.";
				EditDropdown("Left Handed", "_leftHanded", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.LeftHanded));
				EditParsed<float>("Player Height", "_playerHeight", tooltip.GetTooltip(TooltipStrings.Tooltip.PlayerHeight));
				EditDropdown("Automatic Player Height", "_automaticPlayerHeight", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.AutomaticPlayerHeight));
				EditParsed<float>("Sfx Volume", "_sfxVolume", tooltip.GetTooltip(TooltipStrings.Tooltip.SFXVolume));
				EditDropdown("Reduce Debris", "_reduceDebris", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.ReduceDebris));
				EditDropdown("No Hud", "_noTextsAndHuds", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.NoHud));
				EditDropdown("Hide Miss Text", "_noFailEffects", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.HideMissText));
				EditDropdown("Advanced Hud", "_advancedHud", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.AdvancedHud));
				EditDropdown("Auto Restart", "_autoRestart", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.AutoRestart));
				EditParsed<float>("Saber Trail Intensity", "_saberTrailIntensity", tooltip.GetTooltip(TooltipStrings.Tooltip.SaberTrailIntensity));
				EditDropdown("Note Jump Duration Type", "_noteJumpDurationTypeSettings", MapSettings.JumpDurationTypes, tooltip.GetTooltip(TooltipStrings.Tooltip.NoteJumpDurationType));
				EditParsed<float>("Fixed Note Jump Duration", "_noteJumpFixedDuration", tooltip.GetTooltip(TooltipStrings.Tooltip.FixedNoteJumpDuration));
				EditParsed<float>("Note Jump Offset", "_noteJumpStartBeatOffset", tooltip.GetTooltip(TooltipStrings.Tooltip.NoteJumpOffset));
				EditDropdown("Hide Note Spawn Effect", "_hideNoteSpawnEffect", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.HideNoteSpawnEffect));
				EditDropdown("Adaptive Sfx", "_adaptiveSfx", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.AdaptiveSFX));
				EditDropdown("Expert- Effects Filter", "_environmentEffectsFilterDefaultPreset", MapSettings.EffectsFilters, tooltip.GetTooltip(TooltipStrings.Tooltip.ExpertEffectsFilter));
				EditDropdown("Expert+ Effects Filter", "_environmentEffectsFilterExpertPlusPreset", MapSettings.EffectsFilters, tooltip.GetTooltip(TooltipStrings.Tooltip.ExpertPlusEffectsFilter));
				panels.Pop();
			}
			{
				Expando("Modifiers", "Modifiers", true);
				prefix = "_settings._modifiers.";
				EditDropdown("Energy Type", "_energyType", MapSettings.EnergyTypes, tooltip.GetTooltip(TooltipStrings.Tooltip.EnergyType));
				EditDropdown("No Fail", "_noFailOn0Energy", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.NoFail));
				EditDropdown("Instant Fail", "_instaFail", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.InstantFail));
				EditDropdown("Fail When Sabers Touch", "_failOnSaberClash", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.FailWhenSabersTouch));
				EditDropdown("Enabled Obstacle Types", "_enabledObstacleType", MapSettings.ObstacleTypes, tooltip.GetTooltip(TooltipStrings.Tooltip.EnabledOstacleTypes));
				EditDropdown("Fast Notes", "_fastNotes", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.FastNotes));
				EditDropdown("Strict Angles", "_strictAngles", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.StrictAngles));
				EditDropdown("Disappearing Arrows", "_disappearingArrows", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.DisappearingArrows));
				EditDropdown("Ghost Notes", "_ghostNotes", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.GhostNotes));
				EditDropdown("No Bombs", "_noBombs", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.NoBombs));
				EditDropdown("Song Speed", "_songSpeed", MapSettings.SongSpeeds, tooltip.GetTooltip(TooltipStrings.Tooltip.SongSpeed));
				EditDropdown("No Arrows", "_noArrows", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.NoArrows));
				EditDropdown("Pro Mode", "_proMode", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.ProMode));
				EditDropdown("Zen Mode", "_zenMode", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.ZenMode));
				EditDropdown("Small Cubes", "_smallCubes", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.SmallCubes));
				panels.Pop();
			}
			{
				Expando("Environments", "Environments", true);
				prefix = "_settings._environments.";
				EditDropdown("Override Environments", "_overrideEnvironments", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.OverrideEnvironments));
				panels.Pop();
			}
			{
				Expando("Colors", "Colors", true);
				prefix = "_settings._colors.";
				EditDropdown("Override Colors", "_overrideDefaultColors", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.OverrideColors));
				panels.Pop();
			}
			{
				Expando("Graphics", "Graphics", true);
				prefix = "_settings._graphics.";
				EditParsed<int>("Mirror Quality", "_mirrorGraphicsSettings", tooltip.GetTooltip(TooltipStrings.Tooltip.MirrorQuality));
				EditParsed<int>("Bloom Post Process", "_mainEffectGraphicsSettings", tooltip.GetTooltip(TooltipStrings.Tooltip.BloomPostProcess));
				EditParsed<int>("Smoke", "_smokeGraphicsSettings", tooltip.GetTooltip(TooltipStrings.Tooltip.Smoke));
				EditDropdown("Burn Mark Trails", "_burnMarkTrailsEnabled", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.BurnMarkTrails));
				EditDropdown("Screen Displacement", "_screenDisplacementEffectsEnabled", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.ScreenDisplacement));
				EditParsed<int>("Max Shockwave Particles", "_maxShockwaveParticles", tooltip.GetTooltip(TooltipStrings.Tooltip.MaxShockwaveParticles));
				panels.Pop();
			}
			{
				Expando("Chroma", "Chroma", true);
				prefix = "_settings._chroma.";
				EditDropdown("Disable Chroma Events", "_disableChromaEvents", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.DisableChromaEvents));
				EditDropdown("Disable Environment Enhancements", "_disableEnvironmentEnhancements", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.DisableEnvironmentEnhancements));
				EditDropdown("Disable Note Coloring", "_disableNoteColoring", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.DisableNoteColoring));
				EditDropdown("Force Zen Mode Walls", "_forceZenModeWalls", MapSettings.OptionBool, tooltip.GetTooltip(TooltipStrings.Tooltip.ForceZenModeWalls));
				panels.Pop();
			}
			panels.Pop();
		}
		
		if (Settings.ShowChroma.Get()?.AsBool ?? false) {
			pointdefinitions_panel = Collapsible.Create(panel!, "Point Definitions", "Point Definitions", false).panel;
			new_pointdefinition_textbox = UI.AddTextbox(pointdefinitions_panel!, "", (v) => {
				if (!string.IsNullOrEmpty(v)) {
					BeatSaberSongContainer.Instance.Map.PointDefinitions
						.Add(v!, new JSONArray());
					Refresh();
				}
			});
			UI.MoveTransform((RectTransform)new_pointdefinition_textbox.transform, new Vector2(0, 20), new Vector2(0, 0));
			
			Expando("Environment Enhancements", "Environment Enhancements", false, "Edit environment enhancements.");
			environment_list = SelectableList.Create(current_panel!);
			environment_list.OnSelectionChanged = (ehs) => {
				if (ehs is List<BaseEnvironmentEnhancement> list) {
					selection_lock = true;
					Plugin.Trace("Environment List Selection Changed");
					SelectionController.DeselectAll();
					foreach (var eh in list) {
						SelectionController.Select((BaseObject)(object)eh, true, true, true);
					}
					selection_lock = false;
					Selection.OnObjectsSelected();
				}
			};
			environment_list.OnCreateItem = () => {
				var ee = new BaseEnvironmentEnhancement();
				ee.ID = "Foo";
				ee.LookupMethod = EnvironmentLookupMethod.Contains;
				BeatSaberSongContainer.Instance.Map.EnvironmentEnhancements.Add(ee);
				Refresh();
			};
#if CHROMPER_13
			SelectionController.SelectionChangedEvent += UpdateSelectedEEs;
#else
			SelectionController.OnSelectionChanged += UpdateSelectedEEs;
#endif
			panels.Pop();
			
			Expando("Materials", "Materials", false, "Materials used by geometry");
			materials_list = SelectableList.Create(current_panel!);
			materials_list.OnSelectionChanged = (ees) => {
				if (ees is List<BaseMaterial> list) {
					Selection.OnMatsSelected(list);
				}
			};
			materials_list.OnCreateItem = () => {
				PersistentUI.Instance.ShowInputBox("New material's name:", HandleAddMaterial, "NewMaterial");
			};
			Selection.OnSelectionChanged += () => {
				if (Selection.SelectedType != SelectionType.Materials) {
					materials_list.SetSelection(null);
				}
			};
			panels.Pop();
		}
		
		Refresh();
		UI.RefreshTooltips(panel);
		
#if CHROMPER_13
		BeatmapActionContainer.ActionCreatedEvent += UpdateFromAction;
		BeatmapActionContainer.ActionUndoEvent += UpdateFromAction;
		BeatmapActionContainer.ActionRedoEvent += UpdateFromAction;
#else
		BeatmapActionContainer.OnActionCreated += UpdateFromAction;
		BeatmapActionContainer.OnActionUndo += UpdateFromAction;
		BeatmapActionContainer.OnActionRedo += UpdateFromAction;
#endif
	}
	
	private void UpdateFromAction(BeatmapAction? _) {
		Refresh();
	}
	
	private void HandleAddMaterial(string name) {
		if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name)) return;
		
		var mat = new BaseMaterial();
		mat.Shader = "Standard";
		mat.ShaderKeywords = new List<string>();
		BeatSaberSongContainer.Instance.Map.Materials.Add(name, mat);
		Refresh();
	}
	
	private UIDropdown EditDropdown<T>(string name, string path, Map<T?> options, string tooltip = "")
		=> EditDropdown(name, MapCustomField<T>(path), options, true, tooltip);
	
	private void EditParsed<T>(string name, string path, string tooltip = "") where T : struct
		=> EditParsed<T>(name, MapCustomField<T?>(path), tooltip);
	
	private Accessor<JSONNode?> MapCustomField(string path) {
		path = $"{prefix}{path}";
		return new Accessor<JSONNode?>(
			() => Data.GetNode(GetGustomData(), path),
			(v) => {
				if (v == null) {
					Data.RemoveNode(GetGustomData(), path);
				}
				else {
					Data.SetNode(GetGustomData(), path, v);
				}
			}
		);
	}
	
	private Accessor<T?> DifficultyField<T>(string field_name)
		=> new(
			() => (T?)typeof(Beatmap.Info.InfoDifficulty).GetProperty(field_name).GetMethod.Invoke(DifficultyInfo(), null),
			(v) => typeof(Beatmap.Info.InfoDifficulty).GetProperty(field_name).SetMethod.Invoke(DifficultyInfo(), new object?[] {v})
		);
	
	private Accessor<T?> MapCustomField<T>(string path) {
		return MapCustomField(path) + Data.JSONValue<T>();
	}
	
	private void UpdateSelectedEEs() {
		if (selection_lock) return;
		Plugin.Trace("UpdateSelectedEEs");
		var ees = SelectionController.SelectedObjects
			.Select(it => it as BaseEnvironmentEnhancement)
			.Where(it => it != null)
			.ToList();
		environment_list!.SetSelection(ees, false);
	}
	
	private string prefix = "";
	
	private bool refreshing_reqs = false;
	public void RefreshRequirements(bool? force = null) {
		refreshing_reqs = true;
		foreach (Transform child in requirements_panel!.transform) {
			GameObject.Destroy(child.gameObject);
		}
		RestoreReqChecks();
		
		requirements = new Dictionary<string, UIDropdown>();
		forced = new Dictionary<string, Toggle>();
		
		var all_rs = new Dictionary<string, RequirementCheck.RequirementType>();
		
		// Goddammit why can't these just stay in custom data
		// Suggestions first to requirements will take priority
		var rs = new Dictionary<RequirementCheck.RequirementType, List<string>>() {
			{RequirementCheck.RequirementType.Suggestion, DifficultyInfo().CustomSuggestions},
			{RequirementCheck.RequirementType.Requirement, DifficultyInfo().CustomRequirements},
		};
		// Combine all suggested and required into one list
		foreach (var rs_mods in rs) {
			foreach (var req in rs_mods.Value) {
				all_rs.Add(req, rs_mods.Key);
			}
		}
		
		var different = new List<(string, RequirementCheck.RequirementType, RequirementCheck.RequirementType, string)>();
		
		// Add all default mods
		foreach (var rc in default_reqchecks) {
			var default_status = GetReqCheck(rc.Key)!.IsRequiredOrSuggested(DifficultyInfo(), BeatSaberSongContainer.Instance.Map);
			var current_status = all_rs.ContainsKey(rc.Key)
				? all_rs[rc.Key]
				: default_status;
			Plugin.Trace($"{rc.Key}: CM: {default_status} Map: {current_status}");
			if (current_status != default_status) {
				var force_setting = (string)Settings.KeepForcedReqs.Get();
				if (force == null && force_setting == "Ask") {
					different.Add((rc.Key, default_status, current_status, rc.Value.Name));
				}
				if (force == false || force_setting == "Never") {
					ReqField(rc.Key, (int)default_status, false, rc.Value.Name);
				}
				if (force == true || force_setting == "Force") {
					ReqField(rc.Key, (int)current_status, current_status != default_status, rc.Value.Name);
				}
			}
			else {
				ReqField(rc.Key, (int)default_status, false, rc.Value.Name);
			}
		}
		
		if (different.Count > 0) {
			var dialog = PersistentUI.Instance.CreateNewDialogBox().WithNoTitle();
			dialog.AddComponent<TextComponent>().WithInitialValue("This map has requirements different from ChroMapper's defaults:");
			foreach (var diff in different) {
				dialog.AddComponent<TextComponent>().WithInitialValue($"{diff.Item1} - Chromapper: {diff.Item2}, Map: {diff.Item3}");
			}
			dialog.AddComponent<TextComponent>().WithInitialValue("Do you want to keep these changes as forced overrides?");
			dialog.AddFooterButton(() => {
				Plugin.plugin_settings?.force_reqs?.Dropdown.value = 1;
			}, "Never");
			dialog.AddFooterButton(() => {
				RefreshRequirements(false);
			}, "No");
			dialog.AddFooterButton(() => {
				RefreshRequirements(true);
			}, "Yes");
			dialog.AddFooterButton(() => {
				Plugin.plugin_settings?.force_reqs?.Dropdown.value = 2;
			}, "Always");
			
			dialog.Open();
		}
		
		// Add all custom mods
		foreach (var rc in all_rs) {
			if (!default_reqchecks.ContainsKey(rc.Key)) {
				RequirementCheck.RegisterRequirement(new CustomRequirement(rc.Key, rc.Value, DifficultyInfo()));
				ReqField(rc.Key, (int)rc.Value, true);
			}
		}
		
		{ // Textbox to add custom
			var input = UI.AddTextbox(requirements_panel!, "", (s) => {
				if (s == null || s == "") {
					return;
				}
				
				RequirementCheck.RegisterRequirement(new CustomRequirement(s!, RequirementCheck.RequirementType.Requirement, DifficultyInfo()));
				DifficultyInfo().CustomRequirements.Add(s!);
				
				RefreshRequirements();
				Refresh();
			});
			
			UI.MoveTransform((RectTransform)input.transform, new Vector2(0, 20), new Vector2(0, 0));
		}
		refreshing_reqs = false;
	}
	
	// Restore default reqchecks
	private void RestoreReqChecks() {
		Plugin.Trace("Restore default requirement checks!");
		requirementsAndSuggestions!.Clear();
		foreach (var rc in default_reqchecks) {
			RequirementCheck.RegisterRequirement((RequirementCheck)System.Activator.CreateInstance(rc.Value));
		}
	}
	
	// Pretty names for known mods
	private Dictionary<string, string> requirement_names = new Dictionary<string, string>() {
		{ "ChromaReq", "Chroma" },
		{ "LegacyChromaReq", "Legacy Chroma" },
		{ "MappingExtensionsReq", "Mapping Extensions" },
		{ "NoodleExtensionsReq", "Noodle Extensions" },
		{ "CinemaReq", "Cinema" },
		{ "SoundExtensionsReq", "Sound Extensions" },
		{ "VivifyReq", "Vivify" },
		{ "BeatToTheFutureReq", "Beat To The Future" },
	};
	
	private void ReqField(string name, int status, bool force, string typename = "") {
		string tt_name = requirement_names.ContainsKey(typename)
			? requirement_names[typename]
			: name;
		var container = UI.AddField(requirements_panel!, tt_name, null, tooltip.GetTooltip(TooltipStrings.Tooltip.ModReq, tt_name));
		requirements[name] = UI.AddDropdown(container, status, (v) => {
			SetForced(name, true);
		}, MapSettings.RequirementStatus);
		if (default_reqchecks.ContainsKey(name)) {
			var container2 = UI.AddField(requirements_panel!, "Override", null, tooltip.GetTooltip(TooltipStrings.Tooltip.OverrideModReq, tt_name));
			forced[name] = UI.AddCheckbox(container2, force, (v) => {
				SetForced(name, v);
			});
		}
		if (force) SetForced(name, true);
	}
	
	private void SetForced(string name, bool force) {
		// TODO: Update instead of removing, currently unable to change multiple maps in the same set (is this still true?)
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
	
	public void OnDestroy() {
		RestoreReqChecks();
#if CHROMPER_13
		BeatmapActionContainer.ActionCreatedEvent -= UpdateFromAction;
		BeatmapActionContainer.ActionUndoEvent -= UpdateFromAction;
		BeatmapActionContainer.ActionRedoEvent -= UpdateFromAction;
#else
		BeatmapActionContainer.OnActionCreated -= UpdateFromAction;
		BeatmapActionContainer.OnActionUndo -= UpdateFromAction;
		BeatmapActionContainer.OnActionRedo -= UpdateFromAction;
#endif
	}
	
	public void Refresh() {
		Plugin.Trace("MapSettingsWindow Refresh");
		if (!refreshing_reqs) {
			foreach (var r in requirements) {
				r.Value.Dropdown.SetValueWithoutNotify((int)(GetReqCheck(r.Key)!.IsRequiredOrSuggested(DifficultyInfo(), BeatSaberSongContainer.Instance.Map)));
			}
		}
		information_editor?.Refresh();
		warnings_editor?.Refresh();
		if (pointdefinitions_panel != null) {
			var arr_editors = pointdefinitions_panel.GetComponentsInChildren<ArrayEditor>().ToList();
			
			var pds = BeatSaberSongContainer.Instance.Map.PointDefinitions;
			
			foreach (var pd in pds) {
				var accessor = new Accessor<JSONNode?>(
					() => (pds?.ContainsKey(pd.Key) ?? false)
						? pds[pd.Key]
						: null,
					(JSONNode? n) => {
						var v = n as JSONArray;
						if (v != null && v.Count > 0) {
							pds[pd.Key] = v;
						}
						else {
							pds.Remove(pd.Key);
							Refresh();
						}
					});
				arr_editors.Remove(ArrayEditor.Create(pointdefinitions_panel, pd.Key, accessor + ArrayEditor.JsonConverter(true)));
			}
			
			// Is there even a reason for this? Can they even be deleted right now?
			foreach (var ae in arr_editors) {
				GameObject.Destroy(ae.gameObject);
			}
			
			new_pointdefinition_textbox!.transform.SetSiblingIndex(pointdefinitions_panel.transform.childCount);
			new_pointdefinition_textbox!.Value = "";
		}
		
		if (environment_list != null) {
			var ees = BeatSaberSongContainer.Instance.Map.EnvironmentEnhancements;
			environment_list.SetItems(ees, (i, ee) => {
				var name = (ee.Geometry != null)
					? $"{(string)ee.Geometry["type"]} {ee.Track}"
					: $"{ee.ID}";
				return $"{i}: {name}";
			});
			UpdateSelectedEEs();
		}
		
		if (materials_list != null) {
			var mats = BeatSaberSongContainer.Instance.Map.Materials;
			var keys = mats.Keys.ToList();
			materials_list.SetItems(mats.Values.ToList(), (i, mat) => {
				return keys[i];
			});
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
