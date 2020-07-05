﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Extensions.Components
{
    public static class MemberComponentExtensions
    {
        #region Methods

        public static ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers<TRequest>(this IMemberManagerComponent[] components, IMemberManager memberManager, Type type, MemberType memberTypes, MemberFlags flags, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(memberManager, nameof(memberManager));
            for (var i = 0; i < components.Length; i++)
            {
                var members = components[i].TryGetMembers(memberManager, type, memberTypes, flags, request, metadata);
                if (!members.IsNullOrEmpty())
                    return members;
            }

            return default;
        }

        public static void TryAddMembers(this IMemberProviderComponent[] components, IMemberManager memberManager, ICollection<IMemberInfo> result, Type type, string name, MemberType memberTypes, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(memberManager, nameof(memberManager));
            Should.NotBeNull(result, nameof(result));
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            for (var i = 0; i < components.Length; i++)
            {
                var members = components[i].TryGetMembers(memberManager, type, name, memberTypes, metadata);
                for (var j = 0; j < members.Count(); j++)
                    result.Add(members.Get(j));
            }
        }

        public static ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(this IMemberProviderComponent[] components, IMemberManager memberManager, Type type, string name, MemberType memberTypes, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(memberManager, nameof(memberManager));
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            if (components.Length == 1)
                return components[0].TryGetMembers(memberManager, type, name, memberTypes, metadata);
            ItemOrList<IMemberInfo, List<IMemberInfo>> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetMembers(memberManager, type, name, memberTypes, metadata));
            return result.Cast<IReadOnlyList<IMemberInfo>>();
        }

        #endregion
    }
}