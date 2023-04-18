using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Enums;
using Beatmap.Helper;
using Beatmap.V2;
using Beatmap.V2.Customs;
using Beatmap.V3;

using ChroMapper_PropEdit.Components;
using ChroMapper_PropEdit.Enums;
using ChroMapper_PropEdit.Utils;

using Convert = System.Convert;

namespace ChroMapper_PropEdit.UserInterface {

public partial class MainWindow {
	public void UpdateSelection(bool real) {
		foreach (var e in elements) {
			Object.Destroy(e);
		}
		elements.Clear();
		
		// Hopefully bonk some race condition
		if (real) {
			editing = SelectionController.SelectedObjects.Select(it => it).ToList();
		}
		
		if (SelectionController.HasSelectedObjects()) {
			window.SetTitle($"{SelectionController.SelectedObjects.Count} Items selected");
			
			if (editing.GroupBy(o => o.ObjectType).Count() > 1) {
				elements.Add(UI.AddLabel(panel.transform, "Unsupported", "Multi-Type Unsupported!", new Vector2(0, 0)));
				return;
			}
			
			var o = editing.First();
			var type = o.ObjectType;
			var v2 = (o is V2Object);
			
			AddParsed("Beat", Data.GetSet<float>("Time"));
			
			switch (type) {
				case ObjectType.Note:
					var note = o as BaseNote;
					AddParsed("X", Data.GetSet<int>("PosX"));
					AddParsed("Y", Data.GetSet<int>("PosY"));
					AddDropdownI("Type", Data.GetSet<int>("Type"), Notes.NoteTypes);
					AddDropdownI("Direction", Data.GetSet<int>("CutDirection"), Notes.CutDirections);
					if (!v2) {
						AddParsed("Angle Offset", Data.GetSet<int>("AngleOffset"));
					}
					
					if (Settings.Get("Chroma")?.AsBool ?? false) {
						AddLine("");
						AddLine("Chroma");
						AddTextbox("Color", Data.CustomGetSetColor(o.CustomKeyColor));
						if (o is V2Note) {
							AddCheckbox("Disable Spawn Effect", Data.CustomGetSet<bool>("_disableSpawnEffect"), false);
						}
						else {
							AddCheckbox("Spawn Effect", Data.CustomGetSet<bool>("spawnEffect"), true);
							AddCheckbox("Disable Debris", Data.CustomGetSet<bool>("disableDebris"), false);
						}
					}
					
					if (Settings.Get("Noodle")?.AsBool ?? false) {
						AddLine("");
						AddLine("Noodle Extensions");
						AddParsed("NJS", Data.CustomGetSet<float>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"));
						AddParsed("Spawn Offset", Data.CustomGetSet<float>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"));
						AddTextbox("Coordinates", Data.CustomGetSetRaw(note.CustomKeyCoordinate), true);
						AddTextbox("Rotation", Data.CustomGetSetRaw(note.CustomKeyWorldRotation), true);
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(note.CustomKeyLocalRotation), true);
						if (o is V2Note) {
							AddParsed("Exact Angle", Data.GetSet<int>("CustomDirection"));
							AddCheckbox("Fake", Data.CustomGetSet<bool>("_fake"), false);
							AddCheckbox("Interactable", Data.CustomGetSet<bool>("_interactable"), true);
							AddTextbox("Flip", Data.CustomGetSetRaw("_flip"), true);
						}
						else {
							//AddCheckbox("Uninteractable", Data.CustomGetSet<bool>("uninteractable"), false);
							AddCheckbox("Disable Gravity", Data.CustomGetSet<bool>("disableNoteGravity"), false);
							AddCheckbox("Disable Look", Data.CustomGetSet<bool>("disableNoteLook"), false);
							AddCheckbox("No Badcut Direction", Data.CustomGetSet<bool>("disableBadCutDirection"), false);
							AddCheckbox("No Badcut Speed", Data.CustomGetSet<bool>("disableBadCutSpeed"), false);
							AddCheckbox("No Badcut Color", Data.CustomGetSet<bool>("disableBadCutSaberType"), false);
							AddTextbox("Flip", Data.CustomGetSetRaw("flip"), true);
							AddTextbox("Link", Data.CustomGetSet("link"));
						}
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true);
						AddAnimation(v2);
					}
					
					break;
				case ObjectType.CustomNote:
					AddLine("Wow, a custom note! How did you do this?");
					break;
				case ObjectType.Arc: {
					AddParsed("Head X", Data.GetSet<int>("PosX"));
					AddParsed("Head Y", Data.GetSet<int>("PosY"));
					AddDropdownI("Color", Data.GetSet<int>("Color"), Notes.ArcColors);
					AddDropdownI("Direction", Data.GetSet<int>("CutDirection"), Notes.CutDirections);
					AddParsed("Head Multiplier", Data.GetSet<float>("HeadControlPointLengthMultiplier"));
					AddParsed("Tail Beat", Data.GetSet<float>("TailTime"));
					AddParsed("Tail X", Data.GetSet<int>("TailPosX"));
					AddParsed("Tail Y", Data.GetSet<int>("TailPosY"));
					AddDropdownI("Tail Direction", Data.GetSet<int>("TailCutDirection"), Notes.CutDirections);
					AddParsed("Tail Multiplier", Data.GetSet<float>("TailControlPointLengthMultiplier"));
					
					var s = o as BaseSlider;
					
					if (Settings.Get("Chroma")?.AsBool ?? false) {
						AddLine("");
						AddLine("Chroma");
						AddTextbox("Color", Data.CustomGetSetColor(o.CustomKeyColor));
					}
					
					if (Settings.Get("Noodle")?.AsBool ?? false) {
						AddLine("");
						AddLine("Noodle Extensions");
						AddParsed("NJS", Data.CustomGetSet<float>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"));
						AddParsed("Spawn Offset", Data.CustomGetSet<float>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"));
						AddTextbox("Head Coordinates", Data.CustomGetSetRaw(s.CustomKeyCoordinate), true);
						AddTextbox("Tail Coordinates", Data.CustomGetSetRaw(s.CustomKeyTailCoordinate), true);
						AddTextbox("Rotation", Data.CustomGetSetRaw(s.CustomKeyWorldRotation), true);
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(s.CustomKeyLocalRotation), true);
						if (v2) {
							AddCheckbox("Interactable", Data.CustomGetSet<bool>("_interactable"), true);
							AddTextbox("Flip", Data.CustomGetSetRaw("_flip"), true);
						}
						else {
							//AddCheckbox("Uninteractable", Data.CustomGetSet<bool>("uninteractable"), false);
							AddCheckbox("Disable Gravity", Data.CustomGetSet<bool>("disableNoteGravity"), false);
							AddTextbox("Flip", Data.CustomGetSetRaw("flip"), true);
							AddTextbox("Link", Data.CustomGetSet("link"));
						}
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true);
						AddAnimation(v2);
					}
					
				}	break;
				case ObjectType.Chain: {
					AddParsed("Head X", Data.GetSet<int>("PosX"));
					AddParsed("Head Y", Data.GetSet<int>("PosY"));
					AddDropdownI("Color", Data.GetSet<int>("Color"), Notes.ArcColors);
					AddDropdownI("Direction", Data.GetSet<int>("CutDirection"), Notes.CutDirections);
					AddParsed("Slices", Data.GetSet<int>("SliceCount"));
					AddParsed("Squish", Data.GetSet<float>("Squish"));
					AddParsed("Tail X", Data.GetSet<int>("TailPosX"));
					AddParsed("Tail Y", Data.GetSet<int>("TailPosY"));
					
					var s = o as BaseSlider;
					
					if (Settings.Get("Chroma")?.AsBool ?? false) {
						AddLine("");
						AddLine("Chroma");
						AddTextbox("Color", Data.CustomGetSetColor(o.CustomKeyColor));
					}
					
					if (Settings.Get("Noodle")?.AsBool ?? false) {
						AddLine("");
						AddLine("Noodle Extensions");
						AddParsed("NJS", Data.CustomGetSet<float>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"));
						AddParsed("Spawn Offset", Data.CustomGetSet<float>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"));
						AddTextbox("Head Coordinates", Data.CustomGetSetRaw(s.CustomKeyCoordinate), true);
						AddTextbox("Tail Coordinates", Data.CustomGetSetRaw(s.CustomKeyTailCoordinate), true);
						AddTextbox("Rotation", Data.CustomGetSetRaw(s.CustomKeyWorldRotation), true);
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(s.CustomKeyLocalRotation), true);
						if (v2) {
							AddCheckbox("Interactable", Data.CustomGetSet<bool>("_interactable"), true);
							AddTextbox("Flip", Data.CustomGetSetRaw("_flip"), true);
						}
						else {
							//AddCheckbox("Uninteractable", Data.CustomGetSet<bool>("uninteractable"), false);
							AddCheckbox("Disable Gravity", Data.CustomGetSet<bool>("disableNoteGravity"), false);
							AddTextbox("Flip", Data.CustomGetSetRaw("flip"), true);
							AddTextbox("Link", Data.CustomGetSet("link"));
						}
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true);
						AddAnimation(v2);
					}
				}	break;
				case ObjectType.Obstacle:
					var ob = o as BaseObstacle;
					AddParsed("Duration", Data.GetSet<float>("Duration"));
					if (o is V2Obstacle) {
						AddParsed("X", Data.GetSet<int>("PosX"));
						AddParsed("Width", Data.GetSet<int>("Width"));
						AddDropdownI("Height", Data.GetSet<int>("Type"), Obstacles.WallHeights);
					}
					else {
						AddParsed("X (Left)", Data.GetSet<int>("PosX"));
						AddParsed("Y (Bottom)", Data.GetSet<int>("PosY"));
						AddParsed("Width", Data.GetSet<int>("Width"));
						AddParsed("Height", Data.GetSet<int>("Height"));
					}
					
					if (Settings.Get("Chroma")?.AsBool ?? false) {
						AddLine("");
						AddLine("Chroma");
						AddTextbox("Color", Data.CustomGetSetColor(o.CustomKeyColor));
					}
					
					if (Settings.Get("Noodle")?.AsBool ?? false) {
						AddLine("");
						AddLine("Noodle Extensions");
						AddParsed("NJS", Data.CustomGetSet<float>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"));
						AddParsed("Spawn Offset", Data.CustomGetSet<float>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"));
						AddTextbox("Position", Data.CustomGetSetRaw(ob.CustomKeyCoordinate), true);
						AddTextbox("Rotation", Data.CustomGetSetRaw(ob.CustomKeyWorldRotation), true);
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(ob.CustomKeyLocalRotation), true);
						AddTextbox("Size", Data.CustomGetSetRaw(ob.CustomKeySize), true);
						if (o is V2Obstacle) {
							AddCheckbox("Fake", Data.CustomGetSet<bool>("_fake"), false);
							AddCheckbox("Interactable", Data.CustomGetSet<bool>("_interactable"), true);
						}
						else {
							AddCheckbox("Uninteractable", Data.CustomGetSet<bool>("uninteractable"), false);
						}
						AddAnimation(o is V2Obstacle);
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true);
					}
					
					break;
				case ObjectType.Event: {
					var env = BeatSaberSongContainer.Instance.Song.EnvironmentName;
					var events = editing.Select(o => (BaseEvent)o);
					var f = events.First();
					// Light
					if (events.Where(e => e.IsLightEvent(env)).Count() == editing.Count()) {
						if (Settings.Get("split_val", true).AsBool) {
							AddDropdownI("Color", Data.GetSetSplitValue(0b1100), Events.LightColors);
							AddDropdownI("Action", Data.GetSetSplitValue(0b0011), Events.LightActions);
						}
						else {
							AddDropdownI("Value", Data.GetSet<int>("Value"), Events.LightValues);
						}
						AddParsed("Brightness", Data.GetSet<float>("FloatValue"));
						
						if (Settings.Get("Chroma")?.AsBool ?? false) {
							AddLine("");
							AddLine("Chroma");
							AddTextbox("LightID", Data.CustomGetSetRaw(f.CustomKeyLightID), true);
							AddTextbox("Color", Data.CustomGetSetColor(o.CustomKeyColor));
							if (events.Where(e => e.IsTransition).Count() == editing.Count()) {
								AddDropdownS("Easing",    Data.CustomGetSet(f.CustomKeyEasing), Events.Easings, false);
								AddDropdownS("Lerp Type", Data.CustomGetSet(f.CustomKeyLerpType), Events.LerpTypes, false);
							}
							if (o is V2Event e) {
								AddCheckbox("V2 Gradient", Data.GetSetGradient(), false);
								if (e.CustomLightGradient != null) {
									AddParsed("Duration",     Data.CustomGetSet<float>($"{e.CustomKeyLightGradient}._duration"));
									AddTextbox("Start Color", Data.CustomGetSetColor($"{e.CustomKeyLightGradient}._startColor"));
									AddTextbox("End Color",   Data.CustomGetSetColor($"{e.CustomKeyLightGradient}._endColor"));
									AddDropdownS("Easing",    Data.CustomGetSet($"{e.CustomKeyLightGradient}._easing"), Events.Easings, false);
								}
							}
						}
					}
					// Laser Speeds
					if (events.Where(e => e.IsLaserRotationEvent(env)).Count() == editing.Count()) {
						AddParsed("Speed", Data.GetSet<int>("Value"));
						
						if (Settings.Get("Chroma")?.AsBool ?? false) {
							AddLine("");
							AddLine("Chroma");
							AddCheckbox("Lock Rotation", Data.CustomGetSet<bool> (f.CustomKeyLockRotation), false);
							AddDropdownI("Direction",     Data.CustomGetSet<int>  (f.CustomKeyDirection), Events.LaserDirection, true);
							AddParsed("Precise Speed",   Data.CustomGetSet<float>(f.CustomKeyPreciseSpeed));
						}
					}
					// Ring Rotation
					if (events.Where(e => e.Type == (int)EventTypeValue.RingRotation).Count() == editing.Count()) {
						if (Settings.Get("Chroma")?.AsBool ?? false) {
							AddLine("");
							AddLine("Chroma");
							AddTextbox("Filter",     Data.CustomGetSet(f.CustomKeyNameFilter));
							if (o is V2Event) {
								AddCheckbox("Reset", Data.CustomGetSet<bool>("_reset"), false);
							}
							AddParsed("Rotation",    Data.CustomGetSet<int>  (f.CustomKeyLaneRotation));
							AddParsed("Step",        Data.CustomGetSet<float>(f.CustomKeyStep));
							AddParsed("Propagation", Data.CustomGetSet<float>(f.CustomKeyProp));
							AddParsed("Speed",       Data.CustomGetSet<float>(f.CustomKeySpeed));
							AddDropdownI("Direction", Data.CustomGetSet<int>  (f.CustomKeyDirection), Events.RingDirection, true);
							if (o is V2Event) {
								AddCheckbox("Counter Spin", Data.CustomGetSet<bool>("_counterSpin"), false);
							}
						}
					}
					// Ring Zoom
					if (events.Where(e => e.Type == (int)EventTypeValue.RingZoom).Count() == editing.Count()) {
						if (Settings.Get("Chroma")?.AsBool ?? false) {
							AddLine("");
							AddLine("Chroma");
							AddParsed("Step",  Data.CustomGetSet<float>(f.CustomKeyStep));
							AddParsed("Speed", Data.CustomGetSet<float>(f.CustomKeySpeed));
						}
					}
					// Boost Color
					if (events.Where(e => e.IsColorBoostEvent()).Count() == editing.Count()) {
						AddDropdownI("Color Set", Data.GetSet<int>("Value"), Events.BoostSets);
					}
					// Lane Rotations
					if (events.Where(e => e.IsLaneRotationEvent()).Count() == editing.Count()) {
						AddDropdownI("Rotation", Data.GetSet<int>("Value"), Events.LaneRotaions);
					}
				}	break;
				case ObjectType.CustomEvent: {
					var events = editing.Select(o => (BaseCustomEvent)o);
					var f = events.First();
					
					if (events.Where(e => e.Type == "AnimateTrack").Count() == editing.Count()) {
						AddTextbox("Track", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true);
						AddParsed("Duration", Data.JSONGetSet<float>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"));
						AddDropdownS("Easing", Data.JSONGetSet(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, false);
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int>(typeof(BaseCustomEvent), "Data", "repeat"));
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
						AddParsed("Duration", Data.JSONGetSet<float>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"));
						AddDropdownS("Easing", Data.JSONGetSet(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, false);
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int>(typeof(BaseCustomEvent), "Data", "repeat"));
						}
						AddLine("");
						AddTextbox("Color", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_color" : "color"), true);
						foreach (var property in Events.NoodleProperties) {
							AddTextbox(property.Key, Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", property.Value[v2 ? 0 : 1]), true);
						}
						AddTextbox("Definite Position", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_definitePosition" : "definitePosition"), true);
					}
					if (events.Where(e => e.Type == "AssignTrackParent").Count() == editing.Count()) {
						AddTextbox("Parent", Data.JSONGetSet(typeof(BaseCustomEvent), "Data", v2 ? "_parentTrack" : "parentTrack"), true);
						AddTextbox("Children", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_childrenTracks" : "childrenTracks"), true);
						AddCheckbox("Keep Position", Data.JSONGetSet<bool>(typeof(BaseCustomEvent), "Data", v2 ? "_worldPositionStays" : "worldPositionStays"), false);
					}
					if (events.Where(e => e.Type == "AssignPlayerToTrack").Count() == editing.Count()) {
						AddTextbox("Track", Data.JSONGetSet(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true);
					}
					if (events.Where(e => e.Type == "AssignFogTrack").Count() == editing.Count()) {
						AddTextbox("Track", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true);
						AddParsed("Duration", Data.JSONGetSet<float>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"));
						AddDropdownS("Easing", Data.JSONGetSet(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, false);
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int>(typeof(BaseCustomEvent), "Data", "repeat"));
						}
						
						AddParsed("Attenuation", Data.JSONGetSet<float>(typeof(BaseCustomEvent), "Data", "_attenuation"));
						AddParsed("Offset", Data.JSONGetSet<float>(typeof(BaseCustomEvent), "Data", "_offset"));
						AddParsed("Start Y", Data.JSONGetSet<float>(typeof(BaseCustomEvent), "Data", "_startY"));
						AddParsed("Height", Data.JSONGetSet<float>(typeof(BaseCustomEvent), "Data", "_height"));
					}
					if (events.Where(e => e.Type == "AssignComponent").Count() == editing.Count()) {
						AddTextbox("Track", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true);
						AddParsed("Duration", Data.JSONGetSet<float>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"));
						AddDropdownS("Easing", Data.JSONGetSet(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, false);
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int>(typeof(BaseCustomEvent), "Data", "repeat"));
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
			window.SetTitle("No items selected");
		}
		if (real) {
			scroll_to_top.Trigger();
		}
	}
	
	private void AddAnimation(bool v2) {
		var CustomKeyAnimation = v2 ? "_animation" : "animation";
		AddCheckbox("Animation", Data.GetSetAnimation(v2), false);
		if (editing.Where(o => o.CustomData.HasKey(CustomKeyAnimation)).Count() == editing.Count()) {
			AddCheckbox("  Color", Data.CustomGetSetNode(CustomKeyAnimation+"."+ (v2 ? "_color" : "color"), "[[0,0,0,0,0], [1,1,1,1,0.49]],"), false);
			foreach (var property in Events.NoodleProperties) {
				AddCheckbox("  "+property.Key, Data.CustomGetSetNode(CustomKeyAnimation+"."+ property.Value[v2 ? 0 : 1], property.Value[2]), false);
			}
			AddCheckbox("  Definite Position", Data.CustomGetSetNode(CustomKeyAnimation+"."+ (v2 ? "_definitePosition" : "definitePosition"), "[[0,0,0,0], [0,0,0,0.49]]"), true);
			AddLine("");
		}
	}
	
	private void UpdateObjects<T>(System.Action<BaseObject, T?> setter, T? value) where T : struct {
		var beatmapActions = new List<BeatmapObjectModifiedAction>();
		foreach (var o in editing) {
			var clone = BeatmapFactory.Clone(o);
			
			setter(o, value);
			
			beatmapActions.Add(new BeatmapObjectModifiedAction(o, o, clone, $"Edited a {o.ObjectType} with Prop Edit.", true));
		}
		
		BeatmapActionContainer.AddAction(
			new ActionCollectionAction(beatmapActions, true, false, $"Edited ({editing.Count()}) objects with Prop Edit."),
			true);
	}
	
	// I hate c#
	private void UpdateObjects(System.Action<BaseObject, string> setter, string value) {
		var beatmapActions = new List<BeatmapObjectModifiedAction>();
		foreach (var o in editing) {
			var clone = BeatmapFactory.Clone(o);
			
			setter(o, value);
			o.RefreshCustom();
			
			beatmapActions.Add(new BeatmapObjectModifiedAction(o, o, clone, $"Edited a {o.ObjectType} with Prop Edit.", true));
		}
		
		BeatmapActionContainer.AddAction(
			new ActionCollectionAction(beatmapActions, true, false, $"Edited ({editing.Count()}) objects with Prop Edit."),
			true);
	}
}

}
