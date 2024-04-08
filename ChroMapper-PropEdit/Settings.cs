using System.IO;
using SimpleJSON;

using ChroMapper_PropEdit.Utils;

namespace ChroMapper_PropEdit {

public class Settings {
	public static readonly string ShowChromaKey = "Chroma";
	public static readonly string ShowNoodleKey = "Noodle";
	public static readonly string SplitValue = "split_val";
	public static readonly string ColorHex = "color_hex";
	public static readonly string ShowTooltips = "tooltips";

		public static JSONNode? Get(string name, JSONNode? d = null) {
		var o = Data.GetNode(Settings.Instance.json, name);
		if (o == null && d != null) {
			o = Settings.Set(name, d);
		}
		return o;
	}
	
	public static JSONNode Set(string name, JSONNode value) {
		Data.SetNode(Settings.Instance.json, name, value);
		Settings.Instance.Save();
		return value;
	}
	
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
