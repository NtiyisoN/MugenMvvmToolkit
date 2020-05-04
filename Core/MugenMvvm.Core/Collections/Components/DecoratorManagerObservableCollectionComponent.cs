﻿using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Collections.Components
{
    public sealed class DecoratorManagerObservableCollectionComponent<T> : AttachableComponentBase<IObservableCollection<T>>,
        IObservableCollectionChangedListener<T>, IDecoratorManagerObservableCollectionComponent<T>, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = CollectionComponentPriority.DecoratorManager;

        #endregion

        #region Implementation of interfaces

        public IEnumerable<T> DecorateItems(IObservableCollectionDecorator<T>? decorator = null)
        {
            IEnumerable<T> items = Owner;
            var decorators = GetDecorators(decorator, out var startIndex, true);
            for (var i = 0; i < startIndex; i++)
                items = decorators[i].DecorateItems(items);

            return items;
        }

        public void OnItemChanged(IObservableCollectionDecorator<T>? decorator, T item, int index, object? args)
        {
            var decorators = GetDecorators(decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnItemChanged(ref item, ref index, ref args))
                    return;
            }

            GetComponents<IObservableCollectionChangedDecoratorListener<T>>().OnItemChanged(Owner, item, index, args);
        }

        public void OnAdded(IObservableCollectionDecorator<T>? decorator, T item, int index)
        {
            var decorators = GetDecorators(decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnAdded(ref item, ref index))
                    return;
            }

            GetComponents<IObservableCollectionChangedDecoratorListener<T>>().OnAdded(Owner, item, index);
        }

        public void OnReplaced(IObservableCollectionDecorator<T>? decorator, T oldItem, T newItem, int index)
        {
            var decorators = GetDecorators(decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnReplaced(ref oldItem, ref newItem, ref index))
                    return;
            }

            GetComponents<IObservableCollectionChangedDecoratorListener<T>>().OnReplaced(Owner, oldItem, newItem, index);
        }

        public void OnMoved(IObservableCollectionDecorator<T>? decorator, T item, int oldIndex, int newIndex)
        {
            var decorators = GetDecorators(decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnMoved(ref item, ref oldIndex, ref newIndex))
                    return;
            }

            GetComponents<IObservableCollectionChangedDecoratorListener<T>>().OnMoved(Owner, item, oldIndex, newIndex);
        }

        public void OnRemoved(IObservableCollectionDecorator<T>? decorator, T item, int index)
        {
            var decorators = GetDecorators(decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnRemoved(ref item, ref index))
                    return;
            }

            GetComponents<IObservableCollectionChangedDecoratorListener<T>>().OnRemoved(Owner, item, index);
        }

        public void OnReset(IObservableCollectionDecorator<T>? decorator, IEnumerable<T> items)
        {
            var decorators = GetDecorators(decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnReset(ref items))
                    return;
            }

            GetComponents<IObservableCollectionChangedDecoratorListener<T>>().OnReset(Owner, items);
        }

        public void OnCleared(IObservableCollectionDecorator<T>? decorator)
        {
            var decorators = GetDecorators(decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnCleared())
                    return;
            }

            GetComponents<IObservableCollectionChangedDecoratorListener<T>>().OnCleared(Owner);
        }

        void IObservableCollectionChangedListener<T>.OnItemChanged(IObservableCollection<T> collection, T item, int index, object? args)
        {
            OnItemChanged(null, item, index, args);
        }

        void IObservableCollectionChangedListener<T>.OnAdded(IObservableCollection<T> collection, T item, int index)
        {
            OnAdded(null, item, index);
        }

        void IObservableCollectionChangedListener<T>.OnReplaced(IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            OnReplaced(null, oldItem, newItem, index);
        }

        void IObservableCollectionChangedListener<T>.OnMoved(IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            OnMoved(null, item, oldIndex, newIndex);
        }

        void IObservableCollectionChangedListener<T>.OnRemoved(IObservableCollection<T> collection, T item, int index)
        {
            OnRemoved(null, item, index);
        }

        void IObservableCollectionChangedListener<T>.OnReset(IObservableCollection<T> collection, IEnumerable<T> items)
        {
            OnReset(null, items);
        }

        void IObservableCollectionChangedListener<T>.OnCleared(IObservableCollection<T> collection)
        {
            OnCleared(null);
        }

        #endregion

        #region Methods

        private IObservableCollectionDecorator<T>[] GetDecorators(IObservableCollectionDecorator<T>? decorator, out int index, bool isLengthDefault = false)
        {
            var components = GetComponents<IObservableCollectionDecorator<T>>();
            index = isLengthDefault ? components.Length : 0;
            if (decorator == null)
                return components;

            for (var i = 0; i < components.Length; i++)
            {
                if (ReferenceEquals(components[i], decorator))
                {
                    index = i;
                    if (!isLengthDefault)
                        ++index;
                    break;
                }
            }

            return components;
        }

        private TComponent[] GetComponents<TComponent>()
            where TComponent : class
        {
            return Owner.Components.Get<TComponent>();
        }

        #endregion
    }
}