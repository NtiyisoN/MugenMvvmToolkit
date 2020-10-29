﻿namespace MugenMvvm.Bindings.Constants
{
    public static class MemberComponentPriority
    {
        #region Fields

        public const int Attached = 10;
        public const int Instance = 0;
        public const int Extension = -10;
        public const int Dynamic = -20;

        public const int RequestHandler = 100;
        public const int Selector = 0;

        #endregion
    }
}