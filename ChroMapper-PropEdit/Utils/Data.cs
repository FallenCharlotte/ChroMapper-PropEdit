using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using SimpleJSON;

using Beatmap.Base;
using Beatmap.Enums;
using Beatmap.Helper;
using Beatmap.Shared;

using ChroMapper_PropEdit.Enums;

namespace ChroMapper_PropEdit.Utils {

public class Data {
	
#region Getter/setter factories
	
	public static (System.Func<BaseObject, T?>, System.Action<BaseObject, T?>) GetSet<T>(string field_name) where T : struct {
		System.Func<BaseObject, T?> getter = (o) => (T?)o.GetType().GetProperty(field_name).GetMethod.Invoke(o, null) ?? null;
		System.Action<BaseObject, T?> setter = (o, v) => { if (v != null) o.GetType().GetProperty(field_name).SetMethod.Invoke(o, new object[] {v}); };
		return (getter, setter);
	}
	public static (System.Func<BaseObject, T?>, System.Action<BaseObject, T?>) GetSetTest<T>(string field_name) {
		System.Func<BaseObject, T?> getter = (o) => (T?)o.GetType().GetProperty(field_name).GetMethod.Invoke(o, null);
		System.Action<BaseObject, T?> setter = (o, v) => { if (v != null) o.GetType().GetProperty(field_name).SetMethod.Invoke(o, new object[] {(T)v}); };
		return (getter, setter);
	}
	
	// Very cursed value split: subtract 1 then mask
	public static (System.Func<BaseObject, int?>, System.Action<BaseObject, int?>) GetSetSplitValue(int mask) {
		System.Func<BaseObject, int?> getter = (o) => {
			int i = ((BaseEvent)o).Value;
			return (i == 0)
				? 0b1111
				: (i - 1) & mask & 0b1111;
		};
		System.Action<BaseObject, int?> setter = (o, v) => {
			if (v is int value) {
				int i = ((BaseEvent)o).Value;
				// I'm sorry
				((BaseEvent)o).Value = ((((i - (i == 0 ? 0 : 1)) & (~mask)) | (value)) + 1) & 0b1111;
			}
		};
		return (getter, setter);
	}
	
	public static (System.Func<BaseObject, T?>, System.Action<BaseObject, T?>) JSONGetSet<T>(System.Type type, string node_name, string field_name) {
		var node = type.GetProperty(node_name);
		if (node == null) {
			Debug.LogError($"Node {node_name} not found in type {type.FullName}!");
		}
		System.Func<BaseObject, T?> getter = (o) => {
			var root = (SimpleJSON.JSONNode)node!.GetMethod.Invoke(o, null) ?? new SimpleJSON.JSONObject();
			if (GetNode(root, field_name) is JSONNode n) {
				return CreateConvertFunc<JSONNode, T>()(n);
			}
			else {
				return default!;
			}
		};
		System.Action<BaseObject, T?> setter = (o, v) => {
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
	
	// Fine, I'll do arrays. JSON-array text input. Might make an options page that can split it out later:tm:
	public static (System.Func<BaseObject, string?>, System.Action<BaseObject, string?>) JSONGetSetRaw(System.Type type, string node_name, string field_name) {
		var node = type.GetProperty(node_name);
		if (node == null) {
			Debug.LogError($"Node {node_name} not found in type {type.FullName}!");
		}
		System.Func<BaseObject, string?> getter = (o) => {
			var root = (SimpleJSON.JSONNode)node!.GetMethod.Invoke(o, null) ?? new SimpleJSON.JSONObject();
			if (GetNode(root, field_name) is JSONNode n) {
				return n.ToString();
			}
			else {
				return null;
			}
		};
		System.Action<BaseObject, string?> setter = (o, v) => {
			var root = (SimpleJSON.JSONNode)node!.GetMethod.Invoke(o, null) ?? new SimpleJSON.JSONObject();
			if (string.IsNullOrEmpty(v)) {
				RemoveNode(root, field_name);
			}
			else {
				JSONNode n;
				try {
					n = JSON.Parse(v);
				}
				catch (Exception) {
					try {
						n = JSON.Parse($"[{v}]");
					}
					catch (Exception) {
						try {
							n = JSON.Parse($"\"{v}\"");
						}
						catch (Exception) {
							Debug.LogWarning($"Couldn't interpret \"{v}\" as JSON");
							return;
						}
					}
				}
				SetNode(root, field_name, n);
			}
			node.SetMethod.Invoke(o, new object[] { root });
		};
		return (getter, setter);
	}
	
	public static (System.Func<BaseObject, T?>, System.Action<BaseObject, T?>) CustomGetSet<T>(string field_name) {
		return JSONGetSet<T?>(typeof(BaseObject), "CustomData", field_name);
	}
	
	public static (System.Func<BaseObject, string?>, System.Action<BaseObject, string?>) CustomGetSetRaw(string field_name) {
		return JSONGetSetRaw(typeof(BaseObject), "CustomData", field_name);
	}
	
	// Create and delete gradient
	public static (System.Func<BaseObject, bool?>, System.Action<BaseObject, bool?>) GetSetGradient() {
		System.Func<BaseObject, bool?> getter = (o) => ((BaseEvent)o).CustomLightGradient != null;
		System.Action<BaseObject, bool?> setter = (o, v) => { if (o is BaseEvent e) {
			if (!(v ?? false)) {
				if (e.CustomLightGradient != null) {
					var jc = new JSONArray();
					jc.WriteColor(e.CustomLightGradient.StartColor);
					e.CustomData[e.CustomKeyColor] = jc;
				}
				e.CustomData?.Remove(e.CustomKeyLightGradient);
			}
			else if (e.CustomLightGradient == null) {
				var collection = BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Event);
				
				var next = (BaseEvent) collection.LoadedObjects
					.Where(n => (((BaseEvent)n).Type == e.Type) && (n.JsonTime > e.JsonTime))
					.FirstOrDefault();
				
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
	public static (System.Func<BaseObject, bool?>, System.Action<BaseObject, bool?>) GetSetAnimation(bool v2) {
		string animation_key = v2 ? "_animation" : "animation";
		return CustomGetSetNode(animation_key, "{}");
	}
	
	// Create or remove object with default json
	public static (System.Func<BaseObject, bool?>, System.Action<BaseObject, bool?>) CustomGetSetNode(string path, string json) {
		System.Func<BaseObject, bool?> getter = (o) => GetNode(o.CustomData, path) != null;
		System.Action<BaseObject, bool?> setter = (o, v) => {
			if (!(v ?? false)) {
				RemoveNode(o.CustomData, path);
			}
			else if (GetNode(o.CustomData, path) == null) {
				SetNode(o.CustomData, path, JSON.Parse(json));
			}
		};
		return (getter, setter);
	}
	
	public static (System.Func<BaseObject, string?>, System.Action<BaseObject, string?>) CustomGetSetColor(string field_name) {
		System.Func<BaseObject, string?> getter = (o) => {
			if (GetNode(o.CustomData, field_name) is JSONNode n) {
				var color = n.ReadColor();
				return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
			}
			else {
				return null;
			}
		};
		System.Action<BaseObject, string?> setter = (o, v) => {
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
	
	public static T? GetAllOrNothing<T>(IEnumerable<BaseObject> editing, System.Func<BaseObject, T?> getter) {
		var it = editing.GetEnumerator();
		it.MoveNext();
		var last = getter(it.Current);
		while (it.MoveNext()) {
			T? v = getter(it.Current);
			if (v == null || last == null || !(v!.Equals(last))) {
				last = default!;
				break;
			}
		}
		
		return last;
	}
	
	public static void UpdateObjects<T>(IEnumerable<BaseObject> editing, System.Action<BaseObject, T?> setter, T? value, bool time = false) {
		var beatmapActions = new List<BeatmapObjectModifiedAction>();
		foreach (var o in editing!) {
			var orig = BeatmapFactory.Clone(o);
			
			if (time) {
				// Based on SelectionController.MoveSelection
				var collection = BeatmapObjectContainerCollection.GetCollectionForType(o.ObjectType);
				
				collection.DeleteObject(o, false, false, default, true, false);
				
				setter(o, value);
				
				collection.SpawnObject(o, false, true);
			}
			else {
				setter(o, value);
				o.RefreshCustom();
			}
			
			beatmapActions.Add(new BeatmapObjectModifiedAction(o, o, orig, $"Edited a {o.ObjectType} with Prop Edit.", true));
		}
		
		BeatmapActionContainer.AddAction(
			new ActionCollectionAction(beatmapActions, true, false, $"Edited ({editing.Count()}) objects with Prop Edit."),
			true);
		if (time) {
			BeatmapObjectContainerCollection.RefreshAllPools();
		}
	}
	
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
	
	public static Color GetColor(BaseEvent e) {
		return (e.CustomColor ?? (e.Value switch {
			0 => Color.clear,
			(>= 1) and (<= 4) => BeatSaberSongContainer.Instance.DifficultyData.EnvColorRight ?? BeatSaberSong.DefaultRightColor,
			(>= 5) and (<= 8) => BeatSaberSongContainer.Instance.DifficultyData.EnvColorLeft ?? BeatSaberSong.DefaultLeftColor,
			(>= 9) => Color.white,
			_ => Color.clear,
		}));
	}
	
	// https://stackoverflow.com/a/32037899
	public static System.Func<TInput, TOutput> CreateConvertFunc<TInput, TOutput>()
	{
		var source = Expression.Parameter(typeof(TInput), "source");
		// the next will throw if no conversion exists
		var convert = Expression.Convert(source, typeof(TOutput));
		var method = convert.Method;
		return Expression.Lambda<System.Func<TInput, TOutput>>(convert, source).Compile();
	}
}

}
