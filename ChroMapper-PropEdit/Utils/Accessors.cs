using System;
using System.Collections.Generic;

namespace ChroMapper_PropEdit.Utils;

public interface IAccessor {
	public abstract bool IsMixed();
}

// Represets a getter and setter for a single object
public interface IAccessor<T> : IAccessor {
	public T Get();
	public void Set(T value);
	
	public (T, bool) Get2();
}

public class Accessor<T> : IAccessor<T> {
	public delegate T Getter();
	public delegate void Setter(T v);
	
	public Accessor(Getter getter, Setter setter, IAccessor? parent = null) {
		_getter = getter;
		_setter = setter;
		_parent = parent;
	}
	
	public T Get() {
		return _getter();
	}
	
	public (T, bool) Get2() {
		return (Get(), IsMixed());
	}
	
	public void Set(T value) {
		_setter(value);
	}
	
	public virtual bool IsMixed() {
		return _parent?.IsMixed() ?? false;
	}
	
	private Getter _getter;
	private Setter _setter;
	private IAccessor? _parent;
}
