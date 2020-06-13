﻿using System.Runtime.InteropServices;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Serialization
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MementoResult
    {
        #region Fields

        private readonly IReadOnlyMetadataContext? _metadata;

        #endregion

        #region Constructors

        public MementoResult(object target, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
            IsRestored = true;
            _metadata = metadata;
        }

        public MementoResult(bool isRestored, IReadOnlyMetadataContext? metadata = null)
        {
            Target = null;
            IsRestored = isRestored;
            _metadata = metadata;
        }

        #endregion

        #region Properties

        public bool IsRestored { get; }

        public object? Target { get; }

        public IReadOnlyMetadataContext Metadata => _metadata ?? Default.Metadata;

        #endregion
    }
}