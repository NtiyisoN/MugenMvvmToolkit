﻿using System;
using System.Windows.Input;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class EventTargetValueInterceptorBindingComponent : ITargetValueSetterBindingComponent, IDetachableComponent, IEventListener, IHasEventArgsBindingComponent
    {
        #region Fields

        private readonly IBindingManager? _bindingManager;
        private EventHandler? _canExecuteHandler;
        private IReadOnlyMetadataContext? _currentMetadata;
        private object? _currentValue;
        private IMemberAccessorInfo? _enabledMember;
        private bool _isDisposed;
        private IWeakReference? _targetRef;
        private ActionToken _unsubscriber;

        #endregion

        #region Constructors

        public EventTargetValueInterceptorBindingComponent(object? commandParameter, bool toggleEnabledState, IBindingManager? bindingManager = null)
        {
            _bindingManager = bindingManager;
            CommandParameter = commandParameter;
            ToggleEnabledState = toggleEnabledState;
        }

        #endregion

        #region Properties

        public object? CommandParameter { get; }

        public bool ToggleEnabledState { get; }

        bool IEventListener.IsAlive => !_isDisposed;

        bool IEventListener.IsWeak => false;

        public object? EventArgs { get; private set; }

        #endregion

        #region Implementation of interfaces

        bool IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            _unsubscriber.Dispose();
            ClearValue();
            _canExecuteHandler = null;
            _isDisposed = true;
        }

        bool IEventListener.TryHandle(object sender, object? message)
        {
            if (_isDisposed)
                return false;

            try
            {
                OnBeginEvent(sender, message);
                EventArgs = message;
                switch (_currentValue)
                {
                    case ICommand command:
                        command.Execute(GetCommandParameter());
                        return true;
                    case IExpressionValue expression:
                        expression.Invoke(_currentMetadata);
                        return true;
                }

                return false;
            }
            catch (Exception e)
            {
                OnEventError(e, sender, message);
                return true;
            }
            finally
            {
                EventArgs = null;
                OnEndEvent(sender, message);
            }
        }

        public bool TrySetTargetValue(IMemberPathObserver targetObserver, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata)
        {
            if (ReferenceEquals(value, _currentValue))
                return true;

            if (_unsubscriber.IsEmpty)
            {
                if (!(targetMember.Member is IEventInfo eventInfo))
                    return false;

                _unsubscriber = eventInfo.TrySubscribe(targetMember.Target, this, metadata);
                if (_unsubscriber.IsEmpty)
                    return false;
            }

            ClearValue();
            _currentMetadata = metadata;

            if (value == null)
            {
                _unsubscriber.Dispose();
                return true;
            }

            if (value is ICommand command)
            {
                if (ToggleEnabledState && InitializeCanExecute(targetMember.Target, command))
                {
                    OnCanExecuteChanged();
                    command.CanExecuteChanged += _canExecuteHandler;
                }

                _currentValue = value;
                return true;
            }

            if (value is IExpressionValue expressionValue)
            {
                _currentValue = expressionValue;
                return true;
            }

            _unsubscriber.Dispose();
            return false;
        }

        #endregion

        #region Methods

        private object? GetCommandParameter()
        {
            if (CommandParameter is IExpressionValue expression)
                return expression.Invoke(_currentMetadata);
            return CommandParameter;
        }

        private bool InitializeCanExecute(object? target, ICommand command)
        {
            if (target == null)
                return false;
            if (command is IMediatorCommand m && !m.HasCanExecute)
                return false;

            _enabledMember = GetMemberProvider()
                    .ServiceIfNull()
                    .GetMember(target.GetType(), BindableMembers.Object.Enabled, MemberType.Property, MemberFlags.InstancePublic, _currentMetadata) as
                IMemberAccessorInfo;
            if (_enabledMember == null)
                return false;

            _targetRef = target.ToWeakReference();
            if (_canExecuteHandler == null)
            {
                _canExecuteHandler = MugenExtensions
                    .CreateWeakEventHandler<EventTargetValueInterceptorBindingComponent, EventArgs>(this, (closure, _, __) => closure.OnCanExecuteChanged())
                    .Handle;
            }

            return true;
        }

        private void OnCanExecuteChanged()
        {
            if (!(_currentValue is ICommand cmd))
                return;

            var target = _targetRef?.Target;
            if (target != null)
                SetEnabled(cmd.CanExecute(GetCommandParameter()), target);
        }

        private void SetEnabled(bool value, object? target = null)
        {
            var enabledMember = _enabledMember;
            if (enabledMember == null)
                return;

            if (target == null)
                target = _targetRef?.Target;
            if (target == null)
                return;

            enabledMember.SetValue(target, BoxingExtensions.Box(value), _currentMetadata);
        }

        private void ClearValue()
        {
            if (_canExecuteHandler != null && _currentValue is ICommand c)
            {
                c.CanExecuteChanged -= _canExecuteHandler;
                SetEnabled(true);
            }

            _targetRef = null;
            _enabledMember = null;
            _currentMetadata = null;
            _currentValue = null;
        }

        private void OnBeginEvent(object sender, object? message)
        {
            var components = _bindingManager.ServiceIfNull().GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IBindingEventHandlerComponent)?.OnBeginEvent(sender, message, _currentMetadata);
        }

        private void OnEndEvent(object sender, object? message)
        {
            var components = _bindingManager.ServiceIfNull().GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IBindingEventHandlerComponent)?.OnEndEvent(sender, message, _currentMetadata);
        }

        private void OnEventError(Exception exception, object sender, object? message)
        {
            var binding = _currentMetadata?.Get(BindingMetadata.Binding);
            if (binding != null)
                OnSourceUpdateFailed(binding, exception);

            var components = _bindingManager.ServiceIfNull().GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IBindingEventHandlerComponent)?.OnEventError(exception, sender, message, _currentMetadata);
        }

        private void OnSourceUpdateFailed(IBinding binding, Exception error)
        {
            var components = binding.GetComponents();
            var list = components.List;
            if (list == null)
                (components.Item as IBindingSourceListener)?.OnSourceUpdateFailed(binding, error, _currentMetadata);
            else
            {
                for (var i = 0; i < list.Length; i++)
                    (list[i] as IBindingSourceListener)?.OnSourceUpdateFailed(binding, error, _currentMetadata);
            }
        }

        private IMemberProvider GetMemberProvider()
        {
            if (_bindingManager == null)
                return MugenBindingService.MemberProvider;
            return _bindingManager.GetComponent<IBindingManager, IMemberProvider>(true).ServiceIfNull();
        }

        #endregion
    }
}