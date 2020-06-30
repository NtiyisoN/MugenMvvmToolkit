﻿using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components.Parsers
{
    public sealed class ParenTokenParser : ITokenParserComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Paren;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression)
        {
            if (expression != null)
                return null;

            var p = context.Position;
            var position = context.SkipWhitespacesPosition();
            if (!context.IsToken('(', position))
                return null;

            context.Position = position + 1;
            var node = context.TryParseWhileNotNull();
            if (context.SkipWhitespaces().IsToken(')'))
            {
                context.MoveNext();
                return node;
            }

            context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseParenExpressionExpectedToken);
            context.Position = p;
            return null;
        }

        #endregion
    }
}