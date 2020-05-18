﻿using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers.Components
{
    public interface IMemberObserverProviderComponent : IComponent<IObserverProvider>
    {
        MemberObserver TryGetMemberObserver<TMember>(Type type, [DisallowNull]in TMember member, IReadOnlyMetadataContext? metadata);
    }
}