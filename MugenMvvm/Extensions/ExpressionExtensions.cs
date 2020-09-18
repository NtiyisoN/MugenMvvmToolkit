﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Fields

        public static readonly ConstantExpression NullConstantExpression = Expression.Constant(null, typeof(object));
        public static readonly ConstantExpression TrueConstantExpression = Expression.Constant(BoxingExtensions.TrueObject);
        public static readonly ConstantExpression FalseConstantExpression = Expression.Constant(BoxingExtensions.FalseObject);
        private static readonly Expression[] ArrayIndexesCache = GenerateArrayIndexes(25);

        #endregion

        #region Methods

        [return: NotNullIfNotNull("expression")]
        public static Expression? ConvertIfNeed(this Expression? expression, Type? type, bool exactly)
        {
            if (type == null)
                return expression;
            if (expression == null)
                return null;
            if (type == typeof(void) || type == expression.Type)
                return expression;
            if (expression.Type == typeof(void))
                return Expression.Block(expression, type == typeof(object) ? NullConstantExpression : (Expression) Expression.Default(type));
            if (!exactly && !expression.Type.IsValueType && !type.IsValueType && type.IsAssignableFrom(expression.Type))
                return expression;
            if (type.IsByRef && expression is ParameterExpression parameterExpression && parameterExpression.IsByRef && parameterExpression.Type == type.GetElementType())
                return expression;
            if (type == typeof(object) && BoxingExtensions.CanBox(expression.Type))
                return Expression.Call(null, BoxingExtensions.GetBoxMethodInfo(expression.Type), expression);
            return Expression.Convert(expression, type);
        }

        public static ParameterExpression GetParameterExpression<TType>() => ParameterExpressionCache<TType>.Parameter;

        public static ParameterExpression[] GetParametersExpression<TType>() => ParameterExpressionCache<TType>.Parameters;

        public static ConstantExpression GetConstantExpression(int value)
        {
            if (value < 0)
            {
                if (value > -BoxingExtensions.CacheSize)
                    return IntCache.Negative[-value];
            }
            else if (value < BoxingExtensions.CacheSize)
                return IntCache.Positive[value];

            return Expression.Constant(value);
        }

        public static MemberInfo GetMemberInfo(this LambdaExpression expression)
        {
            Should.NotBeNull(expression, nameof(expression));
            if ((expression.Body as UnaryExpression)?.Operand is MemberExpression memberExpression)
                return memberExpression.Member;
            if (expression.Body is MemberExpression expressionBody)
                return expressionBody.Member;
            ExceptionManager.ThrowNotSupported(expression.ToString());
            return null;
        }

        internal static Expression GetIndexExpression(int index)
        {
            if (index >= 0 && index < ArrayIndexesCache.Length)
                return ArrayIndexesCache[index];
            return Expression.ArrayIndex(GetParameterExpression<object[]>(), GetConstantExpression(index));
        }

        internal static Expression[] GenerateArrayIndexes(int length)
        {
            var expressions = new Expression[length];
            for (var i = 0; i < length; i++)
                expressions[i] = Expression.ArrayIndex(GetParameterExpression<object[]>(), GetConstantExpression(i));
            return expressions;
        }

        #endregion

        #region Nested types

        private static class ParameterExpressionCache<TType>
        {
            #region Fields

            public static readonly ParameterExpression Parameter = Expression.Parameter(typeof(TType));
            public static readonly ParameterExpression[] Parameters = {Parameter};

            #endregion
        }

        internal static class IntCache
        {
            #region Fields

            public static readonly ConstantExpression[] Positive = GenerateItems(BoxingExtensions.IntCache.Positive);
            public static readonly ConstantExpression[] Negative = GenerateItems(BoxingExtensions.IntCache.Negative);

            #endregion

            #region Methods

            private static ConstantExpression[] GenerateItems(object[] values)
            {
                var items = new ConstantExpression[values.Length];
                for (var i = 0; i < items.Length; i++)
                    items[i] = Expression.Constant(values[i]);
                return items;
            }

            #endregion
        }

        #endregion
    }
}