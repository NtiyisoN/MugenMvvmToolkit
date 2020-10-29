﻿using System;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using Should;

namespace MugenMvvm.UnitTests.Navigation.Internal
{
    public class TestNavigationEntryListener : INavigationEntryListener
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;

        #endregion

        #region Constructors

        public TestNavigationEntryListener(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public Action<INavigationEntry, IHasNavigationInfo?>? OnNavigationEntryAdded { get; set; }

        public Action<INavigationEntry, IHasNavigationInfo?>? OnNavigationEntryUpdated { get; set; }

        public Action<INavigationEntry, IHasNavigationInfo?>? OnNavigationEntryRemoved { get; set; }

        #endregion

        #region Implementation of interfaces

        void INavigationEntryListener.OnNavigationEntryAdded(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, IHasNavigationInfo? navigationInfo)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            OnNavigationEntryAdded?.Invoke(navigationEntry, navigationInfo);
        }

        void INavigationEntryListener.OnNavigationEntryUpdated(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, IHasNavigationInfo? navigationInfo)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            OnNavigationEntryUpdated?.Invoke(navigationEntry, navigationInfo);
        }

        void INavigationEntryListener.OnNavigationEntryRemoved(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, IHasNavigationInfo? navigationInfo)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            OnNavigationEntryRemoved?.Invoke(navigationEntry, navigationInfo);
        }

        #endregion
    }
}