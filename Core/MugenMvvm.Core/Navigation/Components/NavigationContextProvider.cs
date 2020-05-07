﻿using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Navigation.Components
{
    public sealed class NavigationContextProvider : INavigationContextProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationContextProvider(IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = NavigationComponentPriority.ContextProvider;

        #endregion

        #region Implementation of interfaces

        public INavigationContext? TryGetNavigationContext(INavigationProvider navigationProvider, string navigationOperationId,
            NavigationType navigationType, NavigationMode navigationMode, IReadOnlyMetadataContext? metadata = null)
        {
            return new NavigationContext(navigationProvider, navigationOperationId, navigationType, navigationMode, metadata, _metadataContextProvider);
        }

        #endregion
    }
}