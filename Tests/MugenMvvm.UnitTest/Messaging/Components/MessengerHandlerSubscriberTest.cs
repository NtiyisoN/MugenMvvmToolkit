﻿using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Messaging;
using MugenMvvm.Messaging.Components;
using MugenMvvm.UnitTest.Internal.Internal;
using MugenMvvm.UnitTest.Messaging.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Messaging.Components
{
    public class MessengerHandlerSubscriberTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TrySubscribeUnsubscribeShouldReturnFalseNotSupported()
        {
            var messenger = new Messenger();
            var component = new MessengerHandlerSubscriber();
            messenger.AddComponent(component);

            component.TrySubscribe(this, null, DefaultMetadata).ShouldBeFalse();
            component.TryGetSubscribers(null).ToList().ShouldBeEmpty();
            component.TryUnsubscribe(this, DefaultMetadata).ShouldBeFalse();
        }

        [Fact]
        public void AttachDetachShouldInvalidateCache()
        {
            var invokedCount = 0;
            var messenger = new Messenger();
            var component = new MessengerHandlerSubscriber();
            var hasCache = new TestHasCache { Invalidate = (o, type, arg3) => { ++invokedCount; } };
            messenger.Components.Add(hasCache);

            messenger.AddComponent(component);
            component.TrySubscribe(new TestMessengerHandler(), null, null);
            invokedCount = 0;
            messenger.RemoveComponent(component);
            invokedCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TrySubscribeUnsubscribeGetAllTest(int count)
        {
            var messenger = new Messenger();
            var component = new MessengerHandlerSubscriber();
            messenger.AddComponent(component);

            var hashSet = new HashSet<MessengerSubscriberInfo>();
            for (var i = 0; i < count; i++)
            {
                var handler = new TestMessengerHandler();
                ThreadExecutionMode.TryParse(i, out var mode);
                component.TrySubscribe(handler, mode, DefaultMetadata).ShouldBeTrue();

                if (mode == null)
                    mode = component.ExecutionMode;
                var info = new MessengerSubscriberInfo(handler, mode);
                hashSet.Add(info);
            }

            var subscribers = component.TryGetSubscribers(DefaultMetadata)!.ToList();
            subscribers.Count.ShouldEqual(hashSet.Count);
            foreach (var messengerSubscriberInfo in subscribers)
                hashSet.Remove(messengerSubscriberInfo).ShouldBeTrue();
            hashSet.Count.ShouldEqual(0);

            foreach (var messengerSubscriberInfo in subscribers)
                component.TryUnsubscribe(messengerSubscriberInfo.Subscriber, DefaultMetadata).ShouldBeTrue();
            component.TryGetSubscribers(DefaultMetadata).ToList().ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void TrySubscribeUnsubscribeGetAllTestWeakReference(int count, bool keepAlive)
        {
            var messenger = new Messenger();
            var component = new MessengerHandlerSubscriber();
            var list = new List<object>();
            messenger.AddComponent(component);

            var hashSet = new HashSet<MessengerSubscriberInfo>();
            for (var i = 0; i < count; i++)
            {
                var handler = new TestMessengerHandler().ToWeakReference();
                if (keepAlive)
                    list.Add(handler.Target!);
                ThreadExecutionMode.TryParse(i, out var mode);
                component.TrySubscribe(handler, mode, DefaultMetadata).ShouldBeTrue();

                if (mode == null)
                    mode = component.ExecutionMode;
                var info = new MessengerSubscriberInfo(handler, mode);
                hashSet.Add(info);
            }

            var subscribers = component.TryGetSubscribers(DefaultMetadata)!.ToList();
            subscribers.Count.ShouldEqual(hashSet.Count);
            foreach (var messengerSubscriberInfo in subscribers)
                hashSet.Remove(messengerSubscriberInfo).ShouldBeTrue();
            hashSet.Count.ShouldEqual(0);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            foreach (var messengerSubscriberInfo in subscribers)
                component.TryUnsubscribe(messengerSubscriberInfo.Subscriber, DefaultMetadata).ShouldBeTrue();
            component.TryGetSubscribers(DefaultMetadata).ToList().ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryUnsubscribeAllShouldRemoveAllSubscribers(int count)
        {
            var messenger = new Messenger();
            var component = new MessengerHandlerSubscriber();
            messenger.AddComponent(component);

            for (var i = 0; i < count; i++)
            {
                var handler = new TestMessengerHandler();
                component.TrySubscribe(handler, ThreadExecutionMode.FromValueOrDefault(i % 4, ThreadExecutionMode.Background), DefaultMetadata).ShouldBeTrue();
            }

            component.TryUnsubscribeAll(DefaultMetadata);
            component.TryGetSubscribers(DefaultMetadata).ToList().ShouldBeEmpty();
        }

        [Fact]
        public void TrySubscribeUnsubscribeUnsubscribeAllShouldNotifyListeners()
        {
            var invokedCount = 0;
            var messenger = new Messenger();
            var component = new MessengerHandlerSubscriber();
            var hasCache = new TestHasCache { Invalidate = (o, type, arg3) => { ++invokedCount; } };
            messenger.AddComponent(component);
            messenger.Components.Add(hasCache);

            invokedCount.ShouldEqual(0);
            var handler = new TestMessengerHandler();
            component.TrySubscribe(handler, null, null);
            invokedCount.ShouldEqual(1);

            component.TryUnsubscribe(handler, null);
            invokedCount.ShouldEqual(2);

            component.TrySubscribe(handler, null, null);
            invokedCount = 0;

            component.TryUnsubscribeAll(null);
            invokedCount.ShouldEqual(1);

            component.TryUnsubscribeAll(null);
            invokedCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetMessengerHandlersShouldReturnHandlers1(bool isWeak)
        {
            const string stringMessage = "test";
            const int intMessage = 1;
            var invokedCount = 0;
            var invokedStringCount = 0;
            IMessageContext? ctx = null;
            var messenger = new Messenger();
            var component = new MessengerHandlerSubscriber();
            messenger.AddComponent(component);

            invokedCount.ShouldEqual(0);
            var handler = new TestMessengerHandler
            {
                HandleString = (s, context) =>
                {
                    ++invokedStringCount;
                    context.ShouldEqual(ctx);
                    s.ShouldEqual(stringMessage);
                },
                HandleInt = (i, context) =>
                {
                    ++invokedCount;
                    context.ShouldEqual(ctx);
                    i.ShouldEqual(intMessage);
                }
            };
            if (isWeak)
                component.TrySubscribe(handler.ToWeakReference(), ThreadExecutionMode.Current, null);
            else
                component.TrySubscribe(handler, ThreadExecutionMode.Current, null);

            ctx = new MessageContext(this, intMessage, DefaultMetadata);
            var handlers = component.TryGetMessengerHandlers(typeof(int), DefaultMetadata)!.ToList();
            handlers.Count.ShouldEqual(1);
            handlers[0].ExecutionMode.ShouldEqual(ThreadExecutionMode.Current);
            handlers[0].Handle(ctx).ShouldEqual(MessengerResult.Handled);
            invokedCount.ShouldEqual(1);
            invokedStringCount.ShouldEqual(0);

            ctx = new MessageContext(this, stringMessage, DefaultMetadata);
            handlers = component.TryGetMessengerHandlers(typeof(string), DefaultMetadata)!.ToList();
            handlers.Count.ShouldEqual(1);
            handlers[0].ExecutionMode.ShouldEqual(ThreadExecutionMode.Current);
            handlers[0].Handle(ctx).ShouldEqual(MessengerResult.Handled);
            invokedCount.ShouldEqual(1);
            invokedStringCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetMessengerHandlersShouldReturnHandlers2(bool isWeak)
        {
            const string stringMessage = "test";
            const int intMessage = 1;
            var invokedCount = 0;
            IMessageContext? ctx = null;
            var messenger = new Messenger();
            var component = new MessengerHandlerSubscriber();
            messenger.AddComponent(component);

            invokedCount.ShouldEqual(0);
            var handler = new TestMessengerHandlerGeneric<object>
            {
                Handle = (i, t, context) =>
                {
                    ++invokedCount;
                    context.ShouldEqual(ctx);
                    i.ShouldEqual(context.Message);
                }
            };
            if (isWeak)
                component.TrySubscribe(handler.ToWeakReference(), ThreadExecutionMode.Current, null);
            else
                component.TrySubscribe(handler, ThreadExecutionMode.Current, null);

            ctx = new MessageContext(this, intMessage, DefaultMetadata);
            var handlers = component.TryGetMessengerHandlers(typeof(int), DefaultMetadata)!.ToList();
            handlers.Count.ShouldEqual(1);
            handlers[0].ExecutionMode.ShouldEqual(ThreadExecutionMode.Current);
            handlers[0].Handle(ctx).ShouldEqual(MessengerResult.Handled);
            invokedCount.ShouldEqual(1);

            ctx = new MessageContext(this, stringMessage, DefaultMetadata);
            handlers = component.TryGetMessengerHandlers(typeof(string), DefaultMetadata)!.ToList();
            handlers.Count.ShouldEqual(1);
            handlers[0].ExecutionMode.ShouldEqual(ThreadExecutionMode.Current);
            handlers[0].Handle(ctx).ShouldEqual(MessengerResult.Handled);
            invokedCount.ShouldEqual(2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetMessengerHandlersShouldReturnHandlers3(bool isWeak)
        {
            const string stringMessage = "test";
            const int intMessage = 1;
            var invokedCount = 0;
            Type? canHandleType = null;
            IMessageContext? ctx = null;
            var messenger = new Messenger();
            var component = new MessengerHandlerSubscriber();
            messenger.AddComponent(component);

            invokedCount.ShouldEqual(0);
            var handler = new TestMessengerHandlerRaw
            {
                CanHandle = type =>
                {
                    canHandleType = type;
                    return true;
                },
                Handle = context =>
                {
                    ++invokedCount;
                    context.ShouldEqual(ctx);
                    return MessengerResult.Handled;
                }
            };
            if (isWeak)
                component.TrySubscribe(handler.ToWeakReference(), ThreadExecutionMode.Current, null);
            else
                component.TrySubscribe(handler, ThreadExecutionMode.Current, null);

            ctx = new MessageContext(this, intMessage, DefaultMetadata);
            var handlers = component.TryGetMessengerHandlers(typeof(int), DefaultMetadata)!.ToList();
            canHandleType.ShouldEqual(typeof(int));
            handlers.Count.ShouldEqual(1);
            handlers[0].ExecutionMode.ShouldEqual(ThreadExecutionMode.Current);
            handlers[0].Handle(ctx).ShouldEqual(MessengerResult.Handled);
            invokedCount.ShouldEqual(1);

            ctx = new MessageContext(this, stringMessage, DefaultMetadata);
            handlers = component.TryGetMessengerHandlers(typeof(string), DefaultMetadata)!.ToList();
            canHandleType.ShouldEqual(typeof(string));
            handlers.Count.ShouldEqual(1);
            handlers[0].ExecutionMode.ShouldEqual(ThreadExecutionMode.Current);
            handlers[0].Handle(ctx).ShouldEqual(MessengerResult.Handled);
            invokedCount.ShouldEqual(2);
        }

        [Fact]
        public void TryGetMessengerHandlersShouldUnsubscribeIfSubscriberIsNotAlive()
        {
            var messenger = new Messenger();
            var component = new MessengerHandlerSubscriber();
            messenger.AddComponent(component);
            var handler = new TestMessengerHandler();
            var weakRef = handler.ToWeakReference();
            component.TrySubscribe(weakRef, null, null);

            component.TryGetMessengerHandlers(typeof(string), null)!.ToList().Count.ShouldEqual(1);
            weakRef.Release();
            component.TryGetMessengerHandlers(typeof(string), null).ToList().ShouldBeEmpty();
        }

        [Fact]
        public void HandleShouldReturnInvalidResultTargetIsNotAlive()
        {
            var invokedCount = 0;
            var messenger = new Messenger();
            var component = new MessengerHandlerSubscriber();
            messenger.AddComponent(component);
            var handler = new TestMessengerHandler
            {
                HandleString = (s, context) => { ++invokedCount; }
            };
            var weakRef = handler.ToWeakReference();
            component.TrySubscribe(weakRef, null, null);

            var handlers = component.TryGetMessengerHandlers(typeof(string), null)!.ToList();
            weakRef.Release();
            handlers[0].Handle(new MessageContext(this, "", null)).ShouldEqual(MessengerResult.Invalid);
        }

        #endregion
    }
}