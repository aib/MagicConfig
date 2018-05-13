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
}
