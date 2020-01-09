﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(this ref LazyList<T> list, T item)
        {
            list.Get().Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddIfNotNull<T>(this ref LazyList<T> list, T? item) where T : class
        {
            if (item != null)
                list.Get().Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this ref LazyList<T> list, IReadOnlyCollection<T>? items)
        {
            if (items != null && items.Count != 0)
                list.Get().AddRange(items);
        }

        public static ActionToken TryLock<T>(this IObservableCollection<T>? observableCollection)
        {
            if (!(observableCollection is ICollection c))
                return default;

            bool taken = false;
            var locker = c.SyncRoot;
            Monitor.Enter(locker, ref taken);
            if (taken)
                return new ActionToken((o, _) => Monitor.Exit(o), locker);
            return default;
        }

        [return: NotNullIfNotNull("observableCollection")]
        public static IEnumerable<T>? DecorateItems<T>(this IObservableCollection<T>? observableCollection)
        {
            if (observableCollection == null)
                return null;
            var component = observableCollection.GetComponentOptional<IDecoratorManagerObservableCollectionComponent<T>>();
            if (component == null)
                return observableCollection;
            return component.DecorateItems();
        }

        public static int Count<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class
            where TList : class, IReadOnlyCollection<TItem>
        {
            if (itemOrList.List != null)
                return itemOrList.List.Count;
            return itemOrList.Item == null ? 0 : 1;
        }

        public static int Count<TItem>(this ItemOrList<TItem, List<TItem>> itemOrList)
            where TItem : class
        {
            if (itemOrList.List != null)
                return itemOrList.List.Count;
            return itemOrList.Item == null ? 0 : 1;
        }

        public static int Count<TItem>(this ItemOrList<TItem, TItem[]> itemOrList)
            where TItem : class
        {
            if (itemOrList.List != null)
                return itemOrList.List.Length;
            return itemOrList.Item == null ? 0 : 1;
        }

        public static void Add<TItem>(this ref ItemOrList<TItem, List<TItem>> itemOrList, TItem item)
            where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            if (itemOrList.List != null)
            {
                itemOrList.List.Add(item);
                return;
            }

            if (itemOrList.Item == null)
            {
                itemOrList = item;
                return;
            }

            itemOrList = new ItemOrList<TItem, List<TItem>>(new List<TItem> { itemOrList.Item, item });
        }

        public static void AddRange<TItem>(this ref ItemOrList<TItem, List<TItem>> itemOrList, ItemOrList<TItem, IReadOnlyList<TItem>> value)
            where TItem : class
        {
            var list = value.List;
            var item = value.Item;
            if (item == null && list == null)
                return;

            if (itemOrList.Item != null)
                itemOrList = new List<TItem> { itemOrList.Item };

            if (itemOrList.List != null)
            {
                if (item == null)
                    itemOrList.List.AddRange(list);
                else
                    itemOrList.List.Add(item);
                return;
            }

            if (itemOrList.Item == null)
            {
                if (item == null)
                    itemOrList = new List<TItem>(list);
                else
                    itemOrList = item;
            }
        }

        public static TItem Get<TItem, TList>(this ItemOrList<TItem, TList> itemOrList, int index)
            where TItem : class
            where TList : class, IReadOnlyList<TItem>
        {
            if (itemOrList.List != null)
                return itemOrList.List[index];

            if (index == 0 && itemOrList.Item != null)
                return itemOrList.Item;

            ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
            return null;
        }

        public static TItem Get<TItem>(this ItemOrList<TItem, List<TItem>> itemOrList, int index)
            where TItem : class
        {
            if (itemOrList.List != null)
                return itemOrList.List[index];

            if (index == 0 && itemOrList.Item != null)
                return itemOrList.Item;

            ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
            return null;
        }

        public static TItem Get<TItem>(this ItemOrList<TItem, TItem[]> itemOrList, int index)
            where TItem : class
        {
            if (itemOrList.List != null)
                return itemOrList.List[index];

            if (index == 0 && itemOrList.Item != null)
                return itemOrList.Item;

            ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
            return null;
        }

        public static void Set<TItem>(this ref ItemOrList<TItem, List<TItem>> itemOrList, TItem item, int index)
            where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            if (itemOrList.List != null)
            {
                itemOrList.List[index] = item;
                return;
            }

            if (index == 0 && itemOrList.Item != null)
            {
                itemOrList = item;
                return;
            }

            ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
        }

        public static void Set<TItem, TList>(this ref ItemOrList<TItem, TList> itemOrList, TItem item, int index)
            where TItem : class
            where TList : class, IList<TItem>, IReadOnlyCollection<TItem>
        {
            Should.NotBeNull(item, nameof(item));
            if (itemOrList.List != null)
            {
                itemOrList.List[index] = item;
                return;
            }

            if (index == 0 && itemOrList.Item != null)
            {
                itemOrList = item;
                return;
            }

            ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
        }

        public static bool Remove<TItem, TList>(this ref ItemOrList<TItem, TList> itemOrList, TItem item)
            where TItem : class
            where TList : class, ICollection<TItem>, IReadOnlyList<TItem>
        {
            if (itemOrList.List != null)
                return itemOrList.List.Remove(item);

            if (Equals(itemOrList.Item, item))
            {
                itemOrList = default;
                return true;
            }

            return false;
        }

        public static void RemoveAt<TItem, TList>(this ref ItemOrList<TItem, TList> itemOrList, int index)
            where TItem : class
            where TList : class, IList<TItem>, IReadOnlyList<TItem>
        {
            if (itemOrList.List != null)
            {
                itemOrList.List.RemoveAt(index);
                return;
            }

            if (index == 0 && itemOrList.Item != null)
            {
                itemOrList = default;
                return;
            }

            ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
        }

        public static TItem[] ToArray<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class
            where TList : class, IReadOnlyList<TItem>
        {
            var list = itemOrList.List;
            if (list != null)
            {
                var items = new TItem[list.Count];
                for (int i = 0; i < list.Count; i++)
                    items[i] = list[i];
                return items;
            }

            if (itemOrList.Item == null)
                return Default.EmptyArray<TItem>();
            return new[] { itemOrList.Item };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetRawValue<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class?
            where TList : class?, IReadOnlyCollection<TItem>
        {
            return (object?)itemOrList.Item ?? itemOrList.List;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class
            where TList : class, IReadOnlyCollection<TItem>
        {
            return itemOrList.Item == null && itemOrList.List == null;
        }

        public static T[] ToArray<T>(this T[] array)
        {
            Should.NotBeNull(array, nameof(array));
            var result = new T[array.Length];
            Array.Copy(array, 0, result, 0, array.Length);
            return result;
        }

        public static TResult[] ToArray<T, TResult>(this IReadOnlyList<T> list, Func<T, TResult> selector)
        {
            Should.NotBeNull(list, nameof(list));
            var count = list.Count;
            if (count == 0)
                return Default.EmptyArray<TResult>();
            var array = new TResult[count];
            for (var index = 0; index < list.Count; index++)
                array[index] = selector(list[index]);
            return array;
        }

        public static TResult[] ToArray<T, TResult>(this IReadOnlyCollection<T> collection, Func<T, TResult> selector)
        {
            Should.NotBeNull(collection, nameof(collection));
            var count = collection.Count;
            if (count == 0)
                return Default.EmptyArray<TResult>();
            var array = new TResult[count];
            count = 0;
            foreach (var item in collection)
                array[count++] = selector(item);
            return array;
        }

        public static void AddOrdered<T>(ref T[] items, T item, IComparer<T> comparer)
        {
            var array = new T[items.Length + 1];
            Array.Copy(items, 0, array, 0, items.Length);
            AddOrdered(array, item, items.Length, comparer);
            items = array;
        }

        public static void AddOrdered<T>(T[] items, T item, int size, IComparer<T> comparer)
        {
            var binarySearch = Array.BinarySearch(items, 0, size, item, comparer);
            if (binarySearch < 0)
                binarySearch = ~binarySearch;
            if (binarySearch < size)
                Array.Copy(items, binarySearch, items, binarySearch + 1, size - binarySearch);
            items[binarySearch] = item;
        }

        public static void AddOrdered<T>(List<T> list, T item, IComparer<T> comparer)
        {
            Should.NotBeNull(list, nameof(list));
            var binarySearch = list.BinarySearch(item, comparer);
            if (binarySearch < 0)
                binarySearch = ~binarySearch;
            list.Insert(binarySearch, item);
        }

        public static bool Remove<T>(ref T[] items, T item) where T : class
        {
            if (items.Length == 0)
                return false;
            if (items.Length == 1)
            {
                if (ReferenceEquals(items[0], items))
                {
                    items = Default.EmptyArray<T>();
                    return true;
                }

                return false;
            }

            T[]? array = null;
            for (var i = 0; i < items.Length; i++)
            {
                if (array == null && ReferenceEquals(item, items[i]))
                {
                    array = new T[items.Length - 1];
                    Array.Copy(items, 0, array, 0, i);
                    continue;
                }

                if (array != null)
                    array[i - 1] = items[i];
            }

            if (array != null)
                items = array;
            return array != null;
        }

        #endregion
    }
}