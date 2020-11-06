﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Enums
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct EnumFlags<T> : IEquatable<EnumFlags<T>>, IComparable<EnumFlags<T>> where T : class, IFlagsEnum
    {
        #region Fields

        public readonly long Flags;

        #endregion

        #region Constructors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EnumFlags(long value)
        {
            Flags = value;
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(EnumFlags<T> left, EnumFlags<T> right) => left.Flags >= right.Flags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(EnumFlags<T> left, EnumFlags<T> right) => left.Flags <= right.Flags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(EnumFlags<T> left, EnumFlags<T> right) => left.Flags > right.Flags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(EnumFlags<T> left, EnumFlags<T> right) => left.Flags < right.Flags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(EnumFlags<T> left, EnumFlags<T> right) => left.Flags == right.Flags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(EnumFlags<T> left, EnumFlags<T> right) => left.Flags != right.Flags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnumFlags<T> operator |(EnumFlags<T> left, EnumFlags<T> right) => new EnumFlags<T>(left.Flags | right.Flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnumFlags<T> operator &(EnumFlags<T> left, EnumFlags<T> right) => new EnumFlags<T>(left.Flags & right.Flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnumFlags<T> operator ~(EnumFlags<T> flag) => new EnumFlags<T>(~flag.Flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(EnumFlags<T> other) => Flags.CompareTo(other.Flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(EnumFlags<T> other) => Flags == other.Flags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is EnumFlags<T> other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Flags.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            StringBuilder? builder = null;
            foreach (var @enum in EnumBase.GetAll<T>())
            {
                if (this.HasFlag(@enum.Flag))
                    (builder ??= new StringBuilder()).Append(@enum.Name).Append(' ').Append('|').Append(' ');
            }

            if (builder == null)
                return Flags.ToString();
            builder.Remove(builder.Length - 3, 3);
            return builder.ToString();
        }

        #endregion
    }
}