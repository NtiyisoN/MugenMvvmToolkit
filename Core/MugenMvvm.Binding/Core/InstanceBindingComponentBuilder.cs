﻿using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Core
{
    public sealed class InstanceBindingComponentBuilder : ExpressionNodeBase, IBindingComponentBuilder
    {
        #region Fields

        public static readonly IBindingComponentBuilder OneTimeMode =
            new InstanceBindingComponentBuilder(BindingParameterNameConstants.Mode, OneTimeBindingModeComponent.Instance);

        public static readonly IBindingComponentBuilder OneWayMode =
            new InstanceBindingComponentBuilder(BindingParameterNameConstants.Mode, OneWayBindingModeComponent.Instance);

        public static readonly IBindingComponentBuilder OneWayToSourceMode =
            new InstanceBindingComponentBuilder(BindingParameterNameConstants.Mode, OneWayToSourceBindingModeComponent.Instance);

        public static readonly IBindingComponentBuilder TwoWayMode =
            new InstanceBindingComponentBuilder(BindingParameterNameConstants.Mode, TwoWayBindingModeComponent.Instance);

        public static readonly IBindingComponentBuilder OneTimeDisposeMode =
            new InstanceBindingComponentBuilder(BindingParameterNameConstants.Mode, OneTimeBindingModeComponent.DisposeBindingInstance);

        public static readonly IBindingComponentBuilder NoneMode =
            new InstanceBindingComponentBuilder(BindingParameterNameConstants.Mode);

        #endregion

        #region Constructors

        public InstanceBindingComponentBuilder(string name, IComponent<IBinding> component)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(component, nameof(component));
            Name = name;
            Component = component;
        }

        public InstanceBindingComponentBuilder(string name)
        {
            Should.NotBeNull(name, nameof(name));
            Name = name;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Component == null;

        public string Name { get; }

        public IComponent<IBinding>? Component { get; }

        public override ExpressionNodeType NodeType => ExpressionNodeType.BindingMember;

        #endregion

        #region Implementation of interfaces

        public IComponent<IBinding> GetComponent(IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            return Component!;
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