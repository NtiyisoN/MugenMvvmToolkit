﻿using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    public sealed class ConstantBindingMemberInfo : IBindingMemberAccessorInfo
    {
        #region Fields

        private readonly object? _result;

        public static readonly ConstantBindingMemberInfo Null = new ConstantBindingMemberInfo(null, false);
        public static readonly ConstantBindingMemberInfo Unset = new ConstantBindingMemberInfo(BindingMetadata.UnsetValue, false);
        public static readonly ConstantBindingMemberInfo WritableNull = new ConstantBindingMemberInfo(null, true);

        public static readonly IBindingMemberInfo[] NullArray = {Null};
        public static readonly IBindingMemberInfo[] UnsetArray = {Unset};
        public static readonly IBindingMemberInfo[] WritableNullArray = {WritableNull};

        #endregion

        #region Constructors

        private ConstantBindingMemberInfo(object? result, bool canWrite)
        {
            _result = result;
            CanWrite = canWrite;
        }

        #endregion

        #region Properties

        public string Name => string.Empty;

        public Type Type => typeof(object);

        public object? Member => null;

        public BindingMemberType MemberType => BindingMemberType.Property;

        public BindingMemberFlags AccessModifiers => BindingMemberFlags.StaticPublic;

        public bool CanRead => true;

        public bool CanWrite { get; }

        #endregion

        #region Implementation of interfaces

        public Unsubscriber TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            return default;
        }

        public object? GetValue(object? target, IReadOnlyMetadataContext? metadata = null)
        {
            return _result;
        }

        public void SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata = null)
        {
            if (!CanWrite)
                BindingExceptionManager.ThrowBindingMemberMustBeWritable(this);
        }

        #endregion
    }
}