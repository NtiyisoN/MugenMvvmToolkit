﻿using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Delegates;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal.Components
{
    public abstract class AttachedValueProviderBase : IAttachedValueProviderComponent
    {
        #region Implementation of interfaces

        public abstract bool IsSupported(object item, IReadOnlyMetadataContext? metadata);

        public virtual ItemOrList<KeyValuePair<string, object?>, IReadOnlyList<KeyValuePair<string, object?>>> GetValues(object item, Func<object, KeyValuePair<string, object?>, object?, bool>? predicate, object? state)
        {
            Should.NotBeNull(item, nameof(item));
            var dictionary = GetAttachedDictionary(item, true);
            if (dictionary == null)
                return default;
            lock (dictionary)
            {
                if (dictionary.Count == 0)
                    return default;

                if (predicate == null)
                {
                    if (dictionary.Count == 1)
                        return dictionary.FirstOrDefault();
                    return ItemOrList.FromListToReadOnly(new List<KeyValuePair<string, object?>>(dictionary));
                }

                ItemOrListEditor<KeyValuePair<string, object?>, List<KeyValuePair<string, object?>>> result = ItemOrListEditor.Get<KeyValuePair<string, object?>>(pair => pair.Key == null);
                foreach (var keyValue in dictionary)
                {
                    if (predicate(item, keyValue, state))
                        result.Add(keyValue);
                }

                return result.ToItemOrList<IReadOnlyList<KeyValuePair<string, object?>>>();
            }
        }

        public virtual bool TryGet(object item, string path, out object? value)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetAttachedDictionary(item, true);
            if (dictionary == null)
            {
                value = default!;
                return false;
            }

            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var result))
                {
                    value = result;
                    return true;
                }

                value = default!;
                return false;
            }
        }

        public virtual bool Contains(object item, string path)
        {
            Should.NotBeNull(item, nameof(item));
            var dictionary = GetAttachedDictionary(item, true);
            if (dictionary == null)
                return false;
            lock (dictionary)
            {
                return dictionary.ContainsKey(path);
            }
        }

        public virtual TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TValue addValue, in TState state, UpdateValueDelegate<TItem, TValue, TValue, TState, TValue> updateValueFactory) where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            var dictionary = GetAttachedDictionary(item, false)!;
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var value))
                {
                    value = BoxingExtensions.Box(updateValueFactory(item, addValue, (TValue)value!, state));
                    dictionary[path] = value;
                    return (TValue)value!;
                }

                dictionary.Add(path, BoxingExtensions.Box(addValue));
                return addValue;
            }
        }

        public virtual TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, in TState state, Func<TItem, TState, TValue> addValueFactory,
            UpdateValueDelegate<TItem, TValue, TState, TValue> updateValueFactory) where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(addValueFactory, nameof(addValueFactory));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            var dictionary = GetAttachedDictionary(item, false)!;
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var value))
                {
                    value = BoxingExtensions.Box(updateValueFactory(item, addValueFactory, (TValue)value!, state));
                    dictionary[path] = value;
                    return (TValue)value!;
                }

                value = BoxingExtensions.Box(addValueFactory(item, state));
                dictionary.Add(path, value);
                return (TValue)value!;
            }
        }

        public virtual object? GetOrAdd(object item, string path, Func<object, object?, object?> valueFactory, object? state)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(valueFactory, nameof(valueFactory));
            var dictionary = GetAttachedDictionary(item, false)!;
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var oldValue))
                    return oldValue;
                oldValue = valueFactory(item, state);
                dictionary.Add(path, oldValue);
                return oldValue;
            }
        }


        public virtual object? GetOrAdd(object item, string path, object? value)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetAttachedDictionary(item, false)!;
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var oldValue))
                    return oldValue;
                dictionary.Add(path, value);
                return value;
            }
        }

        public virtual void Set(object item, string path, object? value, out object? oldValue)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetAttachedDictionary(item, false)!;
            lock (dictionary)
            {
                dictionary.TryGetValue(path, out oldValue);
                dictionary[path] = value;
            }
        }

        public virtual bool Clear(object item, string path, out object? oldValue)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetAttachedDictionary(item, true);
            if (dictionary == null)
            {
                oldValue = null;
                return false;
            }

            bool clear;
            bool removed;
            lock (dictionary)
            {
                removed = dictionary.TryGetValue(path!, out oldValue) && dictionary.Remove(path!);
                clear = removed && dictionary.Count == 0;
            }

            if (clear)
                return ClearInternal(item);
            return removed;
        }

        public virtual bool Clear(object item)
        {
            Should.NotBeNull(item, nameof(item));
            return ClearInternal(item);
        }

        #endregion

        #region Methods

        protected abstract IDictionary<string, object?>? GetAttachedDictionary(object item, bool optional);

        protected abstract bool ClearInternal(object item);

        #endregion
    }
}