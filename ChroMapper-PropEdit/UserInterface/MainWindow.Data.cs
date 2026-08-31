using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using SimpleJSON;

using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Enums;
using Beatmap.Helper;
using Beatmap.Shared;

using ChroMapper_PropEdit.Components;
using ChroMapper_PropEdit.Enums;
using ChroMapper_PropEdit.Utils;

namespace ChroMapper_PropEdit.UserInterface {

public partial class MainWindow : UIWindow {
	private MultiAccessor<T?> EditingAccessor<T>(IPerObjectAccessor<T?> o_accessor, bool time = false)
		=> new MultiAccessor<T?>(editing!, o_accessor, time);
	
	private MultiAccessor<T?> ObjectField<T>(string field_name, bool time = false) where T : struct
		=> EditingAccessor(new ObjectFieldAccessor<T>(field_name), time);
	
	private MultiAccessor<T?> NullableField<T>(string field_name, bool time = false)
		=> EditingAccessor(new ObjectFieldAccessorNullable<T>(field_name), time);
	
	private MultiAccessor<JSONNode?> CustomField(string path, string field_name = "CustomData")
		=> EditingAccessor(new JSONAccessor(field_name, path));
	
	private IAccessor<T?> CustomField<T>(string path, string field_name = "CustomData")
		=> CustomField(path, field_name).Insert(Data.JSONValue<T?>());
	private IAccessor<string?> CustomFieldRaw(string path, string field_name = "CustomData")
		=> CustomField(path, field_name).Insert(Data.JSONRaw());
	
	private IAccessor<T?> DataField<T>(string path) => CustomField<T>(path, "Data");
	private IAccessor<string?> DataFieldRaw(string path) => CustomFieldRaw(path, "Data");
	
	public interface IPerObjectAccessor<T> {
		public T Get(object o);
		public void Set(object o, T value);
	}
	
	public class PerObjectAccessor<T> : IPerObjectAccessor<T> {
		public delegate T Getter(object o);
		public delegate void Setter(object o, T value);
		
		public PerObjectAccessor(Getter getter, Setter setter) {
			_getter = getter;
			_setter = setter;
		}
		
		public T Get(object o)
			=> _getter(o);
		public void Set(object o, T value)
			=> _setter(o, value);
		
		private Getter _getter;
		private Setter _setter;
	}
	
	public class ObjectFieldAccessor<T> : IPerObjectAccessor<T?> where T : struct {
		public ObjectFieldAccessor(string field_name)
			=> this.field_name = field_name;
		
		public T? Get(object o)
			=> (T?)o.GetType().GetProperty(field_name).GetMethod.Invoke(o, null) ?? null;
		public void Set(object o, T? v) {
			if (v != null) o.GetType().GetProperty(field_name).SetMethod.Invoke(o, new object[] {v});
		}
		
		private string field_name;
	}
	public class ObjectFieldAccessorNullable<T> : IPerObjectAccessor<T?> {
		public ObjectFieldAccessorNullable(string field_name)
			=> this.field_name = field_name;
		
		public T? Get(object o)
			=> (T?)o.GetType().GetProperty(field_name).GetMethod.Invoke(o, null);
		public void Set(object o, T? v) {
			o.GetType().GetProperty(field_name).SetMethod.Invoke(o, new object?[] {v});
		}
		
		private string field_name;
	}
	
	// Very cursed value split: subtract 1 then mask
	public MultiAccessor<int?> SplitEventValue(int mask)
		=> new MultiAccessor<int?>(editing!, new PerObjectAccessor<int?>(
			(o) => {
				int i = ((BaseEvent)o).Value;
				return (i == 0)
					? 0b1111
					: (i - 1) & mask & 0b1111;
			},
			(o, v) => {
				if (v is int value) {
					int i = ((BaseEvent)o).Value;
					// I'm sorry
					((BaseEvent)o).Value = ((((i - (i == 0 ? 0 : 1)) & (~mask)) | (value)) + 1) & 0b1111;
				}
			}
		), false);
	
	public class JSONAccessor : IPerObjectAccessor<JSONNode?> {
		public JSONAccessor(string field_name, string path) {
			this.field_name = field_name;
			this.path = path;
		}
		
		public JSONNode? Get(object o) {
			var root = (SimpleJSON.JSONNode)o.GetType().GetProperty(field_name).GetMethod.Invoke(o, null) ?? new SimpleJSON.JSONObject();
			return Data.GetNode(root, path);
		}
		
		public void Set(object o, JSONNode? v) {
			var root = (SimpleJSON.JSONNode)o.GetType().GetProperty(field_name).GetMethod.Invoke(o, null) ?? new SimpleJSON.JSONObject();
			if (v is JSONNode value) {
				Data.SetNode(root, path, value);
			}
			else {
				Data.RemoveNode(root, path);
			}
			o.GetType().GetProperty(field_name).SetMethod.Invoke(o, new object[] { root });
			(o as BaseObject)?.RefreshCustom();
		}
		
		private string field_name;
		private string path;
	}
	
	
	
	// Vivify properties are stored as arrays of json objects, grabs a component out of the object with matching id
	public class PropertyAccessor : IPerObjectAccessor<JSONNode?> {
		public PropertyAccessor(string? id, string component, string? default_type = null) {
			_id = id;
			_component = component;
			_default_type = default_type;
		}
		
		public JSONNode? Get(object o) {
			if (_id == null) return null;
			var root = (o as BaseCustomEvent)!.Data ?? new SimpleJSON.JSONObject();
			if (Data.GetNode(root, "properties") is JSONArray props) {
				foreach (var prop in props.Children) {
					if ((string)prop.AsObject["id"] == _id) {
						return prop.AsObject[_component];
					}
				}
			}
			return null;
		}
		
		public void Set(object o, JSONNode? value) {
			var root = (o as BaseCustomEvent)!.Data ?? new SimpleJSON.JSONObject();
			var props = Data.GetNode(root, "properties")?.AsArray ?? new JSONArray();
			if (_id == null) {
				_id = value;
			}
			if (_id == null) {
				return;
			}
			JSONObject? _prop = null;
			foreach (var prop in props.Children) {
				if (prop.AsObject["id"] == _id) {
					_prop = prop.AsObject;
					break;
				}
			}
			if (_prop == null) {
				_prop = new JSONObject();
				_prop["id"] = _id;
				if (_default_type != null) {
					_prop["type"] = _default_type;
				}
				props.Add(_prop);
			}
			if (value == null) {
				props.Remove((JSONNode)_prop);
			}
			else {
				Data.SetNode(_prop, _component, value);
			}
			root["properties"] = props;
			(o as BaseCustomEvent)!.Data = root;
			(o as BaseObject)?.RefreshCustom();
		}
		
		private string? _id; // Null is used for adding new properties
		private string _component;
		private string? _default_type;
	}
	
	private MultiAccessor<JSONNode?> PropertyPart(string? id, string component, string? type = null)
		=> new MultiAccessor<JSONNode?>(editing!, new PropertyAccessor(id, component, type), false);
	
	private IAccessor<string?> PropertyValue(string? id, string? type = null)
		=> PropertyPart(id, "value", type).Insert(Data.JSONRaw());
	
	private IAccessor<string?> PropertyComponent(string? id, string component)
		=> PropertyPart(id, component).Insert(Data.JSONValue<string?>());
	
	public class GeometryAccessor : IPerObjectAccessor<bool?> {
		public bool? Get(object ee)
			=> (ee as BaseEnvironmentEnhancement)!.Geometry != null;
		public void Set(object ee, bool? v) {
			if (v == false) {
				(ee as BaseEnvironmentEnhancement)!.Geometry = null;
			}
			else {
				(ee as BaseEnvironmentEnhancement)!.Geometry ??= new JSONObject();
			}
		}
	}
	private IAccessor<bool?> GeometryField()
		=> EditingAccessor(new GeometryAccessor());
	
	// Create and delete gradient
	private class V2Gradient : IPerObjectAccessor<bool?> {
		public bool? Get(object o) => ((BaseEvent)o).CustomLightGradient != null;
		public void Set(object o, bool? v) { if (o is BaseEvent e) {
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
				
				Color begin = Data.GetColor(e);
				Color end = (next != null) ? Data.GetColor(next) : begin;
				
				float duration = (next != null) ? (next.JsonTime - e.JsonTime) : 1;
				
				e.GetOrCreateCustom()[e.CustomKeyLightGradient] = (new ChromaLightGradient(begin, end, duration)).ToJson();
				e.CustomData.Remove(e.CustomKeyColor);
			}
		}}
	}
	
	private IAccessor<bool?> EEComponent(string name)
		=> EditingAccessor(new PerObjectAccessor<bool?>(
			(ee) => (ee as BaseEnvironmentEnhancement)!.Components?.HasKey(name),
			(o, v) => {
				var ee = (o as BaseEnvironmentEnhancement)!;
				if (v == false) {
					ee.Components?.Remove(name);
				}
				else {
					// ??= just doesn't fucking work for some reason
					if (ee.Components == null) ee.Components = new JSONObject();
					ee.Components[name] = new JSONObject();
				}
			}));
	
	// Create or remove object with default json
	private IAccessor<bool?> CustomJSONNode(string path, string default_json)
		=> EditingAccessor(new PerObjectAccessor<bool?>(
			(o) => Data.GetNode(((BaseObject)o).CustomData, path) != null,
			(o, v) => {
				if (!(v ?? false)) {
					Data.RemoveNode(((BaseObject)o).CustomData, path);
				}
				else if (Data.GetNode(((BaseObject)o).CustomData, path) == null) {
					Data.SetNode(((BaseObject)o).CustomData, path, JSON.Parse(default_json));
				}
			}));
	
	
	public class MultiAccessor<T> : IAccessor<T?> {
		public MultiAccessor(IList objects, IPerObjectAccessor<T?> o_accesser, bool time = false) {
			this.objects = objects;
			this.o_accesser = o_accesser;
			this.time = time;
		}
		
		public T? Get() {
			T? value;
			(value, Mixed) = GetAllOrNothing<T?>(objects, o_accesser.Get);
			return value;
		}
		
		public (T?, bool) Get2() {
			return (Get(), IsMixed());
		}
		
		public void Set(T? value) {
			try { switch (objects) {
			case List<BaseObject> editing:
				bool ees = false;
				var modified = new List<BaseObject>();
				foreach (var o in editing!) {
					// Work around chromapper bug where all edits to any environment enhancement gets applied to [0]
					// Still needed in 0.14
					if (o is BaseEnvironmentEnhancement eh) {
						ees = true;
						Plugin.Trace("Funky workaround!");
						var collection = (GeometryGridContainer)BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.EnvironmentEnhancement);
						
						if (collection.LoadedContainers.ContainsKey(eh)) {
							GameObject.DestroyImmediate(collection.LoadedContainers[eh].gameObject);
							collection.LoadedContainers.Remove(eh);
							collection.ObjectsWithContainers.Remove(eh);
						}
						SelectionController.Deselect(o, false);
						
						o_accesser.Set(o, value);
					}
					else {
						var mod = BeatmapFactory.Clone(o);
						modified.Add(mod);
						
						o_accesser.Set(mod, value);
						
						Plugin.Trace($"{o.ToJson()} => {mod.ToJson()}");
					}
				}
				if (ees) {
					var collection = (GeometryGridContainer)BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.EnvironmentEnhancement);
					collection.RefreshPool(true);
					foreach (var o in editing!) {
						SelectionController.Select(o, true, false, false);
					}
					Plugin.map_settings!.Refresh();
				}
				else {
					BeatmapActionContainer.AddAction(
						new BeatmapObjectModifiedCollectionAction(modified, editing, $"Edited ({modified.Count}) objects with Prop Edit."),
						true);
				}
				// Need to refresh Selection.Selected
				Selection.OnObjectsSelected();
				break;
			default:
				foreach (var i in objects) {
					o_accesser.Set(i, value);
				}
				Debug.LogWarning($"{objects.Count} items edited directly with PropEdit, undo/redo will not work for these!");
				break;
		} }
			catch (Exception e) {
				Debug.LogError("Error editing objects with PropEdit!");
				Debug.LogException(e);
			}
		}
		
		public virtual bool IsMixed()
			=> Mixed;
		
		// Run a converter *before* aggregating
		public MultiAccessor<T2> Insert<T2>(IConverter<T?, T2?> conv)
			=> new(
				objects,
				new PerObjectAccessor<T2?>(
					(o) => conv.Forwards(o_accesser.Get(o)),
					(o, v) => o_accesser.Set(o, conv.Backwards(v))
				),
				time
			);
		
		public bool Mixed { get; private set; }
		
		private bool time;
		private IList objects;
		private IPerObjectAccessor<T?> o_accesser;
	}
	
	// Split off from MultiAccessor so some things can use it easier
	private static (T?, bool) GetAllOrNothing<T>(IEnumerable editing, Func<object, T> getter) {
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
}

}
