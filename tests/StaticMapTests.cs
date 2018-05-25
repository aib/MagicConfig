using MagicConfig;
using MagicConfig.Tests.Helpers;
using System;
using System.Collections.Generic;
using Xunit;

namespace MagicConfig.Tests
{
	public class StaticMapTests
	{
		[Fact]
		public void StaticMapHoldsValue()
		{
			MyComposite sm = new MyComposite { si = 42, ss1 = "foo", ss2 = null, nested = new MyComposite.MyNested { x = 10, y = 20, s = "bar" }, ignored_int = 1, ignored_string = "ignored" };

			Assert.Equal<int>(42, sm.si);
			Assert.Equal("foo", sm.ss1);
			Assert.Null(sm.ss2);
			Assert.Equal<int>(10, sm.nested.x);
			Assert.Equal<int>(20, sm.nested.y);
			Assert.Equal("bar", sm.nested.s);

			Assert.Equal(1, sm.ignored_int);
			Assert.Equal("ignored", sm.ignored_string);
		}

		[Fact]
		public void StaticMapEqualsWorks()
		{
			{
				MyComposite sm1 = new MyComposite { si = 42, ss1 = "foo", ss2 = null, nested = new MyComposite.MyNested { x = 10, y = 20, s = "bar" }, ci = null };
				MyComposite sm2 = new MyComposite { si = 42, ss1 = "foo", ss2 = null, nested = new MyComposite.MyNested { x = 10, y = 20, s = "bar" }, ci = null };
				Assert.True(sm1.Equals(sm2));
				Assert.False(sm1.Equals(null));
			}

			{
				MyComposite sm1 = new MyComposite { ss1 = null,  ss2 = "foo" };
				MyComposite sm2 = new MyComposite { ss1 = "foo", ss2 = null  };
				Assert.False(sm1.Equals(sm2));
				Assert.False(sm2.Equals(sm1));
			}
		}

		[Fact]
		public void StaticMapEqualsCallsItemEquals()
		{
			{
				MyFalseEquatableItem e = new MyFalseEquatableItem();

				MyComposite sm1 = new MyComposite { ci = e };
				MyComposite sm2 = new MyComposite { ci = e };
				Assert.False(sm1.Equals(sm2));
			}

			{
				MyComposite sm1 = new MyComposite { ci = new MyTrueEquatableItem() };
				MyComposite sm2 = new MyComposite { ci = new MyTrueEquatableItem() };
				Assert.True(sm1.Equals(sm2));
			}
		}

		[Fact]
		public void StaticMapInvalidAssignmentThrows()
		{
			MyComposite sm = new MyComposite();
			ConfigItem ci = new MyFalseEquatableItem();
			Assert.Throws<ConfigItem.InvalidTypeAssignmentException>(() => sm.Assign(ci));
		}

		[Fact]
		public void StaticMapAssignmentChangesValue()
		{
			ConfigItem cif = new MyFalseEquatableItem();
			MyComposite sm =  new MyComposite { si = 40, ss1 = "not foo", ss2 = null,  nested = new MyComposite.MyNested { x = 10, y = 20, s = "bar"  }, ci = null };
			MyComposite sm2 = new MyComposite { si = 42, ss1 = "foo",	  ss2 = "bar", nested = new MyComposite.MyNested { x = 11, y = 20, s = "barz" }, ci = cif };

			sm.Assign(sm2);

			Assert.Equal<int>(42, sm.si);
			Assert.Equal("foo", sm.ss1);
			Assert.Equal("bar", sm.ss2);
			Assert.Equal<int>(11, sm.nested.x);
			Assert.Equal("barz", sm.nested.s);
			Assert.Same(cif, sm.ci);
		}

		[Fact]
		public void StaticMapUpdateEventIsCalled()
		{
			MyComposite sm =  new MyComposite { si = 40, ss1 = "not foo", ss2 = null,  nested = new MyComposite.MyNested { x = 10, y = 20, s = "bar"  } };
			MyComposite sm2 = new MyComposite { si = 42, ss1 = "foo",	  ss2 = "bar", nested = new MyComposite.MyNested { x = 11, y = 20, s = "barz" } };

			bool updateCalled = false;
			void updateHandler(object sender, StaticMap<MyComposite>.UpdatedArgs args) {
				Assert.False(updateCalled);
				updateCalled = true;
				Assert.Same(sm, sender);
				Assert.Equal("foo", sm.ss1);
			}
			sm.Updated += updateHandler;

			var itemUpdates = new Dictionary<string, object>();
			sm.ItemUpdated += (sender, args) => { Assert.Same(sm, sender); itemUpdates[args.Key] = args.Item; };

			bool siUpdateCalled = false;
			void siUpdateHandler(object sender, SingleItem<string>.UpdatedArgs args) {
				Assert.False(siUpdateCalled);
				siUpdateCalled = true;
				Assert.Same(sm.nested.s, sender);
				Assert.Equal("bar", args.OldValue);
				Assert.Equal("barz", args.NewValue);
			}
			sm.nested.s.Updated += siUpdateHandler;

			sm.Assign(sm2);

			Assert.True(updateCalled);
			Assert.True(siUpdateCalled);

			Assert.True(itemUpdates.ContainsKey("si"));
			Assert.Equal((SingleValue<int>) 42, itemUpdates["si"]);

			Assert.True(itemUpdates.ContainsKey("ss1"));
			Assert.Equal((SingleItem<string>) "foo", itemUpdates["ss1"]);

			Assert.True(itemUpdates.ContainsKey("ss2"));
			Assert.Equal((SingleItem<string>) "bar", itemUpdates["ss2"]);

			Assert.True(itemUpdates.ContainsKey("nested"));
			Assert.Equal(new MyComposite.MyNested{x=11, y=20, s="barz"}, itemUpdates["nested"]);
		}
	}
}
