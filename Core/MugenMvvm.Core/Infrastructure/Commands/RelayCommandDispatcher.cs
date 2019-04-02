﻿using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Commands
{
    public class RelayCommandDispatcher : IRelayCommandDispatcher
    {
        #region Fields

        private IComponentCollection<IRelayCommandDispatcherListener>? _listeners;
        private IComponentCollection<IRelayCommandMediatorFactory>? _mediatorFactories;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public RelayCommandDispatcher(IExecutorRelayCommandMediatorFactory executorMediatorFactory, IComponentCollection<IRelayCommandMediatorFactory>? mediatorFactories = null,
            IComponentCollection<IRelayCommandDispatcherListener>? listeners = null)
        {
            Should.NotBeNull(executorMediatorFactory, nameof(executorMediatorFactory));
            ExecutorMediatorFactory = executorMediatorFactory;
            _mediatorFactories = mediatorFactories;
            _listeners = listeners;
        }

        #endregion

        #region Properties

        public IExecutorRelayCommandMediatorFactory ExecutorMediatorFactory { get; }

        public IComponentCollection<IRelayCommandMediatorFactory> MediatorFactories
        {
            get
            {
                if (_mediatorFactories == null)
                    MugenExtensions.LazyInitialize(ref _mediatorFactories, this);
                return _mediatorFactories;
            }
        }

        public IComponentCollection<IRelayCommandDispatcherListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    MugenExtensions.LazyInitialize(ref _listeners, this);
                return _listeners;
            }
        }

        #endregion

        #region Implementation of interfaces

        public IExecutorRelayCommandMediator GetExecutorMediator<TParameter>(IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers,
            IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(relayCommand, nameof(relayCommand));
            Should.NotBeNull(execute, nameof(execute));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetExecutorMediatorInternal<TParameter>(relayCommand, execute, canExecute, notifiers, metadata);
        }

        #endregion

        #region Methods

        protected virtual IExecutorRelayCommandMediator GetExecutorMediatorInternal<TParameter>(IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata)
        {
            var mediatorFactory = ExecutorMediatorFactory;
            Should.NotBeNull(mediatorFactory, nameof(ExecutorMediatorFactory));
            var mediators = GetMediatorsInternal<TParameter>(relayCommand, execute, canExecute, notifiers, metadata);
            var mediator = mediatorFactory.GetExecutorMediator<TParameter>(this, relayCommand, mediators, execute, canExecute, notifiers, metadata);

            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnMediatorCreated(this, relayCommand, execute, canExecute, notifiers, metadata, mediator);

            return mediator;
        }

        protected virtual IReadOnlyList<IRelayCommandMediator> GetMediatorsInternal<TParameter>(IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata)
        {
            List<IRelayCommandMediator>? result = null;
            var mediatorFactories = MediatorFactories.GetItems();
            for (var i = 0; i < mediatorFactories.Length; i++)
            {
                var mediators = mediatorFactories[i].GetMediators<TParameter>(this, relayCommand, execute, canExecute, notifiers, metadata);
                if (mediators == null || mediators.Count == 0)
                    continue;
                if (result == null)
                    result = new List<IRelayCommandMediator>();
                result.AddRange(mediators);
            }

            if (result == null)
                return Default.EmptyArray<IRelayCommandMediator>();
            result.Sort(HasPriorityComparer.Instance);
            return result;
        }

        protected IReadOnlyList<IRelayCommandDispatcherListener> GetListeners()
        {
            return _listeners?.GetItems() ?? Default.EmptyArray<IRelayCommandDispatcherListener>();
        }

        #endregion
    }
}