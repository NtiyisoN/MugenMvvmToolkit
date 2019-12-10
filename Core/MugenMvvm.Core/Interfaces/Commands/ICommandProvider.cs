﻿using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands
{
    public interface ICommandProvider : IComponentOwner<ICommandProvider>, IComponent<IMugenApplication>
    {
        ICompositeCommand GetCommand<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata = null);
    }
}