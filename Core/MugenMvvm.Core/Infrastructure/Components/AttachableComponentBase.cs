﻿using System.Threading;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Components
{
    public abstract class AttachableComponentBase<T> : IAttachableComponent<T>, IDetachableComponent<T> where T : class
    {
        #region Fields

        private T? _owner;
        private int _state;
        private const int DetachedState = 0;
        private const int AttachedState = 1;

        #endregion

        #region Properties

        protected T Owner
        {
            get
            {
                if (_owner == null)
                    ExceptionManager.ThrowObjectNotInitialized(this);
                return _owner!;
            }
            private set => _owner = value;
        }

        protected bool IsAttached => _state == AttachedState;

        #endregion

        #region Implementation of interfaces

        public void OnAttached(T owner, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(owner, nameof(owner));
            if (Interlocked.CompareExchange(ref _state, AttachedState, DetachedState) != DetachedState)
                ExceptionManager.ThrowObjectInitialized(this);

            Owner = owner;
            OnAttachedInternal(owner, metadata);
        }

        public void OnDetached(T owner, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(owner, nameof(owner));
            if (Interlocked.Exchange(ref _state, DetachedState) == DetachedState)
                return;

            OnDetachedInternal(owner, metadata);
            Owner = null!;
        }

        #endregion

        #region Methods

        protected virtual void OnAttachedInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual void OnDetachedInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
        }

        #endregion
    }
}