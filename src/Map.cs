using System;
using System.Collections.Generic;

namespace MagicConfig
{
	public abstract class Map: ConfigItem
	{
		protected abstract IEnumerable<string> _mapKeys();
		protected abstract ConfigItem _mapGet(string key);
		protected abstract void _mapSet(string key, ConfigItem value);
	}
}
