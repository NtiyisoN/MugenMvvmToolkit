﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.IoC;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetRawValue<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class?
            where TList : class?, IReadOnlyCollection<TItem>
        {
            return (object?)itemOrList.Item ?? itemOrList.List;
        }

        public static void Merge<TItem, TList>(this ItemOrList<TItem?, TList> itemOrList, ref TItem? currentItem, ref List<TItem>? items)
            where TItem : class
            where TList : class?, IReadOnlyCollection<TItem>
        {
            var list = itemOrList.List;
            var item = itemOrList.Item;
            if (item == null && list == null)
                return;

            if (list == null)
            {
                if (currentItem == null)
                    currentItem = item;
                else
                {
                    if (items == null)
                        items = new List<TItem>();
                    if (items.Count == 0)
                        items.Add(currentItem);
                    items.Add(item!);
                }
            }
            else
            {
                if (items == null)
                    items = new List<TItem>();
                if (currentItem != null && items.Count == 0)
                    items.Add(currentItem);
                items.AddRange(list);
            }
        }

        public static bool IsNullOrEmpty<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class?
            where TList : class?, IReadOnlyCollection<TItem>
        {
            return itemOrList.Item == null && itemOrList.List == null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this BatchUpdateCollectionMode value, BatchUpdateCollectionMode flag)
        {
            return (value & flag) == flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this BusyMessageHandlerType value, BusyMessageHandlerType flag)
        {
            return (value & flag) == flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this BindingFlags value, BindingFlags flag)
        {
            return (value & flag) == flag;
        }

        public static TTo ConvertGenericValue<TFrom, TTo>(TFrom value)
        {
            return ((Func<TFrom, TTo>)(object)GenericConverter<TFrom>.Convert).Invoke(value);
        }

        public static Task<IReadOnlyMetadataContext> CleanupAsync(this IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewInfo, nameof(viewInfo));
            return viewInfo.Initializer.CleanupAsync(viewInfo, viewModel, metadata);
        }

        public static bool MemberNameEqual(string changedMember, string listenedMember, bool emptyListenedMemberResult = false)
        {
            if (string.Equals(changedMember, listenedMember) || string.IsNullOrEmpty(changedMember))
                return true;
            if (string.IsNullOrEmpty(listenedMember))
                return emptyListenedMemberResult;

            if (listenedMember[0] == '[')
            {
                if (Default.IndexerName.Equals(changedMember))
                    return true;
                if (changedMember.StartsWith("Item[", StringComparison.Ordinal))
                {
                    int i = 4, j = 0;
                    while (i < changedMember.Length)
                    {
                        if (j >= listenedMember.Length)
                            return false;
                        var c1 = changedMember[i];
                        var c2 = listenedMember[j];
                        if (c1 == c2)
                        {
                            ++i;
                            ++j;
                        }
                        else if (c1 == '"')
                            ++i;
                        else if (c2 == '"')
                            ++j;
                        else
                            return false;
                    }

                    return j == listenedMember.Length;
                }
            }

            return false;
        }

        public static T[] GetItemsOrDefault<T>(this IComponentCollection<T>? componentCollection) where T : class
        {
            return componentCollection?.GetComponents() ?? Default.EmptyArray<T>();
        }

        [StringFormatMethod("format")]
        public static string Format(this string format, params object?[] args)
        {
            return string.Format(format, args);
        }

        [Pure]
        public static T GetService<T>(this IServiceProvider serviceProvider)
        {
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            return (T)serviceProvider.GetService(typeof(T));
        }

        [Pure]
        public static bool TryGetService<T>(this IServiceProvider serviceProvider, out T service)
        {
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            try
            {
                if (serviceProvider is IIocContainer container)
                {
                    if (container.TryGet(typeof(T), out var o))
                    {
                        service = (T)o!;
                        return true;
                    }

                    service = default!;
                    return false;
                }

                service = (T)serviceProvider.GetService(typeof(T));
                return true;
            }
            catch
            {
                service = default!;
                return false;
            }
        }

        public static bool TryGet<T>(this IIocContainer iocContainer, [NotNullWhen(true)] out T service, IReadOnlyMetadataContext? metadata = null)
        {
            var tryGet = iocContainer.TryGet(typeof(T), out var objService, metadata);
            if (tryGet)
            {
                service = (T)objService!;
                return true;
            }

            service = default!;
            return false;
        }

        public static bool TryGet(this IIocContainer iocContainer, Type serviceType, [NotNullWhen(true)] out object? service, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(iocContainer, nameof(iocContainer));
            Should.NotBeNull(serviceType, nameof(serviceType));
            if (iocContainer.CanResolve(serviceType, metadata))
            {
                try
                {
                    service = iocContainer.Get(serviceType, metadata);
                    return true;
                }
                catch
                {
                    service = null;
                    return false;
                }
            }

            service = null;
            return false;
        }

        #endregion

        #region Nested types

        private static class GenericConverter<T>
        {
            #region Fields

            public static readonly Func<T, T> Convert = arg => arg;

            #endregion
        }

        #endregion
    }
}