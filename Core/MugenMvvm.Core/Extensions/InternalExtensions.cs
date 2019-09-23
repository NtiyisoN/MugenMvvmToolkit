﻿using System;
using System.Collections.Generic;
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

        internal static IWeakReference ToWeakReference(this object? item)
        {
            return MugenService.WeakReferenceProvider.GetWeakReference(item);
        }

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
            List<T>? result = null; //todo check
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

        internal static bool LazyInitialize<T>([EnsuresNotNull] ref T? item, T value) where T : class
        {
            return Interlocked.CompareExchange(ref item, value, null) == null;
        }

        internal static bool LazyInitializeDisposable<T>([EnsuresNotNull] ref T? item, T value) where T : class, IDisposable
        {
            if (!LazyInitialize(ref item, value))
            {
                value.Dispose();
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EqualsEx(this Type x, Type y) //note idkw but default implementation doesn't use ReferenceEquals before equals check
        {
            return ReferenceEquals(x, y) || x.Equals(y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EqualsEx(this MemberInfo x, MemberInfo y)
        {
            return ReferenceEquals(x, y) || x.Equals(y);
        }

        internal static void SetValue<TValue>(this PropertyInfo property, object target, TValue value)
        {
            property.SetValue(target, value, Default.EmptyArray<object>());
        }

        internal static void SetValue<TValue>(this FieldInfo field, object target, TValue value)
        {
            field.SetValue(target, value);
        }

        [Preserve]
        internal static void InitializeArray<T>(T[] target, object[] source)
        {
            for (var i = 0; i < target.Length; i++)
                target[i] = (T)source[i];
        }

        #endregion
    }
}