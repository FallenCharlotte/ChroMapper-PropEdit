

namespace ChroMapper_PropEdit.Utils {

public class CustomRequirement : RequirementCheck
{
	public override string Name { get { return _name; } }
	public string _name;
	public RequirementType _type;
	
	public CustomRequirement(string name, RequirementType type) {
		_name = name;
		_type = type;
	}

	public override RequirementType IsRequiredOrSuggested(BeatSaberSong.DifficultyBeatmap mapInfo, Beatmap.Base.BaseDifficulty map) {
		return _type;
	}
}

}
