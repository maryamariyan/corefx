// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System.ComponentModel.Composition.Hosting;
using System.UnitTesting;
using Xunit;

namespace System.ComponentModel.Composition
{
    public class CatalogExtensionsTests
    {
        [Fact]
        public void CreateCompositionService_NullCatalog_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("composablePartCatalog", () =>
            {
                CatalogExtensions.CreateCompositionService(null);
            });
        }

        [Fact]
        public void CreateCompositionService_ImmutableCatalog_ShouldSucceed()
        {
            //Create and dispose an empty immutable catalog, I.e no INotifyComposablePartCatalogChanged interface
            var catalog = new TypeCatalog();
            using(var cs = catalog.CreateCompositionService())
            {
                //Do nothing
            }
        }
    }
}
