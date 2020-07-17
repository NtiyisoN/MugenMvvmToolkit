﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MugenMvvm.Internal
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct ArrayIterator<TItem>
    {
        #region Fields

        private readonly TItem _item;
        private readonly TItem[]? _list;

        public readonly int Count;

        #endregion

        #region Constructors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArrayIterator(int count, [AllowNull] TItem item, TItem[]? list)
        {
            Count = count;
            _item = item!;
            _list = list;
        }

        #endregion

        #region Properties

        public TItem this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)Count)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                return _list == null ? _item : _list[index];
            }
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TItem[] ToArray()
        {
            if (Count == 0)
                return Default.Array<TItem>();
            var items = new TItem[Count];
            for (var i = 0; i < items.Length; i++)
                items[i] = this[i];
            return items;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TItem[] AsList()
        {
            if (_list != null)
                return _list;
            if (Count == 0)
                return Default.Array<TItem>();
            return new[] { _item };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            return new Enumerator(Count, _item!, _list);
        }

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        public ref struct Enumerator
        {
            #region Fields

            private readonly int _count;
            private readonly TItem _item;
            private readonly TItem[]? _list;
            private int _index;

            #endregion

            #region Constructors

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(int count, TItem item, TItem[]? list)
            {
                _count = count;
                _item = item;
                _list = list;
                _index = -1;
            }

            #endregion

            #region Properties

            public TItem Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _list == null ? _item : _list[_index];
            }

            #endregion

            #region Methods

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                return ++_index < _count;
            }

            #endregion
        }

        #endregion
    }
}