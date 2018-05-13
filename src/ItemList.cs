using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MagicConfig
{
	// This class holds a list or array of values
	public class ItemList<T>: ConfigItem, IList<T>
		where T: IEquatable<T>
	{
		public class AddedArgs:   EventArgs { public T NewItem; }
		public class DeletedArgs: EventArgs { public T OldItem; }
		public event EventHandler<AddedArgs>   Added;
		public event EventHandler<DeletedArgs> Deleted;

		private List<T> list = new List<T>();

		public static implicit operator List<T>(ItemList<T> il)
		{
			return il.list;
		}

		public static implicit operator ItemList<T>(List<T> l)
		{
			return new ItemList<T> { list = l };
		}

		public override bool Equals(ConfigItem other)
		{
			if (other is ItemList<T> otherList) {
				return !(list.Any(l => !otherList.list.Any(l.Equals)) || otherList.Any(r => !list.Any(r.Equals)));
			} else {
				return false;
			}
		}

		public override void Assign(ConfigItem other)
		{
			if (other is ItemList<T> otherList) {
				var left  = list.Where(l => !otherList.list.Any(l.Equals)).ToList();
				var right = otherList.list.Where(r => !list.Any(r.Equals)).ToList();

				foreach (var l in left) {
					list.Remove(l);
					Deleted?.Invoke(this, new DeletedArgs { OldItem = l });
				}

				foreach (var r in right) {
					list.Add(r);
					Added?.Invoke(this, new AddedArgs { NewItem = r });
				}
			} else {
				throw new InvalidTypeAssignmentException(this, other);
			}
		}

		public int IndexOf(T item) => list.IndexOf(item);
		public void Insert(int index, T item) => list.Insert(index, item);
		public void RemoveAt(int index) => list.RemoveAt(index);
		public T this[int index] { get => list[index]; set => list[index] = value; }

		public void Add(T item) => list.Add(item);
		public void Clear() => list.Clear();
		public bool Contains(T item) => list.Contains(item);
		public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);
		public bool Remove(T item) => list.Remove(item);
		public int Count => list.Count;
		public bool IsReadOnly => false;

		public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
