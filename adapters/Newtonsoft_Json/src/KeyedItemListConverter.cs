using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MagicConfig.Adapters.Newtonsoft_Json
{
	// Converter for a single KeyedItemList<T> type
	public class KeyedItemListConverter<T>: JsonConverter<KeyedItemList<T>>
		where T: ConfigItem, IKeyedItem
	{
		public override void WriteJson(JsonWriter writer, KeyedItemList<T> value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, new List<T>(value));
		}

		public override KeyedItemList<T> ReadJson(JsonReader reader, Type objectType, KeyedItemList<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			return new KeyedItemList<T>(serializer.Deserialize<ICollection<T>>(reader));
		}
	}

	// Converter for all KeyedItemList<T> types
	public class KeyedItemListConverter: JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			Type itemType = findItemType(value.GetType());
			Type listType = typeof(List<>).MakeGenericType(itemType);
			Type enumerableType = typeof(IEnumerable<>).MakeGenericType(itemType);

			object list = listType
				.GetConstructor(new[] {enumerableType})
				.Invoke(new[] {value});

			serializer.Serialize(writer, list, listType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			Type itemType = findItemType(objectType);
			Type listType = typeof(List<>).MakeGenericType(itemType);
			object itemList = ReflectionUtils.CallMethod(serializer, "Deserialize", new[] {listType}, new[] {reader});

			return typeof(KeyedItemList<>)
				.MakeGenericType(itemType)
				.GetConstructor(new[] {listType})
				.Invoke(new[] {itemList});
		}

		public override bool CanConvert(Type objectType)
		{
			return (findItemType(objectType) != null);
		}

		private Type findItemType(Type objectType)
		{
			Type itemType = ReflectionUtils.FindInTypeHierarchy(
				t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(KeyedItemList<>),
				objectType);

			return itemType?.GenericTypeArguments[0];
		}
	}
}
