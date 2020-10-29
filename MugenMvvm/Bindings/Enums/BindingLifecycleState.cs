﻿using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class BindingLifecycleState : EnumBase<BindingLifecycleState, string>
    {
        #region Fields

        public static readonly BindingLifecycleState Initialized = new BindingLifecycleState(nameof(Initialized));
        public static readonly BindingLifecycleState Disposed = new BindingLifecycleState(nameof(Disposed));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected BindingLifecycleState()
        {
        }

        public BindingLifecycleState(string value)
            : base(value)
        {
        }

        #endregion
    }
}