﻿#region Copyright

// ****************************************************************************
// <copyright file="OrderedNotifiableCollectionTest.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestInfrastructure;
using Should;

namespace MugenMvvmToolkit.Test.Collections
{
    [TestClass]
    public class OrderedNotifiableCollectionTest : SynchronizedNotifiableCollectionTest
    {
        private static OrderedNotifiableCollection<Item> CreateNotifiableCollection(ExecutionMode executionMode,
            IThreadManager threadManager, IEnumerable<Item> items = null)
        {
            if (items == null)
                items = Enumerable.Empty<Item>();
            return new OrderedNotifiableCollection<Item>(items, (item, item1) => item.Id.CompareTo(item1.Id))
            {
                ThreadManager = threadManager
            };
        }

        #region Overrides of CollectionTestBase

        [TestMethod]
        public void CollectionShouldReorder()
        {
            ThreadManagerMock.IsUiThread = true;
            ThreadManagerMock.ImmediateInvokeOnUiThread = true;
            var src = new int[] { 5, 2, 4, 1, 3 };
            bool orderAsc = true;
            var collection = new OrderedNotifiableCollection<int>((i, i1) =>
                {
                    if (orderAsc)
                        return i.CompareTo(i1);
                    return i1.CompareTo(i);
                })
            { ThreadManager = ThreadManagerMock };
            collection.AddRange(src);
            collection.SequenceEqual(src.OrderBy(i => i)).ShouldBeTrue();

            orderAsc = false;
            int count = 0;
            collection.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Reset)
                    ++count;
            };
            collection.Reorder();
            collection.SequenceEqual(src.OrderByDescending(i => i)).ShouldBeTrue();
            count.ShouldEqual(1);
        }

        [TestMethod]
        public override void CollectionShouldTrackChangesCorrect()
        {
            const int count = 10;
            SynchronizedNotifiableCollection<Item> collection = CreateNotifiableCollection<Item>(ExecutionMode.None,
                ThreadManagerMock);
            var collectionTracker = new NotifiableCollectionTracker<Item>(collection);
            var items = new[] { new Item(), new Item(), new Item() };
            var items2 = new[] { new Item(), new Item(), new Item() };
            using (collection.SuspendNotifications())
            {
                for (int i = 0; i < count; i++)
                {
                    collection.AddRange(items);
                    collection.AddRange(items2);
                    collection.RemoveRange(items);
                }
            }
            ThreadManagerMock.InvokeOnUiThreadAsync();
            collectionTracker.AssertEquals();
            collection.Count.ShouldEqual(count * 3);
        }

        protected override SynchronizedNotifiableCollection<T> CreateNotifiableCollection<T>(ExecutionMode executionMode,
            IThreadManager threadManager)
        {
            Should.BeOfType(typeof(Item), "type", typeof(T));
            return (SynchronizedNotifiableCollection<T>)(object)CreateNotifiableCollection(executionMode, threadManager);
        }

        protected override ICollection<T> CreateCollection<T>(params T[] items)
        {
            Should.BeOfType(typeof(Item), "type", typeof(T));
            return (ICollection<T>)CreateNotifiableCollection(ExecutionMode.None, null, items.OfType<Item>());
        }

        #endregion
    }

    [TestClass]
    public class OrderedNotifiableCollectionSerializationTest :
        SerializationTestBase<OrderedNotifiableCollection<string>>
    {
        #region Overrides of SerializationTestBase

        protected override OrderedNotifiableCollection<string> GetObject()
        {
            return new OrderedNotifiableCollection<string>(TestExtensions.TestStrings);
        }

        protected override void AssertObject(OrderedNotifiableCollection<string> deserializedObj)
        {
            deserializedObj.Items.ShouldBeType<OrderedListInternal<string>>();
            deserializedObj.SequenceEqual(TestExtensions.TestStrings).ShouldBeTrue();
            deserializedObj.IsNotificationsSuspended.ShouldBeFalse();
        }

        #endregion
    }
}
