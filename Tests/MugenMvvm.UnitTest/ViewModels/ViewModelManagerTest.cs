﻿using System;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.ViewModels;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.ViewModels
{
    public class ViewModelManagerTest : ComponentOwnerTestBase<ViewModelManager>
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnLifecycleChangedShouldBeHandledByComponents(int count)
        {
            var manager = new ViewModelManager();
            var context = new MetadataContext();
            var viewModel = new TestViewModel();
            var lifecycleState = ViewModelLifecycleState.Created;
            for (var i = 0; i < count; i++)
            {
                var ctx = new MetadataContext();
                ctx.Set(MetadataContextKey.FromKey<int>("i" + i), i);
                context.Merge(ctx);
                var component = new TestViewModelLifecycleDispatcherComponent
                {
                    OnLifecycleChanged = (vm, state, metadata) =>
                    {
                        vm.ShouldEqual(viewModel);
                        state.ShouldEqual(lifecycleState);
                        metadata.ShouldEqual(DefaultMetadata);
                        return ctx;
                    },
                    Priority = i
                };
                manager.AddComponent(component);
            }

            var changed = manager.OnLifecycleChanged(viewModel, lifecycleState, DefaultMetadata);
            changed.SequenceEqual(context).ShouldBeTrue();
        }

        [Fact]
        public void GetServiceShouldThrowNoComponents()
        {
            var manager = new ViewModelManager();
            ShouldThrow<InvalidOperationException>(() => manager.GetService(new TestViewModel(), typeof(object), DefaultMetadata));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetServiceShouldBeHandledByComponents(int count)
        {
            var manager = new ViewModelManager();
            var viewModel = new TestViewModel();
            var service = new object();
            var executeCount = 0;
            var serviceType = typeof(bool);
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestViewModelServiceResolverComponent
                {
                    TryGetService = (vm, t, ctx) =>
                    {
                        ++executeCount;
                        vm.ShouldEqual(viewModel);
                        t.ShouldEqual(serviceType);
                        ctx.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return service;
                        return null;
                    },
                    Priority = -i
                };
                manager.AddComponent(component);
            }

            manager.GetService(viewModel, serviceType, DefaultMetadata).ShouldEqual(service);
            executeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetViewModelShouldBeHandledByComponents(int count)
        {
            var manager = new ViewModelManager();
            var viewModel = new TestViewModel();
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestViewModelProviderComponent
                {
                    TryGetViewModel = (o, type, arg3) =>
                    {
                        ++executeCount;
                        o.ShouldEqual(this);
                        type.ShouldEqual(GetType());
                        arg3.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return viewModel;
                        return null;
                    },
                    Priority = -i
                };
                manager.AddComponent(component);
            }

            manager.TryGetViewModel(this, DefaultMetadata).ShouldEqual(viewModel);
            executeCount.ShouldEqual(count);
        }

        protected override ViewModelManager GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new ViewModelManager(collectionProvider);
        }

        #endregion
    }
}