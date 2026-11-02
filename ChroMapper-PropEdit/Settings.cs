using System.IO;
using SimpleJSON;

using ChroMapper_PropEdit.Utils;

namespace ChroMapper_PropEdit {

public class Settings {
	// Mostly an enum class, but allows external construction for custom keys
	public class Setting {
		public readonly string Key;
		public readonly JSONNode? Default;
		public Setting(string key, JSONNode? _default = null) {
			Key = key;
			Default = _default;
		}
		
		public JSONNode? Get() {
			var o = Data.GetNode(Settings.Instance.json, Key);
			if (o == null && Default != null) {
				o = Set(Default);
			}
			return o;
		}
		public JSONNode Set(JSONNode value) {
			Data.SetNode(Settings.Instance.json, Key, value);
			Settings.Instance.Save();
			return value;
		}
	}
	public static readonly Setting BypassVersion = new("BypassVersion", false);
	public static readonly Setting ShowChroma = new("Chroma", true);
	public static readonly Setting ShowNoodle = new("Noodle", true);
	public static readonly Setting SplitValue = new("split_val", true);
	public static readonly Setting ColorHex = new("color_hex", true);
	public static readonly Setting ShowTooltips = new("tooltips", true);
	public static readonly Setting ForceLanes = new("force_lanes", true);
	public static readonly Setting KeepForcedReqs = new("keep_forced_reqs", "Ask");
	
	public static void Reload() {
		Settings._instance = new Settings();
	}
	
	public readonly string SETTINGS_FILE = UnityEngine.Application.persistentDataPath + "/PropEdit.json";
	
	private static Settings? _instance = null;
	private static Settings Instance {
		get {
			if (_instance == null) {
				_instance = new Settings();
			}
			return _instance;
		}
	}
	
	private JSONObject json;
	
	private Settings() {
		try {
			if (File.Exists(SETTINGS_FILE)) {
				using (var reader = new StreamReader(SETTINGS_FILE)) {
					json = JSON.Parse(reader.ReadToEnd()).AsObject ?? new JSONObject();
					return;
				}
			}
		}
		catch (System.Exception) { }
		json = new JSONObject();
	}
	
	private void Save() {
		File.WriteAllText(SETTINGS_FILE, json.ToString(4));
	}
}

}
