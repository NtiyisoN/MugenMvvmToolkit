﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Binding.Compiling;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions.Binding;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Compiling
{
    public class CompiledExpressionTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void SetClearExpressionShouldUpdateExpression()
        {
            var member1 = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, "test", null) {Index = 0};
            var compiledExpression = new CompiledExpression(new UnaryExpressionNode(UnaryTokenType.BitwiseNegation, member1));

            var expressionNode = ConstantExpressionNode.False;
            var expression = Expression.Constant(1);

            compiledExpression.TryGetExpression(expressionNode).ShouldBeNull();
            compiledExpression.SetExpression(expressionNode, expression);
            compiledExpression.TryGetExpression(expressionNode).ShouldEqual(expression);
            compiledExpression.ClearExpression(expressionNode);
            compiledExpression.TryGetExpression(expressionNode).ShouldBeNull();
        }

        [Fact]
        public void InvokeShouldThrowNoComponents()
        {
            var member1 = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, "test", null) {Index = 0};
            var compiledExpression = new CompiledExpression(new UnaryExpressionNode(UnaryTokenType.BitwiseNegation, member1));
            ShouldThrow<InvalidOperationException>(() => compiledExpression.Invoke(new ExpressionValue(typeof(object), 1), DefaultMetadata));
        }

        [Fact]
        public void ShouldThrowNotInitializedBindingExpression()
        {
            var member1 = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, "test", null);
            var expressionNode = new UnaryExpressionNode(UnaryTokenType.Minus, member1);
            ShouldThrow<InvalidOperationException>(() => new CompiledExpression(expressionNode));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void CompileShouldReturnValueForBindingExpression1(int count)
        {
            var compileCount = 0;
            var member1 = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, "test", null) {Index = 0};
            var value1 = "test";
            var expressionNode = new UnaryExpressionNode(UnaryTokenType.Minus, member1);
            var compiledExpression = new CompiledExpression(expressionNode);

            var components = new List<IExpressionBuilderCompilerComponent>();
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                components.Add(new TestExpressionBuilderCompilerComponent
                {
                    TryBuild = (context, node) =>
                    {
                        ++compileCount;
                        context.ShouldEqual(compiledExpression);
                        node.ShouldEqual(expressionNode);
                        var expression = context.TryGetExpression(member1);
                        if (isLast)
                            return expression;
                        return null;
                    }
                });
            }

            compiledExpression.ExpressionBuilders = components.ToArray();
            compiledExpression.Invoke(new ExpressionValue(typeof(string), value1), DefaultMetadata).ShouldEqual(value1);
            compileCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void CompileShouldReturnValueForBindingExpression2(int count)
        {
            var compileCount = 0;
            var member1 = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, "test1", null) {Index = 0};
            var member2 = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, "test2", null) {Index = 1};
            var value1 = 1;
            var value2 = -2;
            var result = value1 + value2;
            var expressionNode = new BinaryExpressionNode(BinaryTokenType.Addition, member1, member2);
            var compiledExpression = new CompiledExpression(expressionNode);

            var components = new List<IExpressionBuilderCompilerComponent>();
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                components.Add(new TestExpressionBuilderCompilerComponent
                {
                    TryBuild = (context, node) =>
                    {
                        ++compileCount;
                        context.ShouldEqual(compiledExpression);
                        node.ShouldEqual(expressionNode);
                        var expression1 = context.TryGetExpression(member1);
                        var expression2 = context.TryGetExpression(member2);

                        if (isLast)
                            return Expression.Add(expression1, expression2);
                        return null;
                    }
                });
            }

            compiledExpression.ExpressionBuilders = components.ToArray();
            compiledExpression.Invoke(new[] {new ExpressionValue(typeof(int), value1), new ExpressionValue(typeof(int), value2)}, DefaultMetadata).ShouldEqual(result);
            compileCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void CompileShouldReturnMetadata(int count)
        {
            var compileCount = 0;
            var expressionNode = ConstantExpressionNode.False;
            var compiledExpression = new CompiledExpression(expressionNode);

            var components = new List<IExpressionBuilderCompilerComponent>();
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                components.Add(new TestExpressionBuilderCompilerComponent
                {
                    TryBuild = (context, node) =>
                    {
                        ++compileCount;
                        context.ShouldEqual(compiledExpression);
                        node.ShouldEqual(expressionNode);
                        if (isLast)
                            return context.MetadataExpression;
                        return null;
                    }
                });
            }

            compiledExpression.ExpressionBuilders = components.ToArray();
            compiledExpression.Invoke(default, DefaultMetadata).ShouldEqual(DefaultMetadata);
            compileCount.ShouldEqual(count);
        }

        [Fact]
        public void InvokeShouldCacheExpression1()
        {
            var compileCount = 0;
            var member1 = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, "test", null) {Index = 0};
            var valueSt = "test";
            var valueInt = 1;
            var expressionNode = new UnaryExpressionNode(UnaryTokenType.Minus, member1);
            var compiledExpression = new CompiledExpression(expressionNode);
            compiledExpression.ExpressionBuilders = new[]
            {
                new TestExpressionBuilderCompilerComponent
                {
                    TryBuild = (context, node) =>
                    {
                        ++compileCount;
                        context.ShouldEqual(compiledExpression);
                        node.ShouldEqual(expressionNode);
                        return context.TryGetExpression(member1);
                    }
                }
            };
            compiledExpression.Invoke(new ExpressionValue(typeof(string), valueSt), DefaultMetadata).ShouldEqual(valueSt);
            compileCount.ShouldEqual(1);

            compiledExpression.Invoke(new ExpressionValue(typeof(string), valueSt), DefaultMetadata).ShouldEqual(valueSt);
            compileCount.ShouldEqual(1);

            compiledExpression.Invoke(new ExpressionValue(typeof(int), valueInt), DefaultMetadata).ShouldEqual(valueInt);
            compileCount.ShouldEqual(2);

            compiledExpression.Invoke(new ExpressionValue(typeof(int), valueInt), DefaultMetadata).ShouldEqual(valueInt);
            compileCount.ShouldEqual(2);
        }

        [Fact]
        public void InvokeShouldCacheExpression2()
        {
            var compileCount = 0;
            var member1 = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, "test1", null) {Index = 0};
            var member2 = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, "test2", null) {Index = 1};
            var valueInt1 = 1;
            var valueInt2 = -2;
            var valueFloat1 = 1.5f;
            var valueFloat2 = -2.11f;
            var result1 = valueInt1 + valueInt2;
            var result2 = valueFloat1 + valueFloat2;
            var expressionNode = new BinaryExpressionNode(BinaryTokenType.Addition, member1, member2);
            var compiledExpression = new CompiledExpression(expressionNode);
            compiledExpression.ExpressionBuilders = new[]
            {
                new TestExpressionBuilderCompilerComponent
                {
                    TryBuild = (context, node) =>
                    {
                        ++compileCount;
                        context.ShouldEqual(compiledExpression);
                        node.ShouldEqual(expressionNode);
                        var expression1 = context.TryGetExpression(member1);
                        var expression2 = context.TryGetExpression(member2);
                        return Expression.Add(expression1, expression2);
                    }
                }
            };
            compiledExpression.Invoke(new[] {new ExpressionValue(typeof(int), valueInt1), new ExpressionValue(typeof(int), valueInt2)}, DefaultMetadata).ShouldEqual(result1);
            compileCount.ShouldEqual(1);

            compiledExpression.Invoke(new[] {new ExpressionValue(typeof(int), valueInt1), new ExpressionValue(typeof(int), valueInt2)}, DefaultMetadata).ShouldEqual(result1);
            compileCount.ShouldEqual(1);

            compiledExpression.Invoke(new[] {new ExpressionValue(typeof(float), valueFloat1), new ExpressionValue(typeof(float), valueFloat2)}, DefaultMetadata).ShouldEqual(result2);
            compileCount.ShouldEqual(2);

            compiledExpression.Invoke(new[] {new ExpressionValue(typeof(float), valueFloat1), new ExpressionValue(typeof(float), valueFloat2)}, DefaultMetadata).ShouldEqual(result2);
            compileCount.ShouldEqual(2);
        }

        [Fact]
        public void InvokeShouldClearMetadata()
        {
            var compileCount = 0;
            var key1 = MetadataContextKey.FromKey<int>("i1");
            var key2 = MetadataContextKey.FromKey<string>("i2");
            var value1 = 1;
            var value2 = "test";
            var inputMetadata = MetadataContextValue.Create(key1, value1).ToContext();
            var compiledExpression = new CompiledExpression(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, "test1", null) {Index = 0}, inputMetadata);
            compiledExpression.ExpressionBuilders = new[]
            {
                new TestExpressionBuilderCompilerComponent
                {
                    TryBuild = (context, node) =>
                    {
                        ++compileCount;
                        context.Metadata.Set(key2, value2);
                        return Expression.Constant(1);
                    }
                }
            };
            compiledExpression.Invoke(new ExpressionValue(typeof(int), 1), DefaultMetadata);
            compiledExpression.Metadata.Count.ShouldEqual(1);
            compiledExpression.Metadata.Get(key1).ShouldEqual(value1);
            compileCount.ShouldEqual(1);
        }

        [Fact]
        public void InvokeShouldUseCompileEx()
        {
            var compileCount = 0;
            var result = Expression.Constant(1);
            var compiledExpression = new CompiledExpression(new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, "test1", null) {Index = 0});
            compiledExpression.ExpressionBuilders = new[]
            {
                new TestExpressionBuilderCompilerComponent
                {
                    TryBuild = (context, node) => result
                }
            };

            ILambdaExpressionCompiler compiler = new TestLambdaExpressionCompiler
            {
                CompileGeneric = (expression, type) =>
                {
                    ++compileCount;
                    return null;
                }
            };
            MugenService.Configuration.InitializeInstance(compiler);
            compiledExpression.Invoke(new ExpressionValue(typeof(int), 1), DefaultMetadata);
            compileCount.ShouldEqual(1);
            MugenService.Configuration.Clear<ILambdaExpressionCompiler>();
        }

        #endregion
    }
}