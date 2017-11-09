// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.Factories
{
    internal static partial class PartDefinitionFactory
    {
        public static ComposablePartDefinition CreateAttributed()
        {
            return CreateAttributed(typeof(ComposablePart));
        }

        public static ComposablePartDefinition CreateAttributed(Type type)
        {
            return AttributedModelServices.CreatePartDefinition(type, null);
        }

        public static ComposablePartDefinition Create()
        {
            return new NoOverridesComposablePartDefinition();
        }

        public static ComposablePartDefinition Create(ComposablePart part)
        {
            return Create(part.Metadata, () => part, part.ImportDefinitions, part.ExportDefinitions);
        }

        public static ComposablePartDefinition Create(IDictionary<string, object> metadata,
                                              Func<ComposablePart> partCreator,
                                              IEnumerable<ImportDefinition> imports,
                                              IEnumerable<ExportDefinition> exports)
        {
            return Create(metadata, partCreator, () => imports, () => exports);
        }      

        public static ComposablePartDefinition Create(IDictionary<string, object> metadata,
                                                      Func<ComposablePart> partCreator,
                                                      Func<IEnumerable<ImportDefinition>> importsCreator,
                                                      Func<IEnumerable<ExportDefinition>> exportsCreator)
        {
            return new DerivedComposablePartDefinition(metadata, partCreator, importsCreator, exportsCreator);
        }        
    }
}
