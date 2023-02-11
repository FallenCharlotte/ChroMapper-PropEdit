using System;
using System.Collections;
using System.Collections.Generic;

namespace ChroMapper_PropEdit.Enums {
#nullable enable
public class Map<TKey> : IEnumerable<KeyValuePair<TKey,string>> where TKey : struct {
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
		return null;
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

public class Map : IEnumerable<KeyValuePair<string, string>> {
	public Dictionary<string, string> dict { get { return _dict; } }
	
	public Map() {
		this._dict = new Dictionary<string,string>();
	}
	
	public string? Forward(string key) {
		string? value = null;
		dict.TryGetValue(key, out value);
		return value;
	}
	
	public string? Backward(string value) {
		foreach (var entry in dict) {
			if (entry.Value == value) {
				return entry.Key;
			}
		}
		return null;
	}
	
	public void Add(string key, string value) {
		_dict.Add(key, value);
	}
	
	public IEnumerator<KeyValuePair<string,string>> GetEnumerator() {
		return _dict.GetEnumerator();
	}
	IEnumerator IEnumerable.GetEnumerator()
	{
		// call the generic version of the method
		return this.GetEnumerator();
	}
	
	private readonly Dictionary<string, string> _dict;
}
#nullable disable
}
