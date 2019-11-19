﻿using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Core
{
    public sealed class DelegateBindingComponentBuilder<TState> : ExpressionNodeBase, IBindingComponentBuilder
    {
        #region Fields

        private readonly Func<TState, IBinding, object, object?, IReadOnlyMetadataContext?, IComponent<IBinding>> _componentFactory;
        private readonly TState _state;

        #endregion

        #region Constructors

        public DelegateBindingComponentBuilder(Func<TState, IBinding, object, object?, IReadOnlyMetadataContext?, IComponent<IBinding>> componentFactory,
            string name, TState state)
        {
            Should.NotBeNull(componentFactory, nameof(componentFactory));
            Should.NotBeNull(name, nameof(name));
            _componentFactory = componentFactory;
            _state = state;
            Name = name;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType NodeType => ExpressionNodeType.BindingMember;

        public bool IsEmpty => false;

        public string Name { get; }

        #endregion

        #region Implementation of interfaces

        public IComponent<IBinding> GetComponent(IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            return _componentFactory(_state, binding, target, source, metadata);
        }

        #endregion

        #region Methods

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor)
        {
            return this;
        }

        #endregion
    }
}