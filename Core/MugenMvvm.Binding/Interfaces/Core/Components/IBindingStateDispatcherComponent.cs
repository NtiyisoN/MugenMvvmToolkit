﻿using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingStateDispatcherComponent : IComponent<IBindingManager>
    {
        IReadOnlyMetadataContext? OnLifecycleChanged<TState>(IBinding binding, BindingLifecycleState lifecycleState, [AllowNull]in TState state, IReadOnlyMetadataContext? metadata);
    }
}