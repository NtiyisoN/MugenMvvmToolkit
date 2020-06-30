﻿using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Messaging;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.Internal.Internal;
using MugenMvvm.UnitTest.Messaging.Internal;
using MugenMvvm.UnitTest.Metadata.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Messaging
{
    public class MessengerTest : ComponentOwnerTestBase<Messenger>
    {
        #region Methods

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetMessageContextShouldReturnDefaultContextNoComponents(bool globalContextProvider)
        {
            IMetadataContextManager contextProvider;
            Messenger messenger;
            if (globalContextProvider)
            {
                messenger = new Messenger();
                contextProvider = MugenService.MetadataContextManager;
            }
            else
            {
                contextProvider = new MetadataContextManager();
                messenger = new Messenger(metadataContextManager: contextProvider);
            }

            var metadataContext = new MetadataContext();
            var contextProviderComponent = new TestMetadataContextProviderComponent();
            contextProviderComponent.TryGetMetadataContext = (o, list) => metadataContext;
            using var s = TestComponentSubscriber.Subscribe(contextProvider, contextProviderComponent);

            var sender = new object();
            var message = new object();
            var messageContext = messenger.GetMessageContext(sender, message, DefaultMetadata);
            messageContext.Sender.ShouldEqual(sender);
            messageContext.Message.ShouldEqual(message);
            messageContext.Metadata.ShouldEqual(metadataContext);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMessageContextShouldBeHandledByComponents(int count)
        {
            var sender = new object();
            var message = new object();
            var ctx = new MessageContext(sender, message, DefaultMetadata);
            var invokeCount = 0;
            var messenger = new Messenger();
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestMessageContextProviderComponent
                {
                    Priority = -i,
                    TryGetMessageContext = (o, o1, arg3) =>
                    {
                        ++invokeCount;
                        o.ShouldEqual(sender);
                        o1.ShouldEqual(message);
                        arg3.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return ctx;
                        return null;
                    }
                };
                messenger.AddComponent(component);
            }

            messenger.GetMessageContext(sender, message, DefaultMetadata).ShouldEqual(ctx);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void PublishShouldBeHandledByComponents(int count)
        {
            var ctx = new MessageContext(new object(), new object(), DefaultMetadata);
            var invokeCount = 0;
            bool result = false;
            var messenger = new Messenger();
            for (var i = 0; i < count; i++)
            {
                var component = new TestMessagePublisherComponent
                {
                    Priority = -i,
                    TryPublish = messageContext =>
                    {
                        ++invokeCount;
                        messageContext.ShouldEqual(ctx);
                        return result;
                    }
                };
                messenger.AddComponent(component);
            }

            messenger.Publish(ctx).ShouldEqual(result);
            invokeCount.ShouldEqual(count);

            invokeCount = 0;
            result = true;
            messenger.Publish(ctx).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1, null)]
        [InlineData(1, 1)]
        [InlineData(10, null)]
        [InlineData(10, 1)]
        public void SubscribeShouldBeHandledByComponents(int count, int? executionMode)
        {
            var threadMode = executionMode == null ? null : ThreadExecutionMode.Parse(executionMode.Value);
            var invokeCount = 0;
            var messenger = new Messenger();
            var result = false;
            for (var i = 0; i < count; i++)
            {
                var component = new TestMessengerSubscriberComponent
                {
                    Priority = -i,
                    TrySubscribe = (o, type, arg3, arg4) =>
                    {
                        ++invokeCount;
                        o.ShouldEqual(messenger);
                        type.ShouldEqual(messenger.GetType());
                        arg3.ShouldEqual(threadMode);
                        arg4.ShouldEqual(DefaultMetadata);
                        return result;
                    }
                };
                messenger.AddComponent(component);
            }

            messenger.Subscribe(messenger, threadMode, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(count);

            invokeCount = 0;
            result = true;
            messenger.Subscribe(messenger, threadMode, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void UnsubscribeShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var messenger = new Messenger();
            var result = false;
            for (var i = 0; i < count; i++)
            {
                var component = new TestMessengerSubscriberComponent
                {
                    Priority = -i,
                    TryUnsubscribe = (o, type, arg3) =>
                    {
                        ++invokeCount;
                        o.ShouldEqual(messenger);
                        type.ShouldEqual(messenger.GetType());
                        arg3.ShouldEqual(DefaultMetadata);
                        return result;
                    }
                };
                messenger.AddComponent(component);
            }

            messenger.Unsubscribe(messenger, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(count);

            invokeCount = 0;
            result = true;
            messenger.Unsubscribe(messenger, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void UnsubscribeAllShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var messenger = new Messenger();
            for (var i = 0; i < count; i++)
            {
                var component = new TestMessengerSubscriberComponent
                {
                    Priority = -i,
                    TryUnsubscribeAll = arg3 =>
                    {
                        ++invokeCount;
                        arg3.ShouldEqual(DefaultMetadata);
                    }
                };
                messenger.AddComponent(component);
            }

            messenger.UnsubscribeAll(DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }

        [Fact]
        public void GetSubscribersShouldReturnEmptyListNoComponents()
        {
            var messenger = new Messenger();
            messenger.GetSubscribers(DefaultMetadata).AsList().ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetSubscribersShouldBeHandledByComponents(int count)
        {
            var messenger = new Messenger();
            var subscribers = new HashSet<MessengerSubscriberInfo>();
            for (var i = 0; i < count; i++)
                subscribers.Add(new MessengerSubscriberInfo(new object(), ThreadExecutionMode.Background));
            for (var i = 0; i < count; i++)
            {
                var info = subscribers.ElementAt(i);
                var component = new TestMessengerSubscriberComponent
                {
                    Priority = -i,
                    TryGetSubscribers = arg3 =>
                    {
                        arg3.ShouldEqual(DefaultMetadata);
                        return new[] { info };
                    }
                };
                messenger.AddComponent(component);
            }

            var result = messenger.GetSubscribers(DefaultMetadata).AsList();
            result.Count.ShouldEqual(count);
            foreach (var messengerSubscriberInfo in result)
                subscribers.Remove(messengerSubscriberInfo);
            subscribers.Count.ShouldEqual(0);
        }

        protected override Messenger GetComponentOwner(IComponentCollectionManager? collectionProvider = null)
        {
            return new Messenger(collectionProvider);
        }

        #endregion
    }
}