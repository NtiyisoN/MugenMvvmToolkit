﻿using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class EntityState : FlagsEnumBase<EntityState, int>
    {
        #region Fields

        public static readonly EntityState Unchanged = new EntityState(1 << 0);
        public static readonly EntityState Added = new EntityState(1 << 1);
        public static readonly EntityState Deleted = new EntityState(1 << 2);
        public static readonly EntityState Modified = new EntityState(1 << 3);
        public static readonly EntityState Detached = new EntityState(1 << 4);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected EntityState()
        {
        }

        public EntityState(int value) : base(value)
        {
        }

        #endregion
    }
}