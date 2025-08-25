using System;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;

using ChroMapper_PropEdit.Enums;

namespace ChroMapper_PropEdit.Utils {

public class BundleInfo {
	public JSONObject? _bundleInfo = null;
	public Map<string?>? Materials = null;
	public Map<string?>? Prefabs = null;
	public Dictionary<string, Dictionary<string, Vivify.PropertyType>>? Properties = null;
	
	public bool Exists { get { return _bundleInfo != null; } }
	
	public BundleInfo() {
		var map_dir = BeatSaberSongContainer.Instance.Info.Directory;
		var bundle_file = Path.Combine(map_dir, "bundleinfo.json");
		
		if (!File.Exists(bundle_file)) {
			return;
		}
		
		StreamReader reader = new StreamReader(bundle_file);
		_bundleInfo = JSONNode.Parse(reader.ReadToEnd()).AsObject;
		reader.Close();
		
		Materials = new Map<string?>();
		Properties = new Dictionary<string, Dictionary<string, Vivify.PropertyType>>();
		Prefabs = new Map<string?>();
		
		foreach (var mat in _bundleInfo["materials"]) {
			Materials.Add((string)mat.Value["path"], mat.Key);
			var props = new Dictionary<string, Vivify.PropertyType>();
			foreach (var prop in mat.Value["properties"].AsObject) {
				var it = prop.Value.GetEnumerator();
				it.MoveNext();
				var type_text = it.Current.Key;
				Enum.TryParse(type_text, out Vivify.PropertyType type);
				props.Add(prop.Key, type);
			}
			Properties.Add(mat.Key, props);
		}
		
		foreach (var fab in _bundleInfo["prefabs"]) {
			Prefabs.Add((string)fab.Value, fab.Key);
		}
	}
}

}
