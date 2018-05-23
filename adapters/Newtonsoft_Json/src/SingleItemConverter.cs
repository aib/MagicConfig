using Newtonsoft.Json;
using System;

namespace MagicConfig.Adapters.Newtonsoft_Json
{
	public class SingleItemConverter<T>: JsonConverter<SingleItem<T>>
		where T: IEquatable<T>
	{
		public override void WriteJson(JsonWriter writer, SingleItem<T> value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value.Value);
		}

		public override SingleItem<T> ReadJson(JsonReader reader, Type objectType, SingleItem<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			return new SingleItem<T>(serializer.Deserialize<T>(reader));
		}
	}
}
