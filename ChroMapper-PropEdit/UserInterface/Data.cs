using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using SimpleJSON;

using Beatmap.Base;
using Beatmap.Shared;

namespace ChroMapper_PropEdit.UserInterface {

public class Data {
	
#region Getter/setter factories
	
	public static (System.Func<BaseObject, T?>, System.Action<BaseObject, T?>) GetSet<T>(System.Type type, string field_name) where T : struct {
		var field = type.GetProperty(field_name);
		System.Func<BaseObject, T?> getter = (o) => (T?)field.GetMethod.Invoke(o, null) ?? null;
		System.Action<BaseObject, T?> setter = (o, v) => field.SetMethod.Invoke(o, new object[] {v});
		return (getter, setter);
	}
	
	public static (System.Func<BaseObject, T?>, System.Action<BaseObject, T?>) CustomGetSet<T>(string field_name) where T : struct {
		System.Func<BaseObject, T?> getter = (o) => {
			if (GetNode(o.CustomData, field_name) is JSONNode n) {
				return CreateConvertFunc<JSONNode, T>()(n);
			}
			else {
				return null;
			}
		};
		System.Action<BaseObject, T?> setter = (o, v) => {
			if (v is T value) {
				SetNode(o.GetOrCreateCustom(), field_name, CreateConvertFunc<T, SimpleJSON.JSONNode>()(value));
			}
			else {
				RemoveNode(o.CustomData, field_name);
			}
			o.RefreshCustom();
		};
		return (getter, setter);
	}
	
	// I hate C#
	public static (System.Func<BaseObject, string>, System.Action<BaseObject, string>) CustomGetSet(string field_name) {
		System.Func<BaseObject, string> getter = (o) => {
			if (GetNode(o.CustomData, field_name) is JSONNode n) {
				return n.Value;
			}
			else {
				return null;
			}
		};
		System.Action<BaseObject, string> setter = (o, v) => {
			if (v is string value) {
				SetNode(o.GetOrCreateCustom(), field_name, value);
			}
			else {
				RemoveNode(o.CustomData, field_name);
			}
			o.RefreshCustom();
		};
		return (getter, setter);
	}
	
	// Create and delete gradient
	public static (System.Func<BaseObject, bool?>, System.Action<BaseObject, bool?>) GetSetGradient() {
		System.Func<BaseObject, bool?> getter = (o) => ((BaseEvent)o).CustomLightGradient != null;
		System.Action<BaseObject, bool?> setter = (o, v) => { if (o is BaseEvent e) {
			if (!(v ?? false)) {
				e.CustomData?.Remove(e.CustomKeyLightGradient);
				e.CustomLightGradient = null;
			}
			else if (e.CustomLightGradient == null) {
				// TODO: fix this
				ColorUtility.TryParseHtmlString("#FFFFFF", out var begin);
				ColorUtility.TryParseHtmlString("#000000", out var end);
				e.CustomLightGradient = new ChromaLightGradient(begin, end);
			}
		}};
		return (getter, setter);
	}
	
	public static (System.Func<BaseObject, string>, System.Action<BaseObject, string>) CustomGetSetColor(string field_name) {
		System.Func<BaseObject, string> getter = (o) => {
			if (GetNode(o.CustomData, field_name) is JSONNode n) {
				var color = n.ReadColor();
				return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
			}
			else {
				return null;
			}
		};
		System.Action<BaseObject, string> setter = (o, v) => {
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
	
	public static T? GetAllOrNothing<T>(IEnumerable<BaseObject> editing, System.Func<BaseObject, T?> getter) where T : struct {
		var it = editing.GetEnumerator();
		it.MoveNext();
		var last = getter(it.Current);
		// baby C# though null checks
		if (last is T l) {
			while (it.MoveNext()) {
				if (getter(it.Current) is T v) {
					if (!EqualityComparer<T>.Default.Equals(v, l)) {
						last = null;
						break;
					}
				}
			}
		}
		
		return last;
	}
	
	// I hate C#
	public static string GetAllOrNothing(IEnumerable<BaseObject> editing, System.Func<BaseObject, string> getter) {
		var it = editing.GetEnumerator();
		it.MoveNext();
		var last = getter(it.Current);
		while (last != null && it.MoveNext()) {
			if (last != getter(it.Current)) {
				last = null;
				break;
			}
		}
		
		return last;
	}
	
	public static JSONNode GetNode(JSONNode root, string name) {
		string[] path = name.Split('.');
		foreach (string node in path) {
			if (!(root?.HasKey(node) ?? false)) {
				return null;
			}
			root = root[node];
		}
		return root;
	}
	
	public static void SetNode(JSONNode root, string name, JSONNode o) {
		string[] path = name.Split('.');
		for (int i = 0; i < path.Length - 1; ++i) {
			root = root[path[i]];
		}
		root[path[path.Length-1]] = o;
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
