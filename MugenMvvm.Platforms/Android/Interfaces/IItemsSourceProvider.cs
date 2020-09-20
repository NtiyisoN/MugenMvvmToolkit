﻿using System.Collections;
using MugenMvvm.Android.Native.Interfaces;

namespace MugenMvvm.Android.Interfaces
{
    public interface IItemsSourceProvider : IItemsSourceProviderBase
    {
        IEnumerable? ItemsSource { get; set; }

        object? GetItemAt(int position);

        int IndexOf(object? item);
    }
}