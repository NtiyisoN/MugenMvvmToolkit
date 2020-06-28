﻿using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Metadata;

namespace MugenMvvm.Serialization
{
    public sealed class SerializationContext : MetadataOwnerBase, ISerializationContext
    {
        #region Constructors

        public SerializationContext(IReadOnlyMetadataContext? metadata = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(metadata, metadataContextProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            this.ClearMetadata(true);
        }

        #endregion
    }
}