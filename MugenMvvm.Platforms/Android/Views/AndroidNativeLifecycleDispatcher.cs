﻿using Java.Lang;
using MugenMvvm.Android.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Requests;

namespace MugenMvvm.Android.Views
{
    public sealed class AndroidNativeLifecycleDispatcher : Object, IAndroidNativeLifecycleDispatcher
    {
        #region Fields

        private readonly IViewManager? _viewManager;

        #endregion

        #region Constructors

        public AndroidNativeLifecycleDispatcher(IViewManager? viewManager = null)
        {
            _viewManager = viewManager;
        }

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(Object target, int lifecycleState, Object? state)
        {
            var viewLifecycleState = AndroidViewLifecycleState.TryParseNativeChanged(lifecycleState);
            if (viewLifecycleState != null)
                _viewManager.DefaultIfNull().OnLifecycleChanged(target, viewLifecycleState, state);
        }

        public bool OnLifecycleChanging(Object target, int lifecycleState, Object? state)
        {
            var viewLifecycleState = AndroidViewLifecycleState.TryParseNativeChanging(lifecycleState);
            if (viewLifecycleState != null)
            {
                var request = new CancelableRequest(false, state);
                _viewManager.DefaultIfNull().OnLifecycleChanged(target, viewLifecycleState, request);
                return !request.Cancel;
            }

            return true;
        }

        #endregion
    }
}