﻿using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Components
{
    public interface IViewModelProviderComponent : IComponent<IViewModelManager>
    {
        IViewModelBase? TryGetViewModel(IReadOnlyMetadataContext metadata);
    }
}