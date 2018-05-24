using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace MagicConfig
{
	// This class is a list of "keyed" items, items with a key getter
	// Thus it is actually a map
	// This list does not preserve order
	public class KeyedItemList<T>: DynamicMap<T>, ICollection<T>, IEquatable<KeyedItemList<T>>
		where T: ConfigItem, IKeyedItem
	{
		public KeyedItemList() {}

		public KeyedItemList(IEnumerable<T> enumerable)
		{
			if (enumerable != null) {
				foreach (var item in enumerable) {
					Add(item);
				}
			}
		}

		public bool Equals(KeyedItemList<T> other)
		{
			return !object.ReferenceEquals(other, null) && ((DynamicMap<T>) this).Equals(other);
		}

		// ICollection<T>
		public void Add(T item) => dictionary.Add(item.GetKeyedItemKey(), item);
		public new void Clear() => dictionary.Clear();
		public bool Contains(T item) => dictionary.ContainsKey(item.GetKeyedItemKey());
		public void CopyTo(T[] array, int arrayIndex) => dictionary.Values.CopyTo(array, arrayIndex);
		public bool Remove(T item) => dictionary.Remove(item.GetKeyedItemKey());
		public new int Count => dictionary.Count;
		public new bool IsReadOnly => false;

		// IEnumerable<T>
		public new IEnumerator<T> GetEnumerator() => dictionary.Values.GetEnumerator();
	}
}
