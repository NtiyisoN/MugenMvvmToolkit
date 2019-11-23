﻿namespace MugenMvvm.Binding.Constants
{
    public static class BindingComponentPriority
    {
        #region Fields

        public const int Delay = 0;
        public const int EventHandler = 0;
        public const int ParameterHandler = 1000;
        public const int Mode = -100;

        public const int ComponentProvider = 0;
        public const int DefaultComponentProvider = int.MinValue + 10;

        #endregion
    }
}