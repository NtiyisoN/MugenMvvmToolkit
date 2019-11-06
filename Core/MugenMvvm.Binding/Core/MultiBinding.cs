﻿using MugenMvvm.Binding.Compiling;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core
{
    public sealed class MultiBinding : Binding, IExpressionValue
    {
        #region Fields

        private ICompiledExpression? _expression;

        #endregion

        #region Constructors

        public MultiBinding(IMemberPathObserver target, ItemOrList<IMemberPathObserver?, IMemberPathObserver[]> sources, ICompiledExpression expression)
            : base(target, sources.GetRawValue())
        {
            Should.NotBeNull(expression, nameof(expression));
            _expression = expression;
        }

        #endregion

        #region Implementation of interfaces

        public object? Invoke(IReadOnlyMetadataContext? metadata = null)
        {
            if (metadata == null)
                metadata = this;
            ItemOrList<ExpressionValue, ExpressionValue[]> values;
            if (SourceRaw == null)
                values = Default.EmptyArray<ExpressionValue>();
            else if (SourceRaw is IMemberPathObserver[] sources)
            {
                var expressionValues = new ExpressionValue[sources.Length];
                for (var i = 0; i < sources.Length; i++)
                {
                    var members = sources[i].GetLastMember(metadata);
                    var value = members.GetValue(metadata);
                    if (value.IsUnsetValueOrDoNothing())
                        return value;
                    expressionValues[i] = new ExpressionValue(value?.GetType() ?? members.Member.Type, null);
                }

                values = expressionValues;
            }
            else
            {
                var members = ((IMemberPathObserver)SourceRaw).GetLastMember(metadata);
                var value = members.GetValue(metadata);
                if (value.IsUnsetValueOrDoNothing())
                    return value;

                values = new ExpressionValue(value?.GetType() ?? members.Member.Type, value);
            }

            return _expression!.Invoke(values, metadata);
        }

        #endregion

        #region Methods

        protected override void OnDispose()
        {
            _expression = null;
        }

        protected override object? GetSourceValue(MemberPathLastMember targetMember)
        {
            if (MemberType.Event == targetMember.Member.MemberType)
                return this;
            return Invoke();
        }

        #endregion
    }
}