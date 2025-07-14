using System.Collections;
using System.Collections.Generic;

namespace ChroMapper_PropEdit.Enums {

public class Map<TKey> : IEnumerable<KeyValuePair<TKey,string>> {
	public Dictionary<TKey, string> dict { get { return _dict; } }
	
	public Map() {
		this._dict = new Dictionary<TKey,string>();
	}
	
	public string? Forward(TKey key) {
		string? value = null;
		dict.TryGetValue(key, out value);
		return value;
	}
	
	public TKey? Backward(string value) {
		foreach (var entry in dict) {
			if (entry.Value == value) {
				return entry.Key;
			}
		}
		return default!;
	}
	
	public void Add(TKey key, string value) {
		_dict.Add(key, value);
	}
	
	public IEnumerator<KeyValuePair<TKey,string>> GetEnumerator() {
		return _dict.GetEnumerator();
	}
	IEnumerator IEnumerable.GetEnumerator()
	{
		// call the generic version of the method
		return this.GetEnumerator();
	}
	
	private readonly Dictionary<TKey, string> _dict;
}

public static class MapExt {
	public static Map<string?> AddRange(this Map<string?> map, IEnumerable<string> source) {
		foreach (var item in source) {
			map.Add(item, item);
		}
		
		return map;
	}
	
	public static Map<string?> AddEnum(this Map<string?> map, System.Type source) {
		var options = System.Enum.GetNames(source);
		foreach (var item in options) {
			map.Add(item, item);
		}
		
		return map;
	}
	
	public static Map<int?> AddEnum(this Map<int?> map, System.Type source) {
		var options = System.Enum.GetNames(source) as IList<string>;
		var values = System.Enum.GetValues(source) as IList<int>;
		for (var i = 0; i < values!.Count; ++i) {
			map.Add(values[i], options[i]);
		}
		
		return map;
	}
}

}
