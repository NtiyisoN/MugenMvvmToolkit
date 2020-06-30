﻿using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    //todo implement tracking decorator (SelectedItem, SelectedIndex, Components)
    public interface IObservableCollectionDecorator<T> : IComponent<IObservableCollection<T>>
    {
        IEnumerable<T> DecorateItems(IEnumerable<T> items);

        bool OnItemChanged(ref T item, ref int index, ref object? args);

        bool OnAdded(ref T item, ref int index);

        bool OnReplaced(ref T oldItem, ref T newItem, ref int index);

        bool OnMoved(ref T item, ref int oldIndex, ref int newIndex);

        bool OnRemoved(ref T item, ref int index);

        bool OnReset(ref IEnumerable<T> items);

        bool OnCleared();
    }
}