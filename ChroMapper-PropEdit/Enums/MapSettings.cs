using System.Collections.Generic;

namespace ChroMapper_PropEdit.Enums {

public static class MapSettings {
	public static readonly Map<int> RequirementStatus = new Map<int> {
		{(int)RequirementCheck.RequirementType.Requirement, "Required"},
		{(int)RequirementCheck.RequirementType.Suggestion, "Suggested"},
		{(int)RequirementCheck.RequirementType.None, "None"},
	};
	
	public static readonly Map<bool?> OptionBool = new Map<bool?> {
		{ true, "True" },
		{ false, "False" },
	};
	
	public static readonly Map<string?> JumpDurationTypes = new Map<string?> {
		{ "Dynamic", "Dynamic" },
		{ "Static", "Static" },
	};
	
	public static readonly Map<string?> EffectsFilters = new Map<string?> {
		{ "AllEffects", "All Effects" },
		{ "Strobefilter", "No Strobe" },
		{ "NoEffects", "No Effects" },
	};
	
	public static readonly Map<string?> EnergyTypes = new Map<string?> {
		{ "Bar", "Bar" },
		{ "Battery", "Battery" },
	};
	
	public static readonly Map<string?> ObstacleTypes = new Map<string?> {
		{ "All", "All" },
		{ "FullHeightOnly", "No Crouch" },
		{ "NoObstacles", "No Obstacles" },
	};
	
	public static readonly Map<string?> SongSpeeds = new Map<string?> {
		{ "Slower", "Slower" },
		{ "Normal", "Normal" },
		{ "Faster", "Faster" },
		{ "SuperFast", "Super Fast" },
	};
}

}
