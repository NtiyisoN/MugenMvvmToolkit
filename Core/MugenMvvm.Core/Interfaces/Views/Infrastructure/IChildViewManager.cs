﻿using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IChildViewManager
    {
        int Priority { get; }

        IReadOnlyList<IViewInfo> GetViews(IParentViewManager parentViewManager, IViewModelBase viewModel, IReadOnlyMetadataContext metadata);

        IReadOnlyList<IViewModelViewInitializer> GetInitializersByView(IParentViewManager parentViewManager, object view, IReadOnlyMetadataContext metadata);

        IReadOnlyList<IViewInitializer> GetInitializersByViewModel(IParentViewManager parentViewManager, IViewModelBase viewModel, IReadOnlyMetadataContext metadata);

        IViewManagerResult<IViewInfo>? TryInitialize(IParentViewManager parentViewManager, IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata);
    }
}