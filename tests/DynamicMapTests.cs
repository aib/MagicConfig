using MagicConfig;
using MagicConfig.Tests.Helpers;
using System;
using Xunit;

namespace MagicConfig.Tests
{
	public class DynamicMapTests
	{
		[Fact]
		public void DynamicMapHoldsValues()
		{
			{
				DynamicMap<SingleItem<int>> dm = new DynamicMap<SingleItem<int>> { {"a", 3}, {"b", 4} };
				Assert.Equal((SingleItem<int>) 3, dm["a"]);
				Assert.Equal((SingleItem<int>) 4, dm["b"]);
			}

			{
				DynamicMap<SingleItem<string>> dm = new DynamicMap<SingleItem<string>> { {"a", "foo"}, {"b", "bar"} };
				Assert.Equal((SingleItem<string>) "foo", dm["a"]);
				Assert.Equal((SingleItem<string>) "bar", dm["b"]);
			}

			{
				MyComposite sm = new MyComposite { si = 42, ss1 = "foo", ss2 = null, nested = new MyComposite.MyNested { x = 10, y = 20, s = "bar" }, ci = null };
				DynamicMap<ConfigItem> dm = new DynamicMap<ConfigItem> { {"name", (SingleItem<string>) "foo"}, {"number", (SingleItem<int>) 42}, {"smap", sm} };

				Assert.Equal("foo", (SingleItem<string>) dm["name"]);
				Assert.Equal<int>(42, (SingleItem<int>) dm["number"]);
				Assert.Same(sm, dm["smap"]);
				Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => dm["ThisKeyShouldNotExist"]);
			}
		}

		[Fact]
		public void DynamicMapEqualsWorks()
		{
			{
				MyComposite sm = new MyComposite { si = 42, ss1 = "foo", ss2 = null, nested = new MyComposite.MyNested { x = 10, y = 20, s = "bar" }, ci = null };
				DynamicMap<ConfigItem> dm1 = new DynamicMap<ConfigItem> { {"name",   (SingleItem<string>) "foo"}, {"number", (SingleItem<int>) 42}, {"smap",                      sm}    };
				DynamicMap<ConfigItem> dm2 = new DynamicMap<ConfigItem> { {"number", (SingleItem<int>)    42},    {"smap",                     sm}, {"name", (SingleItem<string>) "foo"} };
				Assert.Equal(dm1, dm2);
				Assert.True(dm1.Equals(dm2));
			}

			{
				DynamicMap<SingleItem<string>> dm1 = new DynamicMap<SingleItem<string>> { {"name", "foo"}, {"number", "42"} };
				DynamicMap<SingleItem<string>> dm2 = new DynamicMap<SingleItem<string>> { {"name", "bar"}, {"number", "42"} };
				Assert.NotEqual(dm1, dm2);
				Assert.False(dm1.Equals(dm2));
			}

			{
				DynamicMap<SingleItem<string>> dm1 = new DynamicMap<SingleItem<string>> { {"name", "foo"}, {"number", "42"} };
				DynamicMap<SingleItem<string>> dm2 = new DynamicMap<SingleItem<string>> { {"foo",  null},  {"bar",    null} };
				Assert.NotEqual(dm1, dm2);
				Assert.False(dm1.Equals(dm2));
			}

			{
				DynamicMap<ConfigItem> dm = new DynamicMap<ConfigItem> { {"name", (SingleItem<string>) "foo"}, {"number", (SingleItem<int>) 42} };
				Assert.False(dm.Equals(null));
				Assert.False(dm.Equals((DynamicMap<ConfigItem>) null));
				Assert.False(dm.Equals((DynamicMap<DynamicMap<ConfigItem>>) null));
				Assert.False(dm.Equals((ConfigItem) null));
			}
		}

		[Fact]
		public void DynamicMapEqualsCallsItemEquals()
		{
			{
				MyFalseEquatableItem e = new MyFalseEquatableItem();

				DynamicMap<MyFalseEquatableItem> dm1 = new DynamicMap<MyFalseEquatableItem> { {"eq", e} };
				DynamicMap<MyFalseEquatableItem> dm2 = new DynamicMap<MyFalseEquatableItem> { {"eq", e} };
				Assert.False(dm1.Equals(dm2));
			}

			{
				DynamicMap<MyTrueEquatableItem> dm1 = new DynamicMap<MyTrueEquatableItem> { {"eq", new MyTrueEquatableItem()} };
				DynamicMap<MyTrueEquatableItem> dm2 = new DynamicMap<MyTrueEquatableItem> { {"eq", new MyTrueEquatableItem()} };
				Assert.True(dm1.Equals(dm2));
			}
		}

		[Fact]
		public void DynamicMapInvalidAssignmentThrows()
		{
			DynamicMap<ConfigItem> dm = new DynamicMap<ConfigItem> { {"name", (SingleItem<string>) "foo"}, {"number", (SingleItem<int>) 42} };
			ConfigItem ci = new MyFalseEquatableItem();
			Assert.Throws<ConfigItem.InvalidTypeAssignmentException>(() => dm.Assign(ci));
		}

		[Fact]
		public void DynamicMapAssignmentChangesValue()
		{
			MyComposite sm1 = new MyComposite { si = 42, ss1 = "foo", ss2 = null, nested = new MyComposite.MyNested { x = 10, y = 20, s = "bar" }, ci = null };
			MyComposite sm2 = new MyComposite { si = 43, ss1 = "foo", ss2 = null, nested = new MyComposite.MyNested { x = 10, y = 20, s = "bar" }, ci = null };
			DynamicMap<ConfigItem> dm  = new DynamicMap<ConfigItem> { {"name", (SingleItem<string>) "foo"}, {"number", (SingleItem<int>) 42}, {"smap", sm1} };
			DynamicMap<ConfigItem> dm2 = new DynamicMap<ConfigItem> { {"name", (SingleItem<string>) "bar"}, {"number", (SingleItem<int>) 40}, {"smap", sm2} };

			dm.Assign(dm2);

			Assert.Equal("bar", (SingleItem<string>) dm["name"]);
			Assert.Equal<int>(40, (SingleItem<int>) dm["number"]);
			Assert.Equal(sm2, dm["smap"]);
		}

		[Fact]
		public void DynamicMapEventsAreCalled()
		{
			SingleItem<int> three = 3;
			SingleItem<int> four = 4;
			SingleItem<int> five = 5;
			DynamicMap<SingleItem<int>> dm  = new DynamicMap<SingleItem<int>> { {"a", three},  {"b", four} };
			DynamicMap<SingleItem<int>> dm2 = new DynamicMap<SingleItem<int>> { {"b", 42},     {"c", five} };

			bool threeDeleted = false;
			bool othersDeleted = false;
			void deleteHandler(object sender, DynamicMap<SingleItem<int>>.DeletedArgs args) {
				Assert.Same(dm, sender);
				Assert.False(threeDeleted);
				Assert.False(othersDeleted);
				if (args.Key == "a" && object.ReferenceEquals(args.OldItem, three)) threeDeleted = true;
				else othersDeleted = true;
			}
			dm.Deleted += deleteHandler;

			bool fiveAdded = false;
			bool othersAdded = false;
			void addHandler(object sender, DynamicMap<SingleItem<int>>.AddedArgs args) {
				Assert.Same(dm, sender);
				Assert.False(fiveAdded);
				Assert.False(othersAdded);
				if (args.Key == "c" && object.ReferenceEquals(args.NewItem, five)) fiveAdded = true;
				else othersAdded = true;
			}
			dm.Added += addHandler;

			bool fourUpdated = false;
			bool othersUpdated = false;
			void updateHandler(object sender, DynamicMap<SingleItem<int>>.UpdatedArgs args) {
				Assert.Same(dm, sender);
				Assert.False(fourUpdated);
				Assert.False(othersUpdated);
				if (args.Key == "b" && object.ReferenceEquals(args.Item, four)) fourUpdated = true;
				else othersUpdated = false;
			}
			dm.Updated += updateHandler;

			bool fourObjectUpdated = false;
			void fourUpdateHandler(object sender, SingleItem<int>.UpdatedArgs args) {
				Assert.False(fourObjectUpdated);
				fourObjectUpdated = true;
			}
			dm["b"].Updated += fourUpdateHandler;

			dm.Assign(dm2);

			Assert.True(threeDeleted);
			Assert.False(othersDeleted);

			Assert.True(fiveAdded);
			Assert.False(othersAdded);

			Assert.True(fourUpdated);
			Assert.False(othersUpdated);
			Assert.True(fourObjectUpdated);

			Assert.Equal(four, dm["b"]);
			Assert.Equal<SingleItem<int>>(42, four);
			Assert.Equal(five, dm["c"]);
		}

		[Fact]
		public void DynamicMapAssigningADifferentTypeFiresAddAndDelete()
		{
			DynamicMap<ConfigItem> dm  = new DynamicMap<ConfigItem> { {"a", new SingleItem<int>(3)},         {"x", new SingleItem<string>("four")}  };
			DynamicMap<ConfigItem> dm2 = new DynamicMap<ConfigItem> { {"x", new SingleItem<string>("four")}, {"a", new SingleItem<string>("three")} };

			bool threeDeleted = false;
			bool othersDeleted = false;
			void deleteHandler(object sender, DynamicMap<ConfigItem>.DeletedArgs args) {
				Assert.Same(dm, sender);
				Assert.False(threeDeleted);
				Assert.False(othersDeleted);
				if (args.Key == "a" && args.OldItem is SingleItem<int> oi && (int) oi == 3) threeDeleted = true;
				else othersDeleted = true;
			}
			dm.Deleted += deleteHandler;

			bool threeAdded = false;
			bool othersAdded = false;
			void addHandler(object sender, DynamicMap<ConfigItem>.AddedArgs args) {
				Assert.Same(dm, sender);
				Assert.False(threeAdded);
				Assert.False(othersAdded);
				if (args.Key == "a" && args.NewItem is SingleItem<string> os && (string) os == "three") threeAdded = true;
				else othersAdded = true;
			}
			dm.Added += addHandler;

			dm.Updated += (sender, args) => Assert.False(true);

			dm.Assign(dm2);

			Assert.True(threeDeleted);
			Assert.False(othersDeleted);

			Assert.True(threeAdded);
			Assert.False(othersAdded);

			Assert.Equal("three", (SingleItem<string>) dm["a"]);
			Assert.Equal("four", (SingleItem<string>) dm["x"]);
		}

		[Fact]
		public void DynamicMapAssignKeepsReferences()
		{
			var three = new MyIntItem(3);
			var four = new MyIntItem(4);
			var newFive = new MyIntItem(5);
			DynamicMap<MyIntItem> dm  = new DynamicMap<MyIntItem> { {"a", three},            {"b", four} };
			DynamicMap<MyIntItem> dm2 = new DynamicMap<MyIntItem> { {"a", new MyIntItem(3)}, {"b", four}, {"c", newFive} };

			dm.Assign(dm2);
			Assert.Same(three, dm["a"]);
			Assert.Same(four, dm["b"]);
			Assert.Same(newFive, dm["c"]);
		}
	}
}
