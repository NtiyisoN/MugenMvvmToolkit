﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Infrastructure.Serialization;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.ViewModels;

namespace MugenMvvm
{
    public static class Default
    {
        #region Fields

        internal static readonly PropertyChangedEventArgs EmptyPropertyChangedArgs;
        internal static readonly PropertyChangedEventArgs IsNotificationsSuspendedChangedArgs;
        internal static readonly PropertyChangedEventArgs IsBusyChangedArgs;
        internal static readonly PropertyChangedEventArgs BusyInfoChangedArgs;
        internal static readonly PropertyChangedEventArgs DisplayNameChangedArgs;
        internal static readonly PropertyChangedEventArgs IsCanExecuteNullChangedArgs;
        internal static readonly PropertyChangedEventArgs IsCanExecuteLastChangedArgs;


        public static readonly NullValue SerializableNullValue;
        public static readonly IReadOnlyMetadataContext MetadataContext;
        public static readonly IDisposable Disposable;
        public static readonly WeakReference WeakReference;
        public static readonly Task CompletedTask;
        public static readonly Task<bool> TrueTask;
        public static readonly Task<bool> FalseTask;

        #endregion

        #region Constructors

        static Default()
        {
            EmptyPropertyChangedArgs = new PropertyChangedEventArgs(string.Empty);
            IsNotificationsSuspendedChangedArgs = new PropertyChangedEventArgs(nameof(ISuspendNotifications.IsNotificationsSuspended));
            IsBusyChangedArgs = new PropertyChangedEventArgs(nameof(ViewModelBase.IsBusy));
            BusyInfoChangedArgs = new PropertyChangedEventArgs(nameof(ViewModelBase.BusyInfo));
            DisplayNameChangedArgs = new PropertyChangedEventArgs(nameof(IHasDisplayName.DisplayName));
            IsCanExecuteNullChangedArgs = new PropertyChangedEventArgs(nameof(IBindableRelayCommandMediator.IsCanExecuteNullParameter));
            IsCanExecuteLastChangedArgs = new PropertyChangedEventArgs(nameof(IBindableRelayCommandMediator.IsCanExecuteLastParameter));


            MetadataContext = new EmptyContext();
            SerializableNullValue = new NullValue();
            WeakReference = new WeakReference(null, false);
            Disposable = (IDisposable)MetadataContext;
            TrueTask = Task.FromResult(true);
            FalseTask = Task.FromResult(false);
            CompletedTask = FalseTask;
        }

        #endregion

        #region Methods

        public static T[] EmptyArray<T>()
        {
            return Value<T>.ArrayInstance;
        }

        public static Task<T> CanceledTask<T>()
        {
            return Value<T>.CanceledTaskField;
        }

        internal static PropertyChangedEventArgs GetOrCreatePropertyChangedArgs(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return EmptyPropertyChangedArgs;
            return new PropertyChangedEventArgs(propertyName);
        }

        #endregion

        #region Nested types

        [DataContract]
        public sealed class NullValue
        {
            #region Constructors

            internal NullValue()
            {
            }

            #endregion

            #region Methods

            public object To(object? value)
            {
                if (value == null)
                    return SerializableNullValue;
                return value;
            }

            public object? From(object? value)
            {
                if (value is NullValue)
                    return null;
                return value;
            }

            #endregion
        }

        private sealed class EmptyContext : IReadOnlyMetadataContext, IDisposable
        {
            #region Fields

            private static IMemento _emptyMemento;

            #endregion

            #region Properties

            public int Count => 0;

            #endregion

            #region Implementation of interfaces

            public void Dispose()
            {
            }

            public IEnumerator<MetadataContextValue> GetEnumerator()
            {
                return Enumerable.Empty<MetadataContextValue>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IMemento? GetMemento()
            {
                if (_emptyMemento == null)
                    _emptyMemento = new EmptyContextMemento();
                return _emptyMemento;
            }

            public bool TryGet(IMetadataContextKey contextKey, out object? value, object? defaultValue = null)
            {
                value = contextKey.GetDefaultValue(this, defaultValue);
                return false;
            }

            public bool TryGet<T>(IMetadataContextKey<T> contextKey, out T value, T defaultValue = default)
            {
                value = contextKey.GetDefaultValue(this, defaultValue);
                return false;
            }

            public bool Contains(IMetadataContextKey contextKey)
            {
                return false;
            }

            #endregion
        }

        [Serializable]
        [DataContract(Namespace = BuildConstants.DataContractNamespace)]
        [Preserve(Conditional = true, AllMembers = true)]
        private sealed class EmptyContextMemento : IMemento
        {
            #region Properties

            [IgnoreDataMember]
            public Type TargetType => typeof(EmptyContext);

            #endregion

            #region Implementation of interfaces

            public void Preserve(ISerializationContext serializationContext)
            {
            }

            public IMementoResult Restore(ISerializationContext serializationContext)
            {
                return new MementoResult(MetadataContext, serializationContext.Metadata);
            }

            #endregion
        }

        private static class Value<T>
        {
            #region Fields

            public static readonly T[] ArrayInstance;
            public static readonly Task<T> CanceledTaskField;

            #endregion

            #region Constructors

            static Value()
            {
                ArrayInstance = new T[0];
                var tcs = new TaskCompletionSource<T>();
                tcs.SetCanceled();
                CanceledTaskField = tcs.Task;
            }

            #endregion
        }

        #endregion
    }
}