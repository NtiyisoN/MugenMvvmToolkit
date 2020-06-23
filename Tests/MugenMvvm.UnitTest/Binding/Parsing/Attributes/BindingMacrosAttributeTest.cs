﻿using System;
using System.Linq.Expressions;
using MugenMvvm.Binding.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Attributes
{
    [BindingMacros(Name, true)]
    public class BindingMacrosAttributeTest : UnitTestBase
    {
        #region Fields

        private const string Name = "Test";
        private const string MethodName = "Method";

        #endregion

        #region Methods

        [Fact]
        public void TryConvertShouldReturnResourceExpression1()
        {
            var attribute = (BindingMacrosAttribute) BindingSyntaxExtensionAttributeBase.TryGet(typeof(BindingMacrosAttributeTest))!;
            var ctx = new ExpressionConverterContext<Expression>();
            attribute.TryConvert(ctx, null, out var result).ShouldBeTrue();
            result.ShouldEqual(UnaryExpressionNode.Get(UnaryTokenType.StaticExpression, MemberExpressionNode.Get(null, Name)));
        }

        [Fact]
        public void TryConvertShouldReturnResourceExpression2()
        {
            var call = Expression.Call(typeof(BindingMacrosAttributeTest), nameof(StaticMethod), Default.Array<Type>());
            var attribute = (BindingMacrosAttribute) BindingSyntaxExtensionAttributeBase.TryGet(call.Method)!;
            var ctx = new ExpressionConverterContext<Expression>();
            attribute.TryConvert(ctx, call, out var result).ShouldBeTrue();
            result.ShouldEqual(UnaryExpressionNode.Get(UnaryTokenType.DynamicExpression, MemberExpressionNode.Get(null, MethodName)));
        }

        [Fact]
        public void TryConvertShouldReturnResourceExpression3()
        {
            const string name = "TT";
            var call = Expression.Call(typeof(BindingMacrosAttributeTest), nameof(ResourceMethod), Default.Array<Type>(), Expression.Constant(name));
            var attribute = (BindingMacrosAttribute) BindingSyntaxExtensionAttributeBase.TryGet(call.Method)!;
            var ctx = new ExpressionConverterContext<Expression>();
            attribute.TryConvert(ctx, call, out var result).ShouldBeTrue();
            result.ShouldEqual(UnaryExpressionNode.Get(UnaryTokenType.DynamicExpression, MemberExpressionNode.Get(null, name)));
        }

        [BindingMacros(MethodName)]
        public static string StaticMethod()
        {
            return "";
        }

        [BindingMacros(0)]
        public static string ResourceMethod(string name)
        {
            return name;
        }

        #endregion
    }
}