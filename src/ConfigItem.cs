using System;
using System.Collections.Generic;

namespace MagicConfig
{
	// This is the base class for all the MagicConfig configuration items
	public abstract class ConfigItem: IEquatable<ConfigItem>
	{
		// Base class for exceptions thrown by Assign on invalid assignments
		public abstract class InvalidAssignmentException: Exception {}

		// Thrown when a non-polymorphic item is assigned an incompatible type or null
		public class InvalidTypeAssignmentException: InvalidAssignmentException {
			public readonly ConfigItem OldItem, NewItem;
			public InvalidTypeAssignmentException(ConfigItem oldItem, ConfigItem newItem)
				{ OldItem = oldItem; NewItem = newItem; }
		}

		// Thrown by multi-item classes when one or more children throw InvalidAssignmentException
		public class InvalidChildAssignmentException: InvalidAssignmentException {
			public readonly IEnumerable<InvalidAssignmentException> ChildExceptions;
			public InvalidChildAssignmentException(IEnumerable<InvalidAssignmentException> childExceptions)
				{ ChildExceptions = childExceptions; }
		}

		public abstract bool Equals(ConfigItem item);

		// Throws InvalidAssignmentException
		public abstract void Assign(ConfigItem item);
	}
}
