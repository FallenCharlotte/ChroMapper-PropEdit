using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using static SwingsPerSecond;
using TMPro;
using UnityEngine;

namespace ChroMapper_PropEdit.UserInterface {
	public class DictionaryFormatter : IFormatProvider, ICustomFormatter {
		private Dictionary<string, string> _dict;
		public DictionaryFormatter(Dictionary<string, string> dict) {
			_dict = dict;
		}

		public string Format(string? format, object? arg, IFormatProvider? formatProvider) {
			var key = arg?.ToString() ?? String.Empty;
			return _dict[key];
		}

		public object? GetFormat(Type? formatType) {
			if (formatType == typeof(ICustomFormatter))
				return this;
			else
				return null;
		}
	}

	public class TooltipStrings {
		private Dictionary<string, string> objects = new Dictionary<string, string>() {
			{ "N", "Note" },
			{ "CN", "CustomNote" },
			{ "A", "Arc" },
			{ "AH", "Arc Head" },
			{ "AT", "Arc Tail" },
			{ "C", "Chain" },
			{ "CH", "Chain Head" },
			{ "CT", "Chain Tail" },
			{ "O", "Obstacle" },
			{ "E", "Event" },
			{ "CE", "CustomEvent" },
			{ "BC", "BpmChange" },
			{ "Obj", "Object"},
			{ "G", "Gradient"},
			{ "SG", "Start Gradient"},
			{ "EG", "End Gradient"},
		};

		public enum Tooltip {
			Beat,
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
		

			

		}

		private Dictionary<Tooltip, string> name = new Dictionary<Tooltip, string>() {
			//Generic Values
			{ Tooltip.Beat, "A specific point in time, as determined by the BPM of the song, when this {0} should reach the player"},
			{ Tooltip.X, "The horizontal row where the {0} should reside on the grid. The indices run from 0 to 3, with 0 being the left-most lane" },
			{ Tooltip.Y, "The vertical column where the {0} should reside on the grid. The indices run from 0 to 2, with 0 being the bottom-most lane"},
			{ Tooltip.Type, "Indicates which saber should be able to successfully cut the {0}." },
			{ Tooltip.CutDirection, "Indicates the direction the player should swing to successfully cut the {0}." },
			{ Tooltip.AngleOffset, "A value (in degrees) which applies a counter-clockwise rotational offset to the {0}'s cut direction."},

			//Chroma
			{ Tooltip.Color, "Changes the color and opacity of the {0}. Is displayed in Hex colors"},
			{ Tooltip.DisableSpawnEffect, "Disables the light effect, that occurs when the {0} spawns"},
			{ Tooltip.DisableDebris, "Disables the debris, that occurs when the {0} spawns"},

			//Noodle Extensions
			{ Tooltip.SpawnEffect, "Toggles the light spawn effect, that occurs when the {0} spawns"},
			{ Tooltip.NJS, "Changes the note jump speed (NJS) of the {0}"},
			{ Tooltip.SpawnOffset, "Changes the jump distance (JD) of the {0}"},
			{ Tooltip.Coordinates, "Allows to set the coordinates [x,y,z] of a {0}. Keep in mind, that the center [0,0,0] is different from vanilla coordinates"},
			{ Tooltip.Rotation, "Allows to set the global rotation [x,y,z]/[yaw, pitch, roll] of a {0}. [0,0,0] is always the rotation, that faces the player"},
			{ Tooltip.LocalRotation, "Allows to set the local rotation [x,y,z]/[yaw, pitch, roll] of a {0}. This won't affect the direction it spawns from or the path it takes"},
			{ Tooltip.Fake, "If activated, the {0} will not count towards your score or combo, meaning, if you miss it, it won't have any effect"},
			{ Tooltip.Uninteractable, "If activated, the {0} cannot be interacted with / cut through"},
			{ Tooltip.Flip, "Allows you to change how the {0} spawns. [flip line index, flip jump] Flip line index is the initial x the note will spawn at and flip jump is how high (or low) the note will jump up (or down) when flipping to its true position. Base game behaviour will set one note's flip jump to -1 and the other to 1."},
			{ Tooltip.DisableGravity, "If disabled, the {0} will no longer do their animation where they float up"},
			{ Tooltip.DisableLook, "If disabled, the {0} will no longer try to face the player as it comes closer to them."},
			{ Tooltip.NoBadcutDirection, "If activated, the {0} cannot be badcut from a wrong direction, meaning it will go straight through"},
			{ Tooltip.NoBadcutSpeed, "If activated, the {0} cannot be badcut with insufficient/slow speed, meaning it will go straight through"},
			{ Tooltip.NoBadcutColor, "If activated, the {0} cannot be badcut with the wrong saber, meaning it will go straight through"},
			{ Tooltip.Track, "Groups the {0} together with different objects, that have the same track name/string"},
			{ Tooltip.Link, "When cut, all {0} that share the same link string will also be cut"},

			//V2 stuff only
			{ Tooltip.Interactable, "If deactivated, the {0} cannot be interacted with / cut through"},

			//Arcs
			{ Tooltip.Multiplier, "A value that controls the magnitude of the curve approaching the {0} respectively. \n If the Cut Direction is set to \"Any\"(8), this value is ignored." },

			//Chains
			{ Tooltip.Slices, "An integer value which represents the number of segments in the {0}. The head counts as a segment" },
			{ Tooltip.Squish, "An integer value which represents the proportion of how much of the path from Head (x, y) to Tail(tx, ty) is used by the {0}. This does not alter the shape of the path." },

			//Obstacles
			{ Tooltip.Duration, "A value (in beats) that determines how long the {0} extends for." },
			{ Tooltip.Width, "An integer value which represents how many columns the obstacle should take up on the grid."},
			{ Tooltip.Height, "An integer value which represents how many rows the obstacle should take up on the grid. The range of acceptable values runs from 1 to 5."},
			{ Tooltip.Size, "The width, height and length of a wall[w, h, l]. [1, 1, 1] will be perfectly square. Each number is fully optional."},
			
			//Events
			{ Tooltip.EventColor, "Changes the vanilla color of the {0}. Allows to be set to the saber colors, white and off"},
			{ Tooltip.EventAction, "Determines how the {0} should behave"},
			{ Tooltip.LegacyEventType, "Changes the color and behaviour of the {0}"},
			{ Tooltip.Brightness, "Used to control the brightness of the {0}. A value of 0 will turn the light off."},
			{ Tooltip.LightID, "Causes the {0} to only affect the specified ID. Can be an array."},
			{ Tooltip.Easing, "The easing effect that the {0} should use. Check out \"easings.net\" for visualization examples"},
			{ Tooltip.LerpType, "Determines, in what way the color should transition. RGB: changing every value. HSV: primarly changing its hue"},
			{ Tooltip.V2Gradient, "If activated, allows to set a color gradient"},

			};
		public string GetTooltip(string BSobject, Tooltip property) {
			Console.WriteLine(string.Format(name[property], objects[BSobject]));
			//Assuming you want to replace {0} in the name dictionary with BSobject

			return string.Format(name[property], objects[BSobject]);
		}
	}
}
