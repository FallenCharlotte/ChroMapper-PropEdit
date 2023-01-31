using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using SimpleJSON;

using Beatmap.Base;

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
			if (o.CustomData?.HasKey(field_name) ?? false) {
				return CreateConvertFunc<JSONNode, T>()(o.CustomData[field_name]);
			}
			else {
				return null;
			}
		};
		System.Action<BaseObject, T?> setter = (o, v) => {
			if (v is T value) {
				o.GetOrCreateCustom()[field_name] = CreateConvertFunc<T, SimpleJSON.JSONNode>()(value);
			}
			else {
				o.CustomData?.Remove(field_name);
			}
			o.RefreshCustom();
		};
		return (getter, setter);
	}
	
	// I hate C#
	public static (System.Func<BaseObject, string>, System.Action<BaseObject, string>) GetSetString(System.Type type, string field_name) {
		var field = type.GetProperty(field_name);
		System.Func<BaseObject, string> getter = (o) => (string)field.GetMethod.Invoke(o, null) ?? null;
		System.Action<BaseObject, string> setter = (o, v) => {
			field.SetMethod.Invoke(o, new object[] {v});
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
	public static string GetAllOrNothingString(IEnumerable<BaseObject> editing, System.Func<BaseObject, string> getter) {
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
	
	// Color float to color int 0-255
	private static int cftoi(float f) {
		return System.Math.Min((int) (f * 255), 255);
	}
	
	public static (System.Func<BaseObject, string>, System.Action<BaseObject, string>) GetSetColor(string field_name) {
		System.Func<BaseObject, string> getter = (o) => {
			if (o.CustomData?.HasKey(field_name) ?? false) {
				var color = o.CustomData[field_name].ReadColor();
				return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", cftoi(color.r), cftoi(color.g), cftoi(color.b), cftoi(color.a));
			}
			else {
				return null;
			}
		};
		System.Action<BaseObject, string> setter = (o, v) => {
			if (string.IsNullOrEmpty(v)) {
				o.CustomColor = null;
				o.CustomData?.Remove(field_name);
			}
			else {
				ColorUtility.TryParseHtmlString(v, out var color);
				o.CustomColor = color;
				var jc = new JSONArray();
				jc[0] = color.r;
				jc[1] = color.g;
				jc[2] = color.b;
				jc[3] = color.a;
				o.GetOrCreateCustom()[field_name] = jc;
				
			}
		};
		return (getter, setter);
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
