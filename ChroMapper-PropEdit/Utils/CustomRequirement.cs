

namespace ChroMapper_PropEdit.Utils {

public class CustomRequirement : RequirementCheck
{
	public override string Name { get { return _name; } }
	public string _name;
	public RequirementType _type;
	public BeatSaberSong.DifficultyBeatmap _map_info;
	
	public CustomRequirement(string name, RequirementType type, BeatSaberSong.DifficultyBeatmap mapInfo) {
		_name = name;
		_type = type;
		_map_info = mapInfo;
	}

	public override RequirementType IsRequiredOrSuggested(BeatSaberSong.DifficultyBeatmap mapInfo, Beatmap.Base.BaseDifficulty map) {
		if (mapInfo == _map_info) {
			return _type;
		}
		return RequirementType.None;
	}
}

}
