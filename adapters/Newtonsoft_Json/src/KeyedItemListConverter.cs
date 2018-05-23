using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MagicConfig.Adapters.Newtonsoft_Json
{
	public class KeyedItemListConverter<T>: JsonConverter<KeyedItemList<T>>
		where T: ConfigItem, IKeyedItem
	{
		public override void WriteJson(JsonWriter writer, KeyedItemList<T> value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, (ICollection<T>) value);
		}

		public override KeyedItemList<T> ReadJson(JsonReader reader, Type objectType, KeyedItemList<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			return new KeyedItemList<T>(serializer.Deserialize<ICollection<T>>(reader));
		}
	}
}
