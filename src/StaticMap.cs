using System;
using System.Reflection;
using System.Linq;

namespace MagicConfig
{
	// This class is the key-value map of the given type
	// Only members of the type ConfigItem are considered
	public class StaticMap<T>: Map
	{
		public class UpdatedArgs: EventArgs {}
		public event EventHandler<UpdatedArgs> Updated;

		public StaticMap() {
			_map = GetType()
				.GetMembers(BindingFlags.Public | BindingFlags.Instance)
				.Where(mi => mi.MemberType == MemberTypes.Field)
				.Select(mi => mi as FieldInfo).Where(fi => fi != null)
				.Where(fi => typeof(ConfigItem).IsAssignableFrom(fi.FieldType))
				.ToDictionary(
					fi => fi.Name,
					fi => new _Mapping {
						Get = () => (ConfigItem) fi.GetValue(this),
						Set = (val) => fi.SetValue(this, val)
					}
				);
		}

		public override bool Equals(ConfigItem other)
		{
			return (other is StaticMap<T> otherMap)
				&& _map.Keys.All(
					k => (object.ReferenceEquals(_map[k].Get(), null) && object.ReferenceEquals(otherMap._map[k].Get(), null)) ||
						_map[k].Get().Equals(otherMap._map[k].Get())
				);
		}

		public override void Assign(ConfigItem other)
		{
			if (other is StaticMap<T> otherMap) {
				bool updated = false;

				foreach (var key in _map.Keys) {
					var oldItem = _map[key].Get();
					var newItem = otherMap._map[key].Get();

					if (object.ReferenceEquals(oldItem, null)) {
						if (object.ReferenceEquals(newItem, null)) {
							continue;
						} else {
							_map[key].Set(newItem);
							updated = true;
						}
					} else {
						if (!oldItem.Equals(newItem)) {
							oldItem.Assign(newItem);
							updated = true;
						}
					}
				}

				if (updated) {
					Updated?.Invoke(this, new UpdatedArgs());
				}
			} else {
				throw new InvalidTypeAssignmentException(this, other);
			}
		}
	}
}
