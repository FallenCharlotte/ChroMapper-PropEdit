using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Enums;
using Beatmap.V2;
using Beatmap.V2.Customs;
using Beatmap.V3;

using ChroMapper_PropEdit.Components;
using ChroMapper_PropEdit.Enums;
using ChroMapper_PropEdit.Utils;

using Convert = System.Convert;

namespace ChroMapper_PropEdit.UserInterface {

public partial class MainWindow {
	public readonly string CHROMA_NAME = "Chroma";
	public readonly string NOODLE_NAME = "Noodle Extensions";
	
	public void UpdateSelection(bool real) { lock(this) {
		foreach (Transform child in panel!.transform) {
			GameObject.Destroy(child.gameObject);
		}
		
		editing = SelectionController.SelectedObjects.Select(it => it).ToList();
		
		if (SelectionController.HasSelectedObjects() && editing.Count() > 0) {
			window!.SetTitle($"{SelectionController.SelectedObjects.Count} Items selected");
			
			if (editing.GroupBy(o => o.ObjectType).Count() > 1) {
				UI.AddLabel(panel!, "Unsupported", "Multi-Type Unsupported!", Vector2.zero);
				return;
			}
			
			var o = editing.First();
			var type = o.ObjectType;
			var v2 = (o is V2Object);
			
			current_panel = panel;
			
			AddParsed("Beat", Data.GetSet<float>("JsonTime"));
			
			switch (type) {
				case ObjectType.Note:
					var note = (o as BaseNote)!;
					AddParsed("X", Data.GetSet<int>("PosX"));
					AddParsed("Y", Data.GetSet<int>("PosY"));
					AddDropdown<int?>("Type", Data.GetSet<int>("Type"), Notes.NoteTypes);
					AddDropdown<int?>("Direction", Data.GetSet<int>("CutDirection"), Notes.CutDirections);
					if (!v2) {
						AddParsed("Angle Offset", Data.GetSet<int>("AngleOffset"));
					}
					AddLine("");
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
						current_panel = collapsible.panel;
						AddColor("Color", o.CustomKeyColor);
						if (o is V2Note) {
							AddCheckbox("Disable Spawn Effect", Data.CustomGetSet<bool?>("_disableSpawnEffect"), false);
						}
						else {
							AddCheckbox("Spawn Effect", Data.CustomGetSet<bool?>("spawnEffect"), true);
							AddCheckbox("Disable Debris", Data.CustomGetSet<bool?>("disableDebris"), false);
						}
						current_panel = panel;
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, NOODLE_NAME, "Noodle Extensions", true);
						current_panel = collapsible.panel;
						AddParsed("NJS", Data.CustomGetSet<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"));
						AddParsed("Spawn Offset", Data.CustomGetSet<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"));
						AddTextbox("Coordinates", Data.CustomGetSetRaw(note.CustomKeyCoordinate), true);
						AddTextbox("Rotation", Data.CustomGetSetRaw(note.CustomKeyWorldRotation), true);
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(note.CustomKeyLocalRotation), true);
						if (o is V2Note) {
							AddParsed("Exact Angle", Data.CustomGetSet<float?>("_cutDirection"));
							AddCheckbox("Fake", Data.CustomGetSet<bool?>("_fake"), false);
							AddCheckbox("Interactable", Data.CustomGetSet<bool?>("_interactable"), true);
							AddTextbox("Flip", Data.CustomGetSetRaw("_flip"), true);
						}
						else {
							//AddCheckbox("Uninteractable", Data.CustomGetSet<bool?>("uninteractable"), false);
							AddCheckbox("Disable Gravity", Data.CustomGetSet<bool?>("disableNoteGravity"), false);
							AddCheckbox("Disable Look", Data.CustomGetSet<bool?>("disableNoteLook"), false);
							AddCheckbox("No Badcut Direction", Data.CustomGetSet<bool?>("disableBadCutDirection"), false);
							AddCheckbox("No Badcut Speed", Data.CustomGetSet<bool?>("disableBadCutSpeed"), false);
							AddCheckbox("No Badcut Color", Data.CustomGetSet<bool?>("disableBadCutSaberType"), false);
							AddTextbox("Flip", Data.CustomGetSetRaw("flip"), true);
							AddTextbox("Link", Data.CustomGetSet<string>("link"));
						}
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true);
						AddAnimation(v2);
						current_panel = panel;
					}
					
					break;
				case ObjectType.CustomNote:
					AddLine("Wow, a custom note! How did you do this?");
					break;
				case ObjectType.Arc: {
					AddParsed("Head X", Data.GetSet<int>("PosX"));
					AddParsed("Head Y", Data.GetSet<int>("PosY"));
					AddDropdown<int?>("Color", Data.GetSet<int>("Color"), Notes.ArcColors);
					AddDropdown<int?>("Direction", Data.GetSet<int>("CutDirection"), Notes.CutDirections);
					AddParsed("Head Multiplier", Data.GetSet<float>("HeadControlPointLengthMultiplier"));
					AddParsed("Tail Beat", Data.GetSet<float>("TailJsonTime"));
					AddParsed("Tail X", Data.GetSet<int>("TailPosX"));
					AddParsed("Tail Y", Data.GetSet<int>("TailPosY"));
					AddDropdown<int?>("Tail Direction", Data.GetSet<int>("TailCutDirection"), Notes.CutDirections);
					AddParsed("Tail Multiplier", Data.GetSet<float>("TailControlPointLengthMultiplier"));
					AddLine("");
					
					var s = (o as BaseSlider)!;
					
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
						current_panel = collapsible.panel;
						AddColor("Color", o.CustomKeyColor);
						current_panel = panel;
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, NOODLE_NAME, "Noodle Extensions", true);
						current_panel = collapsible.panel;
						AddParsed("NJS", Data.CustomGetSet<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"));
						AddParsed("Spawn Offset", Data.CustomGetSet<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"));
						AddTextbox("Head Coordinates", Data.CustomGetSetRaw(s.CustomKeyCoordinate), true);
						AddTextbox("Tail Coordinates", Data.CustomGetSetRaw(s.CustomKeyTailCoordinate), true);
						AddTextbox("Rotation", Data.CustomGetSetRaw(s.CustomKeyWorldRotation), true);
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(s.CustomKeyLocalRotation), true);
						if (v2) {
							AddCheckbox("Interactable", Data.CustomGetSet<bool?>("_interactable"), true);
							AddTextbox("Flip", Data.CustomGetSetRaw("_flip"), true);
						}
						else {
							//AddCheckbox("Uninteractable", Data.CustomGetSet<bool?>("uninteractable"), false);
							AddCheckbox("Disable Gravity", Data.CustomGetSet<bool?>("disableNoteGravity"), false);
							AddTextbox("Flip", Data.CustomGetSetRaw("flip"), true);
							AddTextbox("Link", Data.CustomGetSet<string>("link"));
						}
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true);
						AddAnimation(v2);
						current_panel = panel;
					}
					
				}	break;
				case ObjectType.Chain: {
					AddParsed("Head X", Data.GetSet<int>("PosX"));
					AddParsed("Head Y", Data.GetSet<int>("PosY"));
					AddDropdown<int?>("Color", Data.GetSet<int>("Color"), Notes.ArcColors);
					AddDropdown<int?>("Direction", Data.GetSet<int>("CutDirection"), Notes.CutDirections);
					AddParsed("Slices", Data.GetSet<int>("SliceCount"));
					AddParsed("Squish", Data.GetSet<float>("Squish"));
					AddParsed("Tail X", Data.GetSet<int>("TailPosX"));
					AddParsed("Tail Y", Data.GetSet<int>("TailPosY"));
					AddLine("");
					
					var s = (o as BaseSlider)!;
					
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
						current_panel = collapsible.panel;
						AddColor("Color", o.CustomKeyColor);
						current_panel = panel;
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, NOODLE_NAME, "Noodle Extensions", true);
						current_panel = collapsible.panel;
						AddParsed("NJS", Data.CustomGetSet<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"));
						AddParsed("Spawn Offset", Data.CustomGetSet<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"));
						AddTextbox("Head Coordinates", Data.CustomGetSetRaw(s.CustomKeyCoordinate), true);
						AddTextbox("Tail Coordinates", Data.CustomGetSetRaw(s.CustomKeyTailCoordinate), true);
						AddTextbox("Rotation", Data.CustomGetSetRaw(s.CustomKeyWorldRotation), true);
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(s.CustomKeyLocalRotation), true);
						if (v2) {
							AddCheckbox("Interactable", Data.CustomGetSet<bool?>("_interactable"), true);
							AddTextbox("Flip", Data.CustomGetSetRaw("_flip"), true);
						}
						else {
							//AddCheckbox("Uninteractable", Data.CustomGetSet<bool?>("uninteractable"), false);
							AddCheckbox("Disable Gravity", Data.CustomGetSet<bool?>("disableNoteGravity"), false);
							AddTextbox("Flip", Data.CustomGetSetRaw("flip"), true);
							AddTextbox("Link", Data.CustomGetSet<string>("link"));
						}
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true);
						AddAnimation(v2);
						current_panel = panel;
					}
					
				}	break;
				case ObjectType.Obstacle:
					var ob = (o as BaseObstacle)!;
					AddParsed("Duration", Data.GetSet<float>("Duration"));
					if (o is V2Obstacle) {
						AddParsed("X", Data.GetSet<int>("PosX"));
						AddParsed("Width", Data.GetSet<int>("Width"));
						AddDropdown<int?>("Height", Data.GetSet<int>("Type"), Obstacles.WallHeights);
					}
					else {
						AddParsed("X (Left)", Data.GetSet<int>("PosX"));
						AddParsed("Y (Bottom)", Data.GetSet<int>("PosY"));
						AddParsed("Width", Data.GetSet<int>("Width"));
						AddParsed("Height", Data.GetSet<int>("Height"));
					}
					AddLine("");
					
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
						current_panel = collapsible.panel;
						AddColor("Color", o.CustomKeyColor);
						current_panel = panel;
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, NOODLE_NAME, "Noodle Extensions", true);
						current_panel = collapsible.panel;
						AddParsed("NJS", Data.CustomGetSet<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"));
						AddParsed("Spawn Offset", Data.CustomGetSet<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"));
						AddTextbox("Position", Data.CustomGetSetRaw(ob.CustomKeyCoordinate), true);
						AddTextbox("Rotation", Data.CustomGetSetRaw(ob.CustomKeyWorldRotation), true);
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(ob.CustomKeyLocalRotation), true);
						AddTextbox("Size", Data.CustomGetSetRaw(ob.CustomKeySize), true);
						if (o is V2Obstacle) {
							AddCheckbox("Fake", Data.CustomGetSet<bool?>("_fake"), false);
							AddCheckbox("Interactable", Data.CustomGetSet<bool?>("_interactable"), true);
						}
						else {
							AddCheckbox("Uninteractable", Data.CustomGetSet<bool?>("uninteractable"), false);
						}
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true);
						AddAnimation(o is V2Obstacle);
						current_panel = panel;
					}
					
					break;
				case ObjectType.Event: {
					var env = BeatSaberSongContainer.Instance.Song.EnvironmentName;
					var events = editing.Select(o => (BaseEvent)o);
					var f = events.First();
					// Light
					if (events.Where(e => e.IsLightEvent(env)).Count() == editing.Count()) {
						if (Settings.Get(Settings.SplitValue, true)!.AsBool) {
							AddDropdown<int?>("Color", Data.GetSetSplitValue(0b1100), Events.LightColors);
							AddDropdown<int?>("Action", Data.GetSetSplitValue(0b0011), Events.LightActions);
						}
						else {
							AddDropdown<int?>("Value", Data.GetSet<int>("Value"), Events.LightValues);
						}
						AddParsed("Brightness", Data.GetSet<float>("FloatValue"));
						AddLine("");
						
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
							current_panel = collapsible.panel;
							AddTextbox("LightID", Data.CustomGetSetRaw(f.CustomKeyLightID), true);
							AddColor("Color", o.CustomKeyColor);
							if (events.Where(e => e.IsTransition).Count() == editing.Count()) {
								AddDropdown<string>("Easing",    Data.CustomGetSet<string>(f.CustomKeyEasing), Events.Easings, true);
								AddDropdown<string>("Lerp Type", Data.CustomGetSet<string>(f.CustomKeyLerpType), Events.LerpTypes, true);
							}
							if (o is V2Event e) {
								AddCheckbox("V2 Gradient", Data.GetSetGradient(), false);
								if (e.CustomLightGradient != null) {
									AddParsed("Duration",     Data.CustomGetSet<float?>($"{e.CustomKeyLightGradient}._duration"));
									AddColor("Start Color", $"{e.CustomKeyLightGradient}._startColor");
									AddColor("End Color", $"{e.CustomKeyLightGradient}._endColor");
									AddDropdown<string>("Easing",    Data.CustomGetSet<string>($"{e.CustomKeyLightGradient}._easing"), Events.Easings, false);
								}
							}
							current_panel = panel;
						}
					}
					// Laser Speeds
					if (events.Where(e => e.IsLaserRotationEvent(env)).Count() == editing.Count()) {
						AddParsed("Speed", Data.GetSet<int>("Value"));
						AddLine("");
						
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
							current_panel = collapsible.panel;
							AddCheckbox("Lock Rotation", Data.CustomGetSet<bool?> (f.CustomKeyLockRotation), false);
							AddDropdown<int?>("Direction",     Data.CustomGetSet<int?>  (f.CustomKeyDirection), Events.LaserDirection, true);
							AddParsed("Precise Speed",   Data.CustomGetSet<float?>(f.CustomKeyPreciseSpeed));
							current_panel = panel;
						}
					}
					// Ring Rotation
					if (events.Where(e => e.Type == (int)EventTypeValue.RingRotation).Count() == editing.Count()) {
						AddLine("");
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
							current_panel = collapsible.panel;
							AddTextbox("Filter",     Data.CustomGetSet<string>(f.CustomKeyNameFilter));
							if (o is V2Event) {
								AddCheckbox("Reset", Data.CustomGetSet<bool?>("_reset"), false);
							}
							AddParsed("Rotation",    Data.CustomGetSet<float?>(f.CustomKeyLaneRotation));
							AddParsed("Step",        Data.CustomGetSet<float?>(f.CustomKeyStep));
							AddParsed("Propagation", Data.CustomGetSet<float?>(f.CustomKeyProp));
							AddParsed("Speed",       Data.CustomGetSet<float?>(f.CustomKeySpeed));
							AddDropdown<int?>("Direction", Data.CustomGetSet<int?>  (f.CustomKeyDirection), Events.RingDirection, true);
							if (o is V2Event) {
								AddCheckbox("Counter Spin", Data.CustomGetSet<bool?>("_counterSpin"), false);
							}
							current_panel = panel;
						}
					}
					// Ring Zoom
					if (events.Where(e => e.Type == (int)EventTypeValue.RingZoom).Count() == editing.Count()) {
						AddLine("");
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
							current_panel = collapsible.panel;
							AddParsed("Step",  Data.CustomGetSet<float?>(f.CustomKeyStep));
							AddParsed("Speed", Data.CustomGetSet<float?>(f.CustomKeySpeed));
							current_panel = panel;
						}
					}
					// Boost Color
					if (events.Where(e => e.IsColorBoostEvent()).Count() == editing.Count()) {
						AddDropdown<int?>("Color Set", Data.GetSet<int>("Value"), Events.BoostSets);
					}
					// Lane Rotations
					if (events.Where(e => e.IsLaneRotationEvent()).Count() == editing.Count()) {
						AddDropdown<int?>("Rotation", Data.GetSet<int>("Value"), Events.LaneRotaions);
					}
				}	break;
				case ObjectType.CustomEvent: {
					var events = editing.Select(o => (BaseCustomEvent)o);
					var f = events.First();
					
					if (events.Where(e => e.Type == "AnimateTrack").Count() == editing.Count()) {
						AddTextbox("Track", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true);
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, false);
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "repeat"));
						}
						AddLine("");
						AddTextbox("Color", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_color" : "color"), true);
						foreach (var property in Events.NoodleProperties) {
							AddTextbox(property.Key, Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", property.Value[v2 ? 0 : 1]), true);
						}
						AddTextbox("Time", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_time" : "time"), true);
					}
					if (events.Where(e => e.Type == "AssignPathAnimation").Count() == editing.Count()) {
						AddTextbox("Track", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true);
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, false);
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "repeat"));
						}
						AddLine("");
						AddTextbox("Color", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_color" : "color"), true);
						foreach (var property in Events.NoodleProperties) {
							AddTextbox(property.Key, Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", property.Value[v2 ? 0 : 1]), true);
						}
						AddTextbox("Definite Position", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_definitePosition" : "definitePosition"), true);
					}
					if (events.Where(e => e.Type == "AssignTrackParent").Count() == editing.Count()) {
						AddTextbox("Parent", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_parentTrack" : "parentTrack"), true);
						AddTextbox("Children", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_childrenTracks" : "childrenTracks"), true);
						AddCheckbox("Keep Position", Data.JSONGetSet<bool?>(typeof(BaseCustomEvent), "Data", v2 ? "_worldPositionStays" : "worldPositionStays"), false);
					}
					if (events.Where(e => e.Type == "AssignPlayerToTrack").Count() == editing.Count()) {
						AddTextbox("Track", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true);
					}
					if (events.Where(e => e.Type == "AssignFogTrack").Count() == editing.Count()) {
						AddTextbox("Track", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true);
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, false);
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "repeat"));
						}
						
						AddParsed("Attenuation", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "_attenuation"));
						AddParsed("Offset", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "_offset"));
						AddParsed("Start Y", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "_startY"));
						AddParsed("Height", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "_height"));
					}
					if (events.Where(e => e.Type == "AssignComponent").Count() == editing.Count()) {
						AddTextbox("Track", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true);
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, false);
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "repeat"));
						}
						AddTextbox("Environment Fog", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "BloomFogEnvironment"));
						AddTextbox("Tube Bloom Light", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "TubeBloomPrePassLight"));
					}
					
				}	break;
				case ObjectType.BpmChange:
					AddParsed("BPM", Data.GetSet<float>("Bpm"));
					break;
			}
		}
		else {
			window!.SetTitle("No items selected");
		}
		if (real) {
			scrollbox!.ScrollToTop();
		}
	}}
	
	private void AddAnimation(bool v2) {
		var CustomKeyAnimation = v2 ? "_animation" : "animation";
		AddCheckbox("Animation", Data.GetSetAnimation(v2), false);
		if (editing.Where(o => o.CustomData?.HasKey(CustomKeyAnimation) ?? false).Count() == editing.Count()) {
			AddCheckbox("  Color", Data.CustomGetSetNode(CustomKeyAnimation+"."+ (v2 ? "_color" : "color"), "[[0,0,0,0,0], [1,1,1,1,0.49]],"), false);
			foreach (var property in Events.NoodleProperties) {
				AddCheckbox("  "+property.Key, Data.CustomGetSetNode(CustomKeyAnimation+"."+ property.Value[v2 ? 0 : 1], property.Value[2]), false);
			}
			AddCheckbox("  Definite Position", Data.CustomGetSetNode(CustomKeyAnimation+"."+ (v2 ? "_definitePosition" : "definitePosition"), "[[0,0,0,0], [0,0,0,0.49]]"), true);
			AddLine("");
		}
	}
	
	private void AddColor(string label, string key) {
		if (Settings.Get(Settings.ColorHex, true)) {
			AddTextbox(label, Data.CustomGetSetColor(key));
		}
		else {
			AddTextbox(label, Data.CustomGetSetRaw(key), true);
		}
	}
}

}
