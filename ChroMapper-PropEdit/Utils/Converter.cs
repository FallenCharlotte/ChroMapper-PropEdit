using System;

namespace ChroMapper_PropEdit.Utils {

public interface IConverter<TInput, TOutput> {
	public TOutput Forwards(TInput value);
	public TInput Backwards(TOutput value);
}

public class Converter<TInput, TOutput> : IConverter<TInput, TOutput> {
	private Func<TInput, TOutput> _forwards;
	private Func<TOutput, TInput> _backwards;
	
	public Converter(Func<TInput, TOutput> forwards, Func<TOutput, TInput> backwards) {
		_forwards = forwards;
		_backwards = backwards;
	}
	
	public TOutput Forwards(TInput value) {
		return _forwards(value);
	}
	
	public TInput Backwards(TOutput value) {
		return _backwards(value);
	}
}

public static class ConverterExtention {
	extension<T1, T2, T3>(IConverter<T1, T2>) {
		public static Converter<T1, T3> operator+(IConverter<T1, T2> left, IConverter<T2, T3> right)
			=> new(
				(value) => right.Forwards(left.Forwards(value)),
				(value) => left.Backwards(right.Backwards(value))
			);
	}
	
	extension<T1, T2>(IAccessor<T1>) {
		public static Accessor<T2> operator+(IAccessor<T1> left, IConverter<T1, T2> right)
			=> new(
				() => right.Forwards(left.Get()),
				(v) => left.Set(right.Backwards(v)),
				left
			);
	}
}

}
