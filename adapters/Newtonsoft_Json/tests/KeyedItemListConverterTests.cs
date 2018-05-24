using MagicConfig;
using Newtonsoft.Json;
using Xunit;

namespace MagicConfig.Adapters.Newtonsoft_Json.Test
{
	public class KeyedItemListConverterTests
	{
		public class TestClass: StaticMap<TestClass> {
			public class Foo: StaticMap<Foo>, IKeyedItem {
				public string name;
				public int value;
				public string GetKeyedItemKey() => name;
			}
			public KeyedItemList<Foo> foos;

			public const string JSON = "{ \"foos\": [ { \"name\": \"a\", \"value\": 1 }, { \"name\": \"b\", \"value\": 2 } ] }";
		}

		private void testConversion(JsonConverter[] converters)
		{
			{
				var obj = JsonConvert.DeserializeObject<TestClass>(TestClass.JSON, converters);
				Assert.Equal(2, obj.foos.Count);
				Assert.Equal("a", obj.foos["a"].name);
				Assert.Equal(1, (int) obj.foos["a"].value);
				Assert.Equal("b", obj.foos["b"].name);
				Assert.Equal(2, (int) obj.foos["b"].value);
			}

			{
				var obj1 = new TestClass {
					foos = new KeyedItemList<TestClass.Foo> {
						new TestClass.Foo { name = "foo", value = 42 },
						new TestClass.Foo { name = "bar", value = 11 }
					}
				};
				string json = JsonConvert.SerializeObject(obj1, converters);
				var obj2 = JsonConvert.DeserializeObject<TestClass>(json, converters);

				Assert.True(obj1.Equals(obj2));
				Assert.True(obj2.Equals(obj1));
			}
		}

		[Fact]
		public void TestKeyedItemListConverter()
		{
			testConversion(new JsonConverter[] { new KeyedItemListConverter<TestClass.Foo>() });
		}

		[Fact]
		public void TestKeyedItemListGenericConverter()
		{
			testConversion(new JsonConverter[] { new KeyedItemListConverter() });
		}
	}
}
