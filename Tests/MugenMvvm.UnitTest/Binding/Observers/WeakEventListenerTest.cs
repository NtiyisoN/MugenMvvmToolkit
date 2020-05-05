﻿using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers
{
    public class WeakEventListenerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void DefaultShouldBeEmpty()
        {
            default(WeakEventListener).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            var target = new TestEventListener
            {
                IsWeak = true,
                IsAlive = true
            };
            var listener = new WeakEventListener(target);
            listener.Target.ShouldEqual(target);
            listener.IsAlive.ShouldEqual(true);
            listener.Listener.ShouldEqual(target);
            listener.IsEmpty.ShouldBeFalse();

            target.IsAlive = false;
            listener.IsAlive.ShouldEqual(false);
        }

        [Fact]
        public void ConstructorShouldInitializeValues2()
        {
            var target = new TestEventListener
            {
                IsWeak = false,
                IsAlive = true
            };
            var listener = new WeakEventListener(target);
            ((IWeakReference) listener.Target).Target.ShouldEqual(target);
            listener.IsAlive.ShouldEqual(true);
            listener.Listener.ShouldEqual(target);
            listener.IsEmpty.ShouldBeFalse();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryHandleShouldUseListener(bool isWeak)
        {
            var sender = new object();
            var msg = new object();
            var result = true;
            var invokeCount = 0;
            var target = new TestEventListener
            {
                IsWeak = isWeak,
                IsAlive = true,
                TryHandle = (o, o1) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(sender);
                    o1.ShouldEqual(msg);
                    return result;
                }
            };
            var listener = new WeakEventListener(target);
            listener.TryHandle(sender, msg).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            result = false;
            listener.TryHandle(sender, msg).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        #endregion
    }
}