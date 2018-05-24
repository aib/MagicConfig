using Newtonsoft.Json;
using System;

namespace MagicConfig.Adapters.Newtonsoft_Json
{
	// Converter for a single SingleValue<T> type
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

	// Converter for all SingleValue<T> types
	public class SingleValueConverter: JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			Type itemType = findItemType(value.GetType());
			object itemValue = value.GetType().GetProperty("Value").GetValue(value);

			serializer.Serialize(writer, itemValue, itemType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			Type itemType = findItemType(objectType);
			object itemValue = ReflectionUtils.CallMethod(serializer, "Deserialize", new[] {itemType}, new[] {reader});

			return typeof(SingleValue<>)
				.MakeGenericType(itemType)
				.GetConstructor(new[] {itemType})
				.Invoke(new[] {itemValue});
		}

		public override bool CanConvert(Type objectType)
		{
			return (findItemType(objectType) != null);
		}

		private Type findItemType(Type objectType)
		{
			Type itemType = ReflectionUtils.FindInTypeHierarchy(
				t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(SingleValue<>),
				objectType);

			return itemType?.GenericTypeArguments[0];
		}
	}
}
