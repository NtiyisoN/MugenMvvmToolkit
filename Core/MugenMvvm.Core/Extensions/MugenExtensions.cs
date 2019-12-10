﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvm.Attributes;
using MugenMvvm.Commands;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Validation;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static ICompositeCommand GetCommand(this ICommandProvider? mediatorProvider, Action execute, Func<bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return GetCommandInternal<object>(mediatorProvider, execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand GetCommand<T>(this ICommandProvider? mediatorProvider, Action<T> execute, Func<T, bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return GetCommandInternal<T>(mediatorProvider, execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand GetCommand(this ICommandProvider? mediatorProvider, Func<Task> execute, Func<bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return GetCommandInternal<object>(mediatorProvider, execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand GetCommand<T>(this ICommandProvider? mediatorProvider, Func<T, Task> execute, Func<T, bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return GetCommandInternal<T>(mediatorProvider, execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static bool AddValidator(this IAggregatorValidator aggregatorValidator, IValidator validator, IReadOnlyMetadataContext? metadata = null)
        {
            return aggregatorValidator.AddComponent(validator, metadata);
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

        public static TTo CastGeneric<TFrom, TTo>(TFrom value)
        {
            if (typeof(TFrom) == typeof(TTo))
                return ((Func<TFrom, TTo>)(object)GenericCaster<TFrom>.Cast).Invoke(value);
            return (TTo)(object)value!;
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

        [StringFormatMethod("format")]
        public static string Format(this string format, params object?[] args)
        {
            return string.Format(format, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Execute<TState>(this IThreadDispatcherHandler<TState> handler, object state)
        {
            handler.Execute((TState)state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Invoke<TState>(this Action<TState> handler, object state)
        {
            handler.Invoke((TState)state);
        }

        internal static void ReleaseWeakReference(this IValueHolder<IWeakReference> valueHolder)
        {
            valueHolder.Value?.Release();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IWeakReference ToWeakReference(this object? item)
        {
            return MugenService.WeakReferenceProvider.GetWeakReference(item);
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

        [Preserve(Conditional = true)]
        internal static void InitializeArray<T>(T[] target, object[] source)
        {
            for (var i = 0; i < target.Length; i++)
                target[i] = (T)source[i];
        }

        private static ICompositeCommand GetCommandInternal<T>(ICommandProvider? mediatorProvider, Delegate execute, Delegate? canExecute, bool? allowMultipleExecution, CommandExecutionMode? executionMode,
            ThreadExecutionMode? eventThreadMode, IReadOnlyList<object>? notifiers, Func<object, bool>? canNotify, IReadOnlyMetadataContext? metadata)
        {
            var request = new DelegateCommandRequest((in DelegateCommandRequest r, DelegateCommandRequest.IProvider provider, IReadOnlyMetadataContext? m) => provider.TryGetCommand<T>(r, m),
                execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify);
            return mediatorProvider.DefaultIfNull().GetCommand(request, metadata);
        }

        #endregion

        #region Nested types

        private static class GenericCaster<T>
        {
            #region Fields

            public static readonly Func<T, T> Cast = arg => arg;

            #endregion
        }

        #endregion
    }
}