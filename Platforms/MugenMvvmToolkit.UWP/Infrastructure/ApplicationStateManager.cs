﻿#region Copyright

// ****************************************************************************
// <copyright file="ApplicationStateManager.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.UWP.Interfaces;
using MugenMvvmToolkit.UWP.Models;

namespace MugenMvvmToolkit.UWP.Infrastructure
{
    public class ApplicationStateManager : IApplicationStateManager
    {
        #region Fields

        private const string VmStateKey = "@`vmstate";
        private const string VmTypeKey = "@`vmtype";

        private static readonly Type[] KnownTypesStatic;

        private readonly ISerializer _serializer;
        private readonly IViewManager _viewManager;
        private readonly IViewModelPresenter _viewModelPresenter;
        private readonly IViewModelProvider _viewModelProvider;

        #endregion

        #region Constructors

        static ApplicationStateManager()
        {
            KnownTypesStatic = new[] { typeof(LazySerializableContainer), typeof(DataContext) };
        }

        [Preserve(Conditional = true)]
        public ApplicationStateManager([NotNull] ISerializer serializer, [NotNull] IViewModelProvider viewModelProvider,
            [NotNull] IViewManager viewManager, [NotNull] IViewModelPresenter viewModelPresenter)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(viewModelProvider, nameof(viewModelProvider));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(viewModelPresenter, nameof(viewModelPresenter));
            _serializer = serializer;
            _viewModelProvider = viewModelProvider;
            _viewManager = viewManager;
            _viewModelPresenter = viewModelPresenter;
        }

        #endregion

        #region Properties

        protected ISerializer Serializer => _serializer;

        protected IViewModelProvider ViewModelProvider => _viewModelProvider;

        protected IViewManager ViewManager => _viewManager;

        protected IViewModelPresenter ViewModelPresenter => _viewModelPresenter;

        #endregion

        #region Implementation of IApplicationStateManager

        public virtual IList<Type> KnownTypes => KnownTypesStatic;

        public void OnSaveState(FrameworkElement element, IDictionary<string, object> state, object args,
            IDataContext context = null)
        {
            Should.NotBeNull(element, nameof(element));
            Should.NotBeNull(state, nameof(state));
            var viewModel = element.DataContext as IViewModel;
            if (viewModel != null)
            {
                state[VmTypeKey] = viewModel.GetType().AssemblyQualifiedName;
                PreserveViewModel(viewModel, element, state, args, context ?? DataContext.Empty);
            }
        }

        public void OnLoadState(FrameworkElement element, IDictionary<string, object> state, object args,
            IDataContext context = null)
        {
            Should.NotBeNull(element, nameof(element));
            Should.NotBeNull(state, nameof(state));
            object value;
            if (!state.TryGetValue(VmTypeKey, out value))
                return;
            state.Remove(VmTypeKey);
            object dataContext = element.DataContext;
            Type vmType = Type.GetType(value as string, false);
            if (vmType == null || (dataContext != null && dataContext.GetType().Equals(vmType)))
                return;

            if (context == null)
                context = DataContext.Empty;
            var viewModelState = RestoreViewModelState(element, state, args, context);
            //The navigation is already handled.
            var eventArgs = args as NavigationEventArgs;
            if (eventArgs != null && eventArgs.GetHandled())
            {
                eventArgs.SetHandled(false);
                UwpToolkitExtensions.SetViewModelState(eventArgs.Content, viewModelState);
            }
            else
                RestoreViewModel(vmType, viewModelState, element, state, args, context);
        }

        #endregion

        #region Methods

        [NotNull]
        protected virtual IDataContext RestoreViewModelState([NotNull] FrameworkElement element, [NotNull] IDictionary<string, object> state,
             [NotNull] object args, [NotNull] IDataContext context)
        {
            object value;
            if (state.TryGetValue(VmStateKey, out value))
                return ((LazySerializableContainer)value).GetContext(_serializer);
            return DataContext.Empty;
        }

        protected virtual void RestoreViewModel([NotNull] Type viewModelType, [NotNull] IDataContext viewModelState, [NotNull] FrameworkElement element,
            [NotNull] IDictionary<string, object> state, [NotNull] object args, [NotNull] IDataContext context)
        {
            context = context.ToNonReadOnly();
            context.AddOrUpdate(InitializationConstants.ViewModelType, viewModelType);

#if WINDOWS_UWP
            context.Add(WindowPresenterConstants.RestoredView, element);
            context.Add(WindowPresenterConstants.IsViewOpened, true);
#endif
            IViewModel viewModel = _viewModelProvider.RestoreViewModel(viewModelState, context, true);
            context.AddOrUpdate(NavigationConstants.ViewModel, viewModel);
            _viewManager.InitializeViewAsync(viewModel, element, context);
            _viewModelPresenter.Restore(context);
        }

        protected virtual void PreserveViewModel([NotNull] IViewModel viewModel, [NotNull] FrameworkElement element,
             [NotNull] IDictionary<string, object> state, [NotNull] object args, [NotNull] IDataContext context)
        {
            state[VmStateKey] = new LazySerializableContainer(_serializer, _viewModelProvider.PreserveViewModel(viewModel, context));
        }

        #endregion
    }
}
