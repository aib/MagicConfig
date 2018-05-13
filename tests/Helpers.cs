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

	public class MyComposite: StaticMap<MyComposite>
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
	}
}
