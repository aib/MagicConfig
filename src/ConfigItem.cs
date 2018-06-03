using System;

namespace MagicConfig
{
	// This is the base class for all the MagicConfig configuration items
	public abstract class ConfigItem: IEquatable<ConfigItem>
	{
		// Thrown by Assign when a non-polymorphic item is assigned an incompatible type or null
		public class InvalidTypeAssignmentException: Exception {
			public readonly ConfigItem OldItem, NewItem;
			public InvalidTypeAssignmentException(ConfigItem oldItem, ConfigItem newItem)
				{ OldItem = oldItem; NewItem = newItem; }
		}

		public abstract bool Equals(ConfigItem item);

		// Non-polymorphic configuration item types might throw InvalidTypeAssignment
		public abstract void Assign(ConfigItem item);
	}
}
