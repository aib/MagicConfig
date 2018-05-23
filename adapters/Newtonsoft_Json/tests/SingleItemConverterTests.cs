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

		[Fact]
		public void TestSingleItemConverter()
		{
			var converters = new JsonConverter[] { new SingleItemConverter<int>() };
			var obj = JsonConvert.DeserializeObject<TestClass>(TestClass.JSON, converters);
			Assert.Equal(42, (int) obj.si_int);
		}
	}
}
