﻿using System;
using System.Reflection;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface IMemberReflectionDelegateProviderComponent : IComponent<IReflectionManager>
    {
        Delegate? TryGetMemberGetter(MemberInfo member, Type delegateType);

        Delegate? TryGetMemberSetter(MemberInfo member, Type delegateType);
    }
}