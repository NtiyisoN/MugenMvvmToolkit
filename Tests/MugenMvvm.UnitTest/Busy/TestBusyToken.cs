﻿using MugenMvvm.Interfaces.Busy;

namespace MugenMvvm.UnitTest.Busy
{
    public class TestBusyToken : IBusyToken
    {
        #region Properties

        public bool IsSuspended { get; set; }

        public bool IsCompleted { get; set; }

        public object? Message { get; set; }

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
        }

        public ActionToken Suspend()
        {
            return default;
        }

        public ActionToken RegisterCallback(IBusyTokenCallback callback)
        {
            return default;
        }

        #endregion
    }
}