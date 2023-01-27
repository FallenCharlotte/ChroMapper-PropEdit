using System.IO;
using System.Reflection;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using SimpleJSON;

namespace ChroMapper_PropEdit.UserInterface {

public class Data {
	public static (System.Func<BeatmapObject, T?>, System.Action<BeatmapObject, T?>) BaseGetSet<T>(System.Type type, string field_name) where T : struct {
		var field = type.GetField(field_name);
		System.Func<BeatmapObject, T?> getter = (o) => (T)field.GetValue(o);
		System.Action<BeatmapObject, T?> setter = (o, v) => field.SetValue(o, v);
		return (getter, setter);
	}
	
	public static (System.Func<BeatmapObject, T?>, System.Action<BeatmapObject, T?>) CustomGetSet<T>(string field_name) where T : struct {
		System.Func<BeatmapObject, T?> getter = (o) => {
			if (o.CustomData?.HasKey(field_name) ?? false) {
				return CreateConvertFunc<JSONNode, T>()(o.CustomData[field_name]);
			}
			else {
				return null;
			}
		};
		System.Action<BeatmapObject, T?> setter = (o, v) => {
			if (v is T value) {
				o.GetOrCreateCustomData()[field_name] = CreateConvertFunc<T, SimpleJSON.JSONNode>()(value);
			}
			else {
				o.CustomData?.Remove(field_name);
			}
		};
		return (getter, setter);
	}
	
	// I hate C#
	public static (System.Func<BeatmapObject, string>, System.Action<BeatmapObject, string>) CustomGetSetString(string field_name) {
		System.Func<BeatmapObject, string> getter = (o) => {
			if (o.CustomData?.HasKey(field_name) ?? false) {
				return ((JSONString)o.CustomData[field_name]).Value;
			}
			else {
				return null;
			}
		};
		System.Action<BeatmapObject, string> setter = (o, v) => {
			if (string.IsNullOrEmpty(v)) {
				o.CustomData?.Remove(field_name);
			}
			else {
				o.GetOrCreateCustomData()[field_name] = v;
			}
		};
		return (getter, setter);
	}
	
	// Color float to color int 0-255
	private static int cftoi(float f) {
		return System.Math.Min((int) (f * 255), 255);
	}
	
	public static (System.Func<BeatmapObject, string>, System.Action<BeatmapObject, string>) GetSetColor(string field_name) {
		System.Func<BeatmapObject, string> getter = (o) => {
			if (o.CustomData?.HasKey(field_name) ?? false) {
				var color = o.CustomData[field_name].AsArray;
				if (color.Count == 4) {
					return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", cftoi(color[0]), cftoi(color[1]), cftoi(color[2]), cftoi(color[3]));
				}
				else {
					return string.Format("#{0:X2}{1:X2}{2:X2}", cftoi(color[0]), cftoi(color[1]), cftoi(color[2]));
				}
			}
			else {
				return null;
			}
		};
		System.Action<BeatmapObject, string> setter = (o, v) => {
			if (string.IsNullOrEmpty(v)) {
				o.CustomData?.Remove(field_name);
			}
			else {
				ColorUtility.TryParseHtmlString(v, out var color);
				var jc = new JSONArray();
				jc[0] = color.r;
				jc[1] = color.g;
				jc[2] = color.b;
				jc[3] = color.a;
				o.GetOrCreateCustomData()[field_name] = jc;
				
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
