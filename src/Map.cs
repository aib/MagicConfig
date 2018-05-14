using System;
using System.Collections.Generic;

namespace MagicConfig
{
	public abstract class Map<T>: ConfigItem
	{
		protected abstract IEnumerable<string> _mapKeys();
		protected abstract T _mapGet(string key);
		protected abstract void _mapSet(string key, T value);
		protected abstract void _mapDel(string key);
	}
}
