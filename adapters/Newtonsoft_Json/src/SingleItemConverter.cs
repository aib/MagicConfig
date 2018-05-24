using Newtonsoft.Json;
using System;

namespace MagicConfig.Adapters.Newtonsoft_Json
{
	// Converter for a single SingleItem<T> type
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

	// Converter for all SingleItem<T> types
	public class SingleItemConverter: JsonConverter
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

			return typeof(SingleItem<>)
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
				t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(SingleItem<>),
				objectType);

			return itemType?.GenericTypeArguments[0];
		}
	}
}
