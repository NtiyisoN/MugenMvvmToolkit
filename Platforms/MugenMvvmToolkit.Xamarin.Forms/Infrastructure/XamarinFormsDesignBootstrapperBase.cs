﻿#region Copyright

// ****************************************************************************
// <copyright file="XamarinFormsDesignBootstrapperBase.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

namespace MugenMvvmToolkit.Xamarin.Forms.Infrastructure
{
    public abstract class XamarinFormsDesignBootstrapperBase : XamarinFormsBootstrapperBase
    {
        #region Constructors

        protected XamarinFormsDesignBootstrapperBase(IPlatformService platformService = null, bool isDesignMode = true) : base(platformService, isDesignMode)
        {
        }

        #endregion

        #region Methods

        public sealed override void Start()
        {
        }

        #endregion
    }
}