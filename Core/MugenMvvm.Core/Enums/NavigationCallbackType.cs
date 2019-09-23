﻿using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public class NavigationCallbackType : EnumBase<NavigationCallbackType, int>
    {
        #region Fields

        public static readonly NavigationCallbackType Showing = new NavigationCallbackType(0);
        public static readonly NavigationCallbackType Closing = new NavigationCallbackType(1);
        public static readonly NavigationCallbackType Close = new NavigationCallbackType(2);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected NavigationCallbackType()
        {
        }

        public NavigationCallbackType(int value) : base(value)
        {
        }

        #endregion
    }
}