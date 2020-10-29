﻿using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions.Binding
{
    public abstract class BindingMemberExpressionNodeBase : ExpressionNodeBase, IBindingMemberExpressionNode
    {
        #region Fields

        protected readonly IObservationManager? ObservationManager;

        #endregion

        #region Constructors

        protected BindingMemberExpressionNodeBase(string path, IObservationManager? observationManager)
        {
            Should.NotBeNull(path, nameof(path));
            Path = path;
            ObservationManager = observationManager;
            Index = -1;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.BindingMember;

        public BindingMemberExpressionFlags Flags { get; set; }

        public int Index { get; set; }

        public MemberFlags MemberFlags { get; set; }

        public string? ObservableMethodName { get; set; }

        public string Path { get; }

        #endregion

        #region Implementation of interfaces

        public abstract object? GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags);

        public abstract object? GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata);

        #endregion

        #region Methods

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata) => this;

        protected MemberPathObserverRequest GetObserverRequest(string path, IReadOnlyMetadataContext? metadata) =>
            new MemberPathObserverRequest(ObservationManager.DefaultIfNull().GetMemberPath(path, metadata), MemberFlags,
                Flags.HasFlagEx(BindingMemberExpressionFlags.ObservableMethods) ? ObservableMethodName : null, Flags.HasFlagEx(BindingMemberExpressionFlags.StablePath),
                Flags.HasFlagEx(BindingMemberExpressionFlags.Observable), Flags.HasFlagEx(BindingMemberExpressionFlags.StablePath));

        protected string MergePath(string value)
        {
            if (string.IsNullOrEmpty(Path))
                return value;
            if (Path.StartsWith("[", StringComparison.Ordinal))
                return value + Path;
            return value + "." + Path;
        }

        public override string ToString() => $"bind{Index}({Path})";

        #endregion
    }
}