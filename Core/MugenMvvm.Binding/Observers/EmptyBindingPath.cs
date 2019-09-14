﻿using MugenMvvm.Binding.Interfaces.Observers;

namespace MugenMvvm.Binding.Observers
{
    public sealed class EmptyBindingPath : IBindingPath
    {
        #region Fields

        public static readonly EmptyBindingPath Instance = new EmptyBindingPath();

        #endregion

        #region Constructors

        private EmptyBindingPath()
        {
        }

        #endregion

        #region Properties

        public string Path => "";

        public string[] Members => Default.EmptyArray<string>();

        public bool IsSingle => false;

        #endregion
    }
}