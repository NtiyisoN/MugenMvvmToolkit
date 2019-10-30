﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Serialization;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        internal static void ReleaseWeakReference(this IValueHolder<IWeakReference> valueHolder)
        {
            valueHolder.Value?.Release();
        }

        internal static IWeakReference ToWeakReference(this object? item)
        {
            return MugenService.WeakReferenceProvider.GetWeakReference(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T ServiceIfNull<T>(this T? service) where T : class
        {
            return service ?? Service<T>.Instance;
        }

        internal static void TrySetExceptionEx<T>(this TaskCompletionSource<T> tcs, Exception e)
        {
            if (e is AggregateException aggregateException)
                tcs.TrySetException(aggregateException.InnerExceptions);
            else
                tcs.SetException(e);
        }

        internal static List<T>? ToSerializable<T>(this IReadOnlyList<T>? items, ISerializer serializer, int? size = null)
        {
            if (items == null)
                return null;
            List<T>? result = null;
            for (var i = 0; i < size.GetValueOrDefault(items.Count); i++)
            {
                var item = items[i];
                if (item != null && serializer.CanSerialize(item.GetType()))
                {
                    if (result == null)
                        result = new List<T>();
                    result.Add(item);
                }
            }

            return result;
        }

        internal static bool LazyInitialize<T>([NotNullIfNotNull("value")] ref T? item, T value) where T : class
        {
            return Interlocked.CompareExchange(ref item, value, null) == null;
        }

        internal static bool LazyInitializeDisposable<T>([NotNullIfNotNull("value")] ref T? item, T value) where T : class, IDisposable
        {
            if (!LazyInitialize(ref item, value))
            {
                value.Dispose();
                return false;
            }

            return true;
        }

        internal static void SetValue<TValue>(this PropertyInfo property, object target, TValue value)
        {
            property.SetValue(target, value, Default.EmptyArray<object>());
        }

        internal static void SetValue<TValue>(this FieldInfo field, object target, TValue value)
        {
            field.SetValue(target, value);
        }

        [Preserve(Conditional = true)]
        internal static void InitializeArray<T>(T[] target, object[] source)
        {
            for (var i = 0; i < target.Length; i++)
                target[i] = (T)source[i];
        }

        #endregion
    }
}