﻿using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class UnaryTokenParserComponent : TokenExpressionParserComponent.IParser, IHasPriority
    {
        #region Fields

        private readonly Dictionary<char, UnaryTokenType[]> _tokensMapping;

        #endregion

        #region Constructors

        public UnaryTokenParserComponent(IDictionary<char, UnaryTokenType[]>? mapping = null)
        {
            if (mapping == null)
            {
                _tokensMapping = new Dictionary<char, UnaryTokenType[]>
                {
                    {UnaryTokenType.Minus.Value[0], new[] {UnaryTokenType.Minus}},
                    {UnaryTokenType.BitwiseNegation.Value[0], new[] {UnaryTokenType.BitwiseNegation}},
                    {UnaryTokenType.LogicalNegation.Value[0], new[] {UnaryTokenType.LogicalNegation}},
                    {UnaryTokenType.DynamicExpression.Value[0], new[] {UnaryTokenType.StaticExpression, UnaryTokenType.DynamicExpression}}
                };
            }
            else
                _tokensMapping = new Dictionary<char, UnaryTokenType[]>(mapping);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = BindingParserPriority.Unary;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(TokenExpressionParserComponent.IContext context, IExpressionNode? expression)
        {
            if (expression != null)
                return null;

            var position = context.SkipWhitespacesPosition();
            if (!_tokensMapping.TryGetValue(BindingMugenExtensions.TokenAt(context, position), out var values))
                return null;

            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                if (context.IsToken(value.Value, position))
                {
                    context.SetPosition(position + value.Value.Length);
                    return new UnaryExpressionNode(value, context.Parse());
                }
            }

            return null;
        }

        #endregion
    }
}