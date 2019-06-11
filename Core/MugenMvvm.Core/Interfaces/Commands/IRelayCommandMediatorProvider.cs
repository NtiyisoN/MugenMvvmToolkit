﻿using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IRelayCommandMediatorProvider : IHasListeners<IRelayCommandMediatorProviderListener>
    {
        IComponentCollection<IChildRelayCommandMediatorProvider> Providers { get; }

        IExecutorRelayCommandMediator GetExecutorMediator<TParameter>(IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata);
    }
}