using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MagicConfig
{
	// This class holds a list or array of values
	public class ItemList<T>: ConfigItem, IList<T>, IEquatable<ItemList<T>>
		where T: IEquatable<T>
	{
		public class ItemDeletedArgs: EventArgs { public T OldItem; }
		public class ItemAddedArgs:   EventArgs { public T NewItem; }
		public class UpdatedArgs:     EventArgs { public IEnumerable<T> DeletedItems; public IEnumerable<T> AddedItems; public IList<T> NewList; }
		public event EventHandler<ItemDeletedArgs> ItemDeleted;
		public event EventHandler<ItemAddedArgs>   ItemAdded;
		public event EventHandler<UpdatedArgs>     Updated;

		private List<T> list;

		public ItemList()
		{
			list = new List<T>();
		}

		public ItemList(IEnumerable<T> es)
			:this()
		{
			if (es != null) {
				foreach (T e in es) {
					Add(e);
				}
			}
		}

		public static implicit operator List<T>(ItemList<T> l)
		{
			return l.list;
		}

		public static implicit operator ItemList<T>(List<T> v)
		{
			return new ItemList<T>(v);
		}

		public bool Equals(ItemList<T> other)
		{
			if (object.ReferenceEquals(other, null)) return false;
			if (list.Count != other.list.Count) return false;

			for (int i = 0; i < list.Count; ++i) {
				if (!list[i].Equals(other.list[i])) return false;
			}
			return true;
		}

		public override bool Equals(ConfigItem other)
		{
			return (other is ItemList<T> otherList) && Equals(otherList);
		}

		public override void Assign(ConfigItem other)
		{
			if (other is ItemList<T> otherList) {
				var newList = new List<T>();
				var added = new List<T>();

				foreach (var r in otherList.list) {
					int io = list.FindIndex(r.Equals);
					if (io == -1) {
						newList.Add(r);
						added.Add(r);
					} else {
						newList.Add(list[io]);
						list.RemoveAt(io);
					}
				}

				list.ForEach(i => ItemDeleted?.Invoke(this, new ItemDeletedArgs { OldItem = i }));
				added.ForEach(i => ItemAdded?.Invoke(this, new ItemAddedArgs { NewItem = i }));
				var updatedArgs = new UpdatedArgs { DeletedItems = list, AddedItems = added, NewList = newList };
				list = newList;
				Updated?.Invoke(this, updatedArgs);
			} else {
				throw new InvalidTypeAssignmentException(this, other);
			}
		}

		// IList<T>
		public int IndexOf(T item) => list.IndexOf(item);
		public void Insert(int index, T item) => list.Insert(index, item);
		public void RemoveAt(int index) => list.RemoveAt(index);
		public T this[int index] { get => list[index]; set => list[index] = value; }

		// ICollection<T>
		public void Add(T item) => list.Add(item);
		public void Clear() => list.Clear();
		public bool Contains(T item) => list.Contains(item);
		public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);
		public bool Remove(T item) => list.Remove(item);
		public int Count => list.Count;
		public bool IsReadOnly => false;

		// IEnumerable<T>
		public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

		// IEnumerable
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
