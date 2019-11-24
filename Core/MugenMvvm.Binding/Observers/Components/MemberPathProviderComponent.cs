﻿using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class MemberPathProviderComponent : IMemberPathProviderComponent, IHasPriority
    {
        #region Fields

        private static readonly FuncEx<string, IMemberPath?> GetMemberPathStringDelegate = TryGetMemberPath;

        #endregion

        #region Properties

        public int Priority { get; set; } = ObserverComponentPriority.PathProvider;

        #endregion

        #region Implementation of interfaces

        public IMemberPath? TryGetMemberPath<TPath>(in TPath path, IReadOnlyMetadataContext? metadata)
        {
            if (GetMemberPathStringDelegate is FuncEx<TPath, IMemberPath?> provider)
                return provider.Invoke(path);
            return null;
        }

        #endregion

        #region Methods

        private static IMemberPath? TryGetMemberPath(in string path)
        {
            if (path.Length == 0)
                return EmptyMemberPath.Instance;

            var hasBracket = path.IndexOf('[') >= 0;
            if (path.IndexOf('.') >= 0 || hasBracket)
                return new MultiMemberPath(path, hasBracket);
            return new SingleMemberPath(path);
        }

        #endregion
    }
}