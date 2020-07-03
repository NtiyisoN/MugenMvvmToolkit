﻿using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Views.Internal
{
    public class TestViewMappingProviderComponent : IViewMappingProviderComponent, IHasPriority
    {
        #region Properties

        public Func<object, Type, IReadOnlyMetadataContext?, ItemOrList<IViewMapping, IReadOnlyList<IViewMapping>>>? TryGetMappings { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<IViewMapping, IReadOnlyList<IViewMapping>> IViewMappingProviderComponent.TryGetMappings<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryGetMappings?.Invoke(request!, typeof(TRequest), metadata) ?? default;
        }

        #endregion
    }
}