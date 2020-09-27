﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Validation
{
    public sealed class Validator : ComponentOwnerBase<IValidator>, IValidator, IHasAddedCallbackComponentOwner, IHasRemovedCallbackComponentOwner, IHasAddingCallbackComponentOwner
    {
        #region Fields

        private IReadOnlyMetadataContext? _metadata;
        private int _state;

        private const int DisposedState = -1;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public Validator(IReadOnlyMetadataContext? metadata = null, IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            _metadata = metadata;
        }

        #endregion

        #region Properties

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => MugenExtensions.LazyInitialize(ref _metadata);

        public bool IsDisposed => _state == DisposedState;

        #endregion

        #region Implementation of interfaces

        void IHasAddedCallbackComponentOwner.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IValidatorComponent)
                GetComponents<IValidatorListener>().OnErrorsChanged(this, null, string.Empty, metadata);
        }

        bool IHasAddingCallbackComponentOwner.OnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => !IsDisposed;

        void IHasRemovedCallbackComponentOwner.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IValidatorComponent)
                GetComponents<IValidatorListener>().OnErrorsChanged(this, null, string.Empty, metadata);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _state, DisposedState) == DisposedState)
                return;
            base.GetComponents<IValidatorListener>().OnDisposed(this);
            base.GetComponents<IDisposable>().Dispose();
            this.ClearComponents();
            this.ClearMetadata(true);
        }

        public bool HasErrors(string? memberName = null, IReadOnlyMetadataContext? metadata = null) => GetComponents<IValidatorComponent>(metadata).HasErrors(this, memberName, metadata);

        public ItemOrList<object, IReadOnlyList<object>> GetErrors(string? memberName, IReadOnlyMetadataContext? metadata = null) => GetComponents<IValidatorComponent>(metadata).TryGetErrors(this, memberName, metadata);

        public IReadOnlyDictionary<string, object> GetErrors(IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IValidatorComponent>(metadata).TryGetErrors(this, metadata) ?? Default.ReadOnlyDictionary<string, object>();

        public Task ValidateAsync(string? memberName = null, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IValidatorComponent>(metadata).TryValidateAsync(this, memberName, cancellationToken, metadata);

        public void ClearErrors(string? memberName = null, IReadOnlyMetadataContext? metadata = null) => GetComponents<IValidatorComponent>(metadata).ClearErrors(this, memberName, metadata);

        #endregion

        #region Methods

        private new TComponent[] GetComponents<TComponent>(IReadOnlyMetadataContext? metadata = null)
            where TComponent : class =>
            IsDisposed ? Default.Array<TComponent>() : base.GetComponents<TComponent>(metadata);

        #endregion
    }
}