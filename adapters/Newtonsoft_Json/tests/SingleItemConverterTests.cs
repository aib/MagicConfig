using MagicConfig;
using Newtonsoft.Json;
using Xunit;

namespace MagicConfig.Adapters.Newtonsoft_Json.Test
{
	public class SingleItemConverterTests
	{
		public class TestClass: StaticMap<TestClass> {
			public SingleItem<int> si_int;
			public const string JSON = "{ \"si_int\": 42 }";
		}

		private void testConversion(JsonConverter[] converters)
		{
			{
				var obj = JsonConvert.DeserializeObject<TestClass>(TestClass.JSON, converters);
				Assert.Equal(42, (int) obj.si_int);
			}

			{
				var obj1 = new TestClass { si_int = 42 };
				string json = JsonConvert.SerializeObject(obj1, converters);
				var obj2 = JsonConvert.DeserializeObject<TestClass>(json, converters);

				Assert.True(obj1.Equals(obj2));
				Assert.True(obj2.Equals(obj1));
			}
		}

		[Fact]
		public void TestSingleItemConverter()
		{
			testConversion(new JsonConverter[] { new SingleItemConverter<int>() });
		}

		[Fact]
		public void TestSingleItemGenericConverter()
		{
			testConversion(new JsonConverter[] { new SingleItemConverter() });
		}
	}
}
