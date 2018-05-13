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

	public class MyItem: ConfigItem
	{
		public override bool Equals(ConfigItem other) { return false; }
		public override void Assign(ConfigItem other) {}
	}
}
