﻿using System.Linq.Expressions;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class LambdaExpressionCompilerComponent : ExpressionCompilerComponent.ICompiler, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public Expression? TryCompile(ExpressionCompilerComponent.IContext context, IExpressionNode expression)
        {
            if (!(expression is ILambdaExpressionNode lambdaExpression))
                return null;

            var method = context.CurrentLambdaMethod;
            if (method == null)
                return null;

            var parameters = method.GetParameters();
            if (lambdaExpression.Parameters.Count != parameters.Length)
                return null;

            var lambdaParameters = new ParameterExpression[parameters.Length];
            try
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameterExp = lambdaExpression.Parameters[i];
                    if (parameterExp.Type != null && !parameterExp.Type.IsAssignableFromUnified(parameters[i].ParameterType))
                        return null;

                    var parameter = Expression.Parameter(parameters[i].ParameterType, lambdaExpression.Parameters[i].Name);
                    lambdaParameters[i] = parameter;
                    context.SetParameterExpression(lambdaExpression.Parameters[i], parameter);
                }

                return Expression.Lambda(context.Compile(lambdaExpression.Body), lambdaParameters);
            }
            finally
            {
                for (var i = 0; i < lambdaParameters.Length; i++)
                    context.SetParameterExpression(lambdaExpression.Parameters[i], null);
            }
        }

        #endregion
    }
}