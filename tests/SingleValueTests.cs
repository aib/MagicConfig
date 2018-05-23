using MagicConfig;
using MagicConfig.Tests.Helpers;
using System;
using Xunit;

namespace MagicConfig.Tests
{
	public class SingleValueTests
	{
		[Fact]
		public void SingleValueHoldsValue()
		{
			{
				SingleValue<MyBoolean> sv = MyBoolean.FILE_NOT_FOUND;
				Assert.Equal(MyBoolean.FILE_NOT_FOUND, sv.Value);
				Assert.Equal<MyBoolean>(MyBoolean.FILE_NOT_FOUND, sv);
				Assert.Equal(2, (int) sv.Value);
				Assert.Equal(2, (int) (MyBoolean) sv);
			}

			{
				SingleValue<int> sv = 42;
				Assert.Equal(42, sv.Value);
				Assert.Equal(42, (int) sv);
			}
		}

		[Fact]
		public void SingleValueDoesNotEqualNull()
		{
			SingleValue<int> sv = 42;
			Assert.False(sv.Equals(null));
			Assert.False(sv.Equals((SingleValue<int>) null));
			Assert.False(sv.Equals((SingleValue<MyBoolean>) null));
			Assert.False(sv.Equals((ConfigItem) null));
		}

		[Fact]
		public void SingleValueEqualsWorks()
		{
			{
				SingleValue<MyBoolean> sv1 = MyBoolean.TRUE;
				SingleValue<MyBoolean> sv2 = MyBoolean.TRUE;
				Assert.True(sv1.Equals(sv2));
			}

			{
				SingleValue<MyBoolean> sv1 = MyBoolean.FILE_NOT_FOUND;
				SingleValue<MyBoolean> sv2 = (MyBoolean) 2;
				Assert.True(sv1.Equals(sv2));
			}

			{
				SingleValue<int> sv1 = 42;
				SingleValue<int> sv2 = 42;
				Assert.True(sv1.Equals(sv2));
			}
		}

		[Fact]
		public void SingleValueAssignmentChangesValue()
		{
			SingleValue<MyBoolean> sv1 = MyBoolean.TRUE;
			SingleValue<MyBoolean> sv2 = MyBoolean.FILE_NOT_FOUND;
			sv1.Assign(sv2);
			Assert.Equal<MyBoolean>(MyBoolean.FILE_NOT_FOUND, sv2);
		}

		[Fact]
		public void SingleValueUpdateEventIsCalled()
		{
			SingleValue<MyBoolean> sv1 = MyBoolean.FILE_NOT_FOUND;
			SingleValue<MyBoolean> sv2 = MyBoolean.TRUE;

			bool updateCalled = false;
			void updateHandler(object sender, SingleValue<MyBoolean>.UpdatedArgs args) {
				Assert.False(updateCalled);
				updateCalled = true;
				Assert.Same(sv1, sender);
				Assert.Equal<MyBoolean>(MyBoolean.FILE_NOT_FOUND, args.OldValue);
				Assert.Equal<MyBoolean>(MyBoolean.TRUE, args.NewValue);
			}
			sv1.Updated += updateHandler;

			sv1.Assign(sv2);
			Assert.True(updateCalled);
		}

		[Fact]
		public void SingleValueSingleItemCrossAssignment()
		{
			{
				SingleItem<int> si = 2;
				SingleValue<int> sv = 42;
				si.Assign(sv);
				Assert.Equal(42, (int) si);
			}

			{
				SingleValue<int> sv = 2;
				SingleItem<int> si = 42;
				sv.Assign(si);
				Assert.Equal(42, (int) sv);
			}
		}
	}
}
