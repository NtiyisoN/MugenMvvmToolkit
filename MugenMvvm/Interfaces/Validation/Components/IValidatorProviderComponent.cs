﻿using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation.Components
{
    public interface IValidatorProviderComponent : IComponent<IValidationManager>
    {
        IValidator? TryGetValidator<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}