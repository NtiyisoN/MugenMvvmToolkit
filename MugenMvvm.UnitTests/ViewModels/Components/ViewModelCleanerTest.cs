﻿using System;
using System.Windows.Input;
using MugenMvvm.Busy;
using MugenMvvm.Busy.Components;
using MugenMvvm.Commands;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Internal;
using MugenMvvm.Messaging;
using MugenMvvm.Messaging.Components;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTests.Messaging.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.UnitTests.Views.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.ViewModels.Components;
using MugenMvvm.Views;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.ViewModels.Components
{
    public class ViewModelCleanerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldClearBusyManager()
        {
            var manager = new ViewModelManager();
            manager.AddComponent(new ViewModelCleaner(new ViewManager()));
            var viewModel = new TestCleanerViewModel
            {
                BusyManager = new BusyManager()
            };
            viewModel.BusyManager.AddComponent(new BusyManagerComponent());
            var busyToken = viewModel.BusyManager.BeginBusy(this);
            manager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this);
            busyToken.IsCompleted.ShouldBeTrue();
            viewModel.BusyManager.Components.Count.ShouldEqual(0);
        }

        [Fact]
        public void ShouldClearMessenger()
        {
            var manager = new ViewModelManager();
            manager.AddComponent(new ViewModelCleaner(new ViewManager()));
            var viewModel = new TestCleanerViewModel
            {
                Messenger = new Messenger()
            };
            viewModel.Messenger.AddComponent(new MessengerHandlerSubscriber());
            viewModel.Messenger.TrySubscribe(new TestMessengerHandlerRaw()).ShouldBeTrue();
            manager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this);
            viewModel.Messenger.GetSubscribers().AsList().ShouldBeEmpty();
            viewModel.Messenger.Components.Count.ShouldEqual(0);
        }

        [Fact]
        public void ShouldClearAttachedValues()
        {
            var manager = new ViewModelManager();
            manager.AddComponent(new ViewModelCleaner(new ViewManager()));
            var viewModel = new TestCleanerViewModel();
            const string attachedPath = "t";
            MugenService.AttachedValueManager.TryGetAttachedValues(viewModel).Set(attachedPath, this, out _);
            manager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this);
            MugenService.AttachedValueManager.TryGetAttachedValues(viewModel).Contains(attachedPath).ShouldBeFalse();
        }

        [Fact]
        public void ShouldClearMetadata()
        {
            var manager = new ViewModelManager();
            manager.AddComponent(new ViewModelCleaner(new ViewManager()));
            var viewModel = new TestCleanerViewModel();
            viewModel.Metadata.Set(ViewModelMetadata.LifecycleState, ViewModelLifecycleState.Created);
            manager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this);
            viewModel.Metadata.TryGet(ViewModelMetadata.LifecycleState, out _).ShouldBeFalse();
        }

        [Fact]
        public void ShouldClearWeakReference()
        {
            var manager = new ViewModelManager();
            manager.AddComponent(new ViewModelCleaner(new ViewManager()));
            var viewModel = new TestCleanerViewModel();
            viewModel.Value = new WeakReferenceImpl(viewModel, false);
            manager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this);
            viewModel.Value.Target.ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldDisposeViewModels(bool dispose)
        {
            var manager = new ViewModelManager();
            manager.AddComponent(new ViewModelCleaner(new ViewManager()) {CleanupCommands = dispose});
            var viewModel = new TestCleanerViewModel {Command1 = new CompositeCommand(), Command2 = new CompositeCommand()};
            manager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this);
            ((CompositeCommand) viewModel.Command1).IsDisposed.ShouldEqual(dispose);
            ((CompositeCommand) viewModel.Command2).IsDisposed.ShouldEqual(dispose);
        }

        [Fact]
        public void ShouldClearViews()
        {
            var viewModel = new TestCleanerViewModel();
            var view = new View(new ViewMapping("1", GetType(), typeof(IViewModelBase)), this, viewModel);
            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (o, arg3) =>
                {
                    o.ShouldEqual(viewModel);
                    arg3.ShouldEqual(DefaultMetadata);
                    return view;
                }
            });
            var cleanupCount = 0;
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryCleanupAsync = (v, o, arg4, arg5) =>
                {
                    ++cleanupCount;
                    v.ShouldEqual(view);
                    arg4.ShouldEqual(DefaultMetadata);
                    return Default.TrueTask;
                }
            });
            var manager = new ViewModelManager();
            manager.AddComponent(new ViewModelCleaner(viewManager));
            manager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this, DefaultMetadata);
            cleanupCount.ShouldEqual(1);
        }

        #endregion

        #region Nested types

        private sealed class TestCleanerViewModel : TestViewModel, IHasService<IBusyManager>, IHasService<IMessenger>, IValueHolder<IWeakReference>
        {
            #region Properties

            public ICommand? Command1 { get; set; }

            public ICommand? Command2 { get; set; }

            public IBusyManager? BusyManager { get; set; }

            public IMessenger? Messenger { get; set; }

            IBusyManager IHasService<IBusyManager>.Service => throw new NotSupportedException();

            IMessenger IHasService<IMessenger>.Service => throw new NotSupportedException();

            IBusyManager? IHasService<IBusyManager>.ServiceOptional => BusyManager;

            IMessenger? IHasService<IMessenger>.ServiceOptional => Messenger;

            public IWeakReference? Value { get; set; }

            #endregion
        }

        #endregion
    }
}