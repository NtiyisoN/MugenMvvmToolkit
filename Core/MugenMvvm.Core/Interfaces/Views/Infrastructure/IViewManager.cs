﻿using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewManager : IHasListeners<IViewManagerListener>
    {
        void AddManager(IChildViewManager manager);

        void RemoveManager(IChildViewManager manager);

        IReadOnlyList<IChildViewManager> GetManagers();

        IReadOnlyList<IViewInfo> GetViews(IViewModelBase viewModel, IReadOnlyMetadataContext metadata);

        IReadOnlyList<IViewModelViewInitializer> GetInitializersByView(object view, IReadOnlyMetadataContext metadata);

        IReadOnlyList<IViewInitializer> GetInitializersByViewModel(IViewModelBase viewModel, IReadOnlyMetadataContext metadata);        
    }
}