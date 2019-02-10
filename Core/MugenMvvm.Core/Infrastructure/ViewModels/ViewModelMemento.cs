﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Infrastructure.Serialization;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;

namespace MugenMvvm.Infrastructure.ViewModels
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    [Preserve(Conditional = true, AllMembers = true)]
    public class ViewModelMemento : IMemento
    {
        #region Fields

        protected static readonly object RestorationLocker;

        [IgnoreDataMember, XmlIgnore, NonSerialized]
        private IViewModelBase? _viewModel;

        [DataMember(Name = "B")]
        protected internal IList<IBusyIndicatorProviderListener?>? BusyListeners;

        [DataMember(Name = "C")]
        protected internal IObservableMetadataContext? Metadata;

        [DataMember(Name = "S")]
        protected internal IList<MessengerSubscriberInfo>? Subscribers;

        [DataMember(Name = "T")]
        protected internal Type? ViewModelType;

        [DataMember(Name = "N")]
        protected internal bool NoState;

        #endregion

        #region Constructors

        static ViewModelMemento()
        {
            RestorationLocker = new object();
        }

        internal ViewModelMemento()
        {
        }

        public ViewModelMemento(IViewModelBase viewModel)
        {
            _viewModel = viewModel;
            Metadata = viewModel.Metadata;
            ViewModelType = viewModel.GetType();
        }

        #endregion

        #region Properties

        [IgnoreDataMember, XmlIgnore]
        public Type TargetType => ViewModelType!;

        #endregion

        #region Implementation of interfaces

        public void Preserve(ISerializationContext serializationContext)
        {
            if (_viewModel == null)
                return;
            if (_viewModel.Metadata.Get(ViewModelMetadata.NoState))
            {
                NoState = true;
                Metadata = null;
                Subscribers = null;
                BusyListeners = null;
            }
            else
            {
                NoState = false;
                Metadata = _viewModel.Metadata;
                if (_viewModel is IHasService<IMessenger> hasMessenger)
                    Subscribers = hasMessenger.Service.GetSubscribers().ToSerializable(serializationContext.Serializer);
                if (_viewModel is IHasService<IBusyIndicatorProvider> hasBusyIndicatorProvider)
                    BusyListeners = hasBusyIndicatorProvider.Service.GetListeners().ToSerializable(serializationContext.Serializer);
            }

            OnPreserveInternal(_viewModel!, serializationContext);
        }

        public IMementoResult Restore(ISerializationContext serializationContext)
        {
            if (NoState)
                return MementoResult.Unrestored;

            Should.NotBeNull(Metadata, nameof(Metadata));
            Should.NotBeNull(ViewModelType, nameof(ViewModelType));
            if (_viewModel != null)
                return new MementoResult(_viewModel, serializationContext);

            var dispatcher = serializationContext.ServiceProvider.GetService<IViewModelDispatcher>();
            lock (RestorationLocker)
            {
                if (_viewModel != null)
                    return new MementoResult(_viewModel, serializationContext);

                if (!serializationContext.Metadata.Get(SerializationMetadata.NoCache) && Metadata.TryGet(ViewModelMetadata.Id, out var id))
                {
                    _viewModel = dispatcher.TryGetViewModel(id, serializationContext.Metadata);
                    if (_viewModel != null)
                        return new MementoResult(_viewModel, serializationContext);
                }

                _viewModel = RestoreInternal(serializationContext);
                dispatcher.OnLifecycleChanged(_viewModel, ViewModelLifecycleState.Restoring, serializationContext.Metadata);
                RestoreInternal(_viewModel);
                OnRestoringInternal(_viewModel, serializationContext);
                dispatcher.OnLifecycleChanged(_viewModel, ViewModelLifecycleState.Restored, serializationContext.Metadata);
                return new MementoResult(_viewModel, serializationContext);
            }
        }

        #endregion

        #region Methods

        protected virtual void OnPreserveInternal(IViewModelBase viewModel, ISerializationContext serializationContext)
        {
        }

        protected virtual IViewModelBase RestoreInternal(ISerializationContext serializationContext)
        {
            return (IViewModelBase)serializationContext.ServiceProvider.GetService(ViewModelType);
        }

        protected virtual void OnRestoringInternal(IViewModelBase viewModel, ISerializationContext serializationContext)
        {
        }

        private void RestoreInternal(IViewModelBase viewModel)
        {
            var listeners = Metadata.GetListeners();
            foreach (var listener in listeners)
                viewModel.Metadata.AddListener(listener);
            viewModel.Metadata.Merge(Metadata);

            if (BusyListeners != null && viewModel is IHasService<IBusyIndicatorProvider> hasBusyIndicatorProvider)
            {
                foreach (var busyListener in BusyListeners)
                {
                    if (busyListener != null)
                        hasBusyIndicatorProvider.Service.AddListener(busyListener);
                }
            }

            if (Subscribers != null && viewModel is IHasService<IMessenger> hasMessenger)
            {
                foreach (var subscriber in Subscribers)
                {
                    if (subscriber.Subscriber != null)
                        hasMessenger.Service.Subscribe(subscriber.Subscriber, subscriber.ExecutionMode);
                }
            }
        }

        #endregion
    }
}