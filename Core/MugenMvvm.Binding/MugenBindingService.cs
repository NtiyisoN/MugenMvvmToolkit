﻿using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Resources;

namespace MugenMvvm.Binding
{
    public static class MugenBindingService
    {
        #region Properties

        public static IGlobalValueConverter GlobalValueConverter => Service<IGlobalValueConverter>.Instance;

        public static IBindingManager BindingManager => Service<IBindingManager>.Instance;

        public static IBindingMemberProvider BindingMemberProvider => Service<IBindingMemberProvider>.Instance;

        public static IBindingObserverProvider BindingObserverProvider => Service<IBindingObserverProvider>.Instance;

        public static IResourceResolver ResourceResolver => Service<IResourceResolver>.Instance;

        #endregion
    }
}