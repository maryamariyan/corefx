// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.AttributedModel;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using Microsoft.CLR.UnitTesting;


namespace System.ComponentModel.Composition.Factories
{
    internal static partial class AdaptingContainerFactory
    {
        public static AdaptingCompositionContainer Create()
        {
            return Create((ComposablePart[])null);
        }

        public static AdaptingCompositionContainer Create(ComposablePartCatalog catalog)
        {
            return new AdaptingCompositionContainer(catalog);
        }

        public static AdaptingCompositionContainer Create(AdaptingCompositionContainer parent)
        {
            return new AdaptingCompositionContainer(parent);
        }

        public static AdaptingCompositionContainer Create(params ComposablePart[] parts)
        {
            return Create((AdaptingCompositionContainer)null, parts);
        }

        public static AdaptingCompositionContainer CreateWithDefaultAttributedCatalog()
        {
            var catalog = CatalogFactory.CreateDefaultAttributed();

            return Create(catalog);
        }

        public static AdaptingCompositionContainer CreateWithAttributedCatalog(params Type[] types)
        {
            var catalog = CatalogFactory.CreateAttributed(types);

            return Create(catalog);
        }

        public static AdaptingCompositionContainer CreateAttributed(params object[] parts)
        {
            var container = new AdaptingCompositionContainer();
            var partsArray = new ComposablePart[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                Assert.IsNotInstanceOfType(parts[i], typeof(Type), "You should be using CreateWithAttributedCatalog not CreateAttributed");
                partsArray[i] = PartFactory.CreateAttributed(parts[i]);
            }

            return Create(partsArray);
        }

        public static AdaptingCompositionContainer Create(AdaptingCompositionContainer parent, params ComposablePart[] parts)
        {
            AdaptingCompositionContainer container;
            if (parent == null)
            {
                container = new AdaptingCompositionContainer();
            }
            else
            {
                container = new AdaptingCompositionContainer(parent);
            }

            if (parts != null)
            {
                CompositionBatch batch = new CompositionBatch(parts, Enumerable.Empty<ComposablePart>());
                container.Compose(batch);
            }

            return container;
        }

        public static AdaptingCompositionContainer Create(params MicroExport[] exports)
        {
            var part = PartFactory.CreateExporter(exports);

            return Create(part);
        }

        public static AdaptingCompositionContainer Create(AdaptingCompositionContainer parent, params MicroExport[] exports)
        {
            var part = PartFactory.CreateExporter(exports);

            return Create(parent, part);
        }
    }
}
