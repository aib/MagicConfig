using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace MagicConfig
{
	// This class is the key-value map of the given type
	// Only members of the type ConfigItem are considered
	public class StaticMap<T>: Map<ConfigItem>, IEquatable<StaticMap<T>>, IEquatable<T>
	{
		public class ItemUpdatedArgs: EventArgs { public string Key; public ConfigItem Item; }
		public class UpdatedArgs:     EventArgs { public IEnumerable<KeyValuePair<string, ConfigItem>> UpdatedItems; }
		public event EventHandler<ItemUpdatedArgs> ItemUpdated;
		public event EventHandler<UpdatedArgs>     Updated;

		private readonly Dictionary<string, FieldInfo> fields;

		public StaticMap() {
			fields = GetType()
				.GetMembers(BindingFlags.Public | BindingFlags.Instance)
				.Where(mi => mi.MemberType == MemberTypes.Field)
				.Select(mi => mi as FieldInfo).Where(fi => fi != null)
				.Where(fi => typeof(ConfigItem).IsAssignableFrom(fi.FieldType))
				.ToDictionary(fi => fi.Name);
		}

		public bool Equals(T other)
		{
			return Equals(other as StaticMap<T>);
		}

		public bool Equals(StaticMap<T> other)
		{
			return !object.ReferenceEquals(other, null)
				&& _mapKeys().All(
					k => (object.ReferenceEquals(_mapGet(k), null) && object.ReferenceEquals(other._mapGet(k), null)) ||
						!object.ReferenceEquals(_mapGet(k), null) && _mapGet(k).Equals(other._mapGet(k))
				);
		}

		public override bool Equals(ConfigItem other)
		{
			return (other is StaticMap<T> otherMap) && Equals(otherMap);
		}

		public override void Assign(ConfigItem other)
		{
			if (other is StaticMap<T> otherMap) {
				var updated = new List<KeyValuePair<string, ConfigItem>>();

				foreach (var key in _mapKeys()) {
					var oldItem = _mapGet(key);
					var newItem = otherMap._mapGet(key);

					if (object.ReferenceEquals(oldItem, null)) {
						if (object.ReferenceEquals(newItem, null)) {
							continue;
						} else {
							fields[key].SetValue(this, newItem);
							updated.Add(new KeyValuePair<string, ConfigItem>(key, newItem));
						}
					} else {
						if (!oldItem.Equals(newItem)) {
							oldItem.Assign(newItem);
							updated.Add(new KeyValuePair<string, ConfigItem>(key, oldItem));
						}
					}
				}

				updated.ForEach(kv => ItemUpdated?.Invoke(this, new ItemUpdatedArgs { Key = kv.Key, Item = kv.Value }));
				Updated?.Invoke(this, new UpdatedArgs { UpdatedItems = updated });
			} else {
				throw new InvalidTypeAssignmentException(this, other);
			}
		}

		protected override IEnumerable<string> _mapKeys() => fields.Keys;
		protected override ConfigItem _mapGet(string key) => (ConfigItem) fields[key].GetValue(this);
	}
}
