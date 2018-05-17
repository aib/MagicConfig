using System;
using System.Collections.Generic;

namespace MagicConfig
{
	public abstract class Map<T>: ConfigItem
	{
		protected abstract IEnumerable<string> _mapKeys();
		protected abstract T _mapGet(string key);
	}
}
