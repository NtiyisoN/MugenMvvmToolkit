﻿#region Copyright

// ****************************************************************************
// <copyright file="TwoWayBindingMode.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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

using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Models.EventArg;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    /// <summary>
    ///     Causes changes to either the source property or the target property to automatically update the other. This type of
    ///     binding is appropriate for editable forms or other fully-interactive UI scenarios.
    /// </summary>
    public sealed class TwoWayBindingMode : BindingModeBase
    {
        #region Overrides of BindingBehaviorBase

        /// <summary>
        ///     Attaches to the specified binding.
        /// </summary>
        protected override bool OnAttached()
        {
            SubscribeSources();
            if (!Binding.SourceAccessor.IsAllMembersAvailable() || !Binding.UpdateTarget())
                Binding.TargetAccessor.Source.ValueChanged += OneTimeTargetHandler;
            SubscribeTarget();
            return true;
        }

        /// <summary>
        ///     Detaches this instance from its associated binding.
        /// </summary>
        protected override void OnDetached()
        {
            UnsubscribeSources();
            UnsubscribeTarget();
        }

        /// <summary>
        ///     Creates a new binding behavior that is a copy of the current instance.
        /// </summary>
        protected override IBindingBehavior CloneInternal()
        {
            return new TwoWayBindingMode();
        }

        #endregion

        #region Methods

        private void OneTimeTargetHandler(IBindingSource sender, ValueChangedEventArgs args)
        {
            IDataBinding binding = Binding;
            if (binding != null && binding.SourceAccessor.IsAllMembersAvailable() && binding.UpdateTarget())
                binding.TargetAccessor.Source.ValueChanged -= OneTimeTargetHandler;
        }

        #endregion
    }
}