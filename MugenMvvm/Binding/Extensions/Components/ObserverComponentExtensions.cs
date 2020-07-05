﻿using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Interfaces.Observation.Components;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Extensions.Components
{
    public static class ObserverComponentExtensions
    {
        #region Methods

        public static MemberObserver TryGetMemberObserver<TMember>(this IMemberObserverProviderComponent[] components, IObservationManager observationManager, Type type, [DisallowNull] in TMember member,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(observationManager, nameof(observationManager));
            Should.NotBeNull(type, nameof(type));
            for (var i = 0; i < components.Length; i++)
            {
                var observer = components[i].TryGetMemberObserver(observationManager, type, member, metadata);
                if (!observer.IsEmpty)
                    return observer;
            }

            return default;
        }

        public static IMemberPath? TryGetMemberPath<TPath>(this IMemberPathProviderComponent[] components, IObservationManager observationManager, [DisallowNull] in TPath path, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(observationManager, nameof(observationManager));
            for (var i = 0; i < components.Length; i++)
            {
                var memberPath = components[i].TryGetMemberPath(observationManager, path, metadata);
                if (memberPath != null)
                    return memberPath;
            }

            return null;
        }

        public static IMemberPathObserver? TryGetMemberPathObserver<TRequest>(this IMemberPathObserverProviderComponent[] components, IObservationManager observationManager, object target,
            [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(observationManager, nameof(observationManager));
            Should.NotBeNull(target, nameof(target));
            for (var i = 0; i < components.Length; i++)
            {
                var observer = components[i].TryGetMemberPathObserver(observationManager, target, request, metadata);
                if (observer != null)
                    return observer;
            }

            return null;
        }

        #endregion
    }
}