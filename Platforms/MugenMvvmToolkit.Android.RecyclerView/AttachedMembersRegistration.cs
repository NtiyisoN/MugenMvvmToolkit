#region Copyright

// ****************************************************************************
// <copyright file="AttachedMembersRegistration.cs">
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

using System.Collections;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.RecyclerView.Infrastructure;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;

namespace MugenMvvmToolkit.Android.RecyclerView
{
    public static class AttachedMembersRegistration
    {
        #region Methods

        public static void RegisterRecyclerViewMembers()
        {
            BindingServiceProvider.BindingMemberPriorities[AttachedMembersRecyclerView.RecyclerView.CreateViewHolderDelegate] = BindingServiceProvider.TemplateMemberPriority + 1;

            var provider = BindingServiceProvider.MemberProvider;
            provider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ItemsSource.Override<global::Android.Support.V7.Widget.RecyclerView>(),
                RecyclerViewItemsSourceChanged));
            provider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembersRecyclerView.RecyclerView.CreateViewHolderDelegate));
        }

        private static void RecyclerViewItemsSourceChanged(global::Android.Support.V7.Widget.RecyclerView recyclerView, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            var adapter = recyclerView.GetAdapter() as ItemsSourceRecyclerAdapter;
            if (adapter == null)
            {
                adapter = new ItemsSourceRecyclerAdapter();
                recyclerView.SetAdapter(adapter);
            }
            adapter.ItemsSource = args.NewValue;
        }

        #endregion
    }
}