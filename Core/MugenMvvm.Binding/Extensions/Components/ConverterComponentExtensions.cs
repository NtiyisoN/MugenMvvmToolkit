﻿using System;
using MugenMvvm.Binding.Interfaces.Converters.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Extensions.Components
{
    public static class ConverterComponentExtensions
    {
        #region Methods

        public static bool TryConvert(this IGlobalValueConverterComponent[] components, ref object? value, Type targetType, object? member, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryConvert(ref value, targetType, member, metadata))
                    return true;
            }

            return false;
        }

        #endregion
    }
}