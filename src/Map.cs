using System;
using System.Collections.Generic;

namespace MagicConfig
{
	public abstract class Map: ConfigItem
	{
		protected class _Mapping {
			public Func<ConfigItem> Get;
			public Action<ConfigItem> Set;
		}
		protected IReadOnlyDictionary<string, _Mapping> _map;
	}
}
