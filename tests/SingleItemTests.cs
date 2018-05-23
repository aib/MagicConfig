using MagicConfig;
using MagicConfig.Tests.Helpers;
using System;
using Xunit;

namespace MagicConfig.Tests
{
	public class SingleItemTests
	{
		[Fact]
		public void SingleItemHoldsValue()
		{
			{
				SingleItem<int> si = 42;
				Assert.Equal(42, si.Value);
				Assert.Equal(42, (int) si);
			}

			{
				int i = 42;
				SingleItem<int> si = i;
				Assert.Equal(i, si.Value);
				int o = si;
				Assert.Equal(i, o);
			}

			{
				string i = "foo";
				SingleItem<string> si = i;
				Assert.Equal(i, si.Value);
				string o = si;
				Assert.Equal(i, o);
			}

			{
				MyFalseEquatable i = new MyFalseEquatable();
				SingleItem<MyFalseEquatable> si = i;
				Assert.Same(i, si.Value);
				MyFalseEquatable o = si;
				Assert.Same(i, o);
			}
		}

		[Fact]
		public void SingleItemDoesNotEqualNull()
		{
			SingleItem<int> si = 42;
			Assert.False(si.Equals(null));
			Assert.False(si.Equals((SingleItem<int>) null));
			Assert.False(si.Equals((SingleItem<string>) null));
			Assert.False(si.Equals((ConfigItem) null));
		}

		[Fact]
		public void SingleItemEqualsWorks()
		{
			SingleItem<int> si1 = 42;
			SingleItem<int> si2 = 42;
			Assert.True(si1.Equals(si2));
		}

		[Fact]
		public void SingleItemEqualsCallsItemEquals()
		{
			{
				MyFalseEquatable e = new MyFalseEquatable();
				SingleItem<MyFalseEquatable> si1 = e;
				SingleItem<MyFalseEquatable> si2 = e;
				Assert.False(si1.Equals(si2));
			}

			{
				SingleItem<MyTrueEquatable> si1 = new MyTrueEquatable();
				SingleItem<MyTrueEquatable> si2 = new MyTrueEquatable();
				Assert.True(si1.Equals(si2));
			}
		}

		[Fact]
		public void SingleItemInvalidAssignmentThrows()
		{
			SingleItem<int> i1 = 4;
			MyFalseEquatableItem i2 = new MyFalseEquatableItem();
			Assert.Throws<ConfigItem.InvalidTypeAssignmentException>(() => i1.Assign(i2));
		}

		[Fact]
		public void SingleItemAssignmentChangesValue()
		{
			SingleItem<int> i1 = 4;
			SingleItem<int> i2 = 42;
			i1.Assign(i2);
			Assert.Equal(42, (int) i2);
		}

		[Fact]
		public void SingleItemUpdateEventIsCalled()
		{
			SingleItem<int> si1 = 4;
			SingleItem<int> si2 = 42;

			bool updateCalled = false;
			void updateHandler(object sender, SingleItem<int>.UpdatedArgs args) {
				Assert.False(updateCalled);
				updateCalled = true;
				Assert.Same(si1, sender);
				Assert.Equal(4, args.OldValue);
				Assert.Equal(42, args.NewValue);
			}
			si1.Updated += updateHandler;

			si1.Assign(si2);
			Assert.True(updateCalled);
		}
	}
}
