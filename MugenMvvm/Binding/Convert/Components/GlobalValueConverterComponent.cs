﻿using System;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Convert.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Convert.Components
{
    public sealed class GlobalValueConverterComponent : IGlobalValueConverterComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ConverterComponentPriority.Converter;

        public Func<IFormatProvider>? FormatProvider { get; set; }

        #endregion

        #region Implementation of interfaces

        public bool TryConvert(ref object? value, Type targetType, object? member, IReadOnlyMetadataContext? metadata)
        {
            if (value == null)
            {
                value = targetType.GetDefaultValue();
                return true;
            }

            if (targetType.IsInstanceOfType(value))
                return true;
            if (targetType == typeof(string))
            {
                value = FormatProvider == null
                    ? value.ToString()
                    : System.Convert.ToString(value, FormatProvider.Invoke());
                return true;
            }

            if (targetType.IsEnum)
            {
                value = Enum.Parse(targetType, value.ToString());
                return true;
            }

            if (value is IConvertible)
            {
                value = FormatProvider == null
                    ? System.Convert.ChangeType(value, targetType.GetNonNullableType())
                    : System.Convert.ChangeType(value, targetType.GetNonNullableType(), FormatProvider.Invoke());
                return true;
            }

            return false;
        }

        #endregion
    }
}