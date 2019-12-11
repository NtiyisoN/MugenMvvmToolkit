﻿using System;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Extensions.Components
{
    public static class ObserverComponentExtensions
    {
        #region Methods

        public static MemberObserver TryGetMemberObserver<TMember>(this IMemberObserverProviderComponent[] components, Type type, in TMember member, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var observer = components[i].TryGetMemberObserver(type, member, metadata);
                if (!observer.IsEmpty)
                    return observer;
            }

            return default;
        }

        public static IMemberPath? TryGetMemberPath<TPath>(this IMemberPathProviderComponent[] components, in TPath path, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var memberPath = components[i].TryGetMemberPath(path, metadata);
                if (memberPath != null)
                    return memberPath;
            }

            return null;
        }

        public static IMemberPathObserver? TryGetMemberPathObserver<TRequest>(this IMemberPathObserverProviderComponent[] components, object target, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var observer = components[i].TryGetMemberPathObserver(target, request, metadata);
                if (observer != null)
                    return observer;
            }

            return null;
        }

        #endregion
    }
}