using System;
using System.Collections.Generic;
using UnityEngine;

using ChroMapper_PropEdit.Enums;

namespace ChroMapper_PropEdit.UserInterface {
	public class TooltipStrings {
		public static TooltipStrings Instance {
			get {
				if (_instance == null) {
					_instance = new TooltipStrings();
				}
				return _instance;
			}
		}
		private static TooltipStrings? _instance = null;
		
		private Dictionary<PropertyType, string> objects = new Dictionary<PropertyType, string>() {
			{ PropertyType.Note, "note" },
			{ PropertyType.CustomNote, "custom note" },
			{ PropertyType.Arc, "arc" },
			{ PropertyType.ArcHead, "arc head" },
			{ PropertyType.ArcTail, "arc tail" },
			{ PropertyType.Chain, "chain" },
			{ PropertyType.ChainHead, "chain head" },
			{ PropertyType.ChainTail, "chain tail" },
			{ PropertyType.Obstacle, "obstacle" },
			{ PropertyType.Event, "event" },
			{ PropertyType.CustomEvent, "custom event" },
			{ PropertyType.BpmChange, "BPM change" },
			{ PropertyType.Object, "object"},
			{ PropertyType.Gradient, "gradient"},
			{ PropertyType.GradientStart, "gradient start"},
			{ PropertyType.GradientEnd, "gradient end"},
		};
		
		public enum Tooltip {
			Beat,
			BeatEvent,
			X,
			Y,
			Type,
			CutDirection,
			AngleOffset,
			Color,
			DisableSpawnEffect,
			DisableDebris,
			SpawnEffect,
			NJS,
			SpawnOffset,
			Coordinates,
			Rotation,
			LocalRotation,
			Fake,
			Flip,
			DisableGravity,
			DisableLook,
			NoBadcutDirection,
			NoBadcutSpeed,
			NoBadcutColor,
			Track,
			Interactable,
			Uninteractable,
			Link,
			Multiplier,
			TailMultiplier,
			MidAnchorMode,
			Slices,
			Squish,
			Duration,
			Width,
			Height,
			Size,
			EventColor,
			EventAction,
			LegacyEventType,
			Brightness,
			LightID,
			Easing,
			V2Easing,
			LerpType,
			V2Gradient,
			V2Duration,
			
			LaserSpeed,
			LockRotation,
			LaserDirection,
			PreciseSpeed,
			
			RingFilter,
			RingRotation,
			RingStep,
			RingPropagation,
			RingSpeed,
			RingDirection,
			
			RingV2CounterSpin,
			RingV2Reset,
			
			RingZoomStep,
			
			BoostColorSet,
			LaneRotation,
			
			BPMChange,
			
			TrackDuration,
			TrackEasing,
			TrackProperty,
			TrackRepeat,
			
			AnimatePath,
			
			AnimateColor,
			AnimatePosition,
			AnimateLocalPosition,
			AnimateOffsetPosition,
			AnimateLocalRotation,
			AnimateWorldRotation,
			AnimateOffsetWorldRotation,
			AnimateScale,
			AnimateDissolve,
			AnimateDissolveArrow,
			AnimateInteractable,
			AnimateTime,
			
			AssignTrackParentParent,
			AssignTrackChildren,
			AssignTrackKeepPosition,
			
			V2AssignFogTrackAttenuation,
			V2AssignFogTrackOffset,
			V2AssignFogTrackStartY,
			V2AssignFogTrackHeight,
			
			AssignPathAnimationDefinitePosition,
			
			AssignPlayerToTrackTrack,
			AssignPlayerToTrackTarget,
			
			//There is another track called AnimateComponent, https://github.com/Aeroluna/Heck/wiki/Environment/#Components
			//https://github.com/Aeroluna/Heck/wiki/Animation#AnimateComponent
			AnimateComponentILightWithId,
			AnimateComponentLightID,
			AnimateComponentType,
			AnimateComponentBloomFogEnvironment,
			AnimateComponentTubeBloomPrePassLight,
			AnimateComponentTubeBloomPrePassLightColorAlphaMultiplier,
			AnimateComponentTubeBloomPrePassLightBloomFogIntensityMultiplier,
			
			// Settings
			
			// PropEdit Settings
			ShowChroma,
			ShowNoodleExtensions,
			SplitLightValues,
			ColorsAsHex,
			ShowTooltips,
			ForceLanes,
			
			// Requirements
			Requirement,
			ModReq,
			ChromaReq,
			LegacyChromaReq,
			MappingExtensionsReq,
			NoodleExtensionsReq,
			CinemaReq,
			SoundExtensionsReq,
			
			OverrideModReq,
			OverrideChromaReq,
			OverrideLegacyChromaReq,
			OverrideMappingExtensionsReq,
			OverrideNoodleExtensionsReq,
			OverrideCinemaReq,
			OverrideSoundExtensionsReq,
			
			// Misc
			Information,
			Warning,
			
			// Map Options
			MapOptions,
			
			// Player Options
			LeftHanded,
			PlayerHeight,
			AutomaticPlayerHeight,
			SFXVolume,
			ReduceDebris,
			NoHud,
			HideMissText,
			AdvancedHud,
			AutoRestart,
			SaberTrailIntensity,
			NoteJumpDurationType,
			FixedNoteJumpDuration,
			NoteJumpOffset,
			HideNoteSpawnEffect,
			AdaptiveSFX,
			ExpertEffectsFilter,
			ExpertPlusEffectsFilter,
			
			// Modifiers
			EnergyType,
			NoFail,
			InstantFail,
			FailWhenSabersTouch,
			EnabledOstacleTypes,
			FastNotes,
			StrictAngles,
			DisappearingArrows,
			GhostNotes,
			NoBombs,
			SongSpeed,
			NoArrows,
			ProMode,
			ZenMode,
			SmallCubes,
			
			// Environments
			OverrideEnvironments,
			
			// Colors
			OverrideColors,
			
			// Graphics
			MirrorQuality,
			BloomPostProcess,
			Smoke,
			BurnMarkTrails,
			ScreenDisplacement,
			MaxShockwaveParticles,
			
			// Chroma
			DisableChromaEvents,
			DisableEnvironmentEnhancements,
			DisableNoteColoring,
			ForceZenModeWalls,
		}
		
		private Dictionary<Tooltip, string> name = new Dictionary<Tooltip, string>() {
			// Generic Values
			{ Tooltip.Beat, "The time, as determined by the BPM of the song, when this {0} should reach the player." },
			{ Tooltip.BeatEvent, "The point in time, as determined by the BPM of the song, when this {0} should activate." },
			{ Tooltip.X, "The horizontal row where the {0} should reside on the grid. The indices run from 0 to 3, with 0 being the left-most lane." },
			{ Tooltip.Y, "The vertical column where the {0} should reside on the grid. The indices run from 0 to 2, with 0 being the bottom-most lane." },
			{ Tooltip.Type, "Which saber should be able to successfully cut the {0}." },
			{ Tooltip.CutDirection, "The direction the player should swing to successfully cut the {0}." },
			{ Tooltip.AngleOffset, "A value (in degrees) which applies a counter-clockwise rotational offset to the {0}'s cut direction." },
			
			// Chroma
			{ Tooltip.Color, "Overrides the color of the {0}." },
			{ Tooltip.DisableSpawnEffect, "Disables the light effect that occurs when the {0} spawns." },
			{ Tooltip.SpawnEffect, "Toggles the light effect that occurs when the {0} spawns." },
			{ Tooltip.DisableDebris, "Disables the debris that occurs when the {0} is cut." },
			
			// Noodle Extensions
			{ Tooltip.NJS, "Overrides the note jump speed (NJS) of the {0}." },
			{ Tooltip.SpawnOffset, "Overrides the Start Beat Offset of the {0}. Measured in beats, positive values makes the {0} spawn further away, negative values will make it closer." },
			{ Tooltip.Coordinates, "Overrides the coordinates [x,y] of the {0}. Based on the Beatwalls system: [-2, 2] is the top-left corner and [1, 0] is the bottom-right." },
			{ Tooltip.Rotation, "Sets the global rotation [x,y,z]/[yaw, pitch, roll] of the {0}. This affects which direction the {0} comes at the player from." },
			{ Tooltip.LocalRotation, "Sets the local rotation [x,y,z]/[yaw, pitch, roll] of the {0}. This won't affect the direction it spawns from or the path it takes." },
			{ Tooltip.Fake, "If activated, the {0} will not be counted for scoring." },
			{ Tooltip.Uninteractable, "If activated, the {0} cannot be interacted with / cut through." },
			{ Tooltip.Flip, "Changes how the {0} spawns. [flip line index, flip jump]\nFlip line index is the initial x the note will spawn at and flip jump is how high (or low) the note will jump up (or down) when flipping to its true position." },
			{ Tooltip.DisableGravity, "If disabled, the {0} will no longer bounce up from the bottom row." },
			{ Tooltip.DisableLook, "If disabled, the {0} will no longer try to face the player as it comes closer to them." },
			{ Tooltip.NoBadcutDirection, "If activated, the {0} cannot be badcut from a wrong direction and will be ignored instead." },
			{ Tooltip.NoBadcutSpeed, "If activated, the {0} cannot be badcut with insufficient/slow speed and will be ignored instead." },
			{ Tooltip.NoBadcutColor, "If activated, the {0} cannot be badcut with the wrong saber and will be ignored instead." },
			{ Tooltip.Track, "Assigns the {0} an animation track. The AnimateTrack and AssignPathAnimation custom events apply to all objects that share the same track." },
			{ Tooltip.Link, "When cut, all {0}s that share the same link string will also be cut." },
			
			// V2 Noodle
			{ Tooltip.Interactable, "If deactivated, the {0} cannot be interacted with / cut through." },
			
			// Arcs
			{ Tooltip.Multiplier, "The magnitude of the curve approaching the {0} respectively.\nIf the Cut Direction is set to \"Any\", this value is ignored." },
			{ Tooltip.MidAnchorMode, "A value that determines how the arc curves from the head/tail to the midpoint of the arc."},
			
			// Chains
			{ Tooltip.Slices, "How many segments the {0} is made of. The head counts as a segment" },
			{ Tooltip.Squish, "The proportion of the path from head to tail is used by the {0}. This does not alter the shape of the path." },
			
			// Obstacles
			{ Tooltip.Duration, "How long the {0} extends for in beats." },
			{ Tooltip.Width, "How many columns the {0} should take up on the grid." },
			{ Tooltip.Height, "How many rows the {0} should take up on the grid. The range of acceptable values runs from 1 to 5." },
			{ Tooltip.Size, "Overrides the width, height and length of the {0} [w, h, l]. [1, 1, 1] will be perfectly square. Each number is fully optional." },
			
			// Events
			{ Tooltip.EventColor, "Sets the vanilla color of the {0}. Allows to be set to the left and right environment colors, white, and off" },
			{ Tooltip.EventAction, "Determines how the {0} should behave" },
			{ Tooltip.LegacyEventType, "Changes the color and behaviour of the {0}" },
			{ Tooltip.Brightness, "Sets the brightness of the {0}. A value of 0 will turn the light off." },
			{ Tooltip.LightID, "Causes the {0} to only affect the specified ID. Can be an array." },
			{ Tooltip.Easing, "The easing effect that the {0} should use. Check out \"easings.net\" for visualization examples.\nAffects the next event if it's a transition." },
			{ Tooltip.V2Easing, "The easing effect that the {0} should use. Check out \"easings.net\" for visualization examples." },
			{ Tooltip.LerpType, "What color space the transition should be interpolated in." },
			{ Tooltip.V2Gradient, "Creates a color gradient." },
			
			// Laser Speeds
			{ Tooltip.LaserSpeed, "The speed the lasers should rotate." },
			{ Tooltip.LockRotation, "If activated, the laser will keep its previous rotation." },
			{ Tooltip.LaserDirection, "Sets the spin direction." },
			{ Tooltip.PreciseSpeed, "Overrides the speed the lasers should rotate. Allows for decimal values." },
			
			// Ring Rotation
			{ Tooltip.RingFilter, "Causes the {0} to only affect rings with a listed name. (e.g. SmallTrackLaneRings, BigTrackLaneRings)" },
			{ Tooltip.RingRotation, "How far the first ring will spin. Allows for decimal values." },
			{ Tooltip.RingStep, "How much rotation is added between each ring. Allows for decimal values." },
			{ Tooltip.RingPropagation, "The rate at which rings behind the first one have physics applied to them. High value makes all rings move simultaneously, low value gives them significant delay. Allows for decimal values." },
			{ Tooltip.RingSpeed, "How quickly the rings reach their end position. Allows for decimal values." },
			{ Tooltip.RingDirection, "The spin direction." },
			
			{ Tooltip.RingV2CounterSpin, "Causes the smaller ring to spin in the opposite direction." },
			{ Tooltip.RingV2Reset, "When activated, resets the rings." },
			
			// Ring Zoom
			{ Tooltip.RingZoomStep, "How much position offset is added between each ring. Allows for decimal values." },
			
			// Boost Color
			{ Tooltip.BoostColorSet, "Sets the environment colors to the main or boost set." },
			
			// Lane Rotation (90/360 Maps)
			{ Tooltip.LaneRotation, "Changes the Rotation of the Beatmap to a different one." },
			
			// BPM Changes
			{ Tooltip.BPMChange, "Sets the BPM to the defined value at the indicated beat." },
			
			// Custom Events
			{ Tooltip.TrackDuration, "The length of the animation in beats (defaults to 0)." },
			{ Tooltip.TrackEasing, "An easing for the animation to follow (defaults to easeLinear). Applied to the time value used by the point definitions." },
			{ Tooltip.TrackRepeat, "How many times to repeat this animation (defaults to 0)." },
			
			// Animation
			{ Tooltip.AnimatePath, "Create inline path animations on this {0}. Time values are relative to the {0}'s lifespan.\n0 -> {0} jumps in.\n0.5 -> {0} reaches the player.\n1 -> {0} jumps out." },
			{ Tooltip.AnimateColor, "Animates the color of the {0}." },
			{ Tooltip.AnimatePosition, "Animates the position of an object." },
			{ Tooltip.AnimateLocalPosition, "Animates the local position of an object. Will overwrite `Position`." },
			{ Tooltip.AnimateOffsetPosition, "Animates the position offset of an object. It will continue any normal movement and have this stacked on top of it.\nOnly valid for \"playable\" objects like notes and walls, NOT THE PLAYER." },
			{ Tooltip.AnimateLocalRotation, "Animates the local rotation offset of an object. This means it is rotated with itself as the origin. Uses euler values. Note that the note spawn effect will be rotated accordingly. Notes attempting to look towards the player may look strange, you can disable their look with disableNoteLook." },
			{ Tooltip.AnimateWorldRotation, "Animates the world rotation of a transorm. This means it is rotated with the world as the origin. Uses euler values. \nOnly valid for transforms, not notes or walls." },
			{ Tooltip.AnimateOffsetWorldRotation, "Animates the world rotation offset of an object. This means it is rotated with the world as the origin. Uses euler values. Think of 360 mode.\nOnly valid for \"playable\" objects like notes and walls, NOT THE PLAYER." },
			{ Tooltip.AnimateScale, "Animates the scale of an object. This will be based off their initial size. A scale of 1 is equal to normal size, anything under is smaller, over is larger." },
			{ Tooltip.AnimateDissolve, "Animates the dissolve effect on both notes and walls. It's the effect that happens when things go away upon failing a song. Keep in mind that notes and the arrows on notes have separate dissolve properties, see dissolveArrow.\nNote: How this looks will depend on the player's graphics settings." },
			{ Tooltip.AnimateDissolveArrow, "Animates the dissolve effect on the arrows of notes. Similar to the look of the disappearing notes modifier. Has no effect on obstacles" },
			{ Tooltip.AnimateInteractable, "Animates whether or not the player can interact with the note/obstacle." },
			{ Tooltip.AnimateTime, "Controls what point in the {0}'s lifespan it is at a given time. Note that every object on one track will get the same time values when animating this property. This means they would suddenly appear to all be at the same point. It is recommended for every object to have its own track when using this." },
			
			// AssignTrackParent
			{ Tooltip.AssignTrackParentParent, "The track to assign as the parent." },
			{ Tooltip.AssignTrackChildren, "An array of tracks that get inherited by the parent." },
			{ Tooltip.AssignTrackKeepPosition, "If activated, the parent-relative position, scale and rotation are modified such that the object keeps the same world space position, rotation and scale as before." },
			
			// AssingFogTrack (outdated but used in AnimateComponent)
			{ Tooltip.V2AssignFogTrackAttenuation, "Controls the fog density. Is calculated logarithmically." },
			{ Tooltip.V2AssignFogTrackOffset, "Controls the \"offset\" property of fog." },
			{ Tooltip.V2AssignFogTrackStartY, "Controls the starting elevation of fog." },
			{ Tooltip.V2AssignFogTrackHeight, "Controls the height of the fog gradient." },
			
			// AssignPlayerToTrack
			{ Tooltip.AssignPlayerToTrackTrack, "The track to assign the player to." },
			{ Tooltip.AssignPlayerToTrackTarget, "(optional) The specific player object you wish to target. Available targets are \"Root\", \"Head\", \"LeftHand\", and \"RightHand\"." },
			
			{ Tooltip.AssignPathAnimationDefinitePosition, "Animates the definite position of an object. Will completely overwrite the object's default movement. However, this does take into account lineIndex/lineLayer and world rotation." },
			
			// AnimateComponent
			{ Tooltip.AnimateComponentBloomFogEnvironment, "Animates fog properties." },
			{ Tooltip.AnimateComponentTubeBloomPrePassLight, "Animates tube bloom properties." },
			/* No current UI
			{ Tooltip.AnimateComponentTubeBloomPrePassLightColorAlphaMultiplier, "Changes the multiplier for the color ." },
			{ Tooltip.AnimateComponentTubeBloomPrePassLightBloomFogIntensityMultiplier, "Changes the multiplier for the fog intensity" },
			*/
			
			// SettingsWindow
			
			// PropEdit Settings
			{ Tooltip.ShowChroma, "Shows Chroma options in PropEdit's object window." },
			{ Tooltip.ShowNoodleExtensions, "Shows Noodle Extensions options in PropEdit's object window." },
			{ Tooltip.SplitLightValues, "Shows separate Color and Action options instead of a single Value for events." },
			{ Tooltip.ColorsAsHex, "Shows the Chroma color in hex #RRGGBBAA format instead of an array." },
			{ Tooltip.ShowTooltips, "Shows descriptive tooltips when hovering over options." },
			{ Tooltip.ForceLanes, "Always show custom event lanes for _heck, Chroma, and Noodle Extensions." },
			
			// Requirements
			{ Tooltip.Requirement, "View/Edit the mod requirements for the map.\n\"Required\" -> Players must have the mod installed to play the map.\n\"Suggested\" -> Players can play the map without the mod, but it is recommended. Some mods, like Chroma and Cinema, won't activate unless they are at least a suggestion.\n\"None\" -> Players don't need the mod as the map doesn't contain any mod features.\n\nNOTE: In most cases, the values set by CM are correct, only change them if you know what you're doing." },
			{ Tooltip.ModReq, "Changes the {0} requirement." },
			{ Tooltip.OverrideModReq, "Overrides the {0} requirement determined by CM." },
			
			// Misc
			{ Tooltip.Information, "Any general information you would like the player to be aware of before playing the map." },
			{ Tooltip.Warning, "Any warnings you would like the player to be aware of before playing the map." },
			
			// Map Options
			{ Tooltip.MapOptions, "Options that will show up as a suggestion to the player when they start the map. These settings will only apply for this map, reverting to the player-set options after. It is HIGHLY recommended to only change the settings you NEED." },
			
			// Player Options
			{ Tooltip.LeftHanded, "Mirrors the map. If your map contains text, it can be useful to set this to False so left handed players won't get reversed text." },
			{ Tooltip.PlayerHeight, "Overrides the player height in centimeters." },
			{ Tooltip.AutomaticPlayerHeight, "Sets the player height to be determined by the height of the headset." },
			{ Tooltip.SFXVolume, "Overrides the sound effects volume (i.e. Saber cutting)." },
			{ Tooltip.ReduceDebris, "Reduces debris that occurs when cutting through a note." },
			{ Tooltip.NoHud, "Disables the Hud. Useful if your map is more artistic/modchart related." },
			{ Tooltip.HideMissText, "Hides the miss text that occurs when missing a note." },
			{ Tooltip.AdvancedHud, "In comparison to the normal HUD, the advanced HUD shows the duration of the song, score percentage, and letter rank." },
			{ Tooltip.AutoRestart, "Makes the map automatically restart on failure." },
			{ Tooltip.SaberTrailIntensity, "Changes the trail intensity of the sabers." },
			{ Tooltip.NoteJumpDurationType, "Changes the Note JD Type." },
			{ Tooltip.FixedNoteJumpDuration, "Changes the fixed Note JD. Only works if Note JD Type is set to static." },
			{ Tooltip.NoteJumpOffset, "Changes the dynamic Note Jump Offset. Measured in beats, positive values makes objects spawn further away, negative values will make it closer. Only works if JD Type is set to dynamic." },
			{ Tooltip.HideNoteSpawnEffect, "Hides the note spawn effect that occurs when a note spawns into the map." },
			{ Tooltip.AdaptiveSFX, "Adjusts the sound effect volume to match the song volume." },
			{ Tooltip.ExpertEffectsFilter, "Filters what lighting events will be shown on difficulties under Expert+." },
			{ Tooltip.ExpertPlusEffectsFilter, "Filters what lighting events will be shown on Expert+ difficulties." },
			
			// Modifiers
			{ Tooltip.EnergyType, "Changes the energy/life type.\n\"Battery\"-> 4 Lives mode.\n\"Bar\"-> Normal mode." },
			{ Tooltip.NoFail, "The player can continue playing the level even after they deplete all their energy." },
			{ Tooltip.InstantFail, "Sets the map to one life mode." },
			{ Tooltip.FailWhenSabersTouch, "Instantly fails player when sabers clash." },
			{ Tooltip.EnabledOstacleTypes, "Changes what kind of obstacles appear in the map.\n\"All\"-> All Obstacles appear.\n\"No Crouch\"-> Disables the crouching obstacles.\n\"No Obstacles\"-> Disables all obstacles." },
			{ Tooltip.FastNotes, "Forces the NJS of the map to 20." },
			{ Tooltip.StrictAngles, "Reduces the tolerance for the difference between note arrow and cut direction." },
			{ Tooltip.DisappearingArrows, "Arrows on the notes disappear before they reach the player." },
			{ Tooltip.GhostNotes, "Notes are invisible and arrows disappear before they reach the player." },
			{ Tooltip.NoBombs, "Removes all bombs." },
			{ Tooltip.SongSpeed, "Changes the Song speed.\n\"Slower\"-> -15% Speed\n\"Normal\"-> 100% Speed (Default)\n\"Faster\"-> +20% Speed\n\"Super Fast\"-> +50% Speed" },
			{ Tooltip.NoArrows, "All notes can be cut in any direction." },
			{ Tooltip.ProMode, "Hitboxes match the note models." },
			{ Tooltip.ZenMode, "Removes all notes, obstacles, and bombs." },
			{ Tooltip.SmallCubes, "Notes are 50% of their size." },
			
			// Override stuff
			{ Tooltip.OverrideEnvironments, "Allows the player to override the environment set by the map. Set to False if you don't want players to use another environment." },
			{ Tooltip.OverrideColors, "Allows the player to override the colors set by the map. Set to False if you don't want players to use a different color scheme." },
			
			// Graphics
			{ Tooltip.MirrorQuality, "Changes the Mirror Quality. (0-3)" },
			{ Tooltip.BloomPostProcess, "Changes the Bloom Post Process. (0-1)" },
			{ Tooltip.Smoke, "Adds a subtle smoke to the map. (0-1)" },
			{ Tooltip.BurnMarkTrails, "Hides burn trails left by sabers." },
			{ Tooltip.ScreenDisplacement, "Shows a small distortion effect when a note is cut." },
			{ Tooltip.MaxShockwaveParticles, "The amount of Shockwave particles that occur when cutting through a note." },
			
			//Chroma
			{ Tooltip.DisableChromaEvents, "Disables any form of Chroma events." },
			{ Tooltip.DisableEnvironmentEnhancements, "Disables Chroma's ability to manage the environment." },
			{ Tooltip.DisableNoteColoring, "Disables any note coloring set by chroma (Blame Mawntee)." }, //Actual option tooltip in BS lmao, not sure if to keep it in.
			{ Tooltip.ForceZenModeWalls, "Forces Obstacles to be shown in the Zen Mode modifier." },
		};
		
		private string SafeGet(Tooltip property) {
			if (!name.ContainsKey(property)) {
				Debug.LogWarning($"Missing tooltip for {property}");
				return "";
			}
			return name[property];
		}
		
		public string GetTooltip(Tooltip property, params object?[] args) {
			return string.Format(SafeGet(property), args);
		}
		
		public string GetTooltip(PropertyType type, Tooltip property) {
			
			return string.Format(SafeGet(property), objects[type]);
		}
		
		public string GetTooltip(PropertyType type, string propertyStr) {
			Tooltip property;
			if (Enum.TryParse(propertyStr.Replace(" ", ""), true, out property)) {
				return GetTooltip(type, property);
			}
			else {
				Debug.LogError($"PropEdit: Invalid tooltip \"{propertyStr}\"");
				return "";
			}
		}
	}
}
