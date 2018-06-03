using MagicConfig;
using MagicConfig.Tests.Helpers;
using System;
using System.Collections.Generic;
using Xunit;

namespace MagicConfig.Tests
{
	public class DynamicMapTests
	{
		[Fact]
		public void DynamicMapHoldsValues()
		{
			{
				DynamicMap<SingleValue<int>> dm = new DynamicMap<SingleValue<int>> { {"a", 3}, {"b", 4} };
				Assert.Equal((SingleValue<int>) 3, dm["a"]);
				Assert.Equal((SingleValue<int>) 4, dm["b"]);
			}

			{
				DynamicMap<SingleItem<string>> dm = new DynamicMap<SingleItem<string>> { {"a", "foo"}, {"b", "bar"} };
				Assert.Equal((SingleItem<string>) "foo", dm["a"]);
				Assert.Equal((SingleItem<string>) "bar", dm["b"]);
			}

			{
				MyComposite sm = new MyComposite { si = 42, ss1 = "foo", ss2 = null, nested = new MyComposite.MyNested { x = 10, y = 20, s = "bar" }, ci = null };
				DynamicMap<ConfigItem> dm = new DynamicMap<ConfigItem> { {"name", (SingleItem<string>) "foo"}, {"number", (SingleValue<int>) 42}, {"smap", sm} };

				Assert.Equal("foo", (SingleItem<string>) dm["name"]);
				Assert.Equal<int>(42, (SingleValue<int>) dm["number"]);
				Assert.Same(sm, dm["smap"]);
				Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => dm["ThisKeyShouldNotExist"]);
			}
		}

		[Fact]
		public void DynamicMapEqualsWorks()
		{
			{
				DynamicMap<ConfigItem> dm1 = new DynamicMap<ConfigItem> {};
				DynamicMap<ConfigItem> dm2 = new DynamicMap<ConfigItem> { {"number", (SingleValue<int>) 42} };
				Assert.NotEqual(dm1, dm2);
				Assert.NotEqual(dm2, dm1);
				Assert.False(dm1.Equals(dm2));
				Assert.False(dm2.Equals(dm1));
			}

			{
				DynamicMap<ConfigItem> dm1 = new DynamicMap<ConfigItem> { {"number", null} };
				DynamicMap<ConfigItem> dm2 = new DynamicMap<ConfigItem> { {"number", (SingleValue<int>) 42} };
				Assert.NotEqual(dm1, dm2);
				Assert.NotEqual(dm2, dm1);
				Assert.False(dm1.Equals(dm2));
				Assert.False(dm2.Equals(dm1));
			}

			{
				DynamicMap<ConfigItem> dm1 = new DynamicMap<ConfigItem> { {"name", null} };
				DynamicMap<ConfigItem> dm2 = new DynamicMap<ConfigItem> { {"name", null} };
				Assert.Equal(dm1, dm2);
				Assert.Equal(dm2, dm1);
				Assert.True(dm1.Equals(dm2));
				Assert.True(dm2.Equals(dm1));
			}

			{
				MyComposite sm = new MyComposite { si = 42, ss1 = "foo", ss2 = null, nested = new MyComposite.MyNested { x = 10, y = 20, s = "bar" }, ci = null };
				DynamicMap<ConfigItem> dm1 = new DynamicMap<ConfigItem> { {"name",   (SingleItem<string>) "foo"}, {"number", (SingleValue<int>) 42}, {"smap",                      sm}    };
				DynamicMap<ConfigItem> dm2 = new DynamicMap<ConfigItem> { {"number", (SingleValue<int>)   42},    {"smap",                      sm}, {"name", (SingleItem<string>) "foo"} };
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
				DynamicMap<ConfigItem> dm = new DynamicMap<ConfigItem> { {"name", (SingleItem<string>) "foo"}, {"number", (SingleValue<int>) 42} };
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
			DynamicMap<ConfigItem> dm = new DynamicMap<ConfigItem> { {"name", (SingleItem<string>) "foo"}, {"number", (SingleValue<int>) 42} };
			ConfigItem ci = new MyFalseEquatableItem();
			Assert.Throws<ConfigItem.InvalidTypeAssignmentException>(() => dm.Assign(ci));
		}

		[Fact]
		public void DynamicMapAssignmentChangesValue()
		{
			MyComposite sm1 = new MyComposite { si = 42, ss1 = "foo", ss2 = null, nested = new MyComposite.MyNested { x = 10, y = 20, s = "bar" }, ci = null };
			MyComposite sm2 = new MyComposite { si = 43, ss1 = "foo", ss2 = null, nested = new MyComposite.MyNested { x = 10, y = 20, s = "bar" }, ci = null };
			DynamicMap<ConfigItem> dm  = new DynamicMap<ConfigItem> { {"name", (SingleItem<string>) "foo"}, {"number", (SingleValue<int>) 42}, {"smap", sm1} };
			DynamicMap<ConfigItem> dm2 = new DynamicMap<ConfigItem> { {"name", (SingleItem<string>) "bar"}, {"number", (SingleValue<int>) 40}, {"smap", sm2} };

			dm.Assign(dm2);

			Assert.Equal("bar", (SingleItem<string>) dm["name"]);
			Assert.Equal<int>(40, (SingleValue<int>) dm["number"]);
			Assert.Equal<int>(43, ((MyComposite) dm["smap"]).si);
			Assert.Same(sm1, dm["smap"]);
		}

		[Fact]
		public void DynamicMapEventsAreCalled()
		{
			SingleValue<int> three = 3;
			SingleValue<int> four = 4;
			SingleValue<int> five = 5;
			SingleValue<int> n2 = 6;
			DynamicMap<SingleValue<int>> dm  = new DynamicMap<SingleValue<int>> { {"a", three}, {"b", four}, {"n1", null}, {"n2", null} };
			DynamicMap<SingleValue<int>> dm2 = new DynamicMap<SingleValue<int>> { {"b", 42},    {"c", five}, {"n1", null}, {"n2", n2}   };

			bool threeDeleted = false;
			bool othersDeleted = false;
			void deleteHandler(object sender, DynamicMap<SingleValue<int>>.ItemDeletedArgs args) {
				Assert.Same(dm, sender);
				if (args.Key == "a" && object.ReferenceEquals(args.OldItem, three)) { Assert.False(threeDeleted); threeDeleted = true; }
				else othersDeleted = true;
			}
			dm.ItemDeleted += deleteHandler;

			bool fiveAdded = false;
			bool n2Added = false;
			bool othersAdded = false;
			void addHandler(object sender, DynamicMap<SingleValue<int>>.ItemAddedArgs args) {
				Assert.Same(dm, sender);
				if      (args.Key == "c"  && object.ReferenceEquals(args.NewItem, five)) { Assert.False(fiveAdded); fiveAdded = true; }
				else if (args.Key == "n2" && object.ReferenceEquals(args.NewItem, n2))   { Assert.False(n2Added);   n2Added   = true; }
				else othersAdded = true;
			}
			dm.ItemAdded += addHandler;

			bool fourUpdated = false;
			bool othersUpdated = false;
			void updateHandler(object sender, DynamicMap<SingleValue<int>>.ItemUpdatedArgs args) {
				Assert.Same(dm, sender);
				if (args.Key == "b" && object.ReferenceEquals(args.Item, four)) { Assert.False(fourUpdated); fourUpdated = true; }
				else othersUpdated = true;
			}
			dm.ItemUpdated += updateHandler;

			bool fourObjectUpdated = false;
			void fourUpdateHandler(object sender, SingleValue<int>.UpdatedArgs args) {
				Assert.False(fourObjectUpdated);
				fourObjectUpdated = true;
			}
			dm["b"].Updated += fourUpdateHandler;

			bool mapUpdateCalled = false;
			dm.Updated += (sender, args) => {
				Assert.False(mapUpdateCalled);
				mapUpdateCalled = true;

				var deleted = new List<KeyValuePair<string, SingleValue<int>>>(args.DeletedItems);
				Assert.Single(deleted);
				Assert.Equal("a", deleted[0].Key);
				Assert.Same(three, deleted[0].Value);

				var added = new List<KeyValuePair<string, SingleValue<int>>>(args.AddedItems);
				Assert.Equal(2, added.Count);
				Assert.Equal("c", added[0].Key);
				Assert.Same(five, added[0].Value);
				Assert.Equal("n2", added[1].Key);
				Assert.Same(n2, added[1].Value);

				var updated = new List<KeyValuePair<string, SingleValue<int>>>(args.UpdatedItems);
				Assert.Single(updated);
				Assert.Equal("b", updated[0].Key);
				Assert.Same(four, updated[0].Value);
			};

			dm.Assign(dm2);

			Assert.True(threeDeleted);
			Assert.False(othersDeleted);

			Assert.True(fiveAdded);
			Assert.True(n2Added);
			Assert.False(othersAdded);

			Assert.True(fourUpdated);
			Assert.False(othersUpdated);

			Assert.True(fourObjectUpdated);

			Assert.True(mapUpdateCalled);

			Assert.Equal(four, dm["b"]);
			Assert.Equal<SingleValue<int>>(42, four);
			Assert.Equal(five, dm["c"]);
		}

		[Fact]
		public void DynamicMapAssigningADifferentTypeFiresAddAndDelete()
		{
			DynamicMap<ConfigItem> dm  = new DynamicMap<ConfigItem> { {"a", new SingleValue<int>(3)},        {"x", new SingleItem<string>("four")}  };
			DynamicMap<ConfigItem> dm2 = new DynamicMap<ConfigItem> { {"x", new SingleItem<string>("four")}, {"a", new SingleItem<string>("three")} };

			bool threeDeleted = false;
			bool othersDeleted = false;
			void deleteHandler(object sender, DynamicMap<ConfigItem>.ItemDeletedArgs args) {
				Assert.Same(dm, sender);
				Assert.False(threeDeleted);
				Assert.False(othersDeleted);
				if (args.Key == "a" && args.OldItem is SingleValue<int> oi && (int) oi == 3) threeDeleted = true;
				else othersDeleted = true;
			}
			dm.ItemDeleted += deleteHandler;

			bool threeAdded = false;
			bool othersAdded = false;
			void addHandler(object sender, DynamicMap<ConfigItem>.ItemAddedArgs args) {
				Assert.Same(dm, sender);
				Assert.False(threeAdded);
				Assert.False(othersAdded);
				if (args.Key == "a" && args.NewItem is SingleItem<string> os && (string) os == "three") threeAdded = true;
				else othersAdded = true;
			}
			dm.ItemAdded += addHandler;

			dm.ItemUpdated += (sender, args) => Assert.False(true);

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
