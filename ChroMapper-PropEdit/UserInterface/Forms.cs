using System.IO;
using System.Reflection;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;

namespace ChroMapper_PropEdit.UserInterface {

public class Forms {
	public static (System.Func<BeatmapObject, T>, System.Action<BeatmapObject, T>) BaseGetSet<T>(System.Type type, string field_name) {
		var field = type.GetField(field_name);
		System.Func<BeatmapObject, T> getter = (o) => (T)field.GetValue(o);
		System.Action<BeatmapObject, T> setter = (o, v) => field.SetValue(o, v);
		return (getter, setter);
	}
	
	public static (System.Func<BeatmapObject, T>, System.Action<BeatmapObject, T>) CustomGetSet<T>(string field_name) {
		System.Func<BeatmapObject, T> getter = (o) => CreateConvertFunc<SimpleJSON.JSONNode, T>()(o.GetOrCreateCustomData()[field_name]);
		System.Action<BeatmapObject, T> setter = (o, v) => o.GetOrCreateCustomData()[field_name] = CreateConvertFunc<T, SimpleJSON.JSONNode>()(v);
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
