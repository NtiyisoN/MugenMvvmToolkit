﻿using System;
using System.Reflection;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    public sealed class BindingPropertyInfo : IBindingPropertyInfo
    {
        #region Fields

        private readonly IObserverProvider? _observerProvider;

        private readonly PropertyInfo _propertyInfo;
        private readonly Type _reflectedType;
        private Func<object?, object?> _getterFunc;

        private MemberObserver? _observer;
        private Action<object?, object?> _setterFunc;

        #endregion

        #region Constructors

        public BindingPropertyInfo(string name, PropertyInfo propertyInfo, Type reflectedType, IObserverProvider? observerProvider)
        {
            _propertyInfo = propertyInfo;
            _reflectedType = reflectedType;
            _observerProvider = observerProvider;
            Name = name;
            Type = _propertyInfo.PropertyType;

            var getMethod = propertyInfo.GetGetMethodUnified(true);
            if (getMethod == null)
            {
                CanRead = false;
                _getterFunc = MustBeReadable;
            }
            else
            {
                CanRead = true;
                _getterFunc = CompileGetter;
            }

            var setMethod = propertyInfo.GetSetMethodUnified(true);
            if (setMethod == null)
            {
                CanWrite = false;
                _setterFunc = MustBeWritable;
            }
            else
            {
                CanWrite = true;
                _setterFunc = CompileSetter;
            }

            AccessModifiers = (getMethod ?? setMethod).GetAccessModifiers();
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type Type { get; }

        public object? Member => _propertyInfo;

        public BindingMemberType MemberType => BindingMemberType.Property;

        public MemberFlags AccessModifiers { get; }

        public bool CanRead { get; }

        public bool CanWrite { get; }

        #endregion

        #region Implementation of interfaces

        public IDisposable? TryObserve(object? source, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            if (_observer == null)
                _observer = _observerProvider.ServiceIfNull().GetMemberObserver(_reflectedType, _propertyInfo);
            return _observer.Value.TryObserve(source, listener, metadata);
        }

        public object? GetValue(object? source, IReadOnlyMetadataContext? metadata = null)
        {
            return _getterFunc(source);
        }

        public void SetValue(object? source, object? value, IReadOnlyMetadataContext? metadata = null)
        {
            _setterFunc(source, value);
        }

        #endregion

        #region Methods

        private void MustBeWritable(object? _, object? __)
        {
            BindingExceptionManager.ThrowBindingMemberMustBeWritable(this);
        }

        private object MustBeReadable(object? _)
        {
            BindingExceptionManager.ThrowBindingMemberMustBeReadable(this);
            return null!;
        }

        private void CompileSetter(object? arg1, object? arg2)
        {
            _setterFunc = _propertyInfo.GetMemberSetter<object?>();
            _setterFunc(arg1, arg2);
        }

        private object? CompileGetter(object? arg)
        {
            _getterFunc = _propertyInfo.GetMemberGetter<object>();
            return _getterFunc(arg);
        }

        #endregion
    }
}