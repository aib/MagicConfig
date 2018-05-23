using System;

namespace MagicConfig
{
	// This class holds a single value, usually a scalar
	// Updates to this class destroy the old value
	public class SingleItem<T>: SingleValue<T>, IEquatable<SingleItem<T>>
		where T: IEquatable<T>
	{
		public SingleItem(T value) :base(value) {}

		public static implicit operator T(SingleItem<T> s)
		{
			return s.value;
		}

		public static implicit operator SingleItem<T>(T v)
		{
			return new SingleItem<T>(v);
		}

		public bool Equals(SingleItem<T> other)
		{
			return !object.ReferenceEquals(other, null) && value.Equals(other.value);
		}
	}
}
