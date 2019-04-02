﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IAttachedValueProvider //todo add extensions
    {
        IComponentCollection<IChildAttachedValueProvider> Providers { get; }

        TValue AddOrUpdate<TItem, TValue, TState1, TState2>(TItem item, string path, TValue addValue, TState1 state1, TState2 state2,
            UpdateValueDelegate<TItem, TValue, TValue, TState1, TState2> updateValueFactory)
            where TItem : class;

        TValue AddOrUpdate<TItem, TValue, TState1, TState2>(TItem item, string path, TState1 state1, TState2 state2, Func<TItem, TState1, TState2, TValue> addValueFactory,
            UpdateValueDelegate<TItem, Func<TItem, TState1, TState2, TValue>, TValue, TState1, TState2> updateValueFactory)
            where TItem : class;

        TValue GetOrAdd<TValue>(object item, string path, TValue value);

        TValue GetOrAdd<TItem, TValue, TState1, TState2>(TItem item, string path, TState1 state1, TState2 state2, Func<TItem, TState1, TState2, TValue> valueFactory)
            where TItem : class;

        bool TryGetValue<TValue>(object item, string path, [NotNullWhenTrue] out TValue value);

        void SetValue(object item, string path, object? value);

        bool Contains(object item, string path);

        IReadOnlyList<KeyValuePair<string, object?>> GetValues(object item, Func<string, object?, bool>? predicate);

        bool Clear(object item);

        bool Clear(object item, string path);
    }
}