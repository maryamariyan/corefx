// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using Xunit;

namespace System.ComponentModel.Composition.Factories
{
    partial class PartFactory
    {
        // NOTE: Do not add any more behavior to this class, as ComposablePartTests.cs 
        // uses this to verify default behavior of the base class.
        private class NoOverridesComposablePart : ComposablePart
        {
            public NoOverridesComposablePart()
            {
            }

            public override IEnumerable<ExportDefinition> ExportDefinitions
            {
                get { return Enumerable.Empty<ExportDefinition>(); }
            }

            public override IEnumerable<ImportDefinition> ImportDefinitions
            {
                get { return Enumerable.Empty<ImportDefinition>(); }
            }

            public override object GetExportedValue(ExportDefinition definition)
            {
                throw new NotImplementedException();
            }

            public override void SetImport(ImportDefinition definition, IEnumerable<Export> exports)
            {
                throw new NotImplementedException();
            }
        }
    }
}
