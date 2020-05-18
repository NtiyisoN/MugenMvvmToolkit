﻿using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Components
{
    public interface IViewModelLifecycleDispatcherComponent : IComponent<IViewModelManager>
    {
        IReadOnlyMetadataContext? OnLifecycleChanged<TState>(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, [AllowNull]in TState state, IReadOnlyMetadataContext? metadata);
    }
}