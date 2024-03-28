using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using ChroMapper_PropEdit.Enums;

namespace ChroMapper_PropEdit.UserInterface {
	public class TooltipStrings {
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
			AnimateRotation,
			AnimateLocalRotation,
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

			//Settings
			ChromaReq,
			LegacyChromaReq,
			MappingExtensionsReq,
			NoodleExtensionsReq,
			CinemaReq,
			SoundExtensionsReq,

			OverrideChromaReq,
			OverrideLegacyChromaReq,
			OverrideMappingExtensionsReq,
			OverrideNoodleExtensionsReq,
			OverrideCinemaReq,
			OverrideSoundExtensionsReq,




		}
		
		private Dictionary<Tooltip, string> name = new Dictionary<Tooltip, string>() {

			//Settings
			{Tooltip.ChromaReq, "TestChroma" },
			{Tooltip.LegacyChromaReq, "TestChromaLegacy" },
			{Tooltip.MappingExtensionsReq, "TestME" },
			{Tooltip.NoodleExtensionsReq, "TestNE" },
			{Tooltip.CinemaReq, "TestCinema" },
			{Tooltip.SoundExtensionsReq, "TestSoundExtensions" },

			{Tooltip.OverrideChromaReq, "OverrideTestChroma" },
			{Tooltip.OverrideLegacyChromaReq, "OverrideTestChromaLegacy" },
			{Tooltip.OverrideMappingExtensionsReq, "OverrideTestME" },
			{Tooltip.OverrideNoodleExtensionsReq, "OverrideTestNE" },
			{Tooltip.OverrideCinemaReq, "OverrideTestCinema" },
			{Tooltip.OverrideSoundExtensionsReq, "OverrideTestSoundExtensions" },

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
			{ Tooltip.Easing, "The easing effect that the {0} should use. Check out \"easings.net\" for visualization examples." },
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
			{ Tooltip.RingDirection, "The spin direction." }, //DirectionMerge
			
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
			{ Tooltip.AnimatePosition, "Animates the position offset of an object. It will continue any normal movement and have this stacked on top of it." },
			{ Tooltip.AnimateLocalRotation, "Animates the local rotation offset of an object. This means it is rotated with itself as the origin. Uses euler values. Note that the note spawn effect will be rotated accordingly. Notes attempting to look towards the player may look strange, you can disable their look with disableNoteLook." },
			{ Tooltip.AnimateRotation, "Animates the world rotation offset of an object. This means it is rotated with the world as the origin. Uses euler values. Think of 360 mode." },
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


			/*
			{ Tooltip.AnimateComponentTubeBloomPrePassLightColorAlphaMultiplier, "Changes the multiplier for the color ." }, //Unsure
			{ Tooltip.AnimateComponentTubeBloomPrePassLightBloomFogIntensityMultiplier, "Changes the multiplier for the fog intensity" },
			*/
		};
		
		public string GetTooltip(PropertyType type, Tooltip property) {
			return string.Format(name[property], objects[type]);
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
