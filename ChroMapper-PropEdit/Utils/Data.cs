using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using UnityEngine;
using SimpleJSON;

using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Enums;
using Beatmap.Helper;
using Beatmap.Shared;

using ChroMapper_PropEdit.Enums;

namespace ChroMapper_PropEdit.Utils {

public static class Data {
	
#region Getter/setter factories
	
	public delegate T Getter<T>(BaseObject o);
	public delegate void Setter<T>(BaseObject o, T v);
	
	public static (Getter<T?>, Setter<T?>) GetSet<T>(string field_name) where T : struct {
		Getter<T?> getter = (o) => (T?)o.GetType().GetProperty(field_name).GetMethod.Invoke(o, null) ?? null;
		Setter<T?> setter = (o, v) => { if (v != null) o.GetType().GetProperty(field_name).SetMethod.Invoke(o, new object[] {v}); };
		return (getter, setter);
	}
	public static (Getter<T?>, Setter<T?>) GetSetTest<T>(string field_name) {
		Getter<T?> getter = (o) => (T?)o.GetType().GetProperty(field_name).GetMethod.Invoke(o, null);
		Setter<T?> setter = (o, v) => { if (v != null) o.GetType().GetProperty(field_name).SetMethod.Invoke(o, new object[] {(T)v}); };
		return (getter, setter);
	}
	
	// Very cursed value split: subtract 1 then mask
	public static (Getter<int?>, Setter<int?>) GetSetSplitValue(int mask) {
		Getter<int?> getter = (o) => {
			int i = ((BaseEvent)o).Value;
			return (i == 0)
				? 0b1111
				: (i - 1) & mask & 0b1111;
		};
		Setter<int?> setter = (o, v) => {
			if (v is int value) {
				int i = ((BaseEvent)o).Value;
				// I'm sorry
				((BaseEvent)o).Value = ((((i - (i == 0 ? 0 : 1)) & (~mask)) | (value)) + 1) & 0b1111;
			}
		};
		return (getter, setter);
	}
	
	public static (Getter<T?>, Setter<T?>) JSONGetSet<T>(System.Type type, string node_name, string field_name) {
		var node = type.GetProperty(node_name);
		if (node == null) {
			Debug.LogError($"Node {node_name} not found in type {type.FullName}!");
		}
		Getter<T?> getter = (o) => {
			var root = (SimpleJSON.JSONNode)node!.GetMethod.Invoke(o, null) ?? new SimpleJSON.JSONObject();
			if (GetNode(root, field_name) is JSONNode n) {
				return CreateConvertFunc<JSONNode, T>()(n);
			}
			else {
				return default!;
			}
		};
		Setter<T?> setter = (o, v) => {
			var root = (SimpleJSON.JSONNode)node!.GetMethod.Invoke(o, null) ?? new SimpleJSON.JSONObject();
			if (v is T value) {
				SetNode(root, field_name, CreateConvertFunc<T, SimpleJSON.JSONNode>()(value));
			}
			else {
				RemoveNode(root, field_name);
			}
			node!.SetMethod.Invoke(o, new object[] { root });
			o.RefreshCustom();
		};
		return (getter, setter);
	}
	
	// We love raw JSON dumps :3
	public static (Getter<string?>, Setter<string?>) JSONGetSetRaw(System.Type type, string node_name, string field_name) {
		var node = type.GetProperty(node_name);
		if (node == null) {
			Debug.LogError($"Node {node_name} not found in type {type.FullName}!");
		}
		Getter<string?> getter = (o) => {
			var root = (SimpleJSON.JSONNode)node!.GetMethod.Invoke(o, null) ?? new SimpleJSON.JSONObject();
			if (GetNode(root, field_name) is JSONNode n) {
				return n.ToString();
			}
			else {
				return null;
			}
		};
		Setter<string?> setter = (o, v) => {
			var root = (SimpleJSON.JSONNode)node!.GetMethod.Invoke(o, null) ?? new SimpleJSON.JSONObject();
			if (string.IsNullOrEmpty(v)) {
				RemoveNode(root, field_name);
			}
			else {
				var n = RawToJson(v!);
				if (n != null) {
					SetNode(root, field_name, n);
				}
			}
			node.SetMethod.Invoke(o, new object[] { root });
		};
		return (getter, setter);
	}
	
	public static (Getter<T?>, Setter<T?>) CustomGetSet<T>(string field_name) {
		return JSONGetSet<T?>(typeof(BaseObject), "CustomData", field_name);
	}
	
	public static (Getter<string?>, Setter<string?>) CustomGetSetRaw(string field_name) {
		return JSONGetSetRaw(typeof(BaseObject), "CustomData", field_name);
	}
	
	public static (Getter<string?>, Setter<string?>) PropertyGetSetRaw(string prop_name, string type) {
		return PropertyGetSetPart(prop_name, "value", (JsonToRaw, RawToJson), type);
	}
	
	public static (Getter<string?>, Setter<string?>) PropertyGetSetPart(string? prop_name, string part) {
		return PropertyGetSetPart(prop_name, part, (CreateConvertFunc<JSONNode, string?>(), CreateConvertFunc<string, JSONNode?>()));
	}
	
	public static (Getter<string?>, Setter<string?>) PropertyGetSetPart(string? prop_name, string part, (System.Func<JSONNode, string?>, System.Func<string, JSONNode?>) part_get_set, string? default_type = null) {
		Getter<string?> getter = (o) => {
			if (prop_name == null) {
				return null;
			}
			var root = (o as BaseCustomEvent)!.Data ?? new SimpleJSON.JSONObject();
			if (GetNode(root, "properties") is JSONArray props) {
				foreach (var prop in props.Children) {
					if ((string)prop.AsObject["id"] == prop_name) {
						return part_get_set.Item1(prop.AsObject[part]);
					}
				}
				return null;
			}
			else {
				return null;
			}
		};
		Setter<string?> setter = (o, v) => {
			var root = (o as BaseCustomEvent)!.Data ?? new SimpleJSON.JSONObject();
			var props = GetNode(root, "properties")?.AsArray ?? new JSONArray();
			if (prop_name == null) {
				prop_name = v;
			}
			if (prop_name == null) {
				return;
			}
			JSONObject? _prop = null;
			foreach (var prop in props.Children) {
				if (prop.AsObject["id"] == prop_name) {
					_prop = prop.AsObject;
					break;
				}
			}
			if (_prop == null) {
				_prop = new JSONObject();
				_prop["id"] = prop_name;
				if (default_type != null) {
					_prop["type"] = default_type;
				}
				props.Add(_prop);
			}
			if (string.IsNullOrEmpty(v)) {
				props.Remove((JSONNode)_prop);
			}
			else {
				var n = part_get_set.Item2(v!);
				if (n != null) {
					SetNode(_prop, part, n);
				}
			}
			root["properties"] = props;
			(o as BaseCustomEvent)!.Data = root;
			o.RefreshCustom();
		};
		return (getter, setter);
	}
	
	// Create and delete gradient
	public static (Getter<bool?>, Setter<bool?>) GetSetGradient() {
		Getter<bool?> getter = (o) => ((BaseEvent)o).CustomLightGradient != null;
		Setter<bool?> setter = (o, v) => { if (o is BaseEvent e) {
			if (!(v ?? false)) {
				if (e.CustomLightGradient != null) {
					var jc = new JSONArray();
					jc.WriteColor(e.CustomLightGradient.StartColor);
					e.CustomData[e.CustomKeyColor] = jc;
				}
				e.CustomData?.Remove(e.CustomKeyLightGradient);
			}
			else if (e.CustomLightGradient == null) {
				var collection = BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Event) as EventGridContainer;
				
				var next = collection?.AllLightEvents[e.Type]
					?.Where(n => (n.JsonTime > e.JsonTime))
					?.FirstOrDefault();
				
				Color begin = GetColor(e);
				Color end = (next != null) ? GetColor(next) : begin;
				
				float duration = (next != null) ? (next.JsonTime - e.JsonTime) : 1;
				
				e.GetOrCreateCustom()[e.CustomKeyLightGradient] = (new ChromaLightGradient(begin, end, duration)).ToJson();
				e.CustomData.Remove(e.CustomKeyColor);
			}
		}};
		return (getter, setter);
	}
	
	// Create and delete animation
	public static (Getter<bool?>, Setter<bool?>) GetSetAnimation(bool v2) {
		string animation_key = v2 ? "_animation" : "animation";
		return CustomGetSetNode(animation_key, "{}");
	}
	
	// Create or remove object with default json
	public static (Getter<bool?>, Setter<bool?>) CustomGetSetNode(string path, string json) {
		Getter<bool?> getter = (o) => GetNode(o.CustomData, path) != null;
		Setter<bool?> setter = (o, v) => {
			if (!(v ?? false)) {
				RemoveNode(o.CustomData, path);
			}
			else if (GetNode(o.CustomData, path) == null) {
				SetNode(o.CustomData, path, JSON.Parse(json));
			}
		};
		return (getter, setter);
	}
	
	public static (Getter<string?>, Setter<string?>) CustomGetSetColor(string field_name) {
		Getter<string?> getter = (o) => {
			if (GetNode(o.CustomData, field_name) is JSONNode n) {
				var color = n.ReadColor();
				return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
			}
			else {
				return null;
			}
		};
		Setter<string?> setter = (o, v) => {
			if (string.IsNullOrEmpty(v)) {
				RemoveNode(o.CustomData, field_name);
			}
			else {
				ColorUtility.TryParseHtmlString(v, out var color);
				var jc = new JSONArray();
				jc.WriteColor(color);
				SetNode(o.GetOrCreateCustom(), field_name, jc);
			}
		};
		return (getter, setter);
	}
	
#endregion
	
#region Editing many objects
	
	// Value, Mixed?
	public static (T?, bool) GetAllOrNothing<T>(IEnumerable<BaseObject> editing, Getter<T?> getter) {
		var it = editing.GetEnumerator();
		it.MoveNext();
		var first = getter(it.Current);
		while (it.MoveNext()) {
			T? v = getter(it.Current);
			if (v == null && first == null)
				continue;
			if (!(v?.Equals(first) ?? false)) {
				first = default!;
				return (first, true);
			}
		}
		
		return (first, false);
	}
	
	public static void UpdateObjects<T>(List<BaseObject> editing, Setter<T?> setter, T? value, bool time = false) {
		if (time) {
			var beatmapActions = new List<BeatmapObjectModifiedAction>();
			foreach (var o in editing!) {
				var orig = BeatmapFactory.Clone(o);
				
				// Based on SelectionController.MoveSelection
				var collection = BeatmapObjectContainerCollection.GetCollectionForType(o.ObjectType);
				
				collection.DeleteObject(o, false, false, "", true, false);
				
				setter(o, value);
				
				collection.SpawnObject(o, false, true);
				
				beatmapActions.Add(new BeatmapObjectModifiedAction(o, o, orig, $"Edited a {o.ObjectType} with Prop Edit.", true));
			}
			
			BeatmapActionContainer.AddAction(
				new ActionCollectionAction(beatmapActions, true, false, $"Edited ({editing.Count()}) objects with Prop Edit."),
				true);
			BeatmapObjectContainerCollection.RefreshAllPools();
		}
		else {
			var modified = new List<BaseObject>();
			var beatmapActions = new List<BeatmapObjectModifiedAction>();
			foreach (var o in editing!) {
				var mod = BeatmapFactory.Clone(o);
				modified.Add(mod);
				
				setter(mod, value);
				mod.RefreshCustom();
			}
			BeatmapActionContainer.AddAction(
				new BeatmapObjectModifiedCollectionAction(modified, editing, $"Edited ({editing.Count()}) objects with Prop Edit."),
				true);
		}
	}
	
#endregion
	
#region JSON utils
	
	public static JSONNode? GetNode(JSONNode root, string name) {
		string[] path = name.Split('.');
		foreach (string node in path) {
			if (!(root?.HasKey(node) ?? false)) {
				return null;
			}
			root = root[node];
		}
		return root;
	}
	
	public static JSONNode SetNode(JSONNode root, string name, JSONNode o) {
		string[] path = name.Split('.');
		for (int i = 0; i < path.Length - 1; ++i) {
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
#if CHROMPER_11
			(>= 1) and (<= 4) => BeatSaberSongContainer.Instance.DifficultyData.EnvColorRight ?? BeatSaberSong.DefaultRightColor,
			(>= 5) and (<= 8) => BeatSaberSongContainer.Instance.DifficultyData.EnvColorLeft ?? BeatSaberSong.DefaultLeftColor,
#else
			(>= 1) and (<= 4) => BeatSaberSongContainer.Instance.MapDifficultyInfo.CustomEnvColorRight ?? LoadInitialMap.Platform.DefaultColors.BlueColor,
			(>= 5) and (<= 8) => BeatSaberSongContainer.Instance.MapDifficultyInfo.CustomEnvColorLeft ?? LoadInitialMap.Platform.DefaultColors.RedColor,
#endif
			(>= 9) => Color.white,
			_ => Color.clear,
		}));
	}
	
#region Converters
	
	// https://stackoverflow.com/a/32037899
	public static System.Func<TInput, TOutput> CreateConvertFunc<TInput, TOutput>()
	{
		var source = Expression.Parameter(typeof(TInput), "source");
		// the next will throw if no conversion exists
		var convert = Expression.Convert(source, typeof(TOutput));
		var method = convert.Method;
		return Expression.Lambda<System.Func<TInput, TOutput>>(convert, source).Compile();
	}
	
	private static Regex? MATH_REG = null;
	
	public static JSONNode? RawToJson(string raw) {
		MATH_REG ??= new Regex(@"(?<!""{1}.*)(?:\(?\s*[\d\.]+\)?\s*[\-+/*]\s*)+[\d\.]+\s*\)?");
		var table = new System.Data.DataTable();
		var maths = MATH_REG.Matches(raw);
		foreach (Match math in maths) {
			var computed = table.Compute(math.Value, "");
			raw = raw.Replace(math.Value, computed.ToString());
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
	
	public static string? JsonToRaw(JSONNode node) {
		return node.ToString();
	}
	
	public static JSONNode StringJson(string value) {
		return new JSONString(value);
	}
	
	public static string? JsonString(JSONNode node) {
		return (string?)(node as JSONString);
	}
	
#endregion
}

}
