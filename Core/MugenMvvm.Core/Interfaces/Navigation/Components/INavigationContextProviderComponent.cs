﻿using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationContextProviderComponent : IComponent<INavigationDispatcher>
    {
        INavigationContext? TryGetNavigationContext(INavigationProvider navigationProvider, string navigationOperationId, 
            NavigationType navigationType, NavigationMode navigationMode, IReadOnlyMetadataContext? metadata);
    }
}