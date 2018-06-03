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
				SingleItem<MyInt> si = (MyInt) 42;
				Assert.Equal((MyInt) 42, si.Value);
				Assert.Equal((MyInt) 42, (MyInt) si);
			}

			{
				MyInt i = 42;
				SingleItem<MyInt> si = i;
				Assert.Equal(i, si.Value);
				MyInt o = si;
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
			SingleItem<MyInt> si = new MyInt(42);
			Assert.False(si.Equals(null));
			Assert.False(si.Equals((SingleItem<MyInt>) null));
			Assert.False(si.Equals((SingleItem<string>) null));
			Assert.False(si.Equals((ConfigItem) null));
		}

		[Fact]
		public void SingleItemEqualsWorks()
		{
			{
				SingleItem<MyInt> si1 = (MyInt) 42;
				SingleItem<MyInt> si2 = new MyInt(42);

				Assert.True(si1.Equals(si2));
				Assert.True(si2.Equals(si1));

				Assert.True(si1.Equals(si1));
				Assert.True(si2.Equals(si2));
			}

			{
				SingleItem<MyInt> si = new SingleItem<MyInt>(new MyInt(42));
				SingleItem<MyInt> ni = new SingleItem<MyInt>(null);

				Assert.True(si.Equals(si));
				Assert.False(si.Equals(ni));
				Assert.False(ni.Equals(si));
				Assert.True(ni.Equals(ni));
			}
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
			SingleItem<MyInt> i1 = (MyInt) 4;
			MyFalseEquatableItem i2 = new MyFalseEquatableItem();
			Assert.Throws<ConfigItem.InvalidTypeAssignmentException>(() => i1.Assign(i2));
		}

		[Fact]
		public void SingleItemAssignmentChangesValue()
		{
			SingleItem<MyInt> i1 = (MyInt) 4;
			SingleItem<MyInt> i2 = (MyInt) 42;
			i1.Assign(i2);
			Assert.Equal<MyInt>(new MyInt(42), i2);
		}

		[Fact]
		public void SingleItemUpdateEventIsCalled()
		{
			SingleItem<MyInt> si1 = new MyInt(4);
			SingleItem<MyInt> si2 = new MyInt(42);

			bool updateCalled = false;
			void updateHandler(object sender, SingleItem<MyInt>.UpdatedArgs args) {
				Assert.False(updateCalled);
				updateCalled = true;
				Assert.Same(si1, sender);
				Assert.Equal<MyInt>(4, args.OldValue);
				Assert.Equal<MyInt>(42, args.NewValue);
			}
			si1.Updated += updateHandler;

			si1.Assign(si2);
			Assert.True(updateCalled);
		}
	}
}
