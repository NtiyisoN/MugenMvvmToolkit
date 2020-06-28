﻿using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Serialization;
using MugenMvvm.UnitTest.Metadata;

namespace MugenMvvm.UnitTest.Serialization
{
    public class SerializationContextTest : MetadataOwnerTestBase
    {
        #region Methods

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata, IMetadataContextProvider? metadataContextProvider)
        {
            return new SerializationContext(metadata, metadataContextProvider);
        }

        #endregion
    }
}