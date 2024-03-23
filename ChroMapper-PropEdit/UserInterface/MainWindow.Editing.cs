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
			var v2 = (o is V2Object);
			var animation_branch = (System.Type.GetType("Beatmap.Animations.ObjectAnimator, Main, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null") is System.Type);
			
			current_panel = panel;
			
			AddParsed("Beat", Data.GetSet<float>("JsonTime"), true, "A specific point in time, as determined by the BPM of the song, when this object should reach the player");
			
			switch (type) {
				case ObjectType.Note:
					var note = (o as BaseNote)!;
					AddParsed("X", Data.GetSet<int>("PosX"), false, "The horizontal row where the note should reside on the grid. The indices run from 0 to 3, with 0 being the left-most lane");
					AddParsed("Y", Data.GetSet<int>("PosY"), false, "The vertical column where the note should reside on the grid. The indices run from 0 to 2, with 0 being the bottom-most lane");
					AddDropdown<int?>("Type", Data.GetSet<int>("Type"), Notes.NoteTypes, false, "Indicates which saber should be able to successfully cut the note. Allows to change to a bomb");
					AddDropdown<int?>("Direction", Data.GetSet<int>("CutDirection"), Notes.CutDirections, false, "Indicates the direction the player should swing to successfully cut the note");
					if (!v2) {
						AddParsed("Angle Offset", Data.GetSet<int>("AngleOffset"), false, "A value (in degrees) which applies a counter-clockwise rotational offset to the note's cut direction.");
					}
					AddLine("");
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
						current_panel = collapsible.panel;
						AddColor("Color", o.CustomKeyColor, "Changes the color and opacity of the note. Is displayed in Hex colors");
						if (o is V2Note) {
							AddCheckbox("Disable Spawn Effect", Data.CustomGetSet<bool?>("_disableSpawnEffect"), false, "Disables the light effect, that occurs when the note spawns");
						}
						else {
							AddCheckbox("Spawn Effect", Data.CustomGetSet<bool?>("spawnEffect"), true, "Toggles the light spawn effect, that occurs when the note spawns");
							AddCheckbox("Disable Debris", Data.CustomGetSet<bool?>("disableDebris"), false, "Disables the debris that occurs when cutting through the note");
						}
						current_panel = panel;
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, NOODLE_NAME, "Noodle Extensions", true);
						current_panel = collapsible.panel;
						AddParsed("NJS", Data.CustomGetSet<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"), false, "Changes the note jump speed (NJS) of the note");
						AddParsed("Spawn Offset", Data.CustomGetSet<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"), false, "Changes the jump distance (JD) of the note");
						AddTextbox("Coordinates", Data.CustomGetSetRaw(note.CustomKeyCoordinate), true, "Allows to set the coordinates [x,y,z] of a note. Keep in mind, that the center [0,0,0] is different from vanilla coordinates");
						AddTextbox("Rotation", Data.CustomGetSetRaw(note.CustomKeyWorldRotation), true, "Allows to set the global rotation [x,y,z]/[yaw, pitch, roll] of a note. [0,0,0] is always the rotation, that faces the player");
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(note.CustomKeyLocalRotation), true, "Allows to set the local rotation [x,y,z]/[yaw, pitch, roll] of a note. This won't affect the direction it spawns from or the path it takes");
						if (o is V2Note) {
							AddParsed("Exact Angle", Data.CustomGetSet<float?>("_cutDirection"), false, "A value (in degrees) which applies a counter-clockwise rotational offset to the note's cut direction.");
							AddCheckbox("Fake", Data.CustomGetSet<bool?>("_fake"), false, "If activated, the note will not count towards your score or combo, meaning, if you miss it, it won't have any effect");
							AddCheckbox("Interactable", Data.CustomGetSet<bool?>("_interactable"), true, "If deactivated, the note cannot be interacted with / cut through");
							AddTextbox("Flip", Data.CustomGetSetRaw("_flip"), true, "Allows you to change how the note spawns. [flip line index, flip jump] Flip line index is the initial x the note will spawn at and flip jump is how high (or low) the note will jump up (or down) when flipping to its true position. Base game behaviour will set one note's flip jump to -1 and the other to 1.");
						}
						else {
							if (animation_branch) {
								AddCheckbox("Fake", Data.GetSet<bool>("CustomFake"), null, "If activated, the note will not count towards your score or combo, meaning, if you miss it, it won't have any effect");
								AddCheckbox("Uninteractable", Data.CustomGetSet<bool?>("uninteractable"), false, "If activated, the note cannot be interacted with / cut through.");
							}
							AddCheckbox("Disable Gravity", Data.CustomGetSet<bool?>("disableNoteGravity"), false, "If disabled, the note will no longer do their animation where they float up"); //subject to change
							AddCheckbox("Disable Look", Data.CustomGetSet<bool?>("disableNoteLook"), false, "If disabled, the note will no longer try to face the player as it comes closer to them");
							AddCheckbox("No Badcut Direction", Data.CustomGetSet<bool?>("disableBadCutDirection"), false, "If activated, the note cannot be badcut from a wrong direction, meaning it will go straight through");
							AddCheckbox("No Badcut Speed", Data.CustomGetSet<bool?>("disableBadCutSpeed"), false, "If activated, the note cannot be badcut with insufficient/slow speed, meaning it will go straight through"); //unsure
							AddCheckbox("No Badcut Color", Data.CustomGetSet<bool?>("disableBadCutSaberType"), false, "If activated, the note cannot be badcut with the wrong saber, meaning it will go straight through");
							AddTextbox("Flip", Data.CustomGetSetRaw("flip"), true, "Allows you to change how the note spawns. [flip line index, flip jump] Flip line index is the initial x the note will spawn at and flip jump is how high (or low) the note will jump up (or down) when flipping to its true position. Base game behaviour will set one note's flip jump to -1 and the other to 1.");
							AddTextbox("Link", Data.CustomGetSet<string>("link"), false, "When cut, all notes that share the same link string will also be cut");
						}
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true, "Groups the note together with different objects, that have the same track name/string"); //prob. needs more info
						AddAnimation(v2);
						current_panel = panel;
					}
					
					break;
				case ObjectType.CustomNote:
					AddLine("Wow, a custom note! How did you do this?");
					break;
				case ObjectType.Arc: {
					AddParsed("Head X", Data.GetSet<int>("PosX"), false, "The horizontal row where the head arc should reside on the grid. The indices run from 0 to 3, with 0 being the left-most lane");
					AddParsed("Head Y", Data.GetSet<int>("PosY"), false, "The vertical column where the head arc should reside on the grid. The indices run from 0 to 2, with 0 being the bottom-most lane");
					AddDropdown<int?>("Color", Data.GetSet<int>("Color"), Notes.ArcColors, false, "Indicates which saber this arc should attach to.");
                    AddDropdown<int?>("Direction", Data.GetSet<int>("CutDirection"), Notes.CutDirections, false, "Indicates the direction the head should curve from relative to the note it's attached to.");
					AddParsed("Head Multiplier", Data.GetSet<float>("HeadControlPointLengthMultiplier"), false, "A value that controls the magnitude of the curve approaching the head respectively. \n If the Cut Direction is set to \"Any\"(8), this value is ignored.");
					AddParsed("Tail Beat", Data.GetSet<float>("TailJsonTime"), false, "A specific point in time, as determined by the BPM of the song, when this tail should reach the player"); 
					AddParsed("Tail X", Data.GetSet<int>("TailPosX"), false, "The horizontal row where the tail arc should reside on the grid. The indices run from 0 to 3, with 0 being the left-most lane");
					AddParsed("Tail Y", Data.GetSet<int>("TailPosY"), false, "The vertical column where the tail arc should reside on the grid. The indices run from 0 to 2, with 0 being the bottom-most lane");
					AddDropdown<int?>("Tail Direction", Data.GetSet<int>("TailCutDirection"), Notes.CutDirections, false, "Indicates the direction the tail should curve from relative to the note it's attached to.");
					AddParsed("Tail Multiplier", Data.GetSet<float>("TailControlPointLengthMultiplier"), false, "A value that controls the magnitude of the curve approaching the tail respectively.\n If the Cut Direction is set to \"Any\"(8), this value is ignored.");
					AddLine("");
					
					var s = (o as BaseSlider)!;
					
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
						current_panel = collapsible.panel;
						AddColor("Color", o.CustomKeyColor, "Changes the color and opacity of the note. Is displayed in hex numbers");
						current_panel = panel;
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, NOODLE_NAME, "Noodle Extensions", true);
						current_panel = collapsible.panel;
						AddParsed("NJS", Data.CustomGetSet<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"), false, "Changes the note jump speed (NJS) of the arc");
						AddParsed("Spawn Offset", Data.CustomGetSet<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"), false, "Changes the jump distance (JD) of the arc");
						AddTextbox("Head Coordinates", Data.CustomGetSetRaw(s.CustomKeyCoordinate), true, "Allows to set the coordinates [x,y,z] of the head arc. Keep in mind, that the center [0,0,0] is different from vanilla coordinates");
						AddTextbox("Tail Coordinates", Data.CustomGetSetRaw(s.CustomKeyTailCoordinate), true, "Allows to set the coordinates [x,y,z] of the tail arc. Keep in mind, that the center [0,0,0] is different from vanilla coordinates");
						AddTextbox("Rotation", Data.CustomGetSetRaw(s.CustomKeyWorldRotation), true, "Allows to set the global rotation [x,y,z]/[yaw, pitch, roll] of the arc. [0,0,0] is always the rotation, that faces the player");
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(s.CustomKeyLocalRotation), true, "Allows to set the local rotation [x,y,z]/[yaw, pitch, roll] of the arc. This won't affect the direction it spawns from or the path it takes");
						if (v2) {
							AddCheckbox("Interactable", Data.CustomGetSet<bool?>("_interactable"), true, "If deactivated, the arc cannot be interacted with. ");
							AddTextbox("Flip", Data.CustomGetSetRaw("_flip"), true, "Allows you to change how the arc spawns. [flip line index, flip jump] Flip line index is the initial x the arc will spawn at and flip jump is how high (or low) the arc will jump up (or down) when flipping to its true position. Base game behaviour will set one arc's flip jump to -1 and the other to 1."); //not sure if this works
						}
						else {
							//AddCheckbox("Uninteractable", Data.CustomGetSet<bool?>("uninteractable"), false);
							AddCheckbox("Disable Gravity", Data.CustomGetSet<bool?>("disableNoteGravity"), false, "If disabled, the arc will no longer do their animation where they float up");
							AddTextbox("Flip", Data.CustomGetSetRaw("flip"), true, "Allows you to change how the arc spawns. [flip line index, flip jump] Flip line index is the initial x the arc will spawn at and flip jump is how high (or low) the arc will jump up (or down) when flipping to its true position. Base game behaviour will set one arc's flip jump to -1 and the other to 1.");
							AddTextbox("Link", Data.CustomGetSet<string>("link"), false, "When cut, all arcs that share the same link string will also be cut");
						}
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true, "Groups the arc together with different objects, that have the same track name/string.");
						AddAnimation(v2);
						current_panel = panel;
					}
					
				}	break;
				case ObjectType.Chain: {
					AddParsed("Head X", Data.GetSet<int>("PosX"), false, "The horizontal row where the head should reside on the grid. The indices run from 0 to 3, with 0 being the left-most lane");
					AddParsed("Head Y", Data.GetSet<int>("PosY"), false, "The vertical column where the head should reside on the grid. The indices run from 0 to 2, with 0 being the bottom-most lane");
					AddDropdown<int?>("Color", Data.GetSet<int>("Color"), Notes.ArcColors, false, "Indicates which saber should be able to successfully cut the chain");
					AddDropdown<int?>("Direction", Data.GetSet<int>("CutDirection"), Notes.CutDirections, false, "Indicates the direction the player should swing to successfully cut the head of the chain");
					AddParsed("Slices", Data.GetSet<int>("SliceCount"), false, "An integer value which represents the number of segments in the chain. The head counts as a segment");
					AddParsed("Squish", Data.GetSet<float>("Squish"), false, "An integer value which represents the proportion of how much of the path from Head (x, y) to Tail(tx, ty) is used by the chain. This does not alter the shape of the path.");
					AddParsed("Tail X", Data.GetSet<int>("TailPosX"), false, "The horizontal row where the tail should reside on the grid. The indices run from 0 to 3, with 0 being the left-most lane");
					AddParsed("Tail Y", Data.GetSet<int>("TailPosY"), false, "The vertical column where the tail should reside on the grid. The indices run from 0 to 2, with 0 being the bottom-most lane");
					AddLine("");
					
					var s = (o as BaseSlider)!;
					
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
						current_panel = collapsible.panel;
						AddColor("Color", o.CustomKeyColor, "Changes the color and opacity of the note. Is displayed in hex numbers");
						current_panel = panel;
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, NOODLE_NAME, "Noodle Extensions", true);
						current_panel = collapsible.panel;
						AddParsed("NJS", Data.CustomGetSet<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"));
						AddParsed("Spawn Offset", Data.CustomGetSet<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"));
						AddTextbox("Head Coordinates", Data.CustomGetSetRaw(s.CustomKeyCoordinate), true, "Allows to set the coordinates [x,y,z] of the head chain. Keep in mind, that the center [0,0,0] is different from vanilla coordinates");
						AddTextbox("Tail Coordinates", Data.CustomGetSetRaw(s.CustomKeyTailCoordinate), true, "Allows to set the coordinates [x,y,z] of the tail chain. Keep in mind, that the center [0,0,0] is different from vanilla coordinates");
						AddTextbox("Rotation", Data.CustomGetSetRaw(s.CustomKeyWorldRotation), true, "Allows to set the global rotation [x,y,z]/[yaw, pitch, roll] of the chain. [0,0,0] is always the rotation, that faces the player");
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(s.CustomKeyLocalRotation), true, "Allows to set the local rotation [x,y,z]/[yaw, pitch, roll] of the  chain. This won't affect the direction it spawns from or the path it takes");
						if (animation_branch) {
							AddCheckbox("Fake", Data.GetSet<bool>("CustomFake"), null, "If activated, the chain will not count towards your score or combo, meaning, if you miss it, it won't have any effect");
							AddCheckbox(v2 ? "Interactable" : "Uninteractable", Data.CustomGetSet<bool?>(v2 ? "_interactable" : "uninteractable"), v2, $"If {(v2 ? "deactivated" : "activated")}, the chain will not count towards your score or combo, meaning, if you miss it, it won't have any effect");
						}
						if (v2) {
							AddTextbox("Flip", Data.CustomGetSetRaw("_flip"), true, "Allows you to change how the chain spawns. [flip line index, flip jump] Flip line index is the initial x the chain will spawn at and flip jump is how high (or low) the chain will jump up (or down) when flipping to its true position. Base game behaviour will set one chain's flip jump to -1 and the other to 1.");
						}
						else {
							AddCheckbox("Disable Gravity", Data.CustomGetSet<bool?>("disableNoteGravity"), false, "If disabled, the chain will no longer do their animation where they float up");
							AddTextbox("Flip", Data.CustomGetSetRaw("flip"), true, "Allows you to change how the chain spawns. [flip line index, flip jump] Flip line index is the initial x the chain will spawn at and flip jump is how high (or low) the chain will jump up (or down) when flipping to its true position. Base game behaviour will set one chain's flip jump to -1 and the other to 1.");
							AddTextbox("Link", Data.CustomGetSet<string>("link"), false, "When cut, all chains that share the same link string will also be cut");
						}
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true, "Groups the chain together with different objects, that have the same track name/string");
						AddAnimation(v2);
						current_panel = panel;
					}
					
				}	break;
				case ObjectType.Obstacle:
					var ob = (o as BaseObstacle)!;
					AddParsed("Duration", Data.GetSet<float>("Duration"), false, "A value (in beats) that determines how long the obstacle extends for.");
					if (o is V2Obstacle) {
						AddParsed("X", Data.GetSet<int>("PosX"), false, "The horizontal row where the obstacle should reside on the grid. The indices run from 0 to 3, with 0 being the left-most lane");
						AddParsed("Width", Data.GetSet<int>("Width"), false, "An integer value which represents how many columns the obstacle should take up on the grid.");
						AddDropdown<int?>("Height", Data.GetSet<int>("Type"), Obstacles.WallHeights, false, "An integer value which represents how many rows the obstacle should take up on the grid. The range of acceptable values runs from 1 to 5.");
					}
					else {
						AddParsed("X (Left)", Data.GetSet<int>("PosX"), false, "The horizontal row where the obstacle should reside on the grid. The indices run from 0 to 3, with 0 being the left-most lane");
						AddParsed("Y (Bottom)", Data.GetSet<int>("PosY"), false, "The vertical column where the obstacle should reside on the grid. The indices run from 0 to 2, with 0 being the bottom-most lane");
						AddParsed("Width", Data.GetSet<int>("Width"), false, "An integer value which represents how many columns the obstacle should take up on the grid.");
						AddParsed("Height", Data.GetSet<int>("Height"), false, "An integer value which represents how many rows the obstacle should take up on the grid. The range of acceptable values runs from 1 to 5.");
					}
					AddLine("");
					
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
						current_panel = collapsible.panel;
						AddColor("Color", o.CustomKeyColor, "Changes the color and opacity of the note. Is displayed in hex numbers");
						current_panel = panel;
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, NOODLE_NAME, "Noodle Extensions", true);
						current_panel = collapsible.panel;
						AddParsed("NJS", Data.CustomGetSet<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"), false, "Changes the note jump speed (NJS) of the obstacle");
						AddParsed("Spawn Offset", Data.CustomGetSet<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"), false, "Changes the jump distance (JD) of the obstacle");
						AddTextbox("Position", Data.CustomGetSetRaw(ob.CustomKeyCoordinate), true, "Allows to set the coordinates [x,y,z] of an obstacle. Keep in mind, that the center [0,0,0] is different from vanilla coordinates");
						AddTextbox("Rotation", Data.CustomGetSetRaw(ob.CustomKeyWorldRotation), true, "Allows to set the global rotation [x,y,z]/[yaw, pitch, roll] of the obstacle. [0,0,0] is always the rotation, that faces the player.");
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(ob.CustomKeyLocalRotation), true, "Allows to set the local rotation [x,y,z]/[yaw, pitch, roll] of the obstacle. This won't affect the direction it spawns from or the path it takes.");
						AddTextbox("Size", Data.CustomGetSetRaw(ob.CustomKeySize), true, "The width, height and length of a wall[w, h, l]. [1, 1, 1] will be perfectly square. Each number is fully optional.");
						if (animation_branch) {
							AddCheckbox("Fake", Data.GetSet<bool>("CustomFake"), null, "If activated, the obstacle will not count towards your score or combo, meaning, if you go into it, it won't have any effect");
						}
						if (o is V2Obstacle) {
							AddCheckbox("Interactable", Data.CustomGetSet<bool?>("_interactable"), true, "If deactivated, the obstacle cannot be interacted with.");
						}
						else {
							AddCheckbox("Uninteractable", Data.CustomGetSet<bool?>("uninteractable"), false, "If activated, the obstacle cannot be interacted with."); // not sure if this means that it will screw up your score
						}
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true, "Groups the obstacle together with different objects, that have the same track name/string");
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
							AddDropdown<int?>("Color", Data.GetSetSplitValue(0b1100), Events.LightColors, false, "Changes the vanilla color of the event. Allows to be set to the saber colors, white and off");
							AddDropdown<int?>("Action", Data.GetSetSplitValue(0b0011), Events.LightActions, false, "Determines how the event should behave");
						}
						else {
							AddDropdown<int?>("Value", Data.GetSet<int>("Value"), Events.LightValues, false, "Changes the color and behaviour of the event");
						}
						AddParsed("Brightness", Data.GetSet<float>("FloatValue"), false, "Used to control the brightness of the event. A value of 0 will turn the light off.");
						AddLine("");
						
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
							current_panel = collapsible.panel;
							AddTextbox("LightID", Data.CustomGetSetRaw(f.CustomKeyLightID), true, "Causes the event to only affect the specified ID. Can be an array.");
							AddColor("Color", o.CustomKeyColor, "Changes the color and opacity of the event. Is displayed in Hex colors");
							if (events.Where(e => e.IsTransition).Count() == editing.Count()) {
								AddDropdown<string>("Easing",    Data.CustomGetSet<string>(f.CustomKeyEasing), Events.Easings, true, "The easing effect that the event should use. Check out \"easings.net\" for visualization examples");
								AddDropdown<string>("Lerp Type", Data.CustomGetSet<string>(f.CustomKeyLerpType), Events.LerpTypes, true, "Determines, in what way the color should transition. RGB transitions the color, by changing every value. HSV transitions the color, by primarly changing its hue"); //Unsure
							}
							if (o is V2Event e) {
								AddCheckbox("V2 Gradient", Data.GetSetGradient(), false, "If activated, allows to set a color gradient");
								if (e.CustomLightGradient != null) {
									AddParsed("Duration",     Data.CustomGetSet<float?>($"{e.CustomKeyLightGradient}._duration"), false, "A value (in beats) that determines how long the gradient extends for.");
									AddColor("Start Color", $"{e.CustomKeyLightGradient}._startColor", "Changes the color and opacity of the start gradient. Is displayed in Hex colors");
									AddColor("End Color", $"{e.CustomKeyLightGradient}._endColor",  "Changes the color and opacity of the end gradient. Is displayed in Hex colors");
									AddDropdown<string>("Easing",    Data.CustomGetSet<string>($"{e.CustomKeyLightGradient}._easing"), Events.Easings, false, "The easing effect that the event should use. Check out \"easings.net\" for visualization examples");
								}
							}
							current_panel = panel;
						}
					}
					// Laser Speeds
					if (events.Where(e => e.IsLaserRotationEvent(env)).Count() == editing.Count()) {
						AddParsed("Speed", Data.GetSet<int>("Value"), false, "The ");
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
	
	private void AddColor(string label, string key, string tooltip = "") {
		if (Settings.Get(Settings.ColorHex, true)) {
			AddTextbox(label, Data.CustomGetSetColor(key), false, tooltip);
		}
		else {
			AddTextbox(label, Data.CustomGetSetRaw(key), true, tooltip);
		}
	}
}

}
