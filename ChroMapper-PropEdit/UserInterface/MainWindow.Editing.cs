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
	Enums.Map<int?>? EventLanes;
	
	private ObjectType? old_otype = null;
	private SelectionType? old_stype = null;
	private Events.EventType? old_etype = null;
	private string? old_cetype = null;
	private bool full_rebuild = true;
	private bool refresh_frame = false;
	
	private void wipe(int skip = 0) {
		//Plugin.Trace($"Wipe after {skip}");
		foreach (Transform child in panel!.transform) {
			while (panel!.transform.childCount > skip) {
				//Plugin.Trace($"Delete {panel!.transform.GetChild(skip).gameObject.name}");
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
		
		//Plugin.Trace($"{old_stype} => {new_type}");
		
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
			var v2 = global::Settings.Instance.MapVersion == 2;
			
			if (CheckRefresh(SelectionType.Objects, type != old_otype)) {
				old_etype = null;
			}
			Plugin.Trace($"{old_otype} => {type}: {full_rebuild}");
			
			old_otype = type;
			old_stype = SelectionType.Objects;
			
			panels.Clear();
			panels.Push(panel!);
			
			EditParsed("Beat", ObjectField<float>("JsonTime", true), (o is BaseGrid)
				? tooltip.GetTooltip(PropertyType.Object, TooltipStrings.Tooltip.Beat)
				: tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.BeatEvent));
			
			switch (type) {
				case ObjectType.Note: {
					var note = (o as BaseNote)!;
					EditParsed("X", ObjectField<int>("PosX"), tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.X));
					EditParsed("Y", ObjectField<int>("PosY"), tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Y));
					EditDropdown<int?>("Type", ObjectField<int>("Type"), Notes.NoteTypes, false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Type));
					EditDropdown<int?>("Direction", ObjectField<int>("CutDirection"), Notes.CutDirections, false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.CutDirection));
					if (!v2) {
						EditParsed("Angle Offset", ObjectField<int>("AngleOffset"), tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.AngleOffset));
					}
					Line("");
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						Expando(CHROMA_NAME, "Chroma", true);
						EditColor("Color", o.CustomKeyColor, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Color));
						if (v2) {
							EditCheckbox("Disable Spawn Effect", CustomField<bool?>("_disableSpawnEffect"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.DisableSpawnEffect));
						}
						else {
							EditCheckbox("Spawn Effect", CustomField<bool?>("spawnEffect"), true, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.SpawnEffect));
							EditCheckbox("Disable Debris", CustomField<bool?>("disableDebris"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.DisableDebris));
						}
						panels.Pop();
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						Expando(NOODLE_NAME, "Noodle Extensions", true);
						EditParsed("NJS", CustomField<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"), tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.NJS));
						EditParsed("Spawn Offset", CustomField<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"), tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.SpawnOffset));
						EditTextbox("Coordinates", CustomFieldRaw(note.CustomKeyCoordinate), true, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Coordinates));
						EditTextbox("Rotation", CustomFieldRaw(note.CustomKeyWorldRotation), true, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Rotation));
						EditTextbox("Local Rotation", CustomFieldRaw(note.CustomKeyLocalRotation), true, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.LocalRotation));
						if (v2) {
							EditParsed("Exact Angle", CustomField<float?>("_cutDirection"), tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.CutDirection));
							EditCheckbox("Fake", CustomField<bool?>("_fake"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Fake));
							EditCheckbox("Interactable", CustomField<bool?>("_interactable"), true, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Interactable));
							EditTextbox("Flip", CustomFieldRaw("_flip"), true, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Flip));
						}
						else {
							EditCheckbox("Fake", ObjectField<bool>("CustomFake"), null, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Fake));
							EditCheckbox("Uninteractable", CustomField<bool?>("uninteractable"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Uninteractable));
							EditCheckbox("Disable Gravity", CustomField<bool?>("disableNoteGravity"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.DisableGravity));
							EditCheckbox("Disable Look", CustomField<bool?>("disableNoteLook"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.DisableLook));
							EditCheckbox("No Badcut Direction", CustomField<bool?>("disableBadCutDirection"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.NoBadcutDirection));
							EditCheckbox("No Badcut Speed", CustomField<bool?>("disableBadCutSpeed"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.NoBadcutSpeed));
							EditCheckbox("No Badcut Color", CustomField<bool?>("disableBadCutSaberType"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.NoBadcutColor));
							EditTextbox("Flip", CustomFieldRaw("flip"), true, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Flip));
							EditTextbox("Link", CustomField<string?>("link"), false, tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Link));
						}
						EditTracks("Tracks", CustomField(o.CustomKeyTrack), tooltip.GetTooltip(PropertyType.Note, TooltipStrings.Tooltip.Track)); //prob. needs more info
						EditAnimations(PropertyType.Note, v2);
						panels.Pop();
					}
					
				}	break;
				case ObjectType.CustomNote:
					Line("Wow, a custom note! How did you do this?");
					break;
				case ObjectType.Arc: {
					EditParsed("Head X", ObjectField<int>("PosX"), tooltip.GetTooltip(PropertyType.ArcHead, TooltipStrings.Tooltip.X));
					EditParsed("Head Y", ObjectField<int>("PosY"), tooltip.GetTooltip(PropertyType.ArcHead, TooltipStrings.Tooltip.Y));
					EditDropdown<int?>("Color", ObjectField<int>("Color"), Notes.ArcColors, false,tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Type));
					EditDropdown<int?>("Direction", ObjectField<int>("CutDirection"), Notes.CutDirections, false, tooltip.GetTooltip(PropertyType.ArcHead, TooltipStrings.Tooltip.CutDirection));
					EditParsed("Head Multiplier", ObjectField<float>("HeadControlPointLengthMultiplier"), tooltip.GetTooltip(PropertyType.ArcHead, TooltipStrings.Tooltip.Multiplier));
					EditParsed("Tail Beat", ObjectField<float>("TailJsonTime"), tooltip.GetTooltip(PropertyType.ArcTail, TooltipStrings.Tooltip.Beat)); 
					EditParsed("Tail X", ObjectField<int>("TailPosX"), tooltip.GetTooltip(PropertyType.ArcTail, TooltipStrings.Tooltip.X));
					EditParsed("Tail Y", ObjectField<int>("TailPosY"), tooltip.GetTooltip(PropertyType.ArcTail, TooltipStrings.Tooltip.Y));
					EditDropdown<int?>("Tail Direction", ObjectField<int>("TailCutDirection"), Notes.CutDirections, false, tooltip.GetTooltip(PropertyType.ArcTail, TooltipStrings.Tooltip.CutDirection));
					EditParsed("Tail Multiplier", ObjectField<float>("TailControlPointLengthMultiplier"), tooltip.GetTooltip(PropertyType.ArcTail, TooltipStrings.Tooltip.Multiplier));
					EditDropdown<int?>("Mid-Anchor Mode", ObjectField<int>("MidAnchorMode"), Notes.MidAnchorModes, false, tooltip.GetTooltip(PropertyType.ArcHead, TooltipStrings.Tooltip.MidAnchorMode));
					Line("");
					
					var s = (o as BaseSlider)!;
					
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						Expando(CHROMA_NAME, "Chroma", true);
						EditColor("Color", o.CustomKeyColor, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Color));
						panels.Pop();
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						Expando(NOODLE_NAME, "Noodle Extensions", true);
						EditParsed("NJS", CustomField<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"), tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.NJS));
						EditParsed("Spawn Offset", CustomField<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"), tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.SpawnOffset));
						EditTextbox("Head Coordinates", CustomFieldRaw(s.CustomKeyCoordinate), true, tooltip.GetTooltip(PropertyType.ArcHead, TooltipStrings.Tooltip.Coordinates));
						EditTextbox("Tail Coordinates", CustomFieldRaw(s.CustomKeyTailCoordinate), true, tooltip.GetTooltip(PropertyType.ArcTail, TooltipStrings.Tooltip.Coordinates));
						EditTextbox("Rotation", CustomFieldRaw(s.CustomKeyWorldRotation), true, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Rotation));
						EditTextbox("Local Rotation", CustomFieldRaw(s.CustomKeyLocalRotation), true, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.LocalRotation));
						if (v2) {
							EditCheckbox("Interactable", CustomField<bool?>("_interactable"), true, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Interactable));
							EditTextbox("Flip", CustomFieldRaw("_flip"), true, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Flip)); //not sure if this works
						}
						else {
							EditCheckbox("Uninteractable", CustomField<bool?>("uninteractable"), false, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Uninteractable));
							EditCheckbox("Disable Gravity", CustomField<bool?>("disableNoteGravity"), false, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.DisableGravity));
							EditTextbox("Flip", CustomFieldRaw("flip"), true, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Flip));
							EditTextbox("Link", CustomField<string?>("link"), false, tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Link));
						}
						EditTracks("Tracks", CustomField(o.CustomKeyTrack), tooltip.GetTooltip(PropertyType.Arc, TooltipStrings.Tooltip.Track));
						EditAnimations(PropertyType.Arc, v2);
						panels.Pop();
					}
					
				}	break;
				case ObjectType.Chain: {
					EditParsed("Head X", ObjectField<int>("PosX"), tooltip.GetTooltip(PropertyType.ChainHead, TooltipStrings.Tooltip.X));
					EditParsed("Head Y", ObjectField<int>("PosY"), tooltip.GetTooltip(PropertyType.ChainHead, TooltipStrings.Tooltip.Y));
					EditDropdown<int?>("Color", ObjectField<int>("Color"), Notes.ArcColors, false, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Color));
					EditDropdown<int?>("Direction", ObjectField<int>("CutDirection"), Notes.CutDirections, false, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.CutDirection));
					EditParsed("Slices", ObjectField<int>("SliceCount"), tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Slices));
					EditParsed("Squish", ObjectField<float>("Squish"), tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Squish));
					EditParsed("Tail X", ObjectField<int>("TailPosX"), tooltip.GetTooltip(PropertyType.ChainTail, TooltipStrings.Tooltip.X));
					EditParsed("Tail Y", ObjectField<int>("TailPosY"), tooltip.GetTooltip(PropertyType.ChainTail, TooltipStrings.Tooltip.Y));
					Line("");
					
					var s = (o as BaseSlider)!;
					
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						Expando(CHROMA_NAME, "Chroma", true);
						EditColor("Color", o.CustomKeyColor, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Color));
						panels.Pop();
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						Expando(NOODLE_NAME, "Noodle Extensions", true);
						EditParsed("NJS", CustomField<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"), tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.NJS));
						EditParsed("Spawn Offset", CustomField<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"), tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.SpawnOffset));
						EditTextbox("Head Coordinates", CustomFieldRaw(s.CustomKeyCoordinate), true, tooltip.GetTooltip(PropertyType.ChainHead, TooltipStrings.Tooltip.Coordinates));
						EditTextbox("Tail Coordinates", CustomFieldRaw(s.CustomKeyTailCoordinate), true, tooltip.GetTooltip(PropertyType.ChainTail, TooltipStrings.Tooltip.Coordinates));
						EditTextbox("Rotation", CustomFieldRaw(s.CustomKeyWorldRotation), true, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Rotation));
						EditTextbox("Local Rotation", CustomFieldRaw(s.CustomKeyLocalRotation), true, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.LocalRotation));
						EditCheckbox("Fake", ObjectField<bool>("CustomFake"), null, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Fake));
						EditCheckbox(v2 ? "Interactable" : "Uninteractable", CustomField<bool?>(v2 ? "_interactable" : "uninteractable"), v2, tooltip.GetTooltip(PropertyType.Chain, (v2 ? TooltipStrings.Tooltip.Interactable : TooltipStrings.Tooltip.Uninteractable)));
						if (v2) {
							EditTextbox("Flip", CustomFieldRaw("_flip"), true, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Flip));
						}
						else {
							EditCheckbox("Disable Gravity", CustomField<bool?>("disableNoteGravity"), false, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Flip));
							EditTextbox("Flip", CustomFieldRaw("flip"), true, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Flip));
							EditTextbox("Link", CustomField<string>("link"), false, tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Link));
						}
						EditTracks("Tracks", CustomField(o.CustomKeyTrack), tooltip.GetTooltip(PropertyType.Chain, TooltipStrings.Tooltip.Track));
						EditAnimations(PropertyType.Chain, v2);
						panels.Pop();
					}
					
				}	break;
				case ObjectType.Obstacle: {
					var ob = (o as BaseObstacle)!;
					EditParsed("Duration", ObjectField<float>("Duration"), tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Duration));
					if (v2) {
						EditParsed("X", ObjectField<int>("PosX"), tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.X));
						EditParsed("Width", ObjectField<int>("Width"), tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Width));
						EditDropdown<int?>("Height", ObjectField<int>("Type"), Obstacles.WallHeights, false, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Width));
					}
					else {
						EditParsed("X (Left)", ObjectField<int>("PosX"), tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.X));
						EditParsed("Y (Bottom)", ObjectField<int>("PosY"), tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Y));
						EditParsed("Width", ObjectField<int>("Width"), tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Width));
						EditParsed("Height", ObjectField<int>("Height"), tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Height));
					}
					Line("");
					
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						Expando(CHROMA_NAME, "Chroma", true);
						EditColor("Color", o.CustomKeyColor, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Color));
						panels.Pop();
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						Expando(NOODLE_NAME, "Noodle Extensions", true);
						EditParsed("NJS", CustomField<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"), tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.NJS));
						EditParsed("Spawn Offset", CustomField<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"), tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.SpawnOffset));
						EditTextbox("Coordinates", CustomFieldRaw(ob.CustomKeyCoordinate), true, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Coordinates));
						EditTextbox("Rotation", CustomFieldRaw(ob.CustomKeyWorldRotation), true, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Rotation));
						EditTextbox("Local Rotation", CustomFieldRaw(ob.CustomKeyLocalRotation), true, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.LocalRotation));
						EditTextbox("Size", CustomFieldRaw(ob.CustomKeySize), true, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Size));
						EditCheckbox("Fake", ObjectField<bool>("CustomFake"), null, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Fake));
						if (v2) {
							EditCheckbox("Interactable", CustomField<bool?>("_interactable"), true, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Interactable));
						}
						else {
							EditCheckbox("Uninteractable", CustomField<bool?>("uninteractable"), false, tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Uninteractable)); // not sure if this means that it will screw up your score
						}
						EditTracks("Tracks", CustomField(o.CustomKeyTrack), tooltip.GetTooltip(PropertyType.Obstacle, TooltipStrings.Tooltip.Track));
						EditAnimations(PropertyType.Obstacle, v2);
						panels.Pop();
					}
					
				}	break;
				case ObjectType.Event: {
					var env = BeatSaberSongContainer.Instance.Info.EnvironmentName;
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
					
					var lanes = GetEventLanes();
					if (lanes != null) {
						EditDropdown<int?>("Type", ObjectField<int>("Type"), lanes, false);
					}
					
					switch (new_etype) {
					case Events.EventType.Light:
						if (Settings.Get(Settings.SplitValue, true)!.AsBool) {
							EditDropdown<int?>("Color", SplitEventValue(0b1100), Events.LightColors, false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.EventColor));
							EditDropdown<int?>("Action", SplitEventValue(0b0011), Events.LightActions, false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.EventAction));
						}
						else {
							EditDropdown<int?>("Value", ObjectField<int>("Value"), Events.LightValues, false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LegacyEventType));
						}
						EditParsed("Brightness", ObjectField<float>("FloatValue"), tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.Brightness));
						Line("");
						
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							Expando(CHROMA_NAME, "Chroma", true);
							EditTextbox("LightID", CustomFieldRaw(f.CustomKeyLightID), true, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LightID));
							EditColor("Color", o.CustomKeyColor, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.Color));
							EditDropdown<string>("Easing",    CustomField<string>(f.CustomKeyEasing), Events.Easings, true, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.Easing));
							EditDropdown<string>("Lerp Type", CustomField<string>(f.CustomKeyLerpType), Events.LerpTypes, true, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LerpType)); //Unsure
							if (o is BaseEvent e && v2) {
								EditCheckbox("V2 Gradient", EditingAccessor(new V2Gradient()), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.V2Gradient));
								if (e.CustomLightGradient != null) {
									EditParsed("Duration",     CustomField<float?>($"{e.CustomKeyLightGradient}._duration"), tooltip.GetTooltip(PropertyType.Gradient, TooltipStrings.Tooltip.Duration));
									EditColor("Start Color", $"{e.CustomKeyLightGradient}._startColor", tooltip.GetTooltip(PropertyType.GradientStart, TooltipStrings.Tooltip.Color));
									EditColor("End Color", $"{e.CustomKeyLightGradient}._endColor", tooltip.GetTooltip(PropertyType.GradientEnd, TooltipStrings.Tooltip.Color));
									EditDropdown<string>("Easing",    CustomField<string>($"{e.CustomKeyLightGradient}._easing"), Events.Easings, false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.V2Easing));
								}
							}
							panels.Pop();
						}
						break;
					case Events.EventType.LaserRotation:
						EditParsed("Speed", ObjectField<int>("Value"), tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LaserSpeed));
						Line("");
						
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							Expando(CHROMA_NAME, "Chroma", true);
							EditCheckbox("Lock Rotation", CustomField<bool?> (f.CustomKeyLockRotation), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LockRotation));
							EditDropdown<int?>("Direction", CustomField<int?>  (f.CustomKeyDirection), Events.LaserDirection, true, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LaserDirection));
							EditParsed("Precise Speed", CustomField<float?>(f.CustomKeySpeed), tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.PreciseSpeed));
							panels.Pop();
						}
						break;
					case Events.EventType.RingRotation:
						Line("");
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							Expando(CHROMA_NAME, "Chroma", true);
							EditTextbox("Filter",     CustomField<string>(f.CustomKeyNameFilter), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingFilter));
							if (v2) {
								EditCheckbox("Reset", CustomField<bool?>("_reset"), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingV2Reset));
							}
							EditParsed("Rotation",    CustomField<float?>(f.CustomKeyLaneRotation), tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingRotation));
							EditParsed("Step",        CustomField<float?>(f.CustomKeyStep), tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingStep));
							EditParsed("Propagation", CustomField<float?>(f.CustomKeyProp), tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingPropagation));
							EditParsed("Speed",       CustomField<float?>(f.CustomKeySpeed), tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingSpeed));
							EditDropdown<int?>("Direction", CustomField<int?>  (f.CustomKeyDirection), Events.RingDirection, true, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingDirection));
							if (v2) {
								EditCheckbox("Counter Spin", CustomField<bool?>("_counterSpin"), false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingV2CounterSpin));
							}
							panels.Pop();
						}
						break;
					case Events.EventType.RingZoom:
						Line("");
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							Expando(CHROMA_NAME, "Chroma", true);
							EditParsed("Step",  CustomField<float?>(f.CustomKeyStep), tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingZoomStep));
							EditParsed("Speed", CustomField<float?>(f.CustomKeySpeed), tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.RingSpeed));
							panels.Pop();
						}
						break;
					case Events.EventType.ColorBoost:
						EditDropdown<int?>("Color Set", ObjectField<int>("Value"), Events.BoostSets, false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.BoostColorSet));
						break;
					case Events.EventType.LaneRotation:
						EditDropdown<int?>("Rotation", ObjectField<int>("Value"), Events.LaneRotaions, false, tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.LaneRotation));
						break;
					default:
						EditParsed("Value", ObjectField<int>("Value"));
						EditParsed("FloatValue", ObjectField<float>("FloatValue"));
						//Debug.LogError($"Unhandled event type: {new_etype}");
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
						EditParsed("Duration", DataField<float?>(v2 ? "_duration" : "duration"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						EditDropdown<string>("Easing", DataField<string>(v2 ? "_easing" : "easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						if (!v2) {
							EditParsed("Repeat", DataField<int?>("repeat"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackRepeat));
						}
						EditTracks("Tracks", CustomField(v2 ? "_track" : "track", "Data"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.Track));
						Expando("Properties", "Point Definitions", true);
						foreach (var property in Events.NoodleProperties) {
							if (property.Value[v2 ? 0 : 1] == "") continue;
							EditPointDefinition(property.Key, DataFieldRaw(property.Value[v2 ? 0 : 1]), tooltip.GetTooltip(PropertyType.CustomEvent, $"Animate{property.Key}"));
						}
						EditPointDefinition("Time", DataFieldRaw( v2 ? "_time" : "time"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AnimateTime));
						panels.Pop();
						break;
					
					case "AssignPathAnimation":
						EditParsed("Duration", DataField<float?>(v2 ? "_duration" : "duration"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						EditDropdown<string>("Easing", DataField<string>(v2 ? "_easing" : "easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						if (!v2) {
							EditParsed("Repeat", DataField<int?>("repeat"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackRepeat));
						}
						EditTracks("Tracks", CustomField(v2 ? "_track" : "track", "Data"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.Track));
						Expando("Properties", "Point Definitions", true);
						foreach (var property in Events.NoodleProperties) {
							if (property.Value[v2 ? 0 : 1] == "") continue;
							EditPointDefinition(property.Key, DataFieldRaw(property.Value[v2 ? 0 : 1]), tooltip.GetTooltip(PropertyType.CustomEvent, $"Animate{property.Key}"));
						}
						EditPointDefinition("Definite Position", DataFieldRaw(v2 ? "_definitePosition" : "definitePosition"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AssignPathAnimationDefinitePosition));
						panels.Pop();
						break;
					
					// Noodle
					case "AssignTrackParent":
						EditTrack("Parent", DataField<string>(v2 ? "_parentTrack" : "parentTrack"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AssignTrackParentParent));
						EditTracks("Children", CustomField(v2 ? "_childrenTracks" : "childrenTracks", "Data"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AssignTrackChildren));
						EditCheckbox("Keep Position", DataField<bool?>(v2 ? "_worldPositionStays" : "worldPositionStays"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AssignTrackKeepPosition));
						break;
					
					case "AssignPlayerToTrack":
						EditTextbox("Track", DataField<string>(v2 ? "_track" : "track"), false, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AssignPlayerToTrackTrack));
						EditDropdown("Target", DataField<string>(v2 ? "_target" : "target"), Events.PlayerTargets, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AssignPlayerToTrackTarget));
						break;
					
					// Chroma
					case "AssignFogTrack":
						EditParsed("Duration", DataField<float?>(v2 ? "_duration" : "duration"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						EditDropdown<string>("Easing", DataField<string>(v2 ? "_easing" : "easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						if (!v2) {
							EditParsed("Repeat", DataField<int?>("repeat"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackRepeat));
						}
						EditTracks("Tracks", CustomField(v2 ? "_track" : "track", "Data"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.Track));
						Expando("Properties", "Properties", true);
						EditParsed("Attenuation", DataField<float?>("_attenuation"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.V2AssignFogTrackAttenuation));
						EditParsed("Offset", DataField<float?>("_offset"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.V2AssignFogTrackOffset));
						EditParsed("Start Y", DataField<float?>( "_startY"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.V2AssignFogTrackStartY));
						EditParsed("Height", DataField<float?>("_height"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.V2AssignFogTrackHeight));
						panels.Pop();
						break;
					
					case "AnimateComponent":
						EditParsed("Duration", DataField<float?>(v2 ? "_duration" : "duration"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						EditDropdown<string>("Easing", DataField<string>(v2 ? "_easing" : "easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						if (!v2) {
							EditParsed("Repeat", DataField<int?>("repeat"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackRepeat));
						}
						EditTracks("Tracks", CustomField(v2 ? "_track" : "track", "Data"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.Track));
						//it seems these are only normal json inputs. might have to change the tooltip then.
						EditTextbox("Environment Fog", DataFieldRaw("BloomFogEnvironment"), true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AnimateComponentBloomFogEnvironment));
						EditTextbox("Tube Bloom Light", DataFieldRaw("TubeBloomPrePassLight"), true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.AnimateComponentTubeBloomPrePassLight));
						break;
					
					// Vivify
					case "SetMaterialProperty":
						EditMaterial();
						goto case "SetGlobalProperty";
					case "SetGlobalProperty":
						EditParsed("Duration", DataField<float?>("duration"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						EditDropdown<string>("Easing", DataField<string>("easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						EditVivifyProperties();
						break;
					case "Blit":
						EditMaterial();
						EditParsed("Duration", DataField<float?>("duration"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						EditDropdown<string>("Easing", DataField<string>("easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						EditParsed("Priority", DataField<int?>("priority"));
						EditParsed("Pass", DataField<int?>("pass"));
						EditDropdown("Order", DataField<string>("order"), Vivify.Orders, true);
						EditTextbox("Source Texture", DataField<string>("source"), false);
						EditTextbox("Destination Texture", DataField<string>("destination"), false);
						EditMaterialProperties();
						break;
					case "CreateCamera":
						EditTextbox("Camera ID", DataField<string>("id"), false);
						EditTextbox("Texture", DataField<string>("texture"), false);
						EditTextbox("Depth Texture", DataField<string>("depthTexture"), false);
						EditCameraProperties();
						break;
					case "CreateScreenTexture":
						EditTextbox("Name", DataField<string>("id"), false);
						EditParsed("X Ratio", DataField<float?>("xRatio"));
						EditParsed("Y Ratio", DataField<float?>("yRatio"));
						EditParsed("Width", DataField<int?>("width"));
						EditParsed("Height", DataField<int?>("height"));
						EditDropdown("Color Format", DataField<string>("colorFormat"), Vivify.ColorFormats, true);
						EditDropdown("Filter Mode", DataField<string>("filterMode"), Vivify.FilterModes, true);
						break;
					case "InstantiatePrefab":
						EditPrefab("Prefab", "asset", false);
						EditTextbox("ID", DataField<string>("id"), false);
						EditTrack("Track", DataField<string>("track"));
						EditTextbox("Position", DataFieldRaw("position"), true);
						EditTextbox("Local Position", DataFieldRaw("localPosition"), true);
						EditTextbox("Rotation", DataFieldRaw("rotation"), true);
						EditTextbox("Local Rotation", DataFieldRaw("localRotation"), true);
						EditTextbox("Scale", DataFieldRaw("scale"), true);
						break;
					case "DestroyObject":
						EditTextbox("ID(s)", DataFieldRaw("id"), true);
						// TODO: Array view?
						break;
					case "SetAnimatorProperty":
						EditTextbox("ID", DataField<string>("id"), false);
						EditParsed("Duration", DataField<float?>("duration"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						EditDropdown<string>("Easing", DataField<string>("easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						EditVivifyProperties();
						break;
					case "SetCameraProperty":
						EditTextbox("Camera ID", DataField<string>("id"), false);
						EditCameraProperties();
						break;
					case "AssignObjectPrefab":
						EditDropdown("Load Mode", DataField<string>("loadMode"), Vivify.LoadModes, true);
						EditPrefabProperties();
						break;
					case "SetRenderingSettings":
						EditParsed("Duration", DataField<float?>("duration"), tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackDuration));
						EditDropdown<string>("Easing", DataField<string>("easing"), Events.Easings, true, tooltip.GetTooltip(PropertyType.CustomEvent, TooltipStrings.Tooltip.TrackEasing));
						{
							var (has_any, mixed) = GetAllOrNothing<bool>(editing!, (o) => (o as BaseCustomEvent)?.Data?.HasKey("renderSettings") ?? false);
							Expando("_Render Settings", "Render Settings", has_any || mixed);
							foreach (var prop in Vivify.RenderSettings) {
								EditTextbox(prop.Value, DataFieldRaw(prop.Key), true);
							}
							panels.Pop();
						}
						{
							var (has_any, mixed) = GetAllOrNothing<bool>(editing!, (o) => (o as BaseCustomEvent)?.Data?.HasKey("qualitySettings") ?? false);
							Expando("_Quality Settings", "Quality Settings", has_any || mixed);
							foreach (var prop in Vivify.QualitySettings) {
								EditTextbox(prop.Value, DataFieldRaw(prop.Key), true);
							}
							panels.Pop();
						}
						{
							var (has_any, mixed) = GetAllOrNothing<bool>(editing!, (o) => (o as BaseCustomEvent)?.Data?.HasKey("xrSettings") ?? false);
							Expando("_XR Settings", "XR Settings", has_any || mixed);
							foreach (var prop in Vivify.XRSettings) {
								EditTextbox(prop.Value, DataFieldRaw(prop.Key), true);
							}
							panels.Pop();
						}
						break;
					}
				}	break;
				case ObjectType.BpmChange:
					EditParsed("BPM", ObjectField<float>("Bpm"), tooltip.GetTooltip(PropertyType.Event, TooltipStrings.Tooltip.BPMChange));
					break;
				case ObjectType.NJSEvent: {
					var conv = new Converter<int?, bool?>(
						(i) => (i == 1),
						(b) => (b ?? false) ? 1 : 0
					);
					EditCheckbox("Use Previous", ObjectField<int>("UsePrevious").Insert(conv), null);
					EditParsed("Relative NJS", ObjectField<float>("RelativeNJS"));
					EditDropdown("Easing", ObjectField<int>("Easing"), Events.NJSEasings, false);
				}	break;
#if !CHROMPER_13
				case ObjectType.RotationEvent: {
					EditParsed("Rotation", ObjectField<float>("Rotation"), "Rotation in degrees");
				}	break;
#endif
				case ObjectType.EnvironmentEnhancement: {
					EditEnvironment(objects);
				}	break;
#if !CHROMPER_13
				case ObjectType.GLSColor:
				case ObjectType.GLSRotation:
				case ObjectType.GLSTranslation:
				case ObjectType.GLSEvent: {
					//AddParsed("Color", Data.GetSet<int>("Color"), false, "Test");
					//AddParsed("Brightness", Data.GetSet<float>("Brightness"), false, "Test");
					
					wipe();
					UI.AddLabel(panel!, "Unsupported", "GLS Unsupported!", Vector2.zero);
				}	break;
#endif
				default:
					Debug.LogError($"Unhandled object type: {type}");
					break;
			}
			UI.RefreshTooltips(panel);
			if (full_rebuild) {
				scrollbox!.ScrollTop();
			}
		}	break;
		case List<BaseMaterial> mats: {
			window!.SetTitle($"{mats.Count} Items selected");
			
			CheckRefresh(SelectionType.Materials);
			
			Converter<Color?, string?> cc = new(
				(c) => (c == null)
					? ""
					: (new JSONArray())
						.WriteColor(c ?? Color.white) // microslop can't make the damn ! operator work
						.ToString(),
				(s) => Data.RawToJson(s ?? "")?.ReadColor()
			);
			
			EditTextbox("Color", NullableField<Color?>("Color").Insert(cc), true);
			EditDropdown("Shader", NullableField<string>("Shader"), MapSettings.Shaders, false);
			EditTextbox("Track", NullableField<string?>("Track"), false, "Assign the material to a track, allowing you to animate the color.");
			
			ArrayEditor
				.Singleton(current_panel!, "Shader Keywords", "By default, each shader has its default keywords. This allows overwriting the keywords of the shader.")
				.Set(NullableField<List<string>>("ShaderKeywords"), true);
			
		}	break;
		default:
			window!.SetTitle("No items selected");
			old_otype = null;
			old_stype = null;
			wipe();
			break;
		}
		//Plugin.Trace($"End UpdateSelection: {old_otype}");
	}
	
	private void EditEnvironment(List<BaseObject> ees) {
		window!.SetTitle($"{ees.Count} Items selected");
		
		CheckRefresh(SelectionType.EnvironmentEnhancements);
		
		EditTextbox("ID", NullableField<string>("ID"), true);
		EditDropdown("Lookup Method", NullableField<int?>("LookupMethod"), (new Map<int?>()).AddEnum(typeof(EnvironmentLookupMethod)), false);
		EditCheckbox("Active", NullableField<JSONNode?>("Active").Insert(Data.JSONValue<bool?>()), true);
		EditParsed<int>("Duplicate", NullableField<int?>("Duplicate"));
		EditVector3("Scale", NullableField<Vector3?>("Scale"));
		EditVector3("Position", NullableField<Vector3?>("Position"));
		EditVector3("Local Position", NullableField<Vector3?>("LocalPosition"));
		EditVector3("Rotation", NullableField<Vector3?>("Rotation"));
		EditVector3("Local Rotation", NullableField<Vector3?>("LocalRotation"));
		EditTextbox("Track", NullableField<string?>("Track"));
		
		EditEEComponent("Geometry", GeometryField(), () => {
			EditDropdown("Type", CustomField<string>("type", "Geometry"), MapSettings.GeometryTypes, true);
			var materials = new Map<string?> {
				//{ "[Create New]", "[Create New]" }
			};
			materials.AddRange(BeatSaberSongContainer.Instance.Map.Materials.Keys);
			EditDropdown("Material", CustomField<string>("material", "Geometry"), materials, true);
			EditCheckbox("Collision", CustomField<bool?>("collision", "Geometry"), false);
		});
		
		EditEEComponent("Light", EEComponent("ILightWithId"), () => {
			EditParsed("Light ID", NullableField<int?>("LightID"));
			EditParsed("Light Type", NullableField<int?>("LightType"));
		});
		
		EditEEComponent("Bloom Fog", EEComponent("BloomFogEnvironment"), () => {
			EditParsed("Attenuation", CustomField<float?>("BloomFogEnvironment.attenuation", "Components"));
			EditParsed("Offset", CustomField<float?>("BloomFogEnvironment.offset", "Components"));
			EditParsed("Start Y", CustomField<float?>("BloomFogEnvironment.startY", "Components"));
			EditParsed("Height", CustomField<float?>("BloomFogEnvironment.height", "Components"));
		});
		
		EditEEComponent("Tube Bloom Pre Pass Light", EEComponent("TubeBloomPrePassLight"), () => {
			EditParsed("Color Alpha Multiplier", CustomField<float?>("TubeBloomPrePassLight.colorAlphaMultiplier", "Components"));
			EditParsed("Bloom Fog Intensity Multiplier", CustomField<float?>("TubeBloomPrePassLight.bloomFogIntensityMultiplier", "Components"));
		});
	}
	
	private void EditAnimations(PropertyType type, bool v2) {
		var CustomKeyAnimation = v2 ? "_animation" : "animation";
		
		Expando("Animations", "Animations", true);
		foreach (var property in Events.NoodleProperties) {
			if (property.Value[v2 ? 0 : 1] == "") continue;
			EditAnimation(property.Key, CustomKeyAnimation+"."+ property.Value[v2 ? 0 : 1], property.Value[2], tooltip.GetTooltip(type, $"Animate{property.Key}"));
		}
		EditAnimation("Definite Position", CustomKeyAnimation+"."+ (v2 ? "_definitePosition" : "definitePosition"), "[[0,0,0,0], [0,0,0,0.49]]", tooltip.GetTooltip(type, TooltipStrings.Tooltip.AssignPathAnimationDefinitePosition));
		panels.Pop();
	}
	
	private void EditMaterial() {
		if (bundleInfo?.Materials == null) {
			EditTextbox("Material", DataField<string>("asset"), false);
		}
		else {
			EditDropdown("Material", DataField<string>("asset"), bundleInfo.Materials, true);
		}
	}
	
	private void EditMaterialProperties() {
		var accessor = DataField<string>("asset");
		var asset = accessor.Get();
		if (asset != null) {
			var mat = bundleInfo?.Materials?.Forward(asset);
			if (mat != null && (bundleInfo?.Properties?.ContainsKey(mat) ?? false)) {
				panels.Push(Collapsible.Create(panel!, "Properties", "Properties", true).panel!);
				foreach (var prop in bundleInfo.Properties[mat]) {
					EditTextbox(prop.Key, PropertyValue(prop.Key, prop.Value.ToString()), true);
				}
				panels.Pop();
			}
		}
	}
	
	private void EditCameraProperties() {
		EditTextbox("Depth Texture Mode", DataFieldRaw("properties.depthTextureMode"), true);
		EditDropdown("Clear Flags", DataField<string>("properties.clearFlags"), Vivify.ClearFlags, true);
		EditTextbox("Background Colors", DataFieldRaw("properties.backgroundColor"), true);
		EditTracks("Culling Tracks", CustomField("properties.culling.track", "Data"));
		EditCheckbox("Culling Whitelist", DataField<bool?>("properties.culling.whitelist"), false);
		// Do these have defaults? Idk :3
		EditDropdown("Bloom Pre Pass", DataField<bool?>("properties.bloomPrePass"), MapSettings.OptionBool, true);
		EditDropdown("Main Bloom Effects", DataField<bool?>("properties.mainEffect"), MapSettings.OptionBool, true);
	}
	
	private void EditVivifyProperties() {
		var all_props = new Dictionary<string, Vivify.PropertyType>();
		foreach (var o in editing!) {
			var root = (o as BaseCustomEvent)!.Data ?? new JSONObject();
			if (Data.GetNode(root, "properties") is JSONArray props) {
				foreach (var prop in props.Children) {
					var id = (string)prop.AsObject["id"];
					System.Enum.TryParse((string)prop.AsObject["type"], out Vivify.PropertyType type);
					if (!all_props.ContainsKey(id)) {
						Plugin.Trace($"Add prop: {id}");
						all_props.Add(id, type);
					}
				}
			}
		}
		
		// Dynamic updates do not work, rebuild every time
		if (panel!.transform.Find("Properties") is Transform old) {
			GameObject.DestroyImmediate(old.gameObject);
		}
		
		panels.Push(Collapsible.Create(panel!, "Properties", "Properties", true).panel!);
		foreach (var prop in all_props) {
			var title = prop.Key + " Container";
			var container =  UI.AddChild(current_panel!, title);
			UI.AttachTransform(container, new Vector2(0, 20), pos: new Vector2(0, 0));
			panels.Push(container);
				var id_box = EditTextbox(null, PropertyComponent(prop.Key, "id"));
				UI.LeftColumn(id_box.gameObject);
				EditDropdown(null, PropertyComponent(prop.Key, "type"), Vivify.PropertyTypes, false);
			panels.Pop();
			var value_box = EditTextbox(null, PropertyValue(prop.Key, prop.Value.ToString()), true);
			UI.MoveTransform((RectTransform)value_box.transform, new Vector2(0, 22), new Vector2(0, 0));
		}
		EditTextbox("Add Property", PropertyComponent(null, "id"));
		panels.Pop();
	}
	
	private void EditPrefabProperties() {
		{
			var (has_any, mixed) = GetAllOrNothing<bool>(editing!, (o) => (o as BaseCustomEvent)?.Data?.HasKey("colorNotes") ?? false);
			panels.Push(Collapsible.Create(panel!, "_Color Notes", "Color Notes", has_any || mixed).panel!);
			EditTextbox("Track", DataFieldRaw("colorNotes.track"), true);
			EditPrefab("Asset", "colorNotes.asset");
			EditPrefab("Any Direction Asset", "colorNotes.anyDirectionAsset");
			EditPrefab("Debris Asset", "colorNotes.debrisAsset");
			panels.Pop();
		}
		{
			var (has_any, mixed) = GetAllOrNothing<bool>(editing!, (o) => (o as BaseCustomEvent)?.Data?.HasKey("burstSliders") ?? false);
			Expando("_Burst Sliders", "Burst Sliders", has_any || mixed);
			EditTextbox("Track", DataFieldRaw("burstSliders.track"), true);
			EditPrefab("Asset", "burstSliders.asset");
			EditPrefab("Debris Asset", "burstSliders.debrisAsset");
			panels.Pop();
		}
		{
			var (has_any, mixed) = GetAllOrNothing<bool>(editing!, (o) => (o as BaseCustomEvent)?.Data?.HasKey("burstSliderElemeents") ?? false);
			Expando("_Burst Slider Elements", "Burst Slider Elements", has_any || mixed);
			EditTextbox("Track", DataFieldRaw("burstSliderElemeents.track"), true);
			EditPrefab("Asset", "burstSliderElemeents.asset");
			EditPrefab("Debris Asset",  "burstSliderElemeents.debrisAsset");
			panels.Pop();
		}
		{
			var (has_any, mixed) = GetAllOrNothing<bool>(editing!, (o) => (o as BaseCustomEvent)?.Data?.HasKey("saber") ?? false);
			Expando("_Sabers", "Sabers", has_any || mixed);
			EditDropdown("Type", DataField<string>("saber.type"), Vivify.SaberTypes, false);
			EditPrefab("Asset", "saber.asset");
			EditPrefab("Trail Asset", "saber.trailAsset");
			EditTextbox("Trail Top Position", DataFieldRaw("saber.trailTopPos"), true);
			EditTextbox("Trail Bottom Position", DataFieldRaw("saber.trailBottomPos"), true);
			EditParsed("Trail Duration", DataField<float?>("saber.trailDuration"));
			EditParsed("Trail Sampling Frequency", DataField<int?>("saber.trailSamplingFrequency"));
			EditParsed("Trail Granularity", DataField<int?>("saber.trailGranularity"));
			panels.Pop();
		}
		panels.Pop();
	}
	
	private Enums.Map<int?>? GetEventLanes() {
		if (EventLanes == null) {
			var _etlabels = (CreateEventTypeLabels)Object.FindFirstObjectByType(typeof(CreateEventTypeLabels));
#if CHROMPER_13
			EventLanes = new();
			
			for (int i = 0; i <= _etlabels.MaxLaneId(); ++i) {
				var type = _etlabels.LaneIdToEventType(i);
				
				var lane = _etlabels.LayerInstantiate.transform.parent.GetChild(type + 1);
				var textMesh = lane.GetComponentInChildren<TMPro.TextMeshProUGUI>();
				
				Plugin.Trace($"{i} {type} {textMesh?.text}");
				
				EventLanes.Add(type, textMesh?.text ?? "");
			}
#else
			var context = (BeatmapRuntimeContext)Object.FindFirstObjectByType(typeof(BeatmapRuntimeContext));
			
			var entries = context?.TracksDefinition?.Basic?.ToList() ?? null;
			
			if (entries == null) return null;
			
			EventLanes = new();
			
			for (int i = 0; i < entries.Count; ++i) {
				EventLanes.Add(entries[i].Value.Type, entries[i].Value.Name);
			}
#endif
		}
		return EventLanes;
	}
	
}

}
