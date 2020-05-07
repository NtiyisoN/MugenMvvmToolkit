﻿using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Navigation.Components
{
    public sealed class NavigationEntryProvider : INavigationEntryProviderComponent, INavigationDispatcherNavigatedListener, IHasPriority
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private readonly Dictionary<NavigationType, List<INavigationEntry>> _navigationEntries;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationEntryProvider(IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadataContextProvider = metadataContextProvider;
            _navigationEntries = new Dictionary<NavigationType, List<INavigationEntry>>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = NavigationComponentPriority.EntryProvider;

        #endregion

        #region Implementation of interfaces

        public void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            INavigationEntry? addedEntry = null;
            INavigationEntry? updatedEntry = null;
            INavigationEntry? removedEntry = null;
            lock (_navigationEntries)
            {
                if (navigationContext.NavigationMode.IsRefresh || navigationContext.NavigationMode.IsNew)
                {
                    if (!_navigationEntries.TryGetValue(navigationContext.NavigationType, out var list))
                    {
                        list = new List<INavigationEntry>();
                        _navigationEntries[navigationContext.NavigationType] = list;
                    }

                    updatedEntry = FindEntry(list, navigationContext.NavigationOperationId);
                    if (updatedEntry == null)
                    {
                        addedEntry = new NavigationEntry(navigationContext.NavigationProvider, navigationContext.NavigationOperationId,
                            navigationContext.NavigationType, _metadataContextProvider.DefaultIfNull().GetMetadataContext(this, navigationContext.Metadata));
                        list.Add(addedEntry);
                    }
                }
                if (navigationContext.NavigationMode.IsClose)
                {
                    if (_navigationEntries.TryGetValue(navigationContext.NavigationType, out var list))
                    {
                        removedEntry = FindEntry(list, navigationContext.NavigationOperationId);
                        if (removedEntry != null)
                            list.Remove(removedEntry);
                    }
                }
            }

            if (addedEntry != null)
            {
                navigationDispatcher
                    .GetComponents<INavigationDispatcherEntryListener>(navigationContext.GetMetadataOrDefault())
                    .OnNavigationEntryAdded(navigationDispatcher, addedEntry, navigationContext);
            }
            else if (updatedEntry != null)
            {
                navigationDispatcher
                    .GetComponents<INavigationDispatcherEntryListener>(navigationContext.GetMetadataOrDefault())
                    .OnNavigationEntryUpdated(navigationDispatcher, updatedEntry, navigationContext);
            }
            else if (removedEntry != null)
            {
                navigationDispatcher
                    .GetComponents<INavigationDispatcherEntryListener>(navigationContext.GetMetadataOrDefault())
                    .OnNavigationEntryRemoved(navigationDispatcher, removedEntry, navigationContext);
            }
        }

        public IReadOnlyList<INavigationEntry>? TryGetNavigationEntries(NavigationType? type, IReadOnlyMetadataContext? metadata)
        {
            List<INavigationEntry>? result = null;
            lock (_navigationEntries)
            {
                if (type == null)
                {
                    foreach (var t in _navigationEntries)
                        AddNavigationEntries(t.Key, ref result);
                }
                else
                    AddNavigationEntries(type, ref result);
            }

            return result;
        }

        #endregion

        #region Methods

        private void AddNavigationEntries(NavigationType type, ref List<INavigationEntry>? result)
        {
            if (_navigationEntries.TryGetValue(type, out var list))
            {
                if (result == null)
                    result = new List<INavigationEntry>(list);
                else
                    result.AddRange(list);
            }
        }

        private static INavigationEntry? FindEntry(List<INavigationEntry> entries, string id)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].NavigationOperationId == id)
                    return entries[i];
            }

            return null;
        }

        #endregion
    }
}