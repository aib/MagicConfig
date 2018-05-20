using MagicConfig;
using MagicConfig.Tests.Helpers;
using System;
using Xunit;

namespace MagicConfig.Tests
{
	public class KeyedItemListTests
	{
		[Fact]
		public void KeyedItemListHoldsValues()
		{
			{
				KeyedItemList<MyKeyedInt> kil = new KeyedItemList<MyKeyedInt> { new MyKeyedInt("a", 1), new MyKeyedInt("b", 2) };
				Assert.Contains(new MyKeyedInt(1), kil);
				Assert.Contains(new MyKeyedInt(2), kil);
				Assert.Equal(1, kil["a"]);
				Assert.Equal(2, kil["b"]);
			}

			{
				MyComposite sm1 = new MyComposite { si = 42, ss1 = "foo", ss2 = null, nested = new MyComposite.MyNested { x = 10, y = 20, s = "bar" }, ci = null };
				MyComposite sm2 = new MyComposite { si = 42, ss1 = "bar", ss2 = null, nested = new MyComposite.MyNested { x = 10, y = 20, s = "bar" }, ci = null };
				KeyedItemList<MyComposite> kil = new KeyedItemList<MyComposite> { sm1, sm2 };

				Assert.Equal(sm1, kil["foo"]);
				Assert.Same(sm1, kil["foo"]);
				Assert.Equal(sm2, kil["bar"]);
				Assert.Same(sm2, kil["bar"]);
				Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => kil["ThisKeyShouldNotExist"]);
			}
		}

		[Fact]
		public void KeyedItemListEqualsWorks()
		{
			{
				MyComposite sm1 = new MyComposite { si = 42, ss1 = "foo", ss2 = "key is foo", nested = null, ci = null };
				MyComposite sm2 = new MyComposite { si = 43, ss1 = "bar", ss2 = "key is bar", nested = new MyComposite.MyNested { x = 10, y = 20, s = "bar" }, ci = null };

				KeyedItemList<MyComposite> kil1 = new KeyedItemList<MyComposite> { sm1, sm2 };
				KeyedItemList<MyComposite> kil2 = new KeyedItemList<MyComposite> { sm2, sm1 };

				Assert.Equal(kil1, kil2);
				Assert.True(kil1.Equals(kil2));
			}

			{
				KeyedItemList<MyKeyedInt> kil1 = new KeyedItemList<MyKeyedInt> { new MyKeyedInt("a", 1), new MyKeyedInt("b", 2) };
				KeyedItemList<MyKeyedInt> kil2 = new KeyedItemList<MyKeyedInt> { new MyKeyedInt("c", 3), new MyKeyedInt("d", 4) };

				Assert.NotEqual(kil1, kil2);
				Assert.False(kil1.Equals(kil2));
			}


			{
				KeyedItemList<MyKeyedInt> kil1 = new KeyedItemList<MyKeyedInt> { new MyKeyedInt("a", 1), new MyKeyedInt("b", 2) };
				KeyedItemList<MyKeyedInt> kil2 = new KeyedItemList<MyKeyedInt> { new MyKeyedInt("a", 2), new MyKeyedInt("b", 1) };

				Assert.NotEqual(kil1, kil2);
				Assert.False(kil1.Equals(kil2));
			}

			{
				KeyedItemList<MyKeyedInt> kil = new KeyedItemList<MyKeyedInt>();
				Assert.False(kil.Equals(null));
				Assert.False(kil.Equals((KeyedItemList<MyKeyedInt>) null));
				Assert.False(kil.Equals((ConfigItem) null));
			}
		}

		[Fact]
		public void KeyedItemListEqualsCallsItemEquals()
		{
			{
				MyKeyedFalseEquatableInt e = new MyKeyedFalseEquatableInt("eq", 42);

				KeyedItemList<MyKeyedInt> kil1 = new KeyedItemList<MyKeyedInt> { e };
				KeyedItemList<MyKeyedInt> kil2 = new KeyedItemList<MyKeyedInt> { e };
				Assert.False(kil1.Equals(kil2));
			}

			{
				KeyedItemList<MyKeyedInt> kil1 = new KeyedItemList<MyKeyedInt> { new MyKeyedTrueEquatableInt("e", 42) };
				KeyedItemList<MyKeyedInt> kil2 = new KeyedItemList<MyKeyedInt> { new MyKeyedTrueEquatableInt("f", 42) };
				Assert.False(kil1.Equals(kil2));
			}

			{
				KeyedItemList<MyKeyedInt> kil1 = new KeyedItemList<MyKeyedInt> { new MyKeyedTrueEquatableInt("e", 42) };
				KeyedItemList<MyKeyedInt> kil2 = new KeyedItemList<MyKeyedInt> { new MyKeyedTrueEquatableInt("e", 43) };
				Assert.True(kil1.Equals(kil2));
			}
		}

		[Fact]
		public void KeyedItemListInvalidAssignmentThrows()
		{
			KeyedItemList<MyKeyedInt> kil = new KeyedItemList<MyKeyedInt>();
			ConfigItem ci = new MyFalseEquatableItem();
			Assert.Throws<ConfigItem.InvalidTypeAssignmentException>(() => kil.Assign(ci));
		}

		[Fact]
		public void KeyedItemListAssignmentChangesValue()
		{
			KeyedItemList<MyKeyedInt> kil  = new KeyedItemList<MyKeyedInt> { new MyKeyedInt("a", 1), new MyKeyedInt("b", 2) };
			KeyedItemList<MyKeyedInt> kil2 = new KeyedItemList<MyKeyedInt> { new MyKeyedInt("c", 3), new MyKeyedInt("a", 10) };

			kil.Assign(kil2);

			Assert.Equal(10, kil["a"]);
			Assert.Equal(3, kil["c"]);
			Assert.Contains(new MyKeyedInt(3), kil);
			Assert.Contains(new MyKeyedInt(10), kil);
		}

		[Fact]
		public void KeyedItemListEventsAreCalled()
		{
			MyKeyedInt three = new MyKeyedInt("a", 3);
			MyKeyedInt four = new MyKeyedInt("b", 4);
			MyKeyedInt five = new MyKeyedInt("c", 5);
			MyKeyedInt ftwo = new MyKeyedInt("b", 42);
			KeyedItemList<MyKeyedInt> kil  = new KeyedItemList<MyKeyedInt> { three, four };
			KeyedItemList<MyKeyedInt> kil2 = new KeyedItemList<MyKeyedInt> { ftwo,  five };

			bool threeDeleted = false;
			bool othersDeleted = false;
			void deleteHandler(object sender, KeyedItemList<MyKeyedInt>.DeletedArgs args) {
				Assert.Same(kil, sender);
				Assert.False(threeDeleted);
				Assert.False(othersDeleted);
				if (args.Key == "a" && object.ReferenceEquals(args.OldItem, three)) threeDeleted = true;
				else othersDeleted = true;
			}
			kil.Deleted += deleteHandler;

			bool fiveAdded = false;
			bool othersAdded = false;
			void addHandler(object sender, KeyedItemList<MyKeyedInt>.AddedArgs args) {
				Assert.Same(kil, sender);
				Assert.False(fiveAdded);
				Assert.False(othersAdded);
				if (args.Key == "c" && object.ReferenceEquals(args.NewItem, five)) fiveAdded = true;
				else othersAdded = true;
			}
			kil.Added += addHandler;

			bool fourUpdated = false;
			bool othersUpdated = false;
			void updateHandler(object sender, KeyedItemList<MyKeyedInt>.UpdatedArgs args) {
				Assert.Same(kil, sender);
				Assert.False(fourUpdated);
				Assert.False(othersUpdated);
				if (args.Key == "b" && object.ReferenceEquals(args.Item, four)) fourUpdated = true;
				else othersUpdated = true;
			}
			kil.Updated += updateHandler;

			bool fourObjectUpdated = false;
			void fourUpdateHandler(object sender, MyKeyedInt.UpdatedArgs args) {
				Assert.False(fourObjectUpdated);
				fourObjectUpdated = true;
			}
			kil["b"].Updated += fourUpdateHandler;

			kil.Assign(kil2);

			Assert.True(threeDeleted);
			Assert.False(othersDeleted);

			Assert.True(fiveAdded);
			Assert.False(othersAdded);

			Assert.True(fourUpdated);
			Assert.False(othersUpdated);
			Assert.True(fourObjectUpdated);

			Assert.Equal(four, kil["b"]);
			Assert.Equal(new MyKeyedInt(42), four);
			Assert.Equal(five, kil["c"]);
		}

		[Fact]
		public void KeyedItemListAssignKeepsReferences()
		{
			var three = new MyKeyedInt("a", 3);
			var four = new MyKeyedInt("b", 4);
			var newFive = new MyKeyedInt("c", 5);
			KeyedItemList<MyKeyedInt> kil  = new KeyedItemList<MyKeyedInt> { three,                  four };
			KeyedItemList<MyKeyedInt> kil2 = new KeyedItemList<MyKeyedInt> { new MyKeyedInt("a", 3), four, newFive };

			kil.Assign(kil2);
			Assert.Same(three, kil["a"]);
			Assert.Same(four, kil["b"]);
			Assert.Same(newFive, kil["c"]);
		}
	}
}
