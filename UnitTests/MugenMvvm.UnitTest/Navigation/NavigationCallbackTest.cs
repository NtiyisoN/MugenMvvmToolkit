﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Internal;
using MugenMvvm.Navigation;
using MugenMvvm.UnitTest.Navigation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Navigation
{
    public class NavigationCallbackTest : UnitTestBase
    {
        #region Fields

        private static readonly NavigationContext DefaultContext = new NavigationContext(null, Default.NavigationProvider, "f", NavigationType.Popup, NavigationMode.Close);

        #endregion

        #region Methods

        [Theory]
        [InlineData(1, "1", "Tab")]
        [InlineData(2, "12", "Window")]
        public void ConstructorShouldInitializeCallback(int callbackType, string operationId, string navigationType)
        {
            var navigationCallbackType = NavigationCallbackType.Parse(callbackType);
            var type = NavigationType.Parse(navigationType);
            var callback = new NavigationCallback(navigationCallbackType, operationId, type);
            callback.IsCompleted.ShouldBeFalse();
            callback.NavigationId.ShouldEqual(operationId);
            callback.CallbackType.ShouldEqual(navigationCallbackType);
            callback.NavigationType.ShouldEqual(type);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetCallbacksRegisterCallbackShouldReturnCallbacks(int count)
        {
            var list = new List<INavigationCallbackListener>();
            var callback = new NavigationCallback(NavigationCallbackType.Close, "test", NavigationType.Alert);
            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationCallbackListener();
                list.Add(listener);
                callback.AddCallback(listener);
                callback.GetCallbacks().AsList().SequenceEqual(list).ShouldBeTrue();
            }

            for (var i = 0; i < count; i++)
            {
                callback.RemoveCallback(list[i]);
                var array = callback.GetCallbacks().AsList();
                array.SequenceEqual(list.Skip(i + 1)).ShouldBeTrue();
            }
        }

        [Theory]
        [InlineData(1, true, false)]
        [InlineData(1, false, false)]
        [InlineData(1, true, true)]
        [InlineData(10, true, false)]
        [InlineData(10, false, false)]
        [InlineData(10, true, true)]
        public void SetResultShouldSetResult(int count, bool trySetResult, bool isCompletedCallback)
        {
            var invokeCount = 0;
            var callback = new NavigationCallback(NavigationCallbackType.Close, "test", NavigationType.Alert);
            callback.IsCompleted.ShouldBeFalse();

            if (isCompletedCallback)
                callback.SetResult(DefaultContext);

            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationCallbackListener
                {
                    OnCompleted = context =>
                    {
                        ++invokeCount;
                        context.ShouldEqual(DefaultContext);
                    },
                    OnError = (exception, context) => throw new NotSupportedException(),
                    OnCanceled = (context, cancellationToken) => throw new NotSupportedException()
                };
                callback.AddCallback(listener);
            }

            if (!isCompletedCallback)
            {
                if (trySetResult)
                {
                    callback.TrySetResult(DefaultContext).ShouldBeTrue();
                    callback.TrySetResult(DefaultContext).ShouldBeFalse();
                }
                else
                    callback.SetResult(DefaultContext);
            }

            invokeCount.ShouldEqual(count);
            callback.IsCompleted.ShouldBeTrue();
        }

        [Theory]
        [InlineData(1, true, false)]
        [InlineData(1, false, false)]
        [InlineData(1, true, true)]
        [InlineData(10, true, false)]
        [InlineData(10, false, false)]
        [InlineData(10, true, true)]
        public void SetExceptionShouldSetResult(int count, bool trySetResult, bool isCompletedCallback)
        {
            var ex = new Exception();
            var invokeCount = 0;
            var callback = new NavigationCallback(NavigationCallbackType.Close, "test", NavigationType.Alert);
            callback.IsCompleted.ShouldBeFalse();

            if (isCompletedCallback)
                callback.SetException(DefaultContext, ex);
            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationCallbackListener
                {
                    OnCompleted = context => throw new NotSupportedException(),
                    OnError = (context, exception) =>
                    {
                        exception.ShouldEqual(ex);
                        ++invokeCount;
                        context.ShouldEqual(DefaultContext);
                    },
                    OnCanceled = (context, cancellationToken) => throw new NotSupportedException()
                };
                callback.AddCallback(listener);
            }

            if (!isCompletedCallback)
            {
                if (trySetResult)
                {
                    callback.TrySetException(DefaultContext, ex).ShouldBeTrue();
                    callback.TrySetException(DefaultContext, ex).ShouldBeFalse();
                }
                else
                    callback.SetException(DefaultContext, ex);
            }

            invokeCount.ShouldEqual(count);
            callback.IsCompleted.ShouldBeTrue();
        }

        [Theory]
        [InlineData(1, true, false)]
        [InlineData(1, false, false)]
        [InlineData(1, true, true)]
        [InlineData(10, true, false)]
        [InlineData(10, false, false)]
        [InlineData(10, true, true)]
        public void SetCanceledShouldSetResult(int count, bool trySetResult, bool isCompletedCallback)
        {
            var token = new CancellationToken(true);
            var invokeCount = 0;
            var callback = new NavigationCallback(NavigationCallbackType.Close, "test", NavigationType.Alert);
            callback.IsCompleted.ShouldBeFalse();
            if (isCompletedCallback)
                callback.SetCanceled(DefaultContext, token);
            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationCallbackListener
                {
                    OnCompleted = context => throw new NotSupportedException(),
                    OnError = (exception, context) => throw new NotSupportedException(),
                    OnCanceled = (context, cancellationToken) =>
                    {
                        ++invokeCount;
                        context.ShouldEqual(DefaultContext);
                        cancellationToken.ShouldEqual(cancellationToken);
                    }
                };
                callback.AddCallback(listener);
            }

            if (!isCompletedCallback)
            {
                if (trySetResult)
                {
                    callback.TrySetCanceled(DefaultContext, token).ShouldBeTrue();
                    callback.TrySetCanceled(DefaultContext, token).ShouldBeFalse();
                }
                else
                    callback.SetCanceled(DefaultContext, token);
            }

            invokeCount.ShouldEqual(count);
            callback.IsCompleted.ShouldBeTrue();
        }

        [Fact]
        public void SetResultShouldThrow()
        {
            var callback = new NavigationCallback(NavigationCallbackType.Close, "test", NavigationType.Alert);
            callback.SetResult(DefaultContext);

            ShouldThrow<InvalidOperationException>(() => callback.SetResult(DefaultContext));
            ShouldThrow<InvalidOperationException>(() => callback.SetException(DefaultContext, new Exception()));
            ShouldThrow<InvalidOperationException>(() => callback.SetCanceled(DefaultContext, CancellationToken.None));
        }

        [Fact]
        public void TrySetResultShouldReturnFalse()
        {
            var callback = new NavigationCallback(NavigationCallbackType.Close, "test", NavigationType.Alert);
            callback.SetResult(DefaultContext);

            callback.TrySetResult(DefaultContext).ShouldBeFalse();
            callback.TrySetException(DefaultContext, new Exception()).ShouldBeFalse();
            callback.TrySetCanceled(DefaultContext, CancellationToken.None).ShouldBeFalse();
        }

        #endregion
    }
}