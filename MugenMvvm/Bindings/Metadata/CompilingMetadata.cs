﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.Bindings.Metadata
{
    public static class CompilingMetadata
    {
        #region Fields

        private static IMetadataContextKey<List<string>>? _compilingErrors;
        private static IMetadataContextKey<IParameterInfo>? _lambdaParameter;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<List<string>> CompilingErrors
        {
            get => _compilingErrors ??= GetBuilder(_compilingErrors, nameof(CompilingErrors)).Build();
            set => _compilingErrors = value;
        }

        [AllowNull]
        public static IMetadataContextKey<IParameterInfo> LambdaParameter
        {
            get => _lambdaParameter ??= GetBuilder(_lambdaParameter, nameof(LambdaParameter)).Build();
            set => _lambdaParameter = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(IMetadataContextKey<T>? _, string name) => MetadataContextKey.Create<T>(typeof(CompilingMetadata), name);

        #endregion
    }
}