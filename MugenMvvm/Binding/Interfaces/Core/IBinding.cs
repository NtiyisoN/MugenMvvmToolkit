﻿using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IBinding : IDisposable, IComponentOwner<IBinding>, IMetadataOwner<IReadOnlyMetadataContext>
    {
        BindingState State { get; }

        IMemberPathObserver Target { get; }

        ItemOrList<object?, object?[]> Source { get; }

        ItemOrList<object, object[]> GetComponents();

        void UpdateTarget();

        void UpdateSource();
    }
}