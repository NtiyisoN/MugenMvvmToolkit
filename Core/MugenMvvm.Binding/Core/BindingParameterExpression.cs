﻿using System.Runtime.InteropServices;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Core
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindingParameterExpression
    {
        #region Fields

        private readonly ICompiledExpression? _compiledExpression;
        private readonly object? _value;

        #endregion

        #region Constructors

        public BindingParameterExpression(object? value, ICompiledExpression? compiledExpression)
        {
            if (value is IBindingMemberExpressionNode[])
                Should.NotBeNull(compiledExpression, nameof(compiledExpression));
            _value = value;
            _compiledExpression = compiledExpression;
        }

        #endregion

        #region Properties

        public bool IsEmpty => _value == null && _compiledExpression == null;

        #endregion

        #region Methods

        public BindingParameterValue ToBindingParameter(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            return new BindingParameterValue(MugenBindingExtensions.ToBindingSource(_value, target, source, metadata), _compiledExpression);
        }

        #endregion
    }
}