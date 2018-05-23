using System;

namespace MagicConfig
{
	// This class holds a single value that is not or cannot be made IEquatable -- such as an enum
	// It is an inferior version of SingleItem, please do not use unless you have to
	public class SingleValue<T>: ConfigItem, IEquatable<SingleValue<T>>
	{
		public class UpdatedArgs: EventArgs { public T OldValue; public T NewValue; }
		public event EventHandler<UpdatedArgs> Updated;

		protected T value;

		public SingleValue(T value)
		{
			this.value = value;
		}

		public T Value => value;

		public static implicit operator T(SingleValue<T> s)
		{
			return s.value;
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
	}
}
