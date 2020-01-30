﻿using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Components
{
    public class TestComponentCollectionProviderComponent : IComponentCollectionProviderComponent, IHasPriority
    {
        #region Properties

        public Func<object, IReadOnlyMetadataContext?, IComponentCollection?>? TryGetComponentCollection { get; set; }

        public int Priority { get; set; } = int.MaxValue;

        #endregion

        #region Implementation of interfaces

        IComponentCollection? IComponentCollectionProviderComponent.TryGetComponentCollection(object owner, IReadOnlyMetadataContext? metadata)
        {
            return TryGetComponentCollection?.Invoke(owner, metadata);
        }

        #endregion
    }
}