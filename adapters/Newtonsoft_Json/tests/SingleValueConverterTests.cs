using MagicConfig;
using Newtonsoft.Json;
using Xunit;

namespace MagicConfig.Adapters.Newtonsoft_Json.Test
{
	public class SingleValueConverterTests
	{
		public enum FooEnum {
			FOO,
			MORE_FOO,
			BAR
		}

		public class TestClass: StaticMap<TestClass> {
			public SingleValue<int>     sv_int;
			public SingleValue<FooEnum> sv_foo;
			public const string JSON = "{ \"sv_int\": 42, \"sv_foo\": \"BAR\" }";
		}

		[Fact]
		public void TestSingleValueConverter()
		{
			var converters = new JsonConverter[] { new SingleValueConverter<int>(), new SingleValueConverter<FooEnum>() };
			var obj = JsonConvert.DeserializeObject<TestClass>(TestClass.JSON, converters);

			Assert.Equal(42, (int) obj.sv_int);

			Assert.Equal<FooEnum>(FooEnum.BAR, obj.sv_foo);
			Assert.Equal(2, (int) obj.sv_foo.Value);

			obj.sv_foo = FooEnum.MORE_FOO;
			Assert.Equal<FooEnum>(FooEnum.MORE_FOO, obj.sv_foo);
			Assert.Equal(1, (int) obj.sv_foo.Value);
		}
	}
}
