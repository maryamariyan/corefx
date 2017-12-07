// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------

using System.Linq;
using System.UnitTesting;
using System.ComponentModel.Composition.Hosting;
using Xunit;
#if MEF_FEATURE_REGISTRATION
using System.ComponentModel.Composition.Registration;
#endif //MEF_FEATURE_REGISTRATION

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

#if MEF_FEATURE_REGISTRATION
        [Fact]
        public void SimpleComposition_ShouldSuceed()
        {
            var ctx = new RegistrationBuilder();
            ctx.ForType<CFoo>().Export<IFoo>();
            var catalog = new TypeCatalog(Helpers.GetEnumerableOfTypes(typeof(CFoo)), ctx);
            Assert.True(catalog.Parts.Count() != 0);

            var compositionService = catalog.CreateCompositionService();

            var importer = new FooImporter();
            compositionService.SatisfyImportsOnce(importer);

            Assert.NotNull(importer.fooImporter);

            Assert.NotNull(importer.CompositionService);
        }

        [Fact]
        public void MutateCatalog_ShouldThrowChangeRejectedException()
        {
            ExceptionAssert.Throws<ChangeRejectedException>(() =>
            {
                var ctx = new RegistrationBuilder();
                ctx.ForType<CFoo>().Export<IFoo>();
                var typeCatalog = new TypeCatalog(Helpers.GetEnumerableOfTypes(typeof(CFoo)), ctx);
                Assert.True(typeCatalog.Parts.Count() != 0);

                var aggregateCatalog = new AggregateCatalog();
                aggregateCatalog.Catalogs.Add(typeCatalog);

                var compositionService = aggregateCatalog.CreateCompositionService();

                //Add it again
                aggregateCatalog.Catalogs.Add(typeCatalog);
            });
        }
#endif //MEF_FEATURE_REGISTRATION
    }
}


