﻿using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members.Components
{
    public interface IMethodProviderComponent : IComponent<IMemberProvider>
    {
        IReadOnlyList<IBindingMethodInfo> TryGetMethods(Type type, string name, IReadOnlyMetadataContext? metadata);
    }
}