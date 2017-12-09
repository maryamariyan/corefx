// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------

using System.ComponentModel.Composition.Hosting;
using Xunit;

namespace System.ComponentModel.Composition
{
    public class CompositionServiceTests
    {
        public interface IFoo { }

        public class CFoo : IFoo { }
        public class FooImporter
        {
            [Import]
            public ICompositionService CompositionService;

            [Import]
            public IFoo fooImporter { get; set; }
        }

        [Fact]
        public void SatisfyParts_NullArgument_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("part", () =>
            {
                var compositionService = new TypeCatalog().CreateCompositionService();
                compositionService.SatisfyImportsOnce(null);
            });
        }
    }
}


