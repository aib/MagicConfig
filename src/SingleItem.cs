using System;

namespace MagicConfig
{
	// This class holds a single value, usually a scalar
	// Updates to this class destroy the old value
	public class SingleItem<T>: ConfigItem, IEquatable<SingleItem<T>>
		where T: IEquatable<T>
	{
		public class UpdatedArgs: EventArgs { public T OldValue; public T NewValue; }
		public event EventHandler<UpdatedArgs> Updated;

		private T value;

		public SingleItem(T value)
		{
			this.value = value;
		}

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

		public override bool Equals(ConfigItem other)
		{
			return (other is SingleItem<T> otherItem) && Equals(otherItem);
		}

		public override void Assign(ConfigItem other)
		{
			if (other is SingleItem<T> otherItem) {
				var args = new UpdatedArgs { OldValue = value, NewValue = otherItem.value };
				value = otherItem.value;
				Updated?.Invoke(this, args);
			} else {
				throw new InvalidTypeAssignmentException(this, other);
			}
		}
	}
}
