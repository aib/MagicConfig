using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace MagicConfig
{
	// This class is a free key-value map
	// Values have the ConfigItem type
	public class DynamicMap<T>: Map<T>, IDictionary<string, T>, IEquatable<DynamicMap<T>>
		where T: ConfigItem
	{
		public class ItemDeletedArgs: EventArgs { public string Key; public T OldItem; }
		public class ItemAddedArgs:   EventArgs { public string Key; public T NewItem; }
		public class ItemUpdatedArgs: EventArgs { public string Key; public T Item;    }
		public class UpdatedArgs:     EventArgs {}
		public event EventHandler<ItemDeletedArgs> ItemDeleted;
		public event EventHandler<ItemAddedArgs>   ItemAdded;
		public event EventHandler<ItemUpdatedArgs> ItemUpdated;
		public event EventHandler<UpdatedArgs>     Updated;

		protected readonly Dictionary<string, T> dictionary = new Dictionary<string, T>();

		public bool Equals(DynamicMap<T> other)
		{
			return !object.ReferenceEquals(other, null)
				&& _mapKeys().All(
					k => other._mapKeys().Contains(k) &&
						((object.ReferenceEquals(_mapGet(k), null) && object.ReferenceEquals(other._mapGet(k), null)) ||
							_mapGet(k).Equals(other._mapGet(k)))
				);
		}

		public override bool Equals(ConfigItem other)
		{
			return (other is DynamicMap<T> otherMap) && Equals(otherMap);
		}

		public override void Assign(ConfigItem other)
		{
			if (other is DynamicMap<T> otherMap) {
				var deleted = new List<(string, T)>();
				var added   = new List<(string, T)>();
				var updated = new List<(string, T)>();

				foreach (var key in _mapKeys().ToList()) {
					if (!otherMap._mapKeys().Contains(key)) {
						deleted.Add((key, _mapGet(key)));
						dictionary.Remove(key);
					}
				}

				foreach (var key in otherMap._mapKeys()) {
					var newItem = otherMap._mapGet(key);

					if (_mapKeys().Contains(key)) {
						var oldItem = _mapGet(key);
						if (object.ReferenceEquals(oldItem, null)) {
							if (object.ReferenceEquals(newItem, null)) {
								continue;
							} else {
								dictionary[key] = newItem;
								added.Add((key, newItem));
							}
						} else {
							if (!oldItem.Equals(newItem)) {
								try {
									oldItem.Assign(newItem);
									updated.Add((key, oldItem));
								} catch (InvalidTypeAssignmentException) {
									deleted.Add((key, oldItem));
									dictionary[key] = newItem;
									added.Add((key, newItem));
								}
							}
						}
					} else {
						dictionary[key] = newItem;
						added.Add((key, newItem));
					}
				}

				deleted.ForEach(kv => ItemDeleted?.Invoke(this, new ItemDeletedArgs { Key = kv.Item1, OldItem = kv.Item2 }));
				added  .ForEach(kv => ItemAdded  ?.Invoke(this, new ItemAddedArgs   { Key = kv.Item1, NewItem = kv.Item2 }));
				updated.ForEach(kv => ItemUpdated?.Invoke(this, new ItemUpdatedArgs { Key = kv.Item1,    Item = kv.Item2 }));
				Updated?.Invoke(this, new UpdatedArgs());
			} else {
				throw new InvalidTypeAssignmentException(this, other);
			}
		}

		protected override IEnumerable<string> _mapKeys() => dictionary.Keys;
		protected override T _mapGet(string key) => dictionary[key];

		public void Add(string key, T value) => dictionary.Add(key, value);
		public bool ContainsKey(string key) => dictionary.ContainsKey(key);
		public bool Remove(string key) => dictionary.Remove(key);
		public bool TryGetValue(string key, out T value) => dictionary.TryGetValue(key, out value);
		public T this[string key] { get => dictionary[key]; set => dictionary[key] = value; }
		public ICollection<string> Keys => dictionary.Keys;
		public ICollection<T> Values => dictionary.Values;
		public void Add(KeyValuePair<string, T> item) => ((ICollection<KeyValuePair<string, T>>) dictionary).Add(item);
		public void Clear() => dictionary.Clear();
		public bool Contains(KeyValuePair<string, T> item) => ((ICollection<KeyValuePair<string, T>>) dictionary).Contains(item);
		public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, T>>) dictionary).CopyTo(array, arrayIndex);
		public bool Remove(KeyValuePair<string, T> item) => ((ICollection<KeyValuePair<string, T>>) dictionary).Remove(item);
		public int Count => dictionary.Count;
		public bool IsReadOnly => false;
		public IEnumerator<KeyValuePair<string, T>> GetEnumerator() => dictionary.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
