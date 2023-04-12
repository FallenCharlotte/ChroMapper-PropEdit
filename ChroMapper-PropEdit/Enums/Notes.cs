namespace ChroMapper_PropEdit.Enums {

public static class Notes {
	public static readonly Map<int> NoteTypes = new Map<int> {
		{0, "Left"},
		{1, "Right"},
		{3, "Bomb"},
	};
	
	public static readonly Map<int> ArcColors = new Map<int> {
		{0, "Left"},
		{1, "Right"},
	};
	
	public static readonly Map<int> CutDirections = new Map<int> {
		{0, "Up"},
		{1, "Down"},
		{2, "Left"},
		{3, "Right"},
		{4, "UpLeft"},
		{5, "UpRight"},
		{6, "DownLeft"},
		{7, "DownRight"},
		{8, "Any"},
	};
}

}
