﻿using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    public sealed class MethodMemberInfo : IMethodInfo
    {
        #region Fields

        private readonly MethodInfo _method;
        private readonly IObserverProvider? _observerProvider;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;
        private readonly IParameterInfo[] _parameters;
        private readonly Type _reflectedType;
        private Func<object?, object?[], object?> _invoker;

        private MemberObserver? _observer;

        #endregion

        #region Constructors

        public MethodMemberInfo(string name, MethodInfo method, Type reflectedType, IObserverProvider? observerProvider, IReflectionDelegateProvider? reflectionDelegateProvider)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(method, nameof(method));
            Should.NotBeNull(reflectedType, nameof(reflectedType));
            _method = method;
            _reflectedType = reflectedType;
            _observerProvider = observerProvider;
            _reflectionDelegateProvider = reflectionDelegateProvider;
            _invoker = CompileMethod;
            Name = name;
            AccessModifiers = _method.GetAccessModifiers();
            var parameterInfos = method.GetParameters();
            if (parameterInfos.Length == 0)
                _parameters = Default.EmptyArray<IParameterInfo>();
            else
            {
                _parameters = new IParameterInfo[parameterInfos.Length];
                for (var i = 0; i < parameterInfos.Length; i++)
                    _parameters[i] = new Parameter(parameterInfos[i]);
            }
            if (method.IsStatic && method.IsDefined(typeof(ExtensionAttribute), false) && _parameters.Length > 0)
                AccessModifiers |= MemberFlags.Extension;
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type DeclaringType => _method.DeclaringType;

        public Type Type => _method.ReturnType;

        public object? UnderlyingMember => _method;

        public MemberType MemberType => MemberType.Method;

        public MemberFlags AccessModifiers { get; }

        public bool IsGenericMethod => _method.IsGenericMethod;

        public bool IsGenericMethodDefinition => _method.IsGenericMethodDefinition;

        #endregion

        #region Implementation of interfaces

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            if (_observer == null)
                _observer = _observerProvider.ServiceIfNull().TryGetMemberObserver(_reflectedType, _method);
            return _observer.Value.TryObserve(target, listener, metadata);
        }

        public IParameterInfo[] GetParameters()
        {
            return _parameters;
        }

        public Type[] GetGenericArguments()
        {
            return _method.GetGenericArguments();
        }

        public IMethodInfo MakeGenericMethod(Type[] types)
        {
            return new MethodMemberInfo(Name, _method.MakeGenericMethod(types), _reflectedType, _observerProvider, _reflectionDelegateProvider);
        }

        public object? Invoke(object? target, object?[] args, IReadOnlyMetadataContext? metadata = null)
        {
            if (target != null && AccessModifiers.HasFlagEx(MemberFlags.Extension))
                return _invoker(null, args.InsertFirstArg(target));
            return _invoker(target, args);
        }

        #endregion

        #region Methods

        private object? CompileMethod(object? target, object?[] args)
        {
            _invoker = _method.GetMethodInvoker(_reflectionDelegateProvider);
            return _invoker(target, args);
        }

        #endregion

        #region Nested types

        private sealed class Parameter : IParameterInfo
        {
            #region Fields

            private readonly ParameterInfo _parameterInfo;

            #endregion

            #region Constructors

            public Parameter(ParameterInfo parameterInfo)
            {
                _parameterInfo = parameterInfo;
                IsParamsArray = parameterInfo.IsDefined(typeof(ParamArrayAttribute), true);
            }

            #endregion

            #region Properties

            public bool IsParamsArray { get; }

            public bool HasDefaultValue => _parameterInfo.HasDefaultValue;

            public Type ParameterType => _parameterInfo.ParameterType;

            public object? DefaultValue => _parameterInfo.DefaultValue;

            #endregion
        }

        #endregion
    }
}