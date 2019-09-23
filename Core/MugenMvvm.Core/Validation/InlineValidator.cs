﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Validation
{
    public sealed class InlineValidator<T> : ValidatorBase<T> where T : class
    {
        #region Constructors

        public InlineValidator(IMetadataContext? metadata = null, IComponentCollectionProvider? componentCollectionProvider = null,
            IMetadataContextProvider? metadataContextProvider = null)
            : base(false, metadata, componentCollectionProvider, metadataContextProvider)
        {
        }

        #endregion

        #region Methods

        public void SetErrors(string memberName, params object[] errors)
        {
            SetErrors(memberName, null, errors);
        }

        public void SetErrors(string memberName, IReadOnlyMetadataContext? metadata = null, params object[] errors)
        {
            SetErrors(memberName, errors, metadata);
        }

        public void SetErrors(string memberName, IReadOnlyList<object> errors, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(memberName, nameof(memberName));
            UpdateErrors(memberName, errors, false, metadata);
        }

        protected override ValueTask<ValidationResult> GetErrorsAsync(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return ValidationResult.DoNothingTask;
        }

        #endregion
    }
}