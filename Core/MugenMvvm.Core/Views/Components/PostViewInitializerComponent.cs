﻿using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views.Components
{
    public sealed class PostViewInitializerComponent : IViewManagerListener, IHasPriority //todo listen wrappers from metadata
    {
        #region Fields

        private readonly IViewModelDispatcher? _viewModelDispatcher;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public PostViewInitializerComponent(IViewModelDispatcher? viewModelDispatcher = null)
        {
            _viewModelDispatcher = viewModelDispatcher;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public void OnViewModelCreated(IViewManager viewManager, IViewModelBase viewModel, object view, IMetadataContext metadata)
        {
        }

        public void OnViewCreated(IViewManager viewManager, object view, IViewModelBase viewModel, IMetadataContext metadata)
        {
        }

        public void OnViewInitialized(IViewManager viewManager, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            _viewModelDispatcher.ServiceIfNull().Subscribe(viewModel, viewInfo.View, ThreadExecutionMode.Main, metadata);
            (viewInfo.View as IInitializableView)?.Initialize(viewModel, viewInfo, metadata);
        }

        public void OnViewCleared(IViewManager viewManager, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            _viewModelDispatcher.ServiceIfNull().Unsubscribe(viewModel, viewInfo.View, metadata);
            (viewInfo.View as ICleanableView)?.Cleanup(metadata);
            viewInfo.ClearMetadata(true);
        }

        #endregion
    }
}