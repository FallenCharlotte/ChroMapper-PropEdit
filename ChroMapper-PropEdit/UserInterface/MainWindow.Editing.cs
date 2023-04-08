using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

using Beatmap.Base;
using Beatmap.Enums;
using Beatmap.Helper;
using Beatmap.V2;
using Beatmap.V3;

using ChroMapper_PropEdit.Components;
using ChroMapper_PropEdit.Enums;

using Convert = System.Convert;

namespace ChroMapper_PropEdit.UserInterface {

public partial class MainWindow {
	public void UpdateSelection(bool real) {
		foreach (var e in elements) {
			Object.Destroy(e);
		}
		elements.Clear();
		
		editing = SelectionController.SelectedObjects.Select(it => it);
		
		if (SelectionController.HasSelectedObjects()) {
			title.GetComponent<TextMeshProUGUI>().text = SelectionController.SelectedObjects.Count + " Items selected";
			
			if (editing.GroupBy(o => o.ObjectType).Count() > 1) {
				elements.Add(UI.AddLabel(panel.transform, "Unsupported", "Multi-Type Unsupported!", new Vector2(0, 0)));
				return;
			}
			
			var o = editing.First();
			var type = o.ObjectType;
			
			AddParsed("Beat", Data.GetSet<float>(typeof(BaseObject), "Time"));
			
			switch (type) {
				case ObjectType.Note:
					var note = o as BaseNote;
					AddDropdownI("Type", Data.GetSet<int>(typeof(BaseNote), "Type"), Notes.NoteTypes);
					AddDropdownI("Direction", Data.GetSet<int>(typeof(BaseNote), "CutDirection"), Notes.CutDirections);
					if (o is V3ColorNote) {
						AddParsed("Angle Offset", Data.GetSet<int>(typeof(BaseNote), "AngleOffset"));
					}
					AddField("");
					AddField("Chroma");
					AddTextbox("Color", Data.CustomGetSetColor(o.CustomKeyColor));
					if (o is V2Note) {
						AddCheckbox("Disable Spawn Effect", Data.CustomGetSet<bool>("_disableSpawnEffect"), false);
					}
					else {
						AddCheckbox("Disable Spawn Effect", Data.CustomGetSet<bool>("spawnEffect"), true);
					}
					
					AddField("");
					AddField("Noodle Extensions");
					AddParsed("Direction", Data.GetSet<int>(typeof(BaseNote), "CustomDirection"));
					AddTextbox("Position", Data.CustomGetSetJSON(note.CustomKeyCoordinate), true);
					AddTextbox("Rotation", Data.CustomGetSetJSON(note.CustomKeyWorldRotation), true);
					AddTextbox("Local Rotation", Data.CustomGetSetJSON(note.CustomKeyLocalRotation), true);
					if (o is V2Note) {
						AddParsed("NJS", Data.CustomGetSet<float>("_noteJumpMovementSpeed"));
						AddParsed("Spawn Offset", Data.CustomGetSet<float>("_noteJumpStartBeatOffset"));
						AddCheckbox("Fake", Data.CustomGetSet<bool>("_fake"), false);
						AddCheckbox("Interactable", Data.CustomGetSet<bool>("_interactable"), true);
						AddTextbox("Flip", Data.CustomGetSetJSON("_flip"), true);
					}
					else {
						AddParsed("NJS", Data.CustomGetSet<float>("noteJumpMovementSpeed"));
						AddParsed("Spawn Offset", Data.CustomGetSet<float>("noteJumpStartBeatOffset"));
						AddCheckbox("Interactable", Data.CustomGetSet<bool>("uninteractable"), false);
						AddCheckbox("Disable Gravity", Data.CustomGetSet<bool>("disableNoteGravity"), false);
						AddCheckbox("Disable Look", Data.CustomGetSet<bool>("disableNoteLook"), false);
						AddTextbox("Flip", Data.CustomGetSetJSON("flip"), true);
					}
					
					break;
				case ObjectType.Obstacle:
					var ob = o as BaseObstacle;
					AddParsed("Duration", Data.GetSet<float>(typeof(BaseObstacle), "Duration"));
					AddDropdownI("Height", Data.GetSet<int>(typeof(BaseObstacle), "Type"), Obstacles.WallHeights);
					AddParsed("Width", Data.GetSet<int>(typeof(BaseObstacle), "Width"));
					
					AddField("");
					AddField("Chroma");
					AddTextbox("Color", Data.CustomGetSetColor(o.CustomKeyColor));
					
					AddField("");
					AddField("Noodle Extensions");
					AddTextbox("Position", Data.CustomGetSetJSON(ob.CustomKeyCoordinate), true);
					AddTextbox("Rotation", Data.CustomGetSetJSON(ob.CustomKeyWorldRotation), true);
					AddTextbox("Local Rotation", Data.CustomGetSetJSON(ob.CustomKeyLocalRotation), true);
					AddTextbox("Scale", Data.CustomGetSetJSON(ob.CustomKeySize), true);
					if (o is V2Obstacle) {
						AddParsed("NJS", Data.CustomGetSet<float>("_noteJumpMovementSpeed"));
						AddParsed("Spawn Offset", Data.CustomGetSet<float>("_noteJumpStartBeatOffset"));
						AddCheckbox("Fake", Data.CustomGetSet<bool>("_fake"), false);
						AddCheckbox("Interactable", Data.CustomGetSet<bool>("_interactable"), true);
					}
					else {
						AddParsed("NJS", Data.CustomGetSet<float>("noteJumpMovementSpeed"));
						AddParsed("Spawn Offset", Data.CustomGetSet<float>("noteJumpStartBeatOffset"));
						AddCheckbox("Interactable", Data.CustomGetSet<bool>("uninteractable"), false);
					}
					
					break;
				case ObjectType.Event:
					var env = BeatSaberSongContainer.Instance.Song.EnvironmentName;
					var events = editing.Select(o => (BaseEvent)o);
					var f = events.First();
					// Light
					if (events.Where(e => e.IsLightEvent(env)).Count() == editing.Count()) {
						if (settings["split_val"]) {
							AddDropdownI("Color", Data.GetSetSplitValue(0b1100), Events.LightColors);
							AddDropdownI("Action", Data.GetSetSplitValue(0b0011), Events.LightActions);
						}
						else {
							AddDropdownI("Value", Data.GetSet<int>(typeof(BaseEvent), "Value"), Events.LightValues);
						}
						
						AddField("");
						AddField("Chroma");
						AddTextbox("LightID", Data.CustomGetSetJSON(f.CustomKeyLightID), true);
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
					// Laser Speeds
					if (events.Where(e => e.IsLaserRotationEvent(env)).Count() == editing.Count()) {
						AddParsed("Speed", Data.GetSet<int>(typeof(BaseEvent), "Value"));
						AddField("");
						AddField("Chroma");
						AddCheckbox("Lock Rotation", Data.CustomGetSet<bool> (f.CustomKeyLockRotation), false);
						AddDropdownI("Direction",     Data.CustomGetSet<int>  (f.CustomKeyDirection), Events.LaserDirection, true);
						AddParsed("Precise Speed",   Data.CustomGetSet<float>(f.CustomKeyPreciseSpeed));
					}
					if (events.Where(e => e.Type == (int)EventTypeValue.RingRotation).Count() == editing.Count()) {
						AddField("");
						AddField("Chroma");
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
					if (events.Where(e => e.Type == (int)EventTypeValue.RingZoom).Count() == editing.Count()) {
						AddField("");
						AddField("Chroma");
						AddParsed("Step",  Data.CustomGetSet<float>(f.CustomKeyStep));
						AddParsed("Speed", Data.CustomGetSet<float>(f.CustomKeySpeed));
					}
					break;
			}
		}
		else {
			title.GetComponent<TextMeshProUGUI>().text = "No items selected";
		}
		if (real) {
			scroll_to_top.Trigger();
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
			new ActionCollectionAction(beatmapActions, true, false, $"Edited ({SelectionController.SelectedObjects.Count()}) objects with Prop Edit."),
			true);
		
		// Prevent selecting "--"
		UpdateSelection(false);
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
			new ActionCollectionAction(beatmapActions, true, false, $"Edited ({SelectionController.SelectedObjects.Count()}) objects with Prop Edit."),
			true);
		
		// Prevent selecting "--"
		UpdateSelection(false);
	}
}

}
