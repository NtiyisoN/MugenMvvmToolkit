﻿using System;
using System.Threading;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.UnitTest.Presenters.Internal
{
    public class TestViewModelPresenterMediator : IViewModelPresenterMediator, INavigationProvider
    {
        #region Properties

        public Func<object?, CancellationToken, IReadOnlyMetadataContext?, IPresenterResult?>? TryShow { get; set; }

        public Func<object?, CancellationToken, IReadOnlyMetadataContext?, IPresenterResult?>? TryClose { get; set; }

        public string Id { get; set; } = null!;

        public IViewMapping Mapping { get; set; } = null!;

        public IViewModelBase ViewModel { get; set; } = null!;

        public IView? View { get; set; } = null!;

        #endregion

        #region Implementation of interfaces

        IPresenterResult? IViewModelPresenterMediator.TryShow(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) => TryShow?.Invoke(view, cancellationToken, metadata);

        IPresenterResult? IViewModelPresenterMediator.TryClose(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) => TryClose?.Invoke(view, cancellationToken, metadata);

        #endregion
    }
}