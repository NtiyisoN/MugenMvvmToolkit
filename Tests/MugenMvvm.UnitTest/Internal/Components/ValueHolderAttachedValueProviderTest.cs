﻿using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal.Components;
using MugenMvvm.UnitTest.Internal.Internal;

namespace MugenMvvm.UnitTest.Internal.Components
{
    public class ValueHolderAttachedValueProviderTest : AttachedValueProviderTestBase
    {
        #region Methods

        protected override object GetSupportedItem()
        {
            return new TestValueHolder<LightDictionary<string, object?>>();
        }

        protected override IAttachedValueProviderComponent GetComponent()
        {
            return new ValueHolderAttachedValueProvider();
        }

        #endregion
    }
}