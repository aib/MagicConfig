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
			{
				MyComposite sm = new MyComposite();
				ConfigItem ci = new MyFalseEquatableItem();
				Assert.Throws<ConfigItem.InvalidTypeAssignmentException>(() => sm.Assign(ci));
			}

			{
				MyComposite sm  = new MyComposite();
				MyComposite sm2 = new MyComposite { ss1 = null, nested = new MyComposite.MyNested { s = null } };

				bool thrown = false;
				try {
					sm.Assign(sm2);
				} catch (ConfigItem.InvalidChildAssignmentException e) {
					thrown = true;
					var childExceptions = new List<ConfigItem.InvalidAssignmentException>(e.ChildExceptions);

					Assert.Equal(2, childExceptions.Count);

					Assert.IsType<ConfigItem.InvalidTypeAssignmentException>(childExceptions[0]);
					var ce0 = (ConfigItem.InvalidTypeAssignmentException) childExceptions[0];
					Assert.Equal(new SingleItem<string>(), ce0.OldItem);
					Assert.Null(ce0.NewItem);

					Assert.IsType<ConfigItem.InvalidChildAssignmentException>(childExceptions[1]);
					var ce1 = (ConfigItem.InvalidChildAssignmentException) childExceptions[1];
					var ce1ce = new List<ConfigItem.InvalidAssignmentException>(ce1.ChildExceptions);
					Assert.Single(ce1ce);
					Assert.IsType<ConfigItem.InvalidTypeAssignmentException>(ce1ce[0]);
					var ce1ce0 = (ConfigItem.InvalidTypeAssignmentException) ce1ce[0];
					Assert.Equal(new SingleItem<string>(), ce1ce0.OldItem);
					Assert.Null(ce1ce0.NewItem);
				}
				Assert.True(thrown);
			}
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
			MyComposite.MyNested nested1 = new MyComposite.MyNested { x = 10, y = 20, s = "bar"  };
			MyComposite.MyNested nested2 = new MyComposite.MyNested { x = 11, y = 20, s = "barz" };
			MyComposite sm =  new MyComposite { si = 40, ss1 = "not foo", ss2 = null,  nested = nested1 };
			MyComposite sm2 = new MyComposite { si = 42, ss1 = "foo",	  ss2 = "bar", nested = nested2 };

			bool updateCalled = false;
			void updateHandler(object sender, StaticMap<MyComposite>.UpdatedArgs args) {
				Assert.False(updateCalled);
				updateCalled = true;
				Assert.Same(sm, sender);
				Assert.Equal("foo", sm.ss1);

				var updated = new List<KeyValuePair<string, ConfigItem>>(args.UpdatedItems);
				Assert.Equal(4, updated.Count);
				Assert.Equal("si", updated[0].Key);
				Assert.Equal(new SingleValue<int>(42), (SingleValue<int>) updated[0].Value);
				Assert.Equal("ss1", updated[1].Key);
				Assert.Equal("foo", (SingleItem<string>) updated[1].Value);
				Assert.Equal("ss2", updated[2].Key);
				Assert.Equal("bar", (SingleItem<string>) updated[2].Value);
				Assert.Equal("nested", updated[3].Key);
				Assert.Same(nested1, updated[3].Value);
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

		[Fact]
		public void StaticMapFieldsAreInitialized()
		{
			MyComposite sm = new MyComposite { ss1 = "foo" } ;

			Assert.Equal(new SingleValue<int>(), sm.si);
			Assert.Equal("foo", sm.ss1);
			Assert.Equal(new SingleItem<string>(), sm.ss2);
			Assert.Equal("default", sm.ss_with_default);

			Assert.Equal(new MyComposite.MyNested(), sm.nested);

			Assert.Null(sm.ci);

			Assert.Equal(default(int), sm.ignored_int);
			Assert.Null(sm.ignored_string);
		}
	}
}
