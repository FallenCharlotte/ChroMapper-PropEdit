using System.Collections.Generic;

namespace ChroMapper_PropEdit.Enums {

public static class MapSettings {
	public static readonly Map<int> RequirementStatus = new Map<int> {
		{(int)RequirementCheck.RequirementType.Requirement, "Required"},
		{(int)RequirementCheck.RequirementType.Suggestion, "Suggested"},
		{(int)RequirementCheck.RequirementType.None, "None"},
	};
}

}
