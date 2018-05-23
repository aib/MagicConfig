using Newtonsoft.Json;
using System;

namespace MagicConfig.Adapters.Newtonsoft_Json
{
	public class SingleValueConverter<T>: JsonConverter<SingleValue<T>>
	{
		public override void WriteJson(JsonWriter writer, SingleValue<T> value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value.Value);
		}

		public override SingleValue<T> ReadJson(JsonReader reader, Type objectType, SingleValue<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			return new SingleValue<T>(serializer.Deserialize<T>(reader));
		}
	}
}
