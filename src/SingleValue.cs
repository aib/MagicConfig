using System;

namespace MagicConfig
{
	// This class holds a single value, usually a scalar
	// For reference types, you should prefer SingleItem
	// Updates to this class destroy the old value
	public class SingleValue<T>: ConfigItem, IEquatable<SingleValue<T>>
	{
		public class UpdatedArgs: EventArgs { public T OldValue; public T NewValue; }
		public event EventHandler<UpdatedArgs> Updated;

		private T value;
		public T Value => value;

		public SingleValue() :this(default(T)) {}

		public SingleValue(T value)
		{
			this.value = value;
		}

		public static implicit operator T(SingleValue<T> s)
		{
			return (s == null) ? default(T) : s.value;
		}

		public static implicit operator SingleValue<T>(T v)
		{
			return new SingleValue<T>(v);
		}

		public bool Equals(SingleValue<T> other)
		{
			return !object.ReferenceEquals(other, null) && value.Equals(other.value);
		}

		public override bool Equals(ConfigItem other)
		{
			return (other is SingleValue<T> otherItem) && Equals(otherItem);
		}

		public override void Assign(ConfigItem other)
		{
			if (other is SingleValue<T> otherItem) {
				var args = new UpdatedArgs { OldValue = value, NewValue = otherItem.value };
				value = otherItem.value;
				Updated?.Invoke(this, args);
			} else {
				throw new InvalidTypeAssignmentException(this, other);
			}
		}

		public override string ToString() => value.ToString();
	}
}
