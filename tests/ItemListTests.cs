using MagicConfig;
using MagicConfig.Tests.Helpers;
using System;
using System.Collections.Generic;
using Xunit;

namespace MagicConfig.Tests
{
	public class ItemListTests
	{
		[Fact]
		public void ItemListHoldsValues()
		{
			{
				ItemList<int> il = new ItemList<int> { 3, 4, 5 };
				Assert.Equal(3, il.Count);
				Assert.Equal(3, il[0]);
				Assert.Equal(4, il[1]);
				Assert.Equal(5, il[2]);
			}

			{
				var i = new List<int> { 3, 4, 5 };
				ItemList<int> il = i;
				List<int> o = il;
				Assert.Equal(i, o);
			}

			{
				var i = new List<SingleItem<int>> { 3, 4, 5 };
				ItemList<SingleItem<int>> il = i;
				List<SingleItem<int>> o = il;
				Assert.Equal(i, o);
			}

			{
				MyFalseEquatable i = new MyFalseEquatable();
				ItemList<MyFalseEquatable> il = new ItemList<MyFalseEquatable> { i };
				MyFalseEquatable o = il[0];
				Assert.Same(i, o);
			}
		}

		[Fact]
		public void ItemListDoesNotEqualNull()
		{
			ItemList<int> il = new ItemList<int>();
			Assert.False(il.Equals(null));
			Assert.False(il.Equals((ItemList<int>) null));
			Assert.False(il.Equals((ItemList<string>) null));
			Assert.False(il.Equals((ConfigItem) null));
		}

		[Fact]
		public void ItemListEquality()
		{
			{
				ItemList<int> il1 = new ItemList<int> { 3, 4, 5 };
				ItemList<int> il2 = new ItemList<int> { 3, 4, 5 };
				Assert.Equal(il1, il2);
				Assert.True(il1.Equals(il2));
			}

			{
				ItemList<int> il1 = new ItemList<int> { 3, 4, 5 };
				ItemList<int> il2 = new ItemList<int> { 3, 4 };
				Assert.NotEqual(il1, il2);
				Assert.False(il1.Equals(il2));
			}

			{
				ItemList<int> il1 = new ItemList<int> { 3, 4, 5 };
				ItemList<int> il2 = new ItemList<int> { 3, 4, 5, 6 };
				Assert.NotEqual(il1, il2);
				Assert.False(il1.Equals(il2));
			}

			{
				ItemList<int> il1 = new ItemList<int> { 3, 4, 5 };
				ItemList<int> il2 = new ItemList<int> { 3, 4, 50 };
				Assert.NotEqual(il1, il2);
				Assert.False(il1.Equals(il2));
			}

			{
				ItemList<SingleItem<int>> il1 = new ItemList<SingleItem<int>> { 3, 4, 5 };
				ItemList<SingleItem<int>> il2 = new ItemList<SingleItem<int>> { 3, 4, 5 };
				Assert.Equal(il1, il2);
				Assert.True(il1.Equals(il2));
			}

			{
				ItemList<string> il1 = new ItemList<string> { "foo", "bar" };
				ItemList<string> il2 = new ItemList<string> { "foo", "bar" };
				Assert.Equal(il1, il2);
				Assert.True(il1.Equals(il2));
			}
		}

		[Fact]
		public void ItemListEqualsCallsItemEquals()
		{
			{
				MyFalseEquatable e = new MyFalseEquatable();
				ItemList<MyFalseEquatable> si1 = new ItemList<MyFalseEquatable> { e };
				ItemList<MyFalseEquatable> si2 = new ItemList<MyFalseEquatable> { e };
				Assert.False(si1.Equals(si2));
			}

			{
				ItemList<MyTrueEquatable> si1 = new ItemList<MyTrueEquatable> { new MyTrueEquatable() };
				ItemList<MyTrueEquatable> si2 = new ItemList<MyTrueEquatable> { new MyTrueEquatable() };
				Assert.True(si1.Equals(si2));
			}
		}

		[Fact]
		public void ItemListInvalidAssignmentThrows()
		{
			ItemList<int> i1 = new ItemList<int>();
			MyFalseEquatableItem i2 = new MyFalseEquatableItem();
			Assert.Throws<ConfigItem.InvalidTypeAssignmentException>(() => i1.Assign(i2));
		}

		[Fact]
		public void ItemListAssignmentChangesValue()
		{
			ItemList<int> il  = new ItemList<int> { 3, 4, 5 };
			ItemList<int> il2 = new ItemList<int> { 2, 4, 6 };

			il.Assign(il2);
			Assert.Equal(3, il.Count);
			Assert.Contains(2, il);
			Assert.Contains(4, il);
			Assert.Contains(6, il);
		}

		[Fact]
		public void ItemListEventsAreCalled()
		{
			ItemList<SingleItem<int>> il  = new ItemList<SingleItem<int>> { 3, 4 };
			ItemList<SingleItem<int>> il2 = new ItemList<SingleItem<int>> { 4, 6 };

			bool itemUpdatesCalled = false;
			void itemUpdateHandler(object sender, SingleItem<int>.UpdatedArgs args) { itemUpdatesCalled = true; }
			il[0].Updated += itemUpdateHandler;
			il[1].Updated += itemUpdateHandler;
			il2[0].Updated += itemUpdateHandler;
			il2[1].Updated += itemUpdateHandler;

			bool threeDeleted = false;
			bool othersDeleted = false;
			void deleteHandler(object sender, ItemList<SingleItem<int>>.DeletedArgs args) {
				Assert.Same(il, sender);
				Assert.False(threeDeleted);
				Assert.False(othersDeleted);
				if (args.OldItem == 3) threeDeleted = true;
				else othersDeleted = true;
			}
			il.Deleted += deleteHandler;

			bool sixAdded = false;
			bool othersAdded = false;
			void addHandler(object sender, ItemList<SingleItem<int>>.AddedArgs args) {
				Assert.Same(il, sender);
				Assert.False(sixAdded);
				Assert.False(othersAdded);
				if (args.NewItem == 6) sixAdded = true;
				else othersAdded = true;
			}
			il.Added += addHandler;

			il.Assign(il2);

			Assert.True(threeDeleted);
			Assert.False(othersDeleted);

			Assert.True(sixAdded);
			Assert.False(othersAdded);

			Assert.False(itemUpdatesCalled);
		}
	}
}
