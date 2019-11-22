﻿using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Visitors;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingInitializerExpressionInterceptorComponent : AttachableComponentBase<IBindingManager>, IBindingExpressionInterceptorComponent, IHasPriority
    {
        #region Fields

        private static readonly Func<ValueTuple<ValueTuple<object?, ICompiledExpression?>, ValueTuple<object?, ICompiledExpression?>, ValueTuple<object?, ICompiledExpression?>,
            ValueTuple<object?, ICompiledExpression?>>, IBinding, object, object?, IReadOnlyMetadataContext?, IComponent<IBinding>> GetParametersComponentDelegate = GetParametersComponent;

        private static readonly BindingMemberExpressionVisitor MemberExpressionVisitor = new BindingMemberExpressionVisitor();
        private static readonly BindingMemberExpressionCollectorVisitor MemberExpressionCollectorVisitor = new BindingMemberExpressionCollectorVisitor();

        private readonly BindingParameterContext _context;
        private readonly IExpressionCompiler? _compiler;
        private readonly Func<ValueTuple<ValueTuple<object?, ICompiledExpression?>, bool>, IBinding, object, object?, IReadOnlyMetadataContext?, IComponent<IBinding>> _getEventHandlerDelegate;
        private readonly IMemberProvider? _memberProvider;

        #endregion

        #region Constructors

        public BindingInitializerExpressionInterceptorComponent(IExpressionCompiler? compiler = null, IMemberProvider? memberProvider = null)
        {
            _context = new BindingParameterContext();
            _getEventHandlerDelegate = GetEventHandlerComponent;
            _compiler = compiler;
            _memberProvider = memberProvider;
        }

        #endregion

        #region Properties

        public BindingMemberExpressionFlags Flags { get; set; } = BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.ObservableMethod;

        public bool IgnoreMethodMembers { get; set; }

        public bool IgnoreIndexMembers { get; set; }

        public bool ToggleEnabledState { get; set; }

        public int Priority { get; set; } = int.MinValue;

        public bool IsCachePerTypeRequired { get; set; }

        #endregion

        #region Implementation of interfaces

        public void Intercept(object target, object? source, ref IExpressionNode targetExpression, ref IExpressionNode sourceExpression,
            ref ItemOrList<IExpressionNode?, List<IExpressionNode>> parameters, IReadOnlyMetadataContext? metadata)
        {
            _context.Clear();
            _context.Initialize(parameters);
            SetMemberVisitorSettings();

            targetExpression = MemberExpressionVisitor.Visit(targetExpression, metadata);
            if (IsEvent(target, source, targetExpression, metadata))
            {
                var ignoreMethodMembers = MemberExpressionVisitor.IgnoreMethodMembers;
                var ignoreIndexMembers = MemberExpressionVisitor.IgnoreIndexMembers;
                MemberExpressionVisitor.IgnoreIndexMembers = true;
                MemberExpressionVisitor.IgnoreMethodMembers = true;
                sourceExpression = MemberExpressionVisitor.Visit(sourceExpression, metadata);
                MemberExpressionVisitor.IgnoreIndexMembers = ignoreIndexMembers;
                MemberExpressionVisitor.IgnoreMethodMembers = ignoreMethodMembers;

                if (!_context.ComponentBuilders.ContainsKey(BindingParameterNameConstants.EventHandler))
                {
                    bool _ = false;
                    var parameter = GetValueOrExpression(BindingParameterNameConstants.CommandParameter, metadata, ref _);
                    var toggle = _context.TryGetBool(BindingParameterNameConstants.ToggleEnabled).GetValueOrDefault(ToggleEnabledState);
                    parameters.Add(new DelegateBindingComponentBuilder<ValueTuple<ValueTuple<object?, ICompiledExpression?>, bool>>(_getEventHandlerDelegate,
                        BindingParameterNameConstants.EventHandler, (parameter, toggle)));
                }
                return;
            }

            sourceExpression = MemberExpressionVisitor.Visit(sourceExpression, metadata);

            if (_context.ComponentBuilders.ContainsKey(BindingParameterNameConstants.Parameters))
                return;

            bool hasResult = false;
            var converter = GetValueOrExpression(BindingParameterNameConstants.Converter, metadata, ref hasResult);
            var converterParameter = GetValueOrExpression(BindingParameterNameConstants.ConverterParameter, metadata, ref hasResult);
            var fallback = GetValueOrExpression(BindingParameterNameConstants.Fallback, metadata, ref hasResult);
            var targetNullValue = GetValueOrExpression(BindingParameterNameConstants.TargetNullValue, metadata, ref hasResult);
            if (hasResult)
            {
                parameters.Add(new DelegateBindingComponentBuilder<ValueTuple<ValueTuple<object?, ICompiledExpression?>, ValueTuple<object?, ICompiledExpression?>,
                    ValueTuple<object?, ICompiledExpression?>, ValueTuple<object?, ICompiledExpression?>>>(GetParametersComponentDelegate,
                    BindingParameterNameConstants.Parameters, (converter, converterParameter, fallback, targetNullValue)));
            }
        }

        #endregion

        #region Methods

        private void SetMemberVisitorSettings()
        {
            MemberExpressionVisitor.Flags = Flags;
            MemberExpressionVisitor.IgnoreMethodMembers = _context.TryGetBool(BindingParameterNameConstants.IgnoreMethodMembers).GetValueOrDefault(IgnoreMethodMembers);
            MemberExpressionVisitor.IgnoreIndexMembers = _context.TryGetBool(BindingParameterNameConstants.IgnoreIndexMembers).GetValueOrDefault(IgnoreIndexMembers);
            ApplyFlags(BindingParameterNameConstants.Observable, BindingMemberExpressionFlags.Observable);
            ApplyFlags(BindingParameterNameConstants.Optional, BindingMemberExpressionFlags.Optional);
            ApplyFlags(BindingParameterNameConstants.HasStablePath, BindingMemberExpressionFlags.StablePath);
            ApplyFlags(BindingParameterNameConstants.ObservableMethod, BindingMemberExpressionFlags.ObservableMethod);
        }

        private void ApplyFlags(string parameterName, BindingMemberExpressionFlags value)
        {
            var b = _context.TryGetBool(parameterName);
            if (b.GetValueOrDefault())
                MemberExpressionVisitor.Flags |= value;
        }

        private bool IsEvent(object target, object? source, IExpressionNode targetExpression, IReadOnlyMetadataContext? metadata)
        {
            if (targetExpression is IBindingMemberExpressionNode bindingMemberExpression)
            {
                target = bindingMemberExpression.GetTarget(target, source, metadata, out var path, out var flags);
                return path.GetLastMemberFromPath(flags.GetTargetType(target), target, flags, MemberType.Event, metadata, _memberProvider) != null;
            }

            return false;
        }

        private IComponent<IBinding> GetEventHandlerComponent(ValueTuple<ValueTuple<object?, ICompiledExpression?>, bool> state,
            IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            var (commandParameter, toggleEnabledState) = state;
            return new EventTargetValueInterceptorBindingComponent(GetValueOrExpression(commandParameter, target, source, metadata), toggleEnabledState, Owner);
        }

        private static IComponent<IBinding> GetParametersComponent(ValueTuple<ValueTuple<object?, ICompiledExpression?>, ValueTuple<object?, ICompiledExpression?>, ValueTuple<object?, ICompiledExpression?>,
            ValueTuple<object?, ICompiledExpression?>> state, IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            var (converter, converterParameter, fallback, targetNullValue) = state;
            return new BindingParametersValueInterceptorComponent(GetValueOrExpression(converter, target, source, metadata),
                GetValueOrExpression(converterParameter, target, source, metadata), GetValueOrExpression(fallback, target, source, metadata),
                GetValueOrExpression(targetNullValue, target, source, metadata));
        }

        private static BindingParameterValue GetValueOrExpression(ValueTuple<object?, ICompiledExpression?> value, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            if (value.Item1 is IBindingMemberExpressionNode v)
            {
                var observer = v.GetSourceObserver(target, source, metadata);
                return new BindingParameterValue(observer, value.Item2);
            }

            if (value.Item1 is IBindingMemberExpressionNode[] nodes)
            {
                var observers = new IMemberPathObserver[nodes.Length];
                for (var i = 0; i < nodes.Length; i++)
                    observers[i] = nodes[i].GetSourceObserver(target, source, metadata);
                return new BindingParameterValue(observers, value.Item2);
            }

            return new BindingParameterValue(value.Item1, value.Item2);
        }

        private ValueTuple<object?, ICompiledExpression?> GetValueOrExpression(string parameterName, IReadOnlyMetadataContext? metadata, ref bool hasResult)
        {
            var expression = _context.TryGetExpression(parameterName);
            if (expression == null)
                return default;

            hasResult = true;
            expression = MemberExpressionVisitor.Visit(expression, metadata);
            if (expression is IConstantExpressionNode constant)
                return ValueTuple.Create<object?, ICompiledExpression?>(constant.Value, null);
            if (expression is IBindingMemberExpressionNode)
                return ValueTuple.Create<object?, ICompiledExpression?>(expression, null);

            var collect = MemberExpressionCollectorVisitor.Collect(expression, metadata);
            var compiledExpression = _compiler.DefaultIfNull().Compile(expression, metadata);
            if (collect.Item == null && collect.List == null)
                return ValueTuple.Create<object?, ICompiledExpression?>(compiledExpression.Invoke(default, metadata), null);

            return ValueTuple.Create(collect.GetRawValue(), compiledExpression);
        }

        #endregion
    }
}