﻿using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Messaging.Subscribers;

namespace MugenMvvm.Messaging.Components
{
    public sealed class ViewModelMessengerSubscriberComponent : IMessengerSubscriberComponent, IHasPriority
    {
        #region Fields

        public static readonly ViewModelMessengerSubscriberComponent Instance = new ViewModelMessengerSubscriberComponent();

        #endregion

        #region Properties

        public int Priority { get; set; } = MessengerComponentPriority.Subscriber;

        #endregion

        #region Implementation of interfaces

        public object? TryGetSubscriber(object subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata)
        {
            if (subscriber is IViewModelBase vm)
                return ViewModelMessengerSubscriber.TryGetSubscriber(vm, true);
            return null;
        }

        #endregion
    }
}