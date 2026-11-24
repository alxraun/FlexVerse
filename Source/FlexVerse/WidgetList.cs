// parents: @.frostline/graph/m-widget.xml

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FlexVerse
{
    /// <summary>Provides a semantic-equality widget list used by the Children prop. (M-WIDGET)</summary>
    public class WidgetList
        : IList<Widget>,
            ICollection<Widget>,
            IReadOnlyList<Widget>,
            IEquatable<WidgetList>
    {
        private readonly List<Widget> _items;
        private int _cachedHashCode;
        private bool _isHashValid;

        /// <summary>Creates an empty widget list.</summary>
        public WidgetList()
        {
            _items = [];
        }

        /// <summary>Creates an empty widget list with the given capacity.</summary>
        public WidgetList(int capacity)
        {
            _items = new List<Widget>(capacity);
        }

        /// <summary>Creates a widget list from a widget sequence.</summary>
        public WidgetList(IEnumerable<Widget> items)
        {
            _items = [.. items];
        }

        /// <summary>Gets the number of widgets in the list.</summary>
        public int Count => _items.Count;

        /// <summary>Reports whether the list accepts modifications.</summary>
        public bool IsReadOnly => false;

        /// <summary>Gets or sets the allocated capacity of the list.</summary>
        public int Capacity
        {
            get => _items.Capacity;
            set => _items.Capacity = value;
        }

        /// <summary>Gets or sets the widget at the given index.</summary>
        public Widget this[int index]
        {
            get => _items[index];
            set
            {
                _items[index] = value;
                InvalidateCachedHash();
            }
        }

        /// <summary>Appends a widget to the end of the list.</summary>
        public void Add(Widget item)
        {
            _items.Add(item);
            InvalidateCachedHash();
        }

        /// <summary>Appends a sequence of widgets to the end of the list.</summary>
        public void AddRange(IEnumerable<Widget> items)
        {
            _items.AddRange(items);
            InvalidateCachedHash();
        }

        /// <summary>Returns a read-only view of the list.</summary>
        public ReadOnlyCollection<Widget> AsReadOnly() => _items.AsReadOnly();

        /// <summary>Searches the sorted list for a widget.</summary>
        public int BinarySearch(Widget item) => _items.BinarySearch(item);

        /// <summary>Searches the sorted list with a comparer.</summary>
        public int BinarySearch(Widget item, IComparer<Widget> comparer) =>
            _items.BinarySearch(item, comparer);

        /// <summary>Searches a range of the sorted list with a comparer.</summary>
        public int BinarySearch(int index, int count, Widget item, IComparer<Widget> comparer) =>
            _items.BinarySearch(index, count, item, comparer);

        /// <summary>Removes all widgets from the list.</summary>
        public void Clear()
        {
            if (_items.Count == 0)
                return;
            _items.Clear();
            InvalidateCachedHash();
        }

        /// <summary>Reports whether the list contains a widget.</summary>
        public bool Contains(Widget item) => _items.Contains(item);

        /// <summary>Converts all widgets to another type.</summary>
        public List<TOutput> ConvertAll<TOutput>(Converter<Widget, TOutput> converter) =>
            _items.ConvertAll(converter);

        /// <summary>Copies the widgets into an array at the given offset.</summary>
        public void CopyTo(Widget[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        /// <summary>Copies a range of widgets into an array.</summary>
        public void CopyTo(int index, Widget[] array, int arrayIndex, int count) =>
            _items.CopyTo(index, array, arrayIndex, count);

        /// <summary>Copies the widgets into an array.</summary>
        public void CopyTo(Widget[] array) => _items.CopyTo(array);

        /// <summary>Reports whether any widget matches a predicate.</summary>
        public bool Exists(Predicate<Widget> match) => _items.Exists(match);

        /// <summary>Returns the first widget matching a predicate.</summary>
        public Widget Find(Predicate<Widget> match) => _items.Find(match);

        /// <summary>Returns all widgets matching a predicate.</summary>
        public List<Widget> FindAll(Predicate<Widget> match) => _items.FindAll(match);

        /// <summary>Returns the index of the first matching widget.</summary>
        public int FindIndex(Predicate<Widget> match) => _items.FindIndex(match);

        /// <summary>Returns the index of the first match from a start index.</summary>
        public int FindIndex(int startIndex, Predicate<Widget> match) =>
            _items.FindIndex(startIndex, match);

        /// <summary>Returns the index of the first match within a range.</summary>
        public int FindIndex(int startIndex, int count, Predicate<Widget> match) =>
            _items.FindIndex(startIndex, count, match);

        /// <summary>Returns the last widget matching a predicate.</summary>
        public Widget FindLast(Predicate<Widget> match) => _items.FindLast(match);

        /// <summary>Returns the index of the last matching widget.</summary>
        public int FindLastIndex(Predicate<Widget> match) => _items.FindLastIndex(match);

        /// <summary>Returns the last match index searching backward from a start.</summary>
        public int FindLastIndex(int startIndex, Predicate<Widget> match) =>
            _items.FindLastIndex(startIndex, match);

        /// <summary>Returns the last match index within a range.</summary>
        public int FindLastIndex(int startIndex, int count, Predicate<Widget> match) =>
            _items.FindLastIndex(startIndex, count, match);

        /// <summary>Invokes an action on every widget in the list.</summary>
        public void ForEach(Action<Widget> action) => _items.ForEach(action);

        /// <summary>Returns a struct enumerator over the list.</summary>
        public Enumerator GetEnumerator() => new(_items.GetEnumerator());

        IEnumerator<Widget> IEnumerable<Widget>.GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        /// <summary>Returns a contiguous range of widgets as a list.</summary>
        public List<Widget> GetRange(int index, int count) => _items.GetRange(index, count);

        /// <summary>Returns the index of a widget in the list.</summary>
        public int IndexOf(Widget item) => _items.IndexOf(item);

        /// <summary>Returns the index of a widget from a start index.</summary>
        public int IndexOf(Widget item, int index) => _items.IndexOf(item, index);

        /// <summary>Returns the index of a widget within a range.</summary>
        public int IndexOf(Widget item, int index, int count) => _items.IndexOf(item, index, count);

        /// <summary>Inserts a widget at the given index.</summary>
        public void Insert(int index, Widget item)
        {
            _items.Insert(index, item);
            InvalidateCachedHash();
        }

        /// <summary>Inserts a sequence of widgets at the given index.</summary>
        public void InsertRange(int index, IEnumerable<Widget> items)
        {
            _items.InsertRange(index, items);
            InvalidateCachedHash();
        }

        /// <summary>Returns the last index of a widget in the list.</summary>
        public int LastIndexOf(Widget item) => _items.LastIndexOf(item);

        /// <summary>Returns the last index of a widget from a start index.</summary>
        public int LastIndexOf(Widget item, int index) => _items.LastIndexOf(item, index);

        /// <summary>Returns the last index of a widget within a range.</summary>
        public int LastIndexOf(Widget item, int index, int count) =>
            _items.LastIndexOf(item, index, count);

        /// <summary>Removes the first occurrence of a widget.</summary>
        public bool Remove(Widget item)
        {
            var removed = _items.Remove(item);
            if (removed)
            {
                InvalidateCachedHash();
            }
            return removed;
        }

        /// <summary>Removes every widget matching a predicate.</summary>
        public int RemoveAll(Predicate<Widget> match)
        {
            var removedCount = _items.RemoveAll(match);
            if (removedCount != 0)
            {
                InvalidateCachedHash();
            }
            return removedCount;
        }

        /// <summary>Removes the widget at the given index.</summary>
        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
            InvalidateCachedHash();
        }

        /// <summary>Removes a range of widgets from the list.</summary>
        public void RemoveRange(int index, int count)
        {
            _items.RemoveRange(index, count);
            InvalidateCachedHash();
        }

        /// <summary>Reverses the order of the widgets.</summary>
        public void Reverse()
        {
            _items.Reverse();
            InvalidateCachedHash();
        }

        /// <summary>Reverses a range of the list.</summary>
        public void Reverse(int index, int count)
        {
            _items.Reverse(index, count);
            InvalidateCachedHash();
        }

        /// <summary>Sorts the widgets with their default comparison.</summary>
        public void Sort()
        {
            _items.Sort();
            InvalidateCachedHash();
        }

        /// <summary>Sorts the widgets with a comparer.</summary>
        public void Sort(IComparer<Widget> comparer)
        {
            _items.Sort(comparer);
            InvalidateCachedHash();
        }

        /// <summary>Sorts the widgets with a comparison delegate.</summary>
        public void Sort(Comparison<Widget> comparison)
        {
            _items.Sort(comparison);
            InvalidateCachedHash();
        }

        /// <summary>Sorts a range of the list with a comparer.</summary>
        public void Sort(int index, int count, IComparer<Widget> comparer)
        {
            _items.Sort(index, count, comparer);
            InvalidateCachedHash();
        }

        /// <summary>Copies the widgets into a new array.</summary>
        public Widget[] ToArray() => _items.ToArray();

        /// <summary>Releases unused reserved capacity.</summary>
        public void TrimExcess() => _items.TrimExcess();

        /// <summary>Reports whether every widget matches a predicate.</summary>
        public bool TrueForAll(Predicate<Widget> match) => _items.TrueForAll(match);

        /// <summary>Compares semantic content (keys and types) against another list.</summary>
        public bool Equals(WidgetList other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (Count != other.Count)
                return false;

            var thisHash = GetHashCode();
            var otherHash = other.GetHashCode();
            if (thisHash != otherHash)
                return false;

            for (int i = 0; i < Count; i++)
            {
                var left = this[i];
                var right = other[i];

                if (left is null && right is null)
                    continue;
                if (left is null || right is null)
                    return false;

                if (!Equals(left.Key, right.Key))
                    return false;

                if (left.GetType() != right.GetType())
                    return false;
            }

            return true;
        }

        /// <summary>Compares against another object for equality.</summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as WidgetList);
        }

        /// <summary>Returns the cached semantic hash code of the list.</summary>
        public override int GetHashCode()
        {
            if (_isHashValid)
                return _cachedHashCode;

            _cachedHashCode = ComputeSemanticHashCode();
            _isHashValid = true;
            return _cachedHashCode;
        }

        /// <summary>Reports semantic equality of two widget lists.</summary>
        public static bool operator ==(WidgetList left, WidgetList right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (left is null || right is null)
                return false;

            var leftHash = left.GetHashCode();
            var rightHash = right.GetHashCode();
            if (leftHash != rightHash)
                return false;

            return left.Equals(right);
        }

        /// <summary>Reports semantic inequality of two widget lists.</summary>
        public static bool operator !=(WidgetList left, WidgetList right)
        {
            return !(left == right);
        }

        private void InvalidateCachedHash()
        {
            _isHashValid = false;
        }

        private int ComputeSemanticHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Count;

                for (int i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];
                    if (item is null)
                    {
                        hash = hash * 31 + 0;
                        hash = hash * 31 + 0;
                        continue;
                    }

                    var typeHash = item.GetType().GetHashCode();
                    var keyHash = item.Key is null
                        ? 0
                        : EqualityComparer<object>.Default.GetHashCode(item.Key);

                    hash = hash * 31 + typeHash;
                    hash = hash * 31 + keyHash;
                }

                return hash;
            }
        }

        /// <summary>Provides a struct enumerator over the widget list.</summary>
        public readonly struct Enumerator
        {
            private readonly IEnumerator<Widget> _enumerator;

            internal Enumerator(IEnumerator<Widget> enumerator)
            {
                _enumerator = enumerator;
            }

            /// <summary>Gets the widget at the current enumerator position.</summary>
            public Widget Current => _enumerator.Current;

            /// <summary>Advances the enumerator to the next widget.</summary>
            public bool MoveNext() => _enumerator.MoveNext();
        }
    }
}
