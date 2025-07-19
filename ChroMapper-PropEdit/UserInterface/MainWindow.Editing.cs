using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Enums;
using SimpleJSON;

using ChroMapper_PropEdit.Components;
using ChroMapper_PropEdit.Enums;
using ChroMapper_PropEdit.Utils;

using Convert = System.Convert;
using System.Security;
using UnityEngine.Assertions.Must;
using static ChroMapper_PropEdit.UserInterface.TooltipStrings;

namespace ChroMapper_PropEdit.UserInterface {

public partial class MainWindow : UIWindow {
	public readonly string CHROMA_NAME = "Chroma";
	public readonly string NOODLE_NAME = "Noodle Extensions";
	TooltipStrings tooltip = TooltipStrings.Instance;
	BundleInfo? bundleInfo = null;
	
	private ObjectType? old_otype = null;
	private SelectionType? old_stype = null;
	private Events.EventType? old_etype = null;
	private string? old_cetype = null;
	private bool full_rebuild = true;
	private bool refresh_frame = false;
	
	private void wipe(int skip = 0) {
		Plugin.Trace($"Wipe after {skip}");
		foreach (Transform child in panel!.transform) {
			while (panel!.transform.childCount > skip) {
				Plugin.Trace($"Delete {panel!.transform.GetChild(skip).gameObject.name}");
				GameObject.DestroyImmediate(panel!.transform.GetChild(skip).gameObject);
			}
		}
	}
	
	public void TriggerRefresh() {
		refresh_frame = true;
	}
	public void TriggerFullRefresh() {
		refresh_frame = true;
		old_otype = null;
	}
	
	private bool CheckRefresh(SelectionType new_type, bool force = false) {
		full_rebuild = false;
		
		Plugin.Trace($"{old_stype} => {new_type}");
		
		if (force || new_type != old_stype) {
			wipe();
			full_rebuild = true;
		}
		old_stype = new_type;
		
		return full_rebuild;
	}
	
	private void Update() {
		if (!refresh_frame) return;
		refresh_frame = false;
		
		editing = Selection.Selected;
		switch (editing) {
		case List<BaseObject> objects: {
			window!.SetTitle($"{objects.Count} Items selected");
			
			if (objects.GroupBy(o => o.ObjectType).Count() > 1) {
				wipe();
				UI.AddLabel(panel!, "Unsupported", "Multi-Type Unsupported!", Vector2.zero);
				old_otype = null;
				return;
			}
			
			var o = objects.First();
			var type = o.ObjectType;
			var v2 = BeatSaberSongContainer.Instance.Map.Version[0] == '2';
			
			if (CheckRefresh(SelectionType.Objects, type != old_otype)) {
				old_etype = null;
			}
			Plugin.Trace($"{old_otype} => {type}: {full_rebuild}");
			
			old_otype = type;
			old_stype = SelectionType.Objects;
			
			panels.Clear();
			panels.Push(panel!);
			
			AddParsed("Beat", Data.GetSet<float>("JsonTime"), true, (o is BaseGrid)
				? tooltip.GetTooltip(PropertyType.Object, TooltipStrings.Tooltip.Beat)
				: tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.BeatEvent));
			
			switch (type) {
				case ObjectType.Note:
					var note = (o as BaseNote)!;
					AddParsed("X", Data.GetSet<int>("PosX"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.X));
					AddParsed("Y", Data.GetSet<int>("PosY"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Y));
					AddDropdown<int?>("Type", Data.GetSet<int>("Type"), Notes.NoteTypes, false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Type));
					AddDropdown<int?>("Direction", Data.GetSet<int>("CutDirection"), Notes.CutDirections, false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.CutDirection));
					if (!v2) {
						AddParsed("Angle Offset", Data.GetSet<int>("AngleOffset"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.AngleOffset));
					}
					AddLine("");
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						AddExpando(CHROMA_NAME, "Chroma", true);
						AddColor("Color", o.CustomKeyColor, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Color));
						if (v2) {
							AddCheckbox("Disable Spawn Effect", Data.CustomGetSet<bool?>("_disableSpawnEffect"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.DisableSpawnEffect));
						}
						else {
							AddCheckbox("Spawn Effect", Data.CustomGetSet<bool?>("spawnEffect"), true, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.SpawnEffect));
							AddCheckbox("Disable Debris", Data.CustomGetSet<bool?>("disableDebris"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.DisableDebris));
						}
						panels.Pop();
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						AddExpando(NOODLE_NAME, "Noodle Extensions", true);
						AddParsed("NJS", Data.CustomGetSet<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.NJS));
						AddParsed("Spawn Offset", Data.CustomGetSet<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.SpawnOffset));
						AddTextbox("Coordinates", Data.CustomGetSetRaw(note.CustomKeyCoordinate), true, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Coordinates));
						AddTextbox("Rotation", Data.CustomGetSetRaw(note.CustomKeyWorldRotation), true, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Rotation));
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(note.CustomKeyLocalRotation), true, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.LocalRotation));
						if (v2) {
							AddParsed("Exact Angle", Data.CustomGetSet<float?>("_cutDirection"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.CutDirection));
							AddCheckbox("Fake", Data.CustomGetSet<bool?>("_fake"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Fake));
							AddCheckbox("Interactable", Data.CustomGetSet<bool?>("_interactable"), true, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Interactable));
							AddTextbox("Flip", Data.CustomGetSetRaw("_flip"), true, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Flip));
						}
						else {
							AddCheckbox("Fake", Data.GetSet<bool>("CustomFake"), null, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Fake));
							AddCheckbox("Uninteractable", Data.CustomGetSet<bool?>("uninteractable"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Uninteractable));
							AddCheckbox("Disable Gravity", Data.CustomGetSet<bool?>("disableNoteGravity"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.DisableGravity));
							AddCheckbox("Disable Look", Data.CustomGetSet<bool?>("disableNoteLook"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.DisableLook));
							AddCheckbox("No Badcut Direction", Data.CustomGetSet<bool?>("disableBadCutDirection"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.NoBadcutDirection));
							AddCheckbox("No Badcut Speed", Data.CustomGetSet<bool?>("disableBadCutSpeed"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.NoBadcutSpeed));
							AddCheckbox("No Badcut Color", Data.CustomGetSet<bool?>("disableBadCutSaberType"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.NoBadcutColor));
							AddTextbox("Flip", Data.CustomGetSetRaw("flip"), true, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Flip));
							AddTextbox("Link", Data.CustomGetSet<string>("link"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Link));
						}
						AddTracks("Tracks", Data.CustomGetSetRaw(o.CustomKeyTrack), tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Track)); //prob. needs more info
						AddAnimations(PropertyType.Note, v2);
						panels.Pop();
					}
					
					break;
				case ObjectType.CustomNote:
					AddLine("Wow, a custom note! How did you do this?");
					break;
				case ObjectType.Arc: {
					AddParsed("Head X", Data.GetSet<int>("PosX"), false, tooltip.GetTooltip(PropertyType.ArcHead, TooltipStrings.Tooltip.X));
					AddParsed("Head Y", Data.GetSet<int>("PosY"), false, tooltip.GetTooltip(PropertyType.ArcHead, TooltipStrings.Tooltip.Y));
					AddDropdown<int?>("Color", Data.GetSet<int>("Color"), Notes.ArcColors, false,tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Type));
					AddDropdown<int?>("Direction", Data.GetSet<int>("CutDirection"), Notes.CutDirections, false, tooltip.GetTooltip(PropertyType.ArcHead, TooltipStrings.Tooltip.CutDirection));
					AddParsed("Head Multiplier", Data.GetSet<float>("HeadControlPointLengthMultiplier"), false, tooltip.GetTooltip(PropertyType.ArcHead, TooltipStrings.Tooltip.Multiplier));
					AddParsed("Tail Beat", Data.GetSet<float>("TailJsonTime"), false, tooltip.GetTooltip(PropertyType.ArcTail, TooltipStrings.Tooltip.Beat)); 
					AddParsed("Tail X", Data.GetSet<int>("TailPosX"), false, tooltip.GetTooltip(PropertyType.ArcTail, TooltipStrings.Tooltip.X));
					AddParsed("Tail Y", Data.GetSet<int>("TailPosY"), false, tooltip.GetTooltip(PropertyType.ArcTail, TooltipStrings.Tooltip.Y));
					AddDropdown<int?>("Tail Direction", Data.GetSet<int>("TailCutDirection"), Notes.CutDirections, false, tooltip.GetTooltip(PropertyType.ArcTail, TooltipStrings.Tooltip.CutDirection));
					AddParsed("Tail Multiplier", Data.GetSet<float>("TailControlPointLengthMultiplier"), false, tooltip.GetTooltip(PropertyType.ArcTail, TooltipStrings.Tooltip.Multiplier));
					AddLine("");
					
					var s = (o as BaseSlider)!;
					
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						AddExpando(CHROMA_NAME, "Chroma", true);
						AddColor("Color", o.CustomKeyColor, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Color));
						panels.Pop();
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						AddExpando(NOODLE_NAME, "Noodle Extensions", true);
						AddParsed("NJS", Data.CustomGetSet<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"), false, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.NJS));
						AddParsed("Spawn Offset", Data.CustomGetSet<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"), false, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.SpawnOffset));
						AddTextbox("Head Coordinates", Data.CustomGetSetRaw(s.CustomKeyCoordinate), true, tooltip.GetTooltip(PropertyType.ArcHead, TooltipStrings.Tooltip.Coordinates));
						AddTextbox("Tail Coordinates", Data.CustomGetSetRaw(s.CustomKeyCoordinate), true, tooltip.GetTooltip(PropertyType.ArcTail, TooltipStrings.Tooltip.Coordinates));
						AddTextbox("Rotation", Data.CustomGetSetRaw(s.CustomKeyWorldRotation), true, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Rotation));
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(s.CustomKeyLocalRotation), true, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.LocalRotation));
						if (v2) {
							AddCheckbox("Interactable", Data.CustomGetSet<bool?>("_interactable"), true, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Interactable));
							AddTextbox("Flip", Data.CustomGetSetRaw("_flip"), true, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Flip)); //not sure if this works
						}
						else {
							AddCheckbox("Uninteractable", Data.CustomGetSet<bool?>("uninteractable"), false, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Uninteractable));
							AddCheckbox("Disable Gravity", Data.CustomGetSet<bool?>("disableNoteGravity"), false, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.DisableGravity));
							AddTextbox("Flip", Data.CustomGetSetRaw("flip"), true, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Flip));
							AddTextbox("Link", Data.CustomGetSet<string>("link"), false, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Link));
						}
						AddTracks("Tracks", Data.CustomGetSetRaw(o.CustomKeyTrack), tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Track));
						AddAnimations(PropertyType.Arc, v2);
						panels.Pop();
					}
					
				}	break;
				case ObjectType.Chain: {
					AddParsed("Head X", Data.GetSet<int>("PosX"), false, tooltip.GetTooltip(PropertyType.ChainHead, TooltipStrings.Tooltip.X));
					AddParsed("Head Y", Data.GetSet<int>("PosY"), false, tooltip.GetTooltip(PropertyType.ChainHead, TooltipStrings.Tooltip.Y));
					AddDropdown<int?>("Color", Data.GetSet<int>("Color"), Notes.ArcColors, false, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Color));
					AddDropdown<int?>("Direction", Data.GetSet<int>("CutDirection"), Notes.CutDirections, false, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.CutDirection));
					AddParsed("Slices", Data.GetSet<int>("SliceCount"), false, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Slices));
					AddParsed("Squish", Data.GetSet<float>("Squish"), false, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Squish));
					AddParsed("Tail X", Data.GetSet<int>("TailPosX"), false, tooltip.GetTooltip(PropertyType.ChainTail, TooltipStrings.Tooltip.X));
					AddParsed("Tail Y", Data.GetSet<int>("TailPosY"), false, tooltip.GetTooltip(PropertyType.ChainTail, TooltipStrings.Tooltip.Y));
					AddLine("");
					
					var s = (o as BaseSlider)!;
					
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						AddExpando(CHROMA_NAME, "Chroma", true);
						AddColor("Color", o.CustomKeyColor, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Color));
						panels.Pop();
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						AddExpando(NOODLE_NAME, "Noodle Extensions", true);
						AddParsed("NJS", Data.CustomGetSet<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"), false, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.NJS));
						AddParsed("Spawn Offset", Data.CustomGetSet<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"), false, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.SpawnOffset));
						AddTextbox("Head Coordinates", Data.CustomGetSetRaw(s.CustomKeyCoordinate), true, tooltip.GetTooltip(PropertyType.ChainHead, TooltipStrings.Tooltip.Coordinates));
						AddTextbox("Tail Coordinates", Data.CustomGetSetRaw(s.CustomKeyCoordinate), true, tooltip.GetTooltip(PropertyType.ChainTail, TooltipStrings.Tooltip.Coordinates));
						AddTextbox("Rotation", Data.CustomGetSetRaw(s.CustomKeyWorldRotation), true, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Rotation));
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(s.CustomKeyLocalRotation), true, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.LocalRotation));
						AddCheckbox("Fake", Data.GetSet<bool>("CustomFake"), null, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Fake));
						AddCheckbox(v2 ? "Interactable" : "Uninteractable", Data.CustomGetSet<bool?>(v2 ? "_interactable" : "uninteractable"), v2, tooltip.GetTooltip(PropertyType.Chain, (v2 ? TooltipStrings.Tooltip.Interactable : TooltipStrings.Tooltip.Uninteractable)));
						if (v2) {
							AddTextbox("Flip", Data.CustomGetSetRaw("_flip"), true, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Flip));
						}
						else {
							AddCheckbox("Disable Gravity", Data.CustomGetSet<bool?>("disableNoteGravity"), false, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Flip));
							AddTextbox("Flip", Data.CustomGetSetRaw("flip"), true, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Flip));
							AddTextbox("Link", Data.CustomGetSet<string>("link"), false, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Link));
						}
						AddTracks("Tracks", Data.CustomGetSetRaw(o.CustomKeyTrack), tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Track));
						AddAnimations(PropertyType.Chain, v2);
						panels.Pop();
					}
					
				}	break;
				case ObjectType.Obstacle:
					var ob = (o as BaseObstacle)!;
					AddParsed("Duration", Data.GetSet<float>("Duration"), false, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Duration));
					if (v2) {
						AddParsed("X", Data.GetSet<int>("PosX"), false, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.X));
						AddParsed("Width", Data.GetSet<int>("Width"), false, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Width));
						AddDropdown<int?>("Height", Data.GetSet<int>("Type"), Obstacles.WallHeights, false, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Width));
					}
					else {
						AddParsed("X (Left)", Data.GetSet<int>("PosX"), false, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.X));
						AddParsed("Y (Bottom)", Data.GetSet<int>("PosY"), false, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Y));
						AddParsed("Width", Data.GetSet<int>("Width"), false, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Width));
						AddParsed("Height", Data.GetSet<int>("Height"), false, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Height));
					}
					AddLine("");
					
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						AddExpando(CHROMA_NAME, "Chroma", true);
						AddColor("Color", o.CustomKeyColor, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Color));
						panels.Pop();
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						AddExpando(NOODLE_NAME, "Noodle Extensions", true);
						AddParsed("NJS", Data.CustomGetSet<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"), false, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.NJS));
						AddParsed("Spawn Offset", Data.CustomGetSet<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"), false, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.SpawnOffset));
						AddTextbox("Coordinates", Data.CustomGetSetRaw(ob.CustomKeyCoordinate), true, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Coordinates));
						AddTextbox("Rotation", Data.CustomGetSetRaw(ob.CustomKeyWorldRotation), true, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Rotation));
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(ob.CustomKeyLocalRotation), true, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.LocalRotation));
						AddTextbox("Size", Data.CustomGetSetRaw(ob.CustomKeySize), true, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Size));
						AddCheckbox("Fake", Data.GetSet<bool>("CustomFake"), null, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Fake));
						if (v2) {
							AddCheckbox("Interactable", Data.CustomGetSet<bool?>("_interactable"), true, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Interactable));
						}
						else {
							AddCheckbox("Uninteractable", Data.CustomGetSet<bool?>("uninteractable"), false, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Uninteractable)); // not sure if this means that it will screw up your score
						}
						AddTracks("Tracks", Data.CustomGetSetRaw(o.CustomKeyTrack), tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Track));
						AddAnimations(PropertyType.Obstacle, v2);
						panels.Pop();
					}
					
					break;
				case ObjectType.Event: {
#if CHROMPER_11
					var env = BeatSaberSongContainer.Instance.Song.EnvironmentName;
#else
					var env = BeatSaberSongContainer.Instance.Info.EnvironmentName;
#endif
					var events = objects.Select(o => (BaseEvent)o);
					
					if (events.GroupBy(e => Events.GetEventType(e, env)).Count() > 1) {
						wipe(1);
						old_etype = null;
						break;
					}
					
					var f = events.First();
					var new_etype = Events.GetEventType(f, env);
					
					if (new_etype != old_etype) {
						wipe(1);
						full_rebuild = true;
					}
					Plugin.Trace($"{old_etype} => {new_etype}: {full_rebuild}");
					old_etype = new_etype;
					
					switch (new_etype) {
					case Events.EventType.Light:
						if (Settings.Get(Settings.SplitValue, true)!.AsBool) {
							AddDropdown<int?>("Color", Data.GetSetSplitValue(0b1100), Events.LightColors, false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.EventColor));
							AddDropdown<int?>("Action", Data.GetSetSplitValue(0b0011), Events.LightActions, false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.EventAction));
						}
						else {
							AddDropdown<int?>("Value", Data.GetSet<int>("Value"), Events.LightValues, false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LegacyEventType));
						}
						AddParsed("Brightness", Data.GetSet<float>("FloatValue"), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.Brightness));
						AddLine("");
						
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							AddExpando(CHROMA_NAME, "Chroma", true);
							AddTextbox("LightID", Data.CustomGetSetRaw(f.CustomKeyLightID), true, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LightID));
							AddColor("Color", o.CustomKeyColor, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.Color));
							AddDropdown<string>("Easing",    Data.CustomGetSet<string>(f.CustomKeyEasing), Events.Easings, true, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.Easing));
							AddDropdown<string>("Lerp Type", Data.CustomGetSet<string>(f.CustomKeyLerpType), Events.LerpTypes, true, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LerpType)); //Unsure
							if (o is BaseEvent e && v2) {
								AddCheckbox("V2 Gradient", Data.GetSetGradient(), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.V2Gradient));
								if (e.CustomLightGradient != null) {
									AddParsed("Duration",     Data.CustomGetSet<float?>($"{e.CustomKeyLightGradient}._duration"), false, tooltip.GetTooltip(PropertyType.Gradient, TooltipStrings.Tooltip.Duration));
									AddColor("Start Color", $"{e.CustomKeyLightGradient}._startColor", tooltip.GetTooltip(PropertyType.GradientStart, TooltipStrings.Tooltip.Color));
									AddColor("End Color", $"{e.CustomKeyLightGradient}._endColor", tooltip.GetTooltip(PropertyType.GradientEnd, TooltipStrings.Tooltip.Color));
									AddDropdown<string>("Easing",    Data.CustomGetSet<string>($"{e.CustomKeyLightGradient}._easing"), Events.Easings, false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.V2Easing));
								}
							}
							panels.Pop();
						}
						break;
					case Events.EventType.LaserRotation:
						AddParsed("Speed", Data.GetSet<int>("Value"), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LaserSpeed));
						AddLine("");
						
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							AddExpando(CHROMA_NAME, "Chroma", true);
							AddCheckbox("Lock Rotation", Data.CustomGetSet<bool?> (f.CustomKeyLockRotation), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LockRotation));
							AddDropdown<int?>("Direction",     Data.CustomGetSet<int?>  (f.CustomKeyDirection), Events.LaserDirection, true, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LaserDirection));
							AddParsed("Precise Speed",   Data.CustomGetSet<float?>(f.CustomKeySpeed), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.PreciseSpeed));
							panels.Pop();
						}
						break;
					case Events.EventType.RingRotation:
						AddLine("");
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							AddExpando(CHROMA_NAME, "Chroma", true);
							AddTextbox("Filter",     Data.CustomGetSet<string>(f.CustomKeyNameFilter), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingFilter));
							if (v2) {
								AddCheckbox("Reset", Data.CustomGetSet<bool?>("_reset"), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingV2Reset));
							}
							AddParsed("Rotation",    Data.CustomGetSet<float?>(f.CustomKeyLaneRotation), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingRotation));
							AddParsed("Step",        Data.CustomGetSet<float?>(f.CustomKeyStep), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingStep));
							AddParsed("Propagation", Data.CustomGetSet<float?>(f.CustomKeyProp), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingPropagation));
							AddParsed("Speed",       Data.CustomGetSet<float?>(f.CustomKeySpeed), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingSpeed));
							AddDropdown<int?>("Direction", Data.CustomGetSet<int?>  (f.CustomKeyDirection), Events.RingDirection, true, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingDirection));
							if (v2) {
								AddCheckbox("Counter Spin", Data.CustomGetSet<bool?>("_counterSpin"), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingV2CounterSpin));
							}
							panels.Pop();
						}
						break;
					case Events.EventType.RingZoom:
						AddLine("");
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							AddExpando(CHROMA_NAME, "Chroma", true);
							AddParsed("Step",  Data.CustomGetSet<float?>(f.CustomKeyStep), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingZoomStep));
							AddParsed("Speed", Data.CustomGetSet<float?>(f.CustomKeySpeed), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingSpeed));
							panels.Pop();
						}
						break;
					case Events.EventType.ColorBoost:
						AddDropdown<int?>("Color Set", Data.GetSet<int>("Value"), Events.BoostSets, false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.BoostColorSet));
						break;
					case Events.EventType.LaneRotation:
						AddDropdown<int?>("Rotation", Data.GetSet<int>("Value"), Events.LaneRotaions, false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LaneRotation));
						break;
						
					case Events.EventType.Utility:
						AddParsed("Value", Data.GetSet<int>("Value"), false);
						break;
					}
				}	break;
				case ObjectType.CustomEvent: {
					var events = objects.Select(o => (BaseCustomEvent)o);
					var f = events.First();
					
					var types = events.Select(e => e.Type)
						.Distinct();
					
					if (types.Count() > 1) {
						wipe(1);
						old_cetype = null;
						break;
					}
					if (types.First() != old_cetype) {
						wipe(1);
						full_rebuild = true;
					}
					Plugin.Trace($"{old_cetype} => {types.First()}: {full_rebuild}");
					old_cetype = types.First();
					
					switch (types.First()) {
					// Heck
					case "AnimateTrack":
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "repeat"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackRepeat));
						}
						AddTracks("Tracks", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.Track));
						AddExpando("Properties", "Point Definitions", true);
						foreach (var property in Events.NoodleProperties) {
							AddPointDefinition(property.Key, Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", property.Value[v2 ? 0 : 1]), tooltip.GetTooltip(PropertyType.CustomEvent, $"Animate{property.Key}"));
						}
						AddPointDefinition("Time", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_time" : "time"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AnimateTime));
						panels.Pop();
						break;
					
					case "AssignPathAnimation":
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "repeat"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackRepeat));
						}
						AddTracks("Tracks", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.Track));
						AddExpando("Properties", "Point Definitions", true);
						foreach (var property in Events.NoodleProperties) {
							AddPointDefinition(property.Key, Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", property.Value[v2 ? 0 : 1]), tooltip.GetTooltip(PropertyType.CustomEvent, $"Animate{property.Key}"));
						}
						AddPointDefinition("Definite Position", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_definitePosition" : "definitePosition"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AssignPathAnimationDefinitePosition));
						panels.Pop();
						break;
					
					// Noodle
					case "AssignTrackParent":
						AddTrack("Parent", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_parentTrack" : "parentTrack"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AssignTrackParentParent));
						AddTracks("Children", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_childrenTracks" : "childrenTracks"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AssignTrackChildren));
						AddCheckbox("Keep Position", Data.JSONGetSet<bool?>(typeof(BaseCustomEvent), "Data", v2 ? "_worldPositionStays" : "worldPositionStays"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AssignTrackKeepPosition));
						break;
					
					case "AssignPlayerToTrack":
						AddTextbox("Track", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AssignPlayerToTrackTrack));
						AddDropdown("Target", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_target" : "target"), Events.PlayerTargets, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AssignPlayerToTrackTarget));
						break;
					
					// Chroma
					case "AssignFogTrack":
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "repeat"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackRepeat));
						}
						AddTracks("Tracks", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.Track));
						AddExpando("Properties", "Properties", true);
						AddParsed("Attenuation", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "_attenuation"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.V2AssignFogTrackAttenuation));
						AddParsed("Offset", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "_offset"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.V2AssignFogTrackOffset));
						AddParsed("Start Y", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "_startY"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.V2AssignFogTrackStartY));
						AddParsed("Height", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "_height"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.V2AssignFogTrackHeight));
						panels.Pop();
						break;
					
					case "AnimateComponent":
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "repeat"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackRepeat));
						}
						AddTracks("Tracks", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.Track));
						//it seems these are only normal json inputs. might have to change the tooltip then.
						AddTextbox("Environment Fog", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "BloomFogEnvironment"), true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AnimateComponentBloomFogEnvironment));
						AddTextbox("Tube Bloom Light", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "TubeBloomPrePassLight"), true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AnimateComponentTubeBloomPrePassLight));
						break;
					
					// Vivify
					case "SetMaterialProperty":
						AddMaterial();
						goto case "SetGlobalProperty";
					case "SetGlobalProperty":
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "duration"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						AddCustomProperties();
						break;
					case "Blit":
						AddMaterial();
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "duration"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						AddParsed("Priority", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "priority"));
						AddParsed("Pass", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "pass"));
						AddDropdown("Order", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "order"), Vivify.Orders, true);
						AddTextbox("Source Texture", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "source"), false);
						AddTextbox("Destination Texture", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "destination"), false);
						AddMaterialProperties();
						break;
					case "CreateCamera":
						AddTextbox("Camera ID", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "id"), false);
						AddTextbox("Texture", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "texture"), false);
						AddTextbox("Depth Texture", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "depthTexture"), false);
						AddCameraProperties();
						break;
					case "CreateScreenTexture":
						AddTextbox("Name", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "id"), false);
						AddParsed("X Ratio", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "xRatio"));
						AddParsed("Y Ratio", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "yRatio"));
						AddParsed("Width", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "width"));
						AddParsed("Height", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "height"));
						AddDropdown("Color Format", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "colorFormat"), Vivify.ColorFormats, true);
						AddDropdown("Filter Mode", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "filterMode"), Vivify.FilterModes, true);
						break;
					case "InstantiatePrefab":
						AddPrefab("Prefab", "asset", false);
						AddTextbox("ID", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "id"), false);
						AddTrack("Track", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "track"));
						AddTextbox("Position", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "position"), true);
						AddTextbox("Local Position", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "localPosition"), true);
						AddTextbox("Rotation", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "rotation"), true);
						AddTextbox("Local Rotation", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "localRotation"), true);
						AddTextbox("Scale", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "scale"), true);
						break;
					case "DestroyObject":
						AddTextbox("ID(s)", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "id"), true);
						// TODO: Array view?
						break;
					case "SetAnimatorProperty":
						AddTextbox("ID", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "id"), false);
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "duration"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						AddCustomProperties();
						break;
					case "SetCameraProperty":
						AddTextbox("Camera ID", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "id"), false);
						AddCameraProperties();
						break;
					case "AssignObjectPrefab":
						AddDropdown("Load Mode", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "loadMode"), Vivify.LoadModes, true);
						AddObjectProperties();
						break;
					case "SetRenderingSettings":
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "duration"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						{
							var (has_any, mixed) = Data.GetAllOrNothing<bool>(editing!, (o) => (o as BaseCustomEvent)?.Data?.HasKey("renderSettings") ?? false);
							AddExpando("_Render Settings", "Render Settings", has_any || mixed);
							foreach (var prop in Vivify.RenderSettings) {
								AddTextbox(prop.Value, Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", prop.Key), true);
							}
							panels.Pop();
						}
						{
							var (has_any, mixed) = Data.GetAllOrNothing<bool>(editing!, (o) => (o as BaseCustomEvent)?.Data?.HasKey("qualitySettings") ?? false);
							AddExpando("_Quality Settings", "Quality Settings", has_any || mixed);
							foreach (var prop in Vivify.QualitySettings) {
								AddTextbox(prop.Value, Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", prop.Key), true);
							}
							panels.Pop();
						}
						{
							var (has_any, mixed) = Data.GetAllOrNothing<bool>(editing!, (o) => (o as BaseCustomEvent)?.Data?.HasKey("xrSettings") ?? false);
							AddExpando("_XR Settings", "XR Settings", has_any || mixed);
							foreach (var prop in Vivify.XRSettings) {
								AddTextbox(prop.Value, Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", prop.Key), true);
							}
							panels.Pop();
						}
						break;
					}
				}	break;
				case ObjectType.BpmChange:
					AddParsed("BPM", Data.GetSet<float>("Bpm"), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.BPMChange));
					break;
#if !CHROMPER_11
				case ObjectType.NJSEvent: {
					Data.Getter<bool?> getter = (o) => ((BaseNJSEvent)o).UsePrevious == 1;
					Data.Setter<bool?> setter = (o, v) => { ((BaseNJSEvent)o).UsePrevious = (v ?? false) ? 1 : 0; };
					AddCheckbox("Use Previous", (getter, setter), null);
					AddParsed("Relative NJS", Data.GetSet<float>("RelativeNJS"));
					AddDropdown("Easing", Data.GetSet<int>("Easing"), Events.NJSEasings, false);
					break;
				}
#endif
			}
			UI.RefreshTooltips(panel);
			if (full_rebuild) {
				scrollbox!.ScrollTop();
			}
		}	break;
		case List<BaseEnvironmentEnhancement> ees: {
			window!.SetTitle($"{ees.Count} Items selected");
			
			CheckRefresh(SelectionType.EnvironmentEnhancements);
			
			AddTextbox("ID", Data.GetSetNullable<string>("ID"), true);
			AddDropdown("Lookup Method", Data.GetSetNullable<int?>("LookupMethod"), (new Map<int?>()).AddEnum(typeof(EnvironmentLookupMethod)), false);
			Data.Getter<bool?> dup_get = (ee) => (ee as BaseEnvironmentEnhancement)!.Active?.AsBool ?? null;
			Data.Setter<bool?> dup_set = (ee, v) => (ee as BaseEnvironmentEnhancement)!.Active = v;
			AddCheckbox("Active", (dup_get, dup_set), true);
			AddParsed<int>("Duplicate", Data.GetSetNullable<int?>("Duplicate"));
			EditVector3("Scale", Data.GetSetNullable<Vector3?>("Scale"));
			EditVector3("Position", Data.GetSetNullable<Vector3?>("Position"));
			EditVector3("Local Position", Data.GetSetNullable<Vector3?>("LocalPosition"));
			EditVector3("Rotation", Data.GetSetNullable<Vector3?>("Rotation"));
			EditVector3("Local Rotation", Data.GetSetNullable<Vector3?>("LocalRotation"));
			AddTextbox("Track", Data.GetSetNullable<string?>("Track"));
			
			Data.Getter<bool?> geo_get = (ee) => (ee as BaseEnvironmentEnhancement)!.Geometry != null;
			Data.Setter<bool?> geo_set = (ee, v) => {
				if (v == false) {
					(ee as BaseEnvironmentEnhancement)!.Geometry = null;
				}
				else {
					(ee as BaseEnvironmentEnhancement)!.Geometry ??= new JSONObject();
				}
			};
			EEComponent("Geometry", (geo_get, geo_set), () => {
				AddDropdown("Type", Data.JSONGetSet<string>(typeof(BaseEnvironmentEnhancement), "Geometry", "type"), MapSettings.GeometryTypes, true);
				var materials = new Map<string?> {
					//{ "[Create New]", "[Create New]" }
				};
				materials.AddRange(BeatSaberSongContainer.Instance.Map.Materials.Keys);
				AddDropdown("Material", Data.JSONGetSet<string>(typeof(BaseEnvironmentEnhancement), "Geometry", "material"), materials, true);
				AddCheckbox("Collision", Data.JSONGetSet<bool?>(typeof(BaseEnvironmentEnhancement), "Geometry", "collision"), false);
			});
			
			EEComponent("Light", Data.EEGetSetComp("ILightWithId"), () => {
				AddParsed("Light ID", Data.GetSetNullable<int?>("LightID"));
				AddParsed("Light Type", Data.GetSetNullable<int?>("LightType"));
			});
			
			EEComponent("Bloom Fog", Data.EEGetSetComp("BloomFogEnvironment"), () => {
				AddParsed("Attenuation", Data.JSONGetSet<float?>(typeof(BaseEnvironmentEnhancement), "Components", "BloomFogEnvironment.attenuation"));
				AddParsed("Offset", Data.JSONGetSet<float?>(typeof(BaseEnvironmentEnhancement), "Components", "BloomFogEnvironment.offset"));
				AddParsed("Start Y", Data.JSONGetSet<float?>(typeof(BaseEnvironmentEnhancement), "Components", "BloomFogEnvironment.startY"));
				AddParsed("Height", Data.JSONGetSet<float?>(typeof(BaseEnvironmentEnhancement), "Components", "BloomFogEnvironment.height"));
			});
			
			EEComponent("Tube Bloom Pre Pass Light", Data.EEGetSetComp("TubeBloomPrePassLight"), () => {
				AddParsed("Color Alpha Multiplier", Data.JSONGetSet<float?>(typeof(BaseEnvironmentEnhancement), "Components", "TubeBloomPrePassLight.colorAlphaMultiplier"));
				AddParsed("Bloom Fog Intensity Multiplier", Data.JSONGetSet<float?>(typeof(BaseEnvironmentEnhancement), "Components", "TubeBloomPrePassLight.bloomFogIntensityMultiplier"));
			});
		}	break;
		case List<BaseMaterial> mats: {
			window!.SetTitle($"{mats.Count} Items selected");
			
			CheckRefresh(SelectionType.Materials);
			
			System.Func<Color?, string?> gc = (c) => 
				(new JSONArray())
					.WriteColor(c ?? Color.white);
			System.Func<string?, Color?> sc = (s) => JSON.Parse(s).ReadColor();
			AddTextbox("Color", Data.Add(Data.GetSetNullable<Color?>("Color"), (gc, sc)), true);
			AddDropdown("Shader", Data.GetSetNullable<string>("Shader"), MapSettings.Shaders, false);
			AddTextbox("Track", Data.GetSetNullable<string?>("Track"), false, "Assign the material to a track, allowing you to animate the color.");
			
			var (getter, setter) = Data.GetSetNullable<List<string>>("ShaderKeywords");
			
			var (value, mixed) = Data.GetAllOrNothing<List<string>>(editing!, getter);
			
			Plugin.Trace($"{value?.Count ?? -1} {mixed}");
			
			ArrayEditor.Getter arr_get = () => {
				if (mixed) return null;
				var arr = new JSONArray();
				foreach (var keyword in value!) {
					arr.Add(keyword);
				}
				return arr;
			};
			
			ArrayEditor.Setter arr_set = (JSONArray node) => {
				var arr = new List<string>();
				foreach (var keyword in node) {
					arr.Add(keyword.Value);
				}
				Data.UpdateObjects<List<string>>(editing!, setter, arr);
				// Don't have real actions yet
				UpdateFromAction(null);
			};
			
			ArrayEditor
				.Singleton(current_panel!, "Shader Keywords", "By default, each shader has its default keywords. This allows overwriting the keywords of the shader.")
				.Set((arr_get, arr_set), false);
			
		}	break;
		default:
			window!.SetTitle("No items selected");
			old_otype = null;
			old_stype = null;
			wipe();
			break;
		}
		Plugin.Trace($"End UpdateSelection: {old_otype}");
	}
	
	private void AddAnimations(PropertyType type, bool v2) {
		var CustomKeyAnimation = v2 ? "_animation" : "animation";
		
		AddExpando("Animations", "Animations", true);
		foreach (var property in Events.NoodleProperties) {
			if (property.Value[v2 ? 0 : 1] == "") continue;
			AddAnimation(property.Key, CustomKeyAnimation+"."+ property.Value[v2 ? 0 : 1], property.Value[2], tooltip.GetTooltip(type, $"Animate{property.Key}"));
		}
		AddAnimation("Definite Position", CustomKeyAnimation+"."+ (v2 ? "_definitePosition" : "definitePosition"), "[[0,0,0,0], [0,0,0,0.49]]", tooltip.GetTooltip(type, TooltipStrings.Tooltip.AssignPathAnimationDefinitePosition));
		panels.Pop();
	}
	
	private void AddAnimation(string name, string path, string default_json, string tooltip) {
		var (_, default_set) = Data.CustomGetSetNode(path, default_json);
		var (getter, setter) = Data.CustomGetSetRaw(path);
		var (value, mixed) = Data.GetAllOrNothing(editing!, getter);
		
		PointDefinitionEditor
				.Singleton(
				current_panel!,
				name,
				tooltip)
			.Set(
				value,
				mixed,
				(v) => {
					if (v == "") {
						v = null;
					}
					if (v != value) {
						Data.UpdateObjects<string?>((editing as List<BaseObject>)!, setter, v);
					}
				},
				(v) => {
					Data.UpdateObjects<bool?>((editing as List<BaseObject>)!, default_set, v);
				});
		return;
	}
	
	private void AddColor(string label, string key, string tooltip = "") {
		if (Settings.Get(Settings.ColorHex, true)) {
			AddTextbox(label, Data.CustomGetSetColor(key), false, tooltip);
		}
		else {
			AddTextbox(label, Data.CustomGetSetRaw(key), true, tooltip);
		}
	}
	
	// Unarrayable track
	private void AddTrack(string? title, (Data.Getter<string?>, Data.Setter<string?>) get_set, string tooltip = "") {
		// TODO: Needs a custom option with a textbox
		/*
		var collection = BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.CustomEvent) as CustomEventGridContainer;
		var tracks = new Map<string?>().AddRange(collection!.EventsByTrack.Keys);
		AddDropdown(title, get_set, tracks, true, tooltip);
		*/
		AddTextbox(title, get_set, false, tooltip);
	}
	
	// Arrayable tracks
	private void AddTracks(string title, (Data.Getter<string?>, Data.Setter<string?>) get_set, string tooltip = "") {
		// TODO: dropdowns somehow?
		var staged = editing!;
		var (getter, setter) = get_set;
		var (value, mixed) = Data.GetAllOrNothing<string>(staged, getter);
		
		ArrayEditor.Getter arr_get = () => {
			if (mixed) return null;
			return Data.RawToJson(value ?? "{}") switch {
				JSONArray arr => arr,
				JSONString s => JSONObject.Parse($"[{s}]").AsArray,
				_ => new JSONArray()
			};
		};
		
		ArrayEditor.Setter arr_set =
			(JSONArray node) => Data.UpdateObjects<string?>(staged, setter, node.ToString());
		
		ArrayEditor
			.Singleton(current_panel!, title, tooltip)
			.Set((arr_get, arr_set), false);
	}
	
	private void AddMaterial() {
		if (bundleInfo?.Materials == null) {
			AddTextbox("Material", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "asset"), false);
		}
		else {
			AddDropdown("Material", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "asset"), bundleInfo.Materials, true);
		}
	}
	
	private void AddPrefab(string title, string prop, bool nullable = true) {
		if (bundleInfo?.Prefabs == null) {
			AddTextbox(title, Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", prop), false);
		}
		else {
			AddDropdown(title, Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", prop), bundleInfo.Prefabs, nullable);
		}
	}
	
	private void AddMaterialProperties() {
		var (getter, _) = Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "asset");
		var (asset, _) = Data.GetAllOrNothing(editing!, getter);
		if (asset != null) {
			var mat = bundleInfo?.Materials?.Forward(asset);
			if (mat != null && (bundleInfo?.Properties?.ContainsKey(mat) ?? false)) {
				panels.Push(Collapsible.Create(panel!, "Properties", "Properties", true).panel!);
				foreach (var prop in bundleInfo.Properties[mat]) {
					AddTextbox(prop.Key, Data.PropertyGetSetRaw(prop.Key, prop.Value.ToString()), true);
				}
				panels.Pop();
			}
		}
	}
	
	private void AddCameraProperties() {
		AddTextbox("Depth Texture Mode", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "properties.depthTextureMode"), true);
		AddDropdown("Clear Flags", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "properties.clearFlags"), Vivify.ClearFlags, true);
		AddTextbox("Background Colors", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "properties.backgroundColor"), true);
		AddTracks("Culling Tracks", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "properties.culling.track"));
		AddCheckbox("Culling Whitelist", Data.JSONGetSet<bool?>(typeof(BaseCustomEvent), "Data", "properties.culling.whitelist"), false);
		// Do these have defaults? Idk :3
		AddDropdown("Bloom Pre Pass", Data.JSONGetSet<bool?>(typeof(BaseCustomEvent), "Data", "properties.bloomPrePass"), MapSettings.OptionBool, true);
		AddDropdown("Main Bloom Effects", Data.JSONGetSet<bool?>(typeof(BaseCustomEvent), "Data", "properties.mainEffect"), MapSettings.OptionBool, true);
	}
	
	private void AddCustomProperties() {
		var all_props = new Dictionary<string, Vivify.PropertyType>();
		foreach (var o in editing!) {
			var root = (o as BaseCustomEvent)!.Data ?? new JSONObject();
			if (Data.GetNode(root, "properties") is JSONArray props) {
				foreach (var prop in props.Children) {
					var id = (string)prop.AsObject["id"];
					System.Enum.TryParse((string)prop.AsObject["type"], out Vivify.PropertyType type);
					if (!all_props.ContainsKey(id)) {
						all_props.Add(id, type);
					}
				}
			}
		}
		
		panels.Push(Collapsible.Create(panel!, "Properties", "Properties", true).panel!);
		foreach (var prop in all_props) {
			var container = UI.AddChild(current_panel!, prop + " Container");
			UI.AttachTransform(container, new Vector2(0, 20), pos: new Vector2(0, 0));
			panels.Push(container);
			var id_box = AddTextbox(null, Data.PropertyGetSetPart(prop.Key, "id"));
			UI.LeftColumn(id_box.gameObject);
			AddDropdown(null, Data.PropertyGetSetPart(prop.Key, "type"), Vivify.PropertyTypes, false);
			panels.Pop();
			var value_box = AddTextbox(null, Data.PropertyGetSetRaw(prop.Key, prop.Value.ToString()), true);
			UI.MoveTransform((RectTransform)value_box.transform, new Vector2(0, 22), new Vector2(0, 0));
		}
		AddTextbox("Add Property", Data.PropertyGetSetPart(null, "id"));
		panels.Pop();
	}
	
	private void AddObjectProperties() {
		{
			var (has_any, mixed) = Data.GetAllOrNothing<bool>(editing!, (o) => (o as BaseCustomEvent)?.Data?.HasKey("colorNotes") ?? false);
			panels.Push(Collapsible.Create(panel!, "_Color Notes", "Color Notes", has_any || mixed).panel!);
			AddTextbox("Track", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "colorNotes.track"), true);
			AddPrefab("Asset", "colorNotes.asset");
			AddPrefab("Any Direction Asset", "colorNotes.anyDirectionAsset");
			AddPrefab("Debris Asset", "colorNotes.debrisAsset");
			panels.Pop();
		}
		{
			var (has_any, mixed) = Data.GetAllOrNothing<bool>(editing!, (o) => (o as BaseCustomEvent)?.Data?.HasKey("burstSliders") ?? false);
			AddExpando("_Burst Sliders", "Burst Sliders", has_any || mixed);
			AddTextbox("Track", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "burstSliders.track"), true);
			AddPrefab("Asset", "burstSliders.asset");
			AddPrefab("Debris Asset", "burstSliders.debrisAsset");
			panels.Pop();
		}
		{
			var (has_any, mixed) = Data.GetAllOrNothing<bool>(editing!, (o) => (o as BaseCustomEvent)?.Data?.HasKey("burstSliderElemeents") ?? false);
			AddExpando("_Burst Slider Elements", "Burst Slider Elements", has_any || mixed);
			AddTextbox("Track", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "burstSliderElemeents.track"), true);
			AddPrefab("Asset", "burstSliderElemeents.asset");
			AddPrefab("Debris Asset",  "burstSliderElemeents.debrisAsset");
			panels.Pop();
		}
		{
			var (has_any, mixed) = Data.GetAllOrNothing<bool>(editing!, (o) => (o as BaseCustomEvent)?.Data?.HasKey("saber") ?? false);
			AddExpando("_Sabers", "Sabers", has_any || mixed);
			AddDropdown("Type", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "saber.type"), Vivify.SaberTypes, false);
			AddPrefab("Asset", "saber.asset");
			AddPrefab("Trail Asset", "saber.trailAsset");
			AddTextbox("Trail Top Position", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "saber.trailTopPos"), true);
			AddTextbox("Trail Bottom Position", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "saber.trailBottomPos"), true);
			AddParsed("Trail Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "saber.trailDuration"));
			AddParsed("Trail Sampling Frequency", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "saber.trailSamplingFrequency"));
			AddParsed("Trail Granularity", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "saber.trailGranularity"));
			panels.Pop();
		}
		panels.Pop();
	}
	
	private void EditVector3(string name, (Data.Getter<Vector3?>, Data.Setter<Vector3?>) get_set) {
		Data.Getter<string?> getter = (o) => {
			var value = get_set.Item1(o);
			return (value != null)
				? (new JSONArray()).WriteVector3(value ?? new Vector3()).ToString()
				: "";
		};
		Data.Setter<string?> setter = (o, v) => {
			if (v == null || v == "") {
				get_set.Item2(o, null);
				return;
			}
			var node = Data.RawToJson(v);
			if (node is JSONArray vec) {
				get_set.Item2(o, vec.ReadVector3());
			}
		};
		
		AddTextbox(name, (getter, setter), true);
		
		/* Component version, TODO: combine these two somehow?
		var (value, mixed) = Data.GetAllOrNothing<Vector3?>(editing!, get_set.Item1);
		
		AddExpando("_"+name, name, (value != null || mixed), background: false);
		AddParsed<float>("X", Data.V3Component(get_set, Axis.X));
		AddParsed<float>("Y", Data.V3Component(get_set, Axis.Y));
		AddParsed<float>("Z", Data.V3Component(get_set, Axis.Z));
		panels.Pop();*/
	}
	
	private void EEComponent(string name, (Data.Getter<bool?>, Data.Setter<bool?>) get_set, System.Action editor) {
		var checkbox = AddCheckbox(name, get_set, null);
		
		var comp_container = Collapsible.Singleton(current_panel!, "_"+name, name, false);
		
		panels.Push(comp_container.panel!);
		editor();
		panels.Pop();
		
		checkbox.onValueChanged.AddListener((v) => {
			if (v) {
				comp_container.OnAnimationComplete = null;
				comp_container.SetExpanded(false);
				comp_container.gameObject.SetActive(true);
				comp_container.SetExpanded(true);
			}
			else {
				comp_container.OnAnimationComplete = (v) => {
					comp_container.gameObject.SetActive(v);
				};
				comp_container.SetExpanded(v);
			}
		});
		
		comp_container.gameObject.SetActive(checkbox.isOn);
		comp_container.SetExpanded(checkbox.isOn);
	}
}

}
