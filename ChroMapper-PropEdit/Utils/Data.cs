using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using UnityEngine;
using SimpleJSON;

using Beatmap.Base;

namespace ChroMapper_PropEdit.Utils {

public static class Data {
	
#region JSON utils
	
	public static JSONNode? GetNode(JSONNode root, string name) {
		string[] path = name.Split('.');
		foreach (string node in path) {
			/*if (node.EndsWith("]")) {
				var parts = node.Split('[');
				var part = parts[0];
				var ind = int.Parse(parts[1].TrimEnd(']'));
				root = root[part][ind];
				continue;
			}*/
			if (!(root?.HasKey(node) ?? false)) {
				return null;
			}
			root = root[node];
		}
		return root;
	}
	
	public static JSONNode? SetNode(JSONNode root, string name, JSONNode? o) {
		if (o == null) {
			RemoveNode(root, name);
			return o;
		}
		string[] path = name.Split('.');
		for (int i = 0; i < path.Length - 1; ++i) {
			/*if (path[i].EndsWith("]")) {
				var parts = path[i].Split('[');
				var part = parts[0];
				var ind = int.Parse(parts[1].TrimEnd(']'));
				root = root[part][ind];
				continue;
			}*/
			root = root[path[i]];
		}
		root[path[path.Length-1]] = o;
		return o;
	}
	
	public static void RemoveNode(JSONNode root, string name) {
		string[] path = name.Split('.');
		for (int i = 0; i < path.Length - 1; ++i) {
			if (!(root?.HasKey(path[i]) ?? false)) {
				return;
			}
			root = root[path[i]];
		}
		root?.Remove(path[path.Length - 1]);
	}
	
#endregion
	
	public static Color GetColor(BaseEvent e) {
		return (e.CustomColor ?? (e.Value switch {
			0 => Color.clear,
#if CHROMPER_13
			(>= 1) and (<= 4) => BeatSaberSongContainer.Instance.MapDifficultyInfo.CustomEnvColorRight ?? LoadInitialMap.Platform.DefaultColors.BlueColor,
			(>= 5) and (<= 8) => BeatSaberSongContainer.Instance.MapDifficultyInfo.CustomEnvColorLeft ?? LoadInitialMap.Platform.DefaultColors.RedColor,
#else
			(>= 1) and (<= 4) => BeatSaberSongContainer.Instance.MapDifficultyInfo.CustomEnvColorRight ?? DefaultColors.Right,
			(>= 5) and (<= 8) => BeatSaberSongContainer.Instance.MapDifficultyInfo.CustomEnvColorLeft ?? DefaultColors.Left,
#endif
			(>= 9) => Color.white,
			_ => Color.clear,
		}));
	}
	
#region Converters
	
	private class TextParserConverter<T> : IConverter<T?, string?>  where T : struct {
		public string Forwards(T? input)
			=> (input != null)
				? (string)Convert.ChangeType(input, typeof(string))
				: "";
		// Can throw: catch should be in the textbox setter
		public T? Backwards(string? input) {
			var table = new System.Data.DataTable();
			var computed = table.Compute(input, "");
			T? converted = (computed == System.DBNull.Value)
				? null
				: (T)Convert.ChangeType(computed, typeof(T));
			//Plugin.Trace($"`{input}` => {converted}");
			return converted;
		}
	}
	public static IConverter<T?, string?> TextParser<T>() where T : struct => new TextParserConverter<T>();
	
	public class JSONValueConv<T> : IConverter<JSONNode?, T?> {
		public T? Forwards(JSONNode? node)
			=> (node == null)
				? default(T)!
				: Data.CreateConvertFunc<JSONNode, T>()(node);
		public JSONNode? Backwards(T? v)
			=> (v == null)
				? null
				: Data.CreateConvertFunc<T, SimpleJSON.JSONNode>()(v);
	}
	public static JSONValueConv<T> JSONValue<T>() => new();
	
	public class JSONRawConv : IConverter<JSONNode?, string?> {
		public string? Forwards(JSONNode? node) {
			return (node == null)
				? null
				: node.ToString();
		}
		public JSONNode? Backwards(string? value) {
			if (string.IsNullOrEmpty(value) || value == "{}" || value == "[]") {
				return null;
			}
			return RawToJson(value!);
		}
	}
	public static JSONRawConv JSONRaw() => new();
	
	public class JSONColorCoverter : IConverter<JSONNode?, string?> {
		public string? Forwards(JSONNode? node) {
			if (node == null) {
				return null;
			}
			if (Settings.Get(Settings.ColorHex, true)) {
				var color = node.ReadColor();
				return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
			}
			else {
				return node.ToString();
			}
		}
		public JSONNode? Backwards(string? str) {
			if (string.IsNullOrEmpty(str)) {
				return null;
			}
			else if (str![0] == '#') {
				ColorUtility.TryParseHtmlString(str, out var color);
				var jc = new JSONArray();
				jc.WriteColor(color);
				return jc;
			}
			else {
				return RawToJson(str);
			}
		}
	}
	public static JSONColorCoverter JSONColor() => new();
	
	// https://stackoverflow.com/a/32037899
	public static System.Func<TInput, TOutput> CreateConvertFunc<TInput, TOutput>()
	{
		var source = Expression.Parameter(typeof(TInput), "source");
		// the next will throw if no conversion exists
		var convert = Expression.Convert(source, typeof(TOutput));
		var method = convert.Method;
		return Expression.Lambda<System.Func<TInput, TOutput>>(convert, source).Compile();
	}
	
	private static Regex? NOT_MATH_REG = null;
	
	public static JSONNode? RawToJson(string raw) {
		NOT_MATH_REG ??= new Regex(@"[""a-zA-Z\[\]]");
		var table = new System.Data.DataTable();
		var parts = raw.Split(new Char[] {',', '[', ']'});
		
		foreach (var part in parts) {
			if (!NOT_MATH_REG.IsMatch(part)) {
				try {
					var computed = table.Compute(part, "");
					//Plugin.Trace($"[Raw] `{part}` = {computed}");
					var at = raw.IndexOf(part);
					raw = raw.Substring(0, at) + computed.ToString() + raw.Substring(at + part.Length);
				}
				catch (Exception) { };
			}
			else {
				//Plugin.Trace($"Is not math: {part}");
			}
		}
		
		JSONNode n;
		try {
			n = JSON.Parse(raw);
			return n;
		}
		catch (Exception) { };
		
		try {
			n = JSON.Parse($"[{raw}]");
			return n;
		}
		catch (Exception) { };
		
		try {
			n = JSON.Parse($"\"{raw}\"");
			return n;
		}
		catch (Exception) { };
		
		Debug.LogWarning($"Couldn't interpret \"{raw}\" as JSON");
		return null;
	}
	
#endregion
}

}
