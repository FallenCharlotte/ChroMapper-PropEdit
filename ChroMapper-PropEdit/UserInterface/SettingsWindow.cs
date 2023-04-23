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

public class SettingsController {
	public Window? window;
	public GameObject? panel;
	public GameObject? map_panel;
	public Toggle? chroma_enable;
	public Toggle? noodle_enable;
	public ScrollBox? scrollbox;
	
	public Dictionary<string, UIDropdown> requirements = new Dictionary<string, UIDropdown>();
	public Dictionary<string, Toggle> forced = new Dictionary<string, Toggle>();
	public Dictionary<string, Type> default_reqchecks = new Dictionary<string, Type>();
	public HashSet<RequirementCheck>? requirementsAndSuggestions;
	
	public void Init(MapEditorUI mapEditorUI) {
		var parent = mapEditorUI.MainUIGroup[5];
		
		window = Window.Create("Settings", "Settings", parent.transform, size: new Vector2(200, 80));
		window.onShow += OnResize;
		window.onResize += OnResize;
		
		var window_content = UI.AddChild(window.gameObject, "Settings Window Content");
		UI.AttachTransform(window_content, new Vector2(-10, -40), new Vector2(0, -15), new Vector2(0, 0), new Vector2(1, 1));
		{
			var image = window_content.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.1f, 0.1f, 0.1f, 1);
		}
		var scroll_area = UI.AddChild(window_content, "Scroll Area");
		UI.AttachTransform(scroll_area, new Vector2(0, -10), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1));
		scrollbox = scroll_area.AddComponent<ScrollBox>().Init(scroll_area.transform);
		panel = scrollbox.content;
		UI.AttachTransform(panel!, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1));
		
		
		UI.AddLabel(panel!.transform, "PropEdit", "PropEdit Settings", Vector2.zero);
		{
			var container = UI.AddField(panel, "Show Chroma");
			chroma_enable = UI.AddCheckbox(container, false, (v) => {
				Settings.Set(Settings.ShowChromaKey, v);
				Plugin.main?.UpdateSelection(false);
			});
		}
		{
			var container = UI.AddField(panel, "Show Noodle Extensions");
			noodle_enable = UI.AddCheckbox(container, false, (v) => {
				Settings.Set(Settings.ShowNoodleKey, v);
				Plugin.main?.UpdateSelection(false);
			});
		}
		
		UI.AddField(panel, "");
		UI.AddLabel(panel!.transform, "Map", "Map Settings", Vector2.zero);
		var collapsible = UI.AddChild(panel, "Requirements").AddComponent<Collapsible>().Init("Requirements", true);
		map_panel = collapsible.panel;
		
		foreach (var rc in default_reqchecks) {
			AddReqField(rc.Key, false);
		}
		
		foreach (var req_status in (new Dictionary<string, RequirementCheck.RequirementType>() {{"_requirements", RequirementCheck.RequirementType.Requirement}, {"_suggestions", RequirementCheck.RequirementType.Suggestion}})) {
			if (BeatSaberSongContainer.Instance.DifficultyData.CustomData?[req_status.Key] is JSONArray reqs) {
				foreach (var req in reqs.Children) {
					var reqcheck = GetReqCheck(req);
					if (reqcheck == null) {
						RequirementCheck.RegisterRequirement(new CustomRequirement(req, req_status.Value));
						AddReqField(req, true);
					}
					else {
						if (reqcheck.IsRequiredOrSuggested(BeatSaberSongContainer.Instance.DifficultyData, BeatSaberSongContainer.Instance.Map) != req_status.Value) {
							// Triggers forced
							requirements[req].Dropdown.value = (int)req_status.Value;
						}
					}
				}
			}
		}
		
		Refresh();
	}
	
	private void AddReqField(string name, bool force) {
		var container = UI.AddField(map_panel!, name);
		requirements[name] = UI.AddDropdown(container, 0, (v) => {
			SetForced(name, true);
		}, MapSettings.RequirementStatus);
		var container2 = UI.AddField(map_panel!, "Override");
		forced[name] = UI.AddCheckbox(container2, force, (v) => {
			// Can't un-force a custom requirement
			if (!default_reqchecks.ContainsKey(name)) {
				v = true;
			}
			SetForced(name, v);
		});
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
		requirementsAndSuggestions!.Remove(GetReqCheck(name)!);
		RequirementCheck.RegisterRequirement(force
			? (new CustomRequirement(name, (RequirementCheck.RequirementType)requirements[name].Dropdown.value))
			: ((RequirementCheck)Activator.CreateInstance(default_reqchecks[name])));
		forced[name].isOn = force;
		Refresh();
	}
	
	private RequirementCheck? GetReqCheck(string name) {
		return requirementsAndSuggestions.FirstOrDefault((r) => r.Name == name);
	}
	
	public SettingsController() {
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
		foreach (var r in requirements) {
			r.Value.Dropdown.SetValueWithoutNotify((int)(GetReqCheck(r.Key)!.IsRequiredOrSuggested(BeatSaberSongContainer.Instance.DifficultyData, BeatSaberSongContainer.Instance.Map)));
		}
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
