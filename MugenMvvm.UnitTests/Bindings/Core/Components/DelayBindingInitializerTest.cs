﻿using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class DelayBindingInitializerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void InitializeShouldIgnoreEmptyParameters()
        {
            var initializer = new DelayBindingInitializer();
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, default, DefaultMetadata);
            initializer.Initialize(null!, context);
            context.BindingComponents.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InitializeShouldAddDelayComponent(bool ignore)
        {
            const int delay = 100;
            var initializer = new DelayBindingInitializer();
            var context = new BindingExpressionInitializerContext(this);
            var parameter = new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.Delay), ConstantExpressionNode.Get(delay));
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, parameter, DefaultMetadata);
            if (ignore)
                context.BindingComponents[BindingParameterNameConstant.Delay] = null;

            initializer.Initialize(null!, context);
            context.BindingComponents.Count.ShouldEqual(1);
            if (ignore)
            {
                context.BindingComponents[BindingParameterNameConstant.Delay].ShouldBeNull();
                return;
            }

            var provider = (IBindingComponentProvider) context.BindingComponents[BindingParameterNameConstant.Delay]!;
            var component = (DelayBindingComponent.Source) provider.TryGetComponent(null!, null!, null, DefaultMetadata)!;
            component.Delay.ShouldEqual((ushort) delay);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InitializeShouldAddTargetDelayComponent(bool ignore)
        {
            const int delay = 100;
            var initializer = new DelayBindingInitializer();
            var context = new BindingExpressionInitializerContext(this);
            var parameter = new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.TargetDelay), ConstantExpressionNode.Get(delay));
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, parameter, DefaultMetadata);
            if (ignore)
                context.BindingComponents[BindingParameterNameConstant.TargetDelay] = null;

            initializer.Initialize(null!, context);
            context.BindingComponents.Count.ShouldEqual(1);
            if (ignore)
            {
                context.BindingComponents[BindingParameterNameConstant.TargetDelay].ShouldBeNull();
                return;
            }

            var provider = (IBindingComponentProvider) context.BindingComponents[BindingParameterNameConstant.TargetDelay]!;
            var component = (DelayBindingComponent.Target) provider.TryGetComponent(null!, null!, null, DefaultMetadata)!;
            component.Delay.ShouldEqual((ushort) delay);
        }

        #endregion
    }
}