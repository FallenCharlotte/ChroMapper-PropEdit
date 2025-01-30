

namespace ChroMapper_PropEdit.Utils {

#if CHROMPER_11
using InfoType = BeatSaberSong.DifficultyBeatmap;
#else
using InfoType = Beatmap.Info.InfoDifficulty;
#endif

public class CustomRequirement : RequirementCheck
{
	public override string Name { get { return _name; } }
	public string _name;
	public RequirementType _type;
	public InfoType _map_info;
	
	public CustomRequirement(string name, RequirementType type, InfoType mapInfo) {
		_name = name;
		_type = type;
		_map_info = mapInfo;
	}

	public override RequirementType IsRequiredOrSuggested(InfoType mapInfo, Beatmap.Base.BaseDifficulty map) {
		if (mapInfo == _map_info) {
			return _type;
		}
		return RequirementType.None;
	}
}

}
