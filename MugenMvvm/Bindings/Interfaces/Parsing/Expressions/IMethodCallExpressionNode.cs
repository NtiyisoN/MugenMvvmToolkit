﻿using System.Collections.Generic;

namespace MugenMvvm.Bindings.Interfaces.Parsing.Expressions
{
    public interface IMethodCallExpressionNode : IHasTargetExpressionNode<IMethodCallExpressionNode>, IHasArgumentsExpressionNode<IMethodCallExpressionNode>
    {
        string Method { get; }

        IReadOnlyList<string> TypeArgs { get; }
    }
}