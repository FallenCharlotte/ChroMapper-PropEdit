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
using System.Security;
using UnityEngine.Assertions.Must;
using static ChroMapper_PropEdit.UserInterface.TooltipStrings;

namespace ChroMapper_PropEdit.UserInterface {

public partial class MainWindow {
	public readonly string CHROMA_NAME = "Chroma";
	public readonly string NOODLE_NAME = "Noodle Extensions";
	TooltipStrings tooltip = TooltipStrings.Instance;
	BundleInfo? bundleInfo = null;
	
	public void UpdateSelection(bool real) { lock(this) {
		scrollbox!.TargetScroll = real ? 1f : scrollbox!.scrollbar!.value;
		
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
			var v2 = BeatSaberSongContainer.Instance.Map.Version[0] == '2';
			
			current_panel = panel;
			
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
						var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
						current_panel = collapsible.panel;
						AddColor("Color", o.CustomKeyColor, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Color));
						if (v2) {
							AddCheckbox("Disable Spawn Effect", Data.CustomGetSet<bool?>("_disableSpawnEffect"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.DisableSpawnEffect));
						}
						else {
							AddCheckbox("Spawn Effect", Data.CustomGetSet<bool?>("spawnEffect"), true, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.SpawnEffect));
							AddCheckbox("Disable Debris", Data.CustomGetSet<bool?>("disableDebris"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.DisableDebris));
						}
						current_panel = panel;
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, NOODLE_NAME, "Noodle Extensions", true);
						current_panel = collapsible.panel;
						AddParsed("NJS", Data.CustomGetSet<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"), false,tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.NJS));
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
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true,tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Track)); //prob. needs more info
						AddAnimation(PropertyType.Note, v2);
						current_panel = panel;
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
						var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
						current_panel = collapsible.panel;
						AddColor("Color", o.CustomKeyColor, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Color));
						current_panel = panel;
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, NOODLE_NAME, "Noodle Extensions", true);
						current_panel = collapsible.panel;
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
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Track));
						AddAnimation(PropertyType.Arc, v2);
						current_panel = panel;
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
						var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
						current_panel = collapsible.panel;
						AddColor("Color", o.CustomKeyColor, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Color));
						current_panel = panel;
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, NOODLE_NAME, "Noodle Extensions", true);
						current_panel = collapsible.panel;
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
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Track));
						AddAnimation(PropertyType.Chain, v2);
						current_panel = panel;
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
						var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
						current_panel = collapsible.panel;
						AddColor("Color", o.CustomKeyColor, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Color));
						current_panel = panel;
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, NOODLE_NAME, "Noodle Extensions", true);
						current_panel = collapsible.panel;
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
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Track));
						AddAnimation(PropertyType.Obstacle, v2);
						current_panel = panel;
					}
					
					break;
				case ObjectType.Event: {
					var env = BeatSaberSongContainer.Instance.Info.EnvironmentName;
					var events = editing.Select(o => (BaseEvent)o);
					var f = events.First();
					// Light
					if (events.Where(e => e.IsLightEvent(env)).Count() == editing.Count()) {
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
							var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
							current_panel = collapsible.panel;
							AddTextbox("LightID", Data.CustomGetSetRaw(f.CustomKeyLightID), true, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LightID));
							AddColor("Color", o.CustomKeyColor, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.Color));
							if (events.Where(e => e.IsTransition).Count() == editing.Count()) {
								AddDropdown<string>("Easing",    Data.CustomGetSet<string>(f.CustomKeyEasing), Events.Easings, true, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.Easing));
								AddDropdown<string>("Lerp Type", Data.CustomGetSet<string>(f.CustomKeyLerpType), Events.LerpTypes, true, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LerpType)); //Unsure
							}
							if (o is BaseEvent e && v2) {
								AddCheckbox("V2 Gradient", Data.GetSetGradient(), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.V2Gradient));
								if (e.CustomLightGradient != null) {
									AddParsed("Duration",     Data.CustomGetSet<float?>($"{e.CustomKeyLightGradient}._duration"), false, tooltip.GetTooltip(PropertyType.Gradient, TooltipStrings.Tooltip.Duration));
									AddColor("Start Color", $"{e.CustomKeyLightGradient}._startColor", tooltip.GetTooltip(PropertyType.GradientStart, TooltipStrings.Tooltip.Color));
									AddColor("End Color", $"{e.CustomKeyLightGradient}._endColor", tooltip.GetTooltip(PropertyType.GradientEnd, TooltipStrings.Tooltip.Color));
									AddDropdown<string>("Easing",    Data.CustomGetSet<string>($"{e.CustomKeyLightGradient}._easing"), Events.Easings, false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.Easing));
								}
							}
							current_panel = panel;
						}
					}
					// Laser Speeds
					if (events.Where(e => e.IsLaserRotationEvent(env)).Count() == editing.Count()) {
						AddParsed("Speed", Data.GetSet<int>("Value"), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LaserSpeed));
						AddLine("");
						
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
							current_panel = collapsible.panel;
							AddCheckbox("Lock Rotation", Data.CustomGetSet<bool?> (f.CustomKeyLockRotation), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LockRotation));
							AddDropdown<int?>("Direction",     Data.CustomGetSet<int?>  (f.CustomKeyDirection), Events.LaserDirection, true, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LaserDirection));
							AddParsed("Precise Speed",   Data.CustomGetSet<float?>(f.CustomKeyPreciseSpeed), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.PreciseSpeed));
							current_panel = panel;
						}
					}
					// Ring Rotation
					if (events.Where(e => e.Type == (int)EventTypeValue.RingRotation).Count() == editing.Count()) {
						AddLine("");
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
							current_panel = collapsible.panel;
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
							current_panel = panel;
						}
					}
					// Ring Zoom
					if (events.Where(e => e.Type == (int)EventTypeValue.RingZoom).Count() == editing.Count()) {
						AddLine("");
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
							current_panel = collapsible.panel;
							AddParsed("Step",  Data.CustomGetSet<float?>(f.CustomKeyStep), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingZoomStep));
							AddParsed("Speed", Data.CustomGetSet<float?>(f.CustomKeySpeed), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingSpeed));
							current_panel = panel;
						}
					}
					// Boost Color
					if (events.Where(e => e.IsColorBoostEvent()).Count() == editing.Count()) {
						AddDropdown<int?>("Color Set", Data.GetSet<int>("Value"), Events.BoostSets, false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.BoostColorSet));
					}
					// Lane Rotations
					if (events.Where(e => e.IsLaneRotationEvent()).Count() == editing.Count()) {
						AddDropdown<int?>("Rotation", Data.GetSet<int>("Value"), Events.LaneRotaions, false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LaneRotation));
					}
				}	break;
				case ObjectType.CustomEvent: {
					var events = editing.Select(o => (BaseCustomEvent)o);
					var f = events.First();
					
					var types = events.Select(e => e.Type)
						.Distinct();
					
					if (types.Count() > 1) {
						// Still not attempting multi-type :3
						break;
					}
					
					switch (types.First()) {
					// Heck
					case "AnimateTrack":
						AddTextbox("Track", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.Track));
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "repeat"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackRepeat));
						}
						AddLine("");
						AddTextbox("Color", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_color" : "color"), true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AnimateColor));
						foreach (var property in Events.NoodleProperties) {
							AddTextbox(property.Key, Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", property.Value[v2 ? 0 : 1]), true, tooltip.GetTooltip(PropertyType.CustomEvent, $"Animate{property.Key}"));
						}
						AddTextbox("Time", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_time" : "time"), true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AnimateTime));
						break;
					
					case "AssignPathAnimation":
						AddTextbox("Track", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.Track));
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "repeat"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackRepeat));
						}
						AddLine("");
						AddTextbox("Color", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_color" : "color"), true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AnimateColor));
						foreach (var property in Events.NoodleProperties) {
							AddTextbox(property.Key, Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", property.Value[v2 ? 0 : 1]), true, tooltip.GetTooltip(PropertyType.CustomEvent, $"Animate{property.Key}"));
						}
						AddTextbox("Definite Position", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_definitePosition" : "definitePosition"), true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AssignPathAnimationDefinitePosition));
						break;
					
					// Noodle
					case "AssignTrackParent":
						AddTextbox("Parent", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_parentTrack" : "parentTrack"), true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AssignTrackParentParent));
						AddTextbox("Children", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_childrenTracks" : "childrenTracks"), true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AssignTrackChildren));
						AddCheckbox("Keep Position", Data.JSONGetSet<bool?>(typeof(BaseCustomEvent), "Data", v2 ? "_worldPositionStays" : "worldPositionStays"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AssignTrackKeepPosition));
						break;
					
					case "AssignPlayerToTrack":
						AddTextbox("Track", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AssignPlayerToTrackTrack));
						AddTextbox("Target", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_target" : "target"), true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AssignPlayerToTrackTarget));
						break;
					
					// Chroma
					case "AssignFogTrack":
						AddTextbox("Track", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.Track));
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "repeat"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackRepeat));
						}
						
						AddParsed("Attenuation", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "_attenuation"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.V2AssignFogTrackAttenuation));
						AddParsed("Offset", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "_offset"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.V2AssignFogTrackOffset));
						AddParsed("Start Y", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "_startY"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.V2AssignFogTrackStartY));
						AddParsed("Height", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "_height"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.V2AssignFogTrackHeight));
						break;
					
					case "AnimateComponent":
						AddTextbox("Track", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.Track));
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "repeat"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackRepeat));
						}
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
						// TODO: Grid? +/-?
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
						AddTextbox("Asset", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "asset"), false);
						AddTextbox("ID", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "id"), false);
						AddTextbox("Track", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "track"), false);
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
						// TODO: Animator Properties
						break;
					case "SetCameraProperty":
						AddTextbox("Camera ID", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "id"), false);
						AddCameraProperties();
						break;
					case "AssignObjectPrefab":
						AddDropdown("Load Mode", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "loadMode"), Vivify.LoadModes, true);
						AddTextbox("Object", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "object"), true);
						break;
					case "SetRenderingSettings":
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "duration"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						{
							var collapsible = Collapsible.Create(panel, "Render Settings", "Render Settings", true);
							current_panel = collapsible.panel;
							foreach (var prop in Vivify.RenderSettings) {
								AddTextbox(prop.Value, Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", prop.Key), true);
							}
							current_panel = panel;
						}
						{
							var collapsible = Collapsible.Create(panel, "Quality Settings", "Quality Settings", true);
							current_panel = collapsible.panel;
							foreach (var prop in Vivify.QualitySettings) {
								AddTextbox(prop.Value, Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", prop.Key), true);
							}
							current_panel = panel;
						}
						{
							var collapsible = Collapsible.Create(panel, "XR Settings", "XR Settings", true);
							current_panel = collapsible.panel;
							foreach (var prop in Vivify.XRSettings) {
								AddTextbox(prop.Value, Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", prop.Key), true);
							}
							current_panel = panel;
						}
						break;
					}
				}	break;
				case ObjectType.BpmChange:
					AddParsed("BPM", Data.GetSet<float>("Bpm"), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.BPMChange));
					break;
			}
			UI.RefreshTooltips(panel);
		}
		else {
			window!.SetTitle("No items selected");
		}
	}}
	
	private void AddAnimation(PropertyType type, bool v2) {
		var CustomKeyAnimation = v2 ? "_animation" : "animation";
		AddCheckbox("Animation", Data.GetSetAnimation(v2), false, tooltip.GetTooltip(type, TooltipStrings.Tooltip.AnimatePath));
		if (editing.Where(o => o.CustomData?.HasKey(CustomKeyAnimation) ?? false).Count() == editing.Count()) {
			AddCheckbox("  Color", Data.CustomGetSetNode(CustomKeyAnimation+"."+ (v2 ? "_color" : "color"), "[[0,0,0,0,0], [1,1,1,1,0.49]],"), false, tooltip.GetTooltip(type, TooltipStrings.Tooltip.AnimateColor));
			foreach (var property in Events.NoodleProperties) {
				AddCheckbox("  "+property.Key, Data.CustomGetSetNode(CustomKeyAnimation+"."+ property.Value[v2 ? 0 : 1], property.Value[2]), false, tooltip.GetTooltip(type, $"Animate{property.Key}"));
			}
			AddCheckbox("  Definite Position", Data.CustomGetSetNode(CustomKeyAnimation+"."+ (v2 ? "_definitePosition" : "definitePosition"), "[[0,0,0,0], [0,0,0,0.49]]"), false, tooltip.GetTooltip(type, TooltipStrings.Tooltip.AssignPathAnimationDefinitePosition));
			AddLine("");
		}
	}
	
	private void AddColor(string label, string key, string tooltip = "") {
		if (Settings.Get(Settings.ColorHex, true)) {
			AddTextbox(label, Data.CustomGetSetColor(key), false, tooltip);
		}
		else {
			AddTextbox(label, Data.CustomGetSetRaw(key), true, tooltip);
		}
	}
	
	private void AddMaterial() {
		if (bundleInfo?.Materials == null) {
			AddTextbox("Material", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "asset"), false);
		}
		else {
			AddDropdown("Material", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "asset"), bundleInfo.Materials, true);
		}
	}
	
	private void AddMaterialProperties() {
		var (getter, _) = Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "asset");
		var (asset, _) = Data.GetAllOrNothing(editing!, getter);
		if (asset != null) {
			var mat = bundleInfo?.Materials?.Forward(asset);
			if (mat != null && (bundleInfo?.Properties?.ContainsKey(mat) ?? false)) {
				AddLine("Properties:");
				foreach (var prop in bundleInfo.Properties[mat]) {
					AddTextbox(prop.Key, Data.PropertyGetSetRaw(prop.Key, prop.Value.ToString()), true);
				}
			}
		}
	}
	
	private void AddCameraProperties() {
		AddTextbox("Depth Texture Mode", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "properties.depthTextureMode"), true);
		AddDropdown("Clear Flags", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", "properties.clearFlags"), Vivify.ClearFlags, true);
		AddTextbox("Background Colors", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "properties.backgroundColor"), true);
		AddTextbox("Culling Track(s)", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "properties.culling.track"), true);
		AddCheckbox("Culling Whitelist", Data.JSONGetSet<bool?>(typeof(BaseCustomEvent), "Data", "properties.culling.whitelist"), false);
		// Do these have defaults? Idk :3
		AddDropdown("Bloom Pre Pass", Data.JSONGetSet<bool?>(typeof(BaseCustomEvent), "Data", "properties.bloomPrePass"), MapSettings.OptionBool, true);
		AddDropdown("Main Bloom Effects", Data.JSONGetSet<bool?>(typeof(BaseCustomEvent), "Data", "properties.mainEffect"), MapSettings.OptionBool, true);
	}
}

}
