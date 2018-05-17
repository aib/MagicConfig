using MagicConfig;
using System;

namespace MagicConfig.Tests.Helpers
{
	public class MyFalseEquatable: IEquatable<MyFalseEquatable>
	{
		public bool Equals(MyFalseEquatable other) { return false; }
	}

	public class MyTrueEquatable: IEquatable<MyTrueEquatable>
	{
		public bool Equals(MyTrueEquatable other) { return true; }
	}

	public class MyFalseEquatableItem: ConfigItem
	{
		public override bool Equals(ConfigItem other) { return false; }
		public override void Assign(ConfigItem other) {}
	}

	public class MyTrueEquatableItem: ConfigItem
	{
		public override bool Equals(ConfigItem other) { return true; }
		public override void Assign(ConfigItem other) {}
	}

	public class MyComposite: StaticMap<MyComposite>, IKeyedItem
	{
		public SingleItem<int> si;
		public SingleItem<string> ss1;
		public SingleItem<string> ss2;

		public class MyNested: StaticMap<MyNested> {
			public SingleItem<int> x;
			public SingleItem<int> y;
			public SingleItem<string> s;
		}
		public MyNested nested;

		public ConfigItem ci;

		public int ignored_int;
		public string ignored_string;

		public string GetKeyedItemKey() => ss1;
	}

	public class MyInt: IEquatable<MyInt>
	{
		private readonly int val;
		public MyInt(int val) { this.val = val; }
		public bool Equals(MyInt other) {
			return !object.ReferenceEquals(other, null) && val == other.val;
		}
		public override string ToString() => $"MyInt({val})";
	}

	public class MyIntItem: ConfigItem
	{
		private readonly int val;
		public MyIntItem(int val) { this.val = val; }
		public override bool Equals(ConfigItem other) {
			return (other is MyIntItem otherItem) && val == otherItem.val;
		}
		public override void Assign(ConfigItem other) => throw new InvalidOperationException();
		public override string ToString() => $"MyInt({val})";
	}

	public class MyKeyedInt: SingleItem<int>, IKeyedItem, IEquatable<MyKeyedInt>
	{
		private readonly string key;
		public MyKeyedInt(string key, int val) :base(val) { this.key = key; }
		public MyKeyedInt(int val) :this("NOKEY", val) {}
		public string GetKeyedItemKey() => key;
		public bool Equals(MyKeyedInt other) {
			return !object.ReferenceEquals(other, null) && (int) this == (int) other;
		}
		public override string ToString() => $"MyKeyedInt({key}: {(int)this})";
	}

	public class MyKeyedFalseEquatableInt: MyKeyedInt
	{
		public MyKeyedFalseEquatableInt(string key, int val) :base(key, val) {}
		public override bool Equals(ConfigItem other) => false;
	}

	public class MyKeyedTrueEquatableInt:  MyKeyedInt
	{
		public MyKeyedTrueEquatableInt(string key, int val) :base(key, val) {}
		public override bool Equals(ConfigItem other) => true;
	}
}
