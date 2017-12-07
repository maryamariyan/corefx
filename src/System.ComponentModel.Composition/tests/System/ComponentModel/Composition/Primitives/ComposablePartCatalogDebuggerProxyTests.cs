// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System.ComponentModel.Composition.Factories;
using System.UnitTesting;
using Xunit;

namespace System.ComponentModel.Composition.Primitives
{
    public class ComposablePartCatalogDebuggerProxyTests
    {
        [Fact]
        public void Constructor_NullAsCatalogArgument_ShouldThrowArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("catalog", () =>
            {
                new ComposablePartCatalogDebuggerProxy((ComposablePartCatalog)null);
            });
        }

        [Fact]
        public void Constructor_ValueAsCatalogArgument_ShouldSetPartsProperty()
        {
            var expectations = Expectations.GetCatalogs();
            foreach (var e in expectations)
            {
                var proxy = new ComposablePartCatalogDebuggerProxy(e);

                EqualityExtensions.CheckEquals(e.Parts, proxy.Parts);
            }
        }

        [Fact]
        [ActiveIssue(812029)]
        public void Parts_ShouldNotCacheUnderlyingParts()
        {
            var catalog = CatalogFactory.CreateAggregateCatalog();
            var proxy = CreateComposablePartCatalogDebuggerProxy(catalog);

            Assert.Empty(proxy.Parts);

            var expectations = Expectations.GetCatalogs();
            foreach (var e in expectations)
            {
                catalog.Catalogs.Add(e);

                EqualityExtensions.CheckEquals(catalog.Parts, proxy.Parts);

                catalog.Catalogs.Remove(e);
            }
        }

        private ComposablePartCatalogDebuggerProxy CreateComposablePartCatalogDebuggerProxy(ComposablePartCatalog catalog)
        {
            return new ComposablePartCatalogDebuggerProxy(catalog);
        }
   }
}
