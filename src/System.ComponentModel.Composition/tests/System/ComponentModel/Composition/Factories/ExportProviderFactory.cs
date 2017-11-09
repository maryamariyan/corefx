// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System.ComponentModel.Composition.Hosting;

namespace System.ComponentModel.Composition.Factories
{
    internal partial class ExportProviderFactory
    {
        public static ExportProvider Create()
        {
            return new NoOverridesExportProvider();
        }

        public static RecomposableExportProvider CreateRecomposable()
        {
            return new RecomposableExportProvider();
        }
    }
}
