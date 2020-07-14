﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class ConditionalWeakTableAttachedValueProvider : AttachedValueProviderBase, IHasPriority
    {
        #region Fields

        private readonly ConditionalWeakTable<object, SortedList<string, object?>> _weakTable;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ConditionalWeakTableAttachedValueProvider()
        {
            _weakTable = new ConditionalWeakTable<object, SortedList<string, object?>>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = InternalComponentPriority.AttachedValueProvider;

        #endregion

        #region Methods

        public override bool IsSupported(object item, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        protected override IDictionary<string, object?>? GetAttachedDictionary(object item, bool optional)
        {
            if (optional)
            {
                _weakTable.TryGetValue(item, out var value);
                return value;
            }

            return _weakTable.GetValue(item, key => new SortedList<string, object?>(StringComparer.Ordinal));
        }

        protected override bool ClearInternal(object item)
        {
            return _weakTable.Remove(item);
        }

        #endregion
    }
}