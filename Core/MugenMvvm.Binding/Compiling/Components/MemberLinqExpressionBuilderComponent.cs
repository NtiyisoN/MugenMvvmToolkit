﻿using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class MemberLinqExpressionBuilderComponent : ILinqExpressionBuilderComponent, IHasPriority
    {
        #region Fields

        private readonly IMemberProvider? _memberProvider;
        private readonly Expression _thisExpression;

        private static readonly MethodInfo GetValuePropertyMethod =
            typeof(IBindingMemberAccessorInfo).GetMethodOrThrow(nameof(IBindingMemberAccessorInfo.GetValue), BindingFlagsEx.InstancePublic);

        private static readonly MethodInfo GetValueDynamicMethod = typeof(MemberLinqExpressionBuilderComponent).GetMethodOrThrow(nameof(GetValueDynamic), BindingFlagsEx.InstancePublic);

        #endregion

        #region Constructors

        public MemberLinqExpressionBuilderComponent(IMemberProvider? memberProvider = null)
        {
            _memberProvider = memberProvider;
            _thisExpression = Expression.Constant(this);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = BindingLinqCompilerPriority.Member;

        public BindingMemberFlags MemberFlags { get; set; } = BindingMemberFlags.All & ~BindingMemberFlags.NonPublic;

        #endregion

        #region Implementation of interfaces

        public Expression? TryBuild(ILinqExpressionBuilderContext context, IExpressionNode expression)
        {
            if (!(expression is IMemberExpressionNode memberExpression) || memberExpression.Target == null)
                return null;

            var target = context.Build(memberExpression.Target);
            var type = MugenBindingExtensions.GetTargetType(ref target);
            var member = memberExpression.Member;
            if (member == null)
            {
                BindingMemberFlags flags;
                if (target == null)
                {
                    var @enum = type.TryParseEnum(memberExpression.MemberName);
                    if (@enum != null)
                        return Expression.Constant(@enum);

                    flags = MemberFlags & ~BindingMemberFlags.Instance;
                }
                else
                    flags = MemberFlags & ~BindingMemberFlags.Static;

                member = _memberProvider
                    .ServiceIfNull()
                    .GetMember(type, memberExpression.MemberName,
                        BindingMemberType.Property | BindingMemberType.Field, flags, context.GetMetadataOrDefault()) as IBindingMemberAccessorInfo;
            }

            if (member == null)
            {
                if (target == null)
                    BindingExceptionManager.ThrowInvalidBindingMember(type, memberExpression.MemberName);

                return Expression.Call(_thisExpression, GetValueDynamicMethod,
                    target.ConvertIfNeed(typeof(object), false),
                    Expression.Constant(memberExpression.MemberName),
                    context.MetadataParameter);
            }

            var result = TryCompile(target, member.Member);
            if (result != null)
                return result;

            var methodCall = Expression.Call(Expression.Constant(member),
                GetValuePropertyMethod, target.ConvertIfNeed(typeof(object), false), context.MetadataParameter);
            return Expression.Convert(methodCall, member.Type);
        }

        #endregion

        #region Methods

        [Preserve(Conditional = true)]
        public object? GetValueDynamic(object? target, string member, IReadOnlyMetadataContext? metadata)
        {
            if (target == null)
                return null;
            var property = MugenBindingService
                    .MemberProvider
                    .GetMember(target.GetType(), member, BindingMemberType.Property | BindingMemberType.Field, MemberFlags & ~BindingMemberFlags.Static, metadata) as
                IBindingMemberAccessorInfo;
            if (property == null)
                BindingExceptionManager.ThrowInvalidBindingMember(target.GetType(), member);
            return property!.GetValue(target, metadata);
        }

        private static Expression? TryCompile(Expression? target, object? member)
        {
            if (member == null)
                return null;
            if (member is PropertyInfo property)
                return Expression.Property(target, property);
            if (member is FieldInfo field)
                return Expression.Field(target, field);
            return null;
        }

        #endregion
    }
}