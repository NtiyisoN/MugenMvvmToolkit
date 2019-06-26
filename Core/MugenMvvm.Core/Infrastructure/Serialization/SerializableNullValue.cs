﻿using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Infrastructure.Serialization
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public sealed class SerializableNullValue
    {
        #region Fields

        private static readonly SerializableNullValue Instance = new SerializableNullValue();

        #endregion

        #region Constructors

        internal SerializableNullValue()
        {
        }

        #endregion

        #region Methods

        public static object To(object? value)
        {
            if (value == null)
                return Instance;
            return value;
        }

        public static object? From(object? value)
        {
            if (value is SerializableNullValue)
                return null;
            return value;
        }

        public static bool IsNull(object? value)
        {
            return value is SerializableNullValue;
        }

        #endregion
    }
}