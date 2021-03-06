﻿#region Copyright

// ****************************************************************************
// <copyright file="AttachedMembers.cs">
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

using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Models;
#if WINDOWS_UWP
using UIElementEx = Windows.UI.Xaml.UIElement;
#elif XAMARIN_FORMS
using UIElementEx = Xamarin.Forms.VisualElement;
#else
using UIElementEx = System.Windows.UIElement;
#endif


#if WPF
namespace MugenMvvmToolkit.WPF.Binding
#elif WINDOWS_UWP
namespace MugenMvvmToolkit.UWP.Binding
#elif XAMARIN_FORMS
namespace MugenMvvmToolkit.Xamarin.Forms.Binding
#endif

{
    public static class AttachedMembers
    {
        #region Nested types

        public abstract class Object : AttachedMembersBase.Object
        {
        }

#if XAMARIN_FORMS
        public abstract class VisualElement : Object
        {
        #region Fields

            public static readonly BindingMemberDescriptor<UIElementEx, bool> Visible;
            public static readonly BindingMemberDescriptor<UIElementEx, bool> Hidden;

        #endregion

        #region Constructors

            static VisualElement()
            {
                Visible = new BindingMemberDescriptor<UIElementEx, bool>(nameof(Visible));
                Hidden = new BindingMemberDescriptor<UIElementEx, bool>(nameof(Hidden));
            }

        #endregion
        }
#else
        public abstract class UIElement : Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<UIElementEx, bool> Visible;
            public static readonly BindingMemberDescriptor<UIElementEx, bool> Hidden;

            #endregion

            #region Constructors

            static UIElement()
            {
                Visible = new BindingMemberDescriptor<UIElementEx, bool>(nameof(Visible));
                Hidden = new BindingMemberDescriptor<UIElementEx, bool>(nameof(Hidden));
            }

            #endregion
        }
#endif
        #endregion
    }
}
