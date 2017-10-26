// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using System.Collections;

namespace System.ComponentModel.Composition.Factories
{
    public static partial class CatalogFactory
    {
        public static AggregateCatalog CreateAggregateCatalog()
        {
            return new AggregateCatalog();
        }

        public static ComposablePartCatalog Create()
        {
            return new NoOverridesComposablePartCatalog();
        }

        public static ComposablePartCatalog Create(params ComposablePart[] parts)
        {
            var definitions = parts.Select(part => PartDefinitionFactory.Create(part));

            return Create(definitions.ToArray());
        }

        public static ComposablePartCatalog Create(params ComposablePartDefinition[] definitions)
        {
            return new DerivedComposablePartCatalog(definitions);
        }

        public static ComposablePartCatalog CreateDefaultAttributed()
        {
            return CreateAttributed(typeof(CatalogFactory).Assembly);
        }

        public static ComposablePartCatalog CreateDisposable(Action<bool> disposeCallback)
        {
            return new DisposableComposablePartCatalog(disposeCallback);
        }

        public static ComposablePartCatalog CreateAttributed(Assembly assembly)
        {
            return new AssemblyCatalog(assembly);
        }

        public static ComposablePartCatalog CreateAttributed(params Type[] types)
        {
            return new TypeCatalog(types);
        }

        public static ComposablePartCatalog CreateNonFilteredAttributed(params Type[] types)
        {
            return new NonFilteringTypeCatalog(types);
        }

        public static ComposablePartCatalog CreateFiltered(ComposablePartCatalog catalog, Func<ComposablePartDefinition, bool> filter)
        {
            return new FilteredComposablePartCatalog(catalog, filter);
        }
    }
}
