﻿#region Copyright

// ****************************************************************************
// <copyright file="PlatformDataBindingModule.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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

using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.iOS.Binding.Converters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.iOS.Binding.Modules
{
    public class PlatformDataBindingModule : IModule
    {
        #region Properties

        public int Priority => ApplicationSettings.ModulePriorityBinding;

        #endregion

        #region Implementation of interfaces

        public bool Load(IModuleContext context)
        {
            //new BindingErrorProvider() todo init service provide
            context.TryRegisterDataTemplateSelectorsAndValueConverters(null);
            MugenMvvmToolkit.Binding.AttachedMembersRegistration.RegisterDefaultMembers();

            var converter = new BooleanToCheckmarkAccessoryConverter();
            BindingServiceProvider.ResourceResolver.AddConverter("BooleanToCheckmark", converter);
            BindingServiceProvider.ResourceResolver.AddConverter("BoolToCheckmark", converter);
            BindingServiceProvider.BindingProvider.DefaultBehaviors.Add(DisableEqualityCheckingBehavior.TargetTrueNotTwoWay);

            AttachedMembersRegistration.RegisterObjectMembers();
            AttachedMembersRegistration.RegisterViewMembers();
            AttachedMembersRegistration.RegisterSegmentedControlMembers();
            AttachedMembersRegistration.RegisterButtonMembers();
            AttachedMembersRegistration.RegisterDatePickerMembers();
            AttachedMembersRegistration.RegisterSwitchMembers();
            AttachedMembersRegistration.RegisterControlMembers();
            AttachedMembersRegistration.RegisterTextFieldMembers();
            AttachedMembersRegistration.RegisterTextViewMembers();
            AttachedMembersRegistration.RegisterLabelMembers();
            AttachedMembersRegistration.RegisterBaseViewControllerMembers();
            AttachedMembersRegistration.RegisterViewControllerMembers();
            AttachedMembersRegistration.RegisterTabBarControllerMembers();
            AttachedMembersRegistration.RegisterSplitViewControllerMembers();
            AttachedMembersRegistration.RegisterToolbarMembers();
            AttachedMembersRegistration.RegisterPickerViewMembers();
            AttachedMembersRegistration.RegisterBarButtonItemMembers();
            AttachedMembersRegistration.RegisterSearchBarMembers();
            AttachedMembersRegistration.RegisterSliderMembers();
            AttachedMembersRegistration.RegisterProgressViewMembers();
            AttachedMembersRegistration.RegisterCollectionViewMembers();
            AttachedMembersRegistration.RegisterCollectionViewCellMembers();
            AttachedMembersRegistration.RegisterDialogElementMembers();
            AttachedMembersRegistration.RegisterDialogEntryElementMembers();
            AttachedMembersRegistration.RegisterStringElementMembers();
            AttachedMembersRegistration.RegisterTableViewMembers();
            AttachedMembersRegistration.RegisterTableViewCellMembers();

            return true;
        }

        public void Unload(IModuleContext context)
        {
        }

        #endregion
    }
}