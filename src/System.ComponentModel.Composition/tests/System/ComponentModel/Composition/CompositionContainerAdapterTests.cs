// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.UnitTesting;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CLR.UnitTesting;
using System.UnitTesting;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition
{
    // TODO: Rename and change these tests so that they are AdaptingExportProvider tests
    [TestClass]
    public class CompositionContainerAdapterTests
    {
        [TestMethod]
        public void ChainedAdapter_ShouldNotBeCalledToAdapt()
        {   // Tests that chaining adapters is not supported, ie that 
            // OldContract -> NewContract -> NewerContract does not work

            var adapter1 = AdapterFactory.CreateAdapter("OldContract", "NewContract");
            var adapter2 = AdapterFactory.CreateAdapter("NewContract", "NewerContract");

            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", 1, 2, 3));
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(adapter1);
            batch.AddPart(adapter2);
            container.Compose(batch);

            var exports = container.GetExports(typeof(object), (Type)null, "NewerContract");

            Assert.AreEqual(0, exports.Count());

            exports = container.GetExports(typeof(object), (Type)null, "NewContract");

            Assert.AreEqual(3, exports.Count());
        }

        [TestMethod]
        public void MultipleAdaptersForSameContract_ShouldDuplicateExports()
        {
            var adapter1 = AdapterFactory.CreateAdapter("OldContract", "NewContract");
            var adapter2 = AdapterFactory.CreateAdapter("OldContract", "NewContract");

            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", 1, 2, 3));
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(adapter1);
            batch.AddPart(adapter2);
            container.Compose(batch);

            var exports = container.GetExports(typeof(int), (Type)null, "NewContract");

            ExportsAssert.AreEqual(exports, 1, 2, 3, 1, 2, 3);
        }

        [TestMethod]
        public void AddingAdapterAfterCompose_ShouldRecomposeNewContractsViaGetExports()
        {
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", 4, 5, 6),
                                                                     new MicroExport("NewContract", 1, 2, 3));

            var exports = container.GetExports<int>("NewContract");
            Assert.AreEqual(3, exports.Count());

            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(AdapterFactory.CreateAdapter("OldContract", "NewContract"));
            container.Compose(batch);

            exports = container.GetExports<int>("NewContract");

            ExportsAssert.AreEqual(exports, 1, 2, 3, 4, 5, 6);
        }

        [TestMethod]
        public void AddingAdapterAfterCompose_ShouldRecomposeNewContractsViaParts()
        {
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", "Value"));
            var importer = PartFactory.CreateImporter("NewContract", true);
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(importer);
            container.Compose(batch);
            Assert.IsNull(importer.Value);
            Assert.AreEqual(1, importer.ImportSatisfiedCount);

            batch = new CompositionBatch();
            batch.AddPart(AdapterFactory.CreateAdapter("OldContract", "NewContract"));            
            container.Compose(batch);

            Assert.AreEqual("Value", importer.Value);
            Assert.AreEqual(2, importer.ImportSatisfiedCount);
        }

        [TestMethod]
        public void RemovingAdapterAfterCompose_ShouldRecomposeNewContractsViaParts()
        {
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", "Value"));
            CompositionBatch batch = new CompositionBatch();
            var importer = PartFactory.CreateImporter("NewContract", true);
            var adapter = AdapterFactory.CreateAdapter("OldContract", "NewContract");
            batch.AddPart(importer);
            batch.AddPart(adapter);
            container.Compose(batch);

            Assert.AreEqual("Value", importer.Value);
            Assert.AreEqual(1, importer.ImportSatisfiedCount);

            batch = new CompositionBatch();
            batch.RemovePart(adapter);
            container.Compose(batch);

            Assert.IsNull(importer.Value);
            Assert.AreEqual(2, importer.ImportSatisfiedCount);
        }

        [TestMethod]
        public void RemovingAndAddingAdapterAfterCompose_ShouldRecomposeNewContractsViaParts()
        {
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", "Value"));
            CompositionBatch batch = new CompositionBatch();
            var importer = PartFactory.CreateImporter("NewContract", true);
            var adapter1 = AdapterFactory.CreateAdapter("OldContract", "NewContract");
            var adapter2 = AdapterFactory.CreateAdapter("OldContract", "NewContract", export => ExportFactory.Create("NewContract", () => "AnotherValue"));
            batch.AddPart(importer);
            batch.AddPart(adapter1);
            container.Compose(batch);

            Assert.AreEqual("Value", importer.Value);
            Assert.AreEqual(1, importer.ImportSatisfiedCount);

            batch = new CompositionBatch();
            batch.RemovePart(adapter1);
            batch.AddPart(adapter2);
            container.Compose(batch);

            Assert.AreEqual(2, importer.ImportSatisfiedCount);
            Assert.AreEqual("AnotherValue", importer.Value);
        }

        [TestMethod]
        public void AddingAdapter_ShouldNotRecomposeUnrelatedParts()
        {
            var adapter1 = AdapterFactory.CreateAdapter("OldContract1", "NewContract1");
            var importer = PartFactory.CreateImporter("NewContract1");
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(adapter1, importer);
            CompositionBatch batch = new CompositionBatch();
            container.Compose(batch);

            Assert.AreEqual(1, importer.ImportSatisfiedCount);

            var adapter2 = AdapterFactory.CreateAdapter("OldContract2", "NewContract2");

            batch = new CompositionBatch();
            batch.AddPart(adapter2);
            container.Compose(batch);

            Assert.AreEqual(1, importer.ImportSatisfiedCount);
        }

        [TestMethod]
        public void RemovingAdapter_ShouldNotRecomposeUnrelatedParts()
        {
            var adapter1 = AdapterFactory.CreateAdapter("OldContract1", "NewContract1");
            var adapter2 = AdapterFactory.CreateAdapter("OldContract2", "NewContract2");
            var importer = PartFactory.CreateImporter("NewContract2");
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(adapter1, adapter2, importer);
            CompositionBatch batch = new CompositionBatch();
            container.Compose(batch);

            Assert.AreEqual(1, importer.ImportSatisfiedCount);

            batch = new CompositionBatch();
            batch.RemovePart(adapter1);
            container.Compose(batch);

            Assert.AreEqual(1, importer.ImportSatisfiedCount);
        }

        [TestMethod]
        public void AddingAnAdaptedContract_ShouldRecomposeNewContractsViaGetExports()
        {
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", 1, 2, 3));
            var adapter = AdapterFactory.CreateAdapter("OldContract", "NewContract");
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(adapter);
            container.Compose(batch);

            var exports = container.GetExports<int>("NewContract");
            Assert.AreEqual(3, exports.Count());

            batch = new CompositionBatch();
            batch.AddExportedValue("OldContract", 4);
            container.Compose(batch);

            exports = container.GetExports<int>("NewContract");
            ExportsAssert.AreEqual(exports, 1, 2, 3, 4);
        }

        [TestMethod]
        public void AddingAnAdaptedContract_ShouldRecomposeNewContractsViaParts()
        {
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(); 
            CompositionBatch batch = new CompositionBatch();
            var adapter = AdapterFactory.CreateAdapter("OldContract", "NewContract");
            var importer = PartFactory.CreateImporter("NewContract", true);
            batch.AddPart(importer);
            batch.AddPart(adapter);
            container.Compose(batch);

            Assert.IsNull(importer.Value);
            Assert.AreEqual(1, importer.ImportSatisfiedCount);

            batch = new CompositionBatch();
            batch.AddExportedValue("OldContract", "Value");
            container.Compose(batch);

            Assert.AreEqual("Value", importer.Value);
            Assert.AreEqual(2, importer.ImportSatisfiedCount);
        }

        [TestMethod]
        public void RemovingAnAdapter_ShouldRecomposeNewContracts()
        {
            var adapter = AdapterFactory.CreateAdapter("OldContract", "NewContract");
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", 1, 2, 3));
            CompositionBatch batch = new CompositionBatch();

            batch.AddPart(adapter);
            container.Compose(batch);

            var exports = container.GetExports(typeof(int), (Type)null, "NewContract");
            Assert.AreEqual(3, exports.Count());

            batch = new CompositionBatch();
            batch.RemovePart(adapter);
            container.Compose(batch);

            exports = container.GetExports(typeof(int), (Type)null, "NewContract");
            Assert.AreEqual(0, exports.Count());
        }

        [TestMethod]
        public void AdapterWithSameToAndFromContract_ShouldThrowRollback()
        {
            var adapter = AdapterFactory.CreateAdapter("Contract", "Contract");
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("Contract", 1));
            CompositionBatch batch = new CompositionBatch();

            batch.AddPart(adapter);
            
            // After rejection adapters must be executed to support events during recomposition,
            // so this can be detected early (and is actually prevented from getting into the
            // container as a result)
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.Unknown, () =>
            {
                container.Compose(batch);
            });
        }

        [TestMethod]
        public void AdapterWithSameToAndFromContractType_ShouldThrowRollback()
        {
            var adapter = AdapterFactory.CreateAdapter(typeof(string), typeof(string));
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport(typeof(string), 1));
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(adapter);
            
            // After rejection adapters must be executed to support events during recomposition,
            // so this can be detected early (and is actually prevented from getting into the
            // container as a result)
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.Unknown, () =>
            {
                container.Compose(batch);
            });
        }

        [TestMethod]
        public void AdapterWithSameToAndFromContractWithNoExportsMatchingFromContract_ShouldThrowRollback()
        {
            var adapter = AdapterFactory.CreateAdapter("Contract", "Contract");
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(); 
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(adapter);
            
            // After rejection adapters must be executed to support events during recomposition,
            // so this can be detected early (and is actually prevented from getting into the
            // container as a result)
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.Unknown, () =>
            {
                container.Compose(batch);
            });
        }

        [TestMethod]
        public void AdapterWithSameToAndFromContractTypeWithNoExportsMatchingFromContract_ShouldThrowRollback()
        {
            var adapter = AdapterFactory.CreateAdapter(typeof(string), typeof(string));
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(); 
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(adapter);
            
            // After rejection adapters must be executed to support events during recomposition,
            // so this can be detected early (and is actually prevented from getting into the
            // container as a result)
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.Unknown, () =>
            {
                container.Compose(batch);
            });
        }

        [TestMethod]
        public void AdapterWithNullFromContract_ShouldThrowRollback()
        {
            var adapter = AdapterFactory.CreateAdapter((string)null, "Contract");
            
            // After rejection adapters must be executed to support events during recomposition,
            // so this can be detected early (and is actually prevented from getting into the
            // container as a result)
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.Unknown, () =>
            {
                AdaptingCompositionContainer container = AdaptingContainerFactory.Create(adapter);
            });

        }

        [TestMethod]
        public void AdapterWithNullToContract_ShouldThrowRollback()
        {
            var adapter = AdapterFactory.CreateAdapter("OldContract", (string)null);

            // After rejection adapters must be executed to support events during recomposition,
            // so this can be detected early (and is actually prevented from getting into the
            // container as a result)
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.Unknown, () =>
            {
                AdaptingCompositionContainer container = AdaptingContainerFactory.Create(adapter);
            });
        }

        [TestMethod]
        public void AdapterWithEmptyFromContract_ShouldThrowRollback()
        {
            var adapter = AdapterFactory.CreateAdapter("", "Contract");

            // After rejection adapters must be executed to support events during recomposition,
            // so this can be detected early (and is actually prevented from getting into the
            // container as a result)
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.Unknown,  () =>
            {
                AdaptingCompositionContainer container = AdaptingContainerFactory.Create(adapter);
            });
        }

        [TestMethod]
        public void AdapterWithEmptyToContract_ShouldThrowRollback()
        {
            var adapter = AdapterFactory.CreateAdapter("OldContract", "");
            
            // After rejection adapters must be executed to support events during recomposition,
            // so this can be detected early (and is actually prevented from getting into the
            // container as a result)
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.Unknown, () =>
            {
                AdaptingCompositionContainer container = AdaptingContainerFactory.Create(adapter);
            });
        }

        [TestMethod]
        public void AdapterAlwaysReturningNull_ShouldNotAddToAvailableExports()
        {
            var adapter = AdapterFactory.CreateAdapter("OldContract", "NewContract", export => null);
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", "Value"));
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(adapter);
            container.Compose(batch);

            var exports = container.GetExportedValues<string>("NewContract");

            EnumerableAssert.IsEmpty(exports);
        }

        [TestMethod]
        public void AdapterSometimesReturningNull_ShouldNotAddToAvailableExports()
        {
            int count = 0;
            Func<Export, Export> adapt = export =>
            {
                count++;
                if (count % 2 == 0)
                {
                    return new Export(ExportDefinitionFactory.Create("NewContract", export.Metadata), () => export.Value);
                }

                return null;
            };

            var adapter = AdapterFactory.CreateAdapter("OldContract", "NewContract", adapt);
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", "Value1", "Value2"));
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(adapter);
            container.Compose(batch);

            var exports = container.GetExportedValues<string>("NewContract");
            
            EnumerableAssert.AreEqual(exports, "Value2");
        }

        [TestMethod]
        public void AdapterAddedDuringCompositionWithImporterWithZeroOrMoreCardinality_ShouldBeUsed()
        {
            var adapter = AdapterFactory.CreateAdapter("OldContract", "NewContract");
            var importer = PartFactory.CreateImporter("NewContract", ImportCardinality.ZeroOrMore, true);

            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", "Value"));
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(importer);
            batch.AddPart(new CallbackExecuteCodeDuringCompose(() => 
                {
                    CompositionBatch nestedBatch = new CompositionBatch();
                    nestedBatch.AddPart(adapter);
                    container.Compose(nestedBatch);
                }));

            container.Compose(batch);

            Assert.AreEqual(2, importer.ImportSatisfiedCount);
            Assert.AreEqual(importer.Value, "Value");
        }

        [TestMethod]
        public void AdapterAddedDuringCompositionWithImporterWithZeroOrOneCardinality_ShouldBeUsed()
        {
            var adapter = AdapterFactory.CreateAdapter("OldContract", "NewContract");
            var importer = PartFactory.CreateImporter("NewContract", ImportCardinality.ZeroOrOne, true);

            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", "Value"));
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(importer);
            batch.AddPart(new CallbackExecuteCodeDuringCompose(() =>
            {
                CompositionBatch nestedBatch = new CompositionBatch();
                nestedBatch.AddPart(adapter);
                container.Compose(nestedBatch);
            }));

            container.Compose(batch);

            Assert.AreEqual(2, importer.ImportSatisfiedCount);
            Assert.AreEqual(importer.Value, "Value");
        }

        [TestMethod]
        public void ExportAddedDuringComposition_ShouldBeUsed()
        {
            var adapter = AdapterFactory.CreateAdapter("OldContract", "NewContract");
            var importer = PartFactory.CreateImporter("NewContract", ImportCardinality.ZeroOrMore, true);

            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(); 
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(importer);
            batch.AddPart(adapter);
            batch.AddPart(new CallbackExecuteCodeDuringCompose(() =>
            {
                CompositionBatch nestedBacth = new CompositionBatch();
                nestedBacth.AddExportedValue("OldContract", "Value");
                container.Compose(nestedBacth);
            }));

            container.Compose(batch);

            Assert.AreEqual(2, importer.ImportSatisfiedCount);
            Assert.AreEqual(importer.Value, "Value");
        }

        [TestMethod]
        public void PartWithAdaptMethodAndFromAndToMetadata_ShouldNotBeUsedAsAdapter()
        {
            var metadata = AdapterFactory.CreateAdapterMetadata("OldContract", "NewContract");
            Func<Export, Export> adapt = export => export;

            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("NotAnAdapter", metadata, adapt),
                                                    new MicroExport("OldContract", "Value"));

            Assert.IsFalse(container.IsPresent("NewContract"));
        }

        [TestMethod]
        public void AdapterExportingNull_ShouldThrowRollback()
        {
            IDictionary<string, object> metadata = AdapterFactory.CreateAdapterMetadata("OldContract", "NewContract");

            // After rejection adapters must be executed to support events during recomposition,
            // so this can be detected early (and is actually prevented from getting into the
            // container as a result)
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.Unknown, () =>
            {
                AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", 1),
                                                    new MicroExport(AdaptationConstants.AdapterContractName, typeof(int), metadata, new object[] { null }));
            });
        }

        [TestMethod]
        public void AdapterExportingInt32_ShouldThrowRollback()
        {
            IDictionary<string, object> metadata = AdapterFactory.CreateAdapterMetadata("OldContract", "NewContract");

            // After rejection adapters must be executed to support events during recomposition,
            // so this can be detected early (and is actually prevented from getting into the
            // container as a result)
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.Unknown, () =>
            {
                AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", 1),
                                                        new MicroExport(AdaptationConstants.AdapterContractName, metadata, 1));
            });
        }

        [TestMethod]
        public void AdapterExportingFuncTakingExportOfT_ShouldThrowRollback()
        {
            IDictionary<string, object> metadata = AdapterFactory.CreateAdapterMetadata("OldContract", "NewContract");

            Func<Lazy<string>, Export> adapter = (e) => null;

            // After rejection adapters must be executed to support events during recomposition,
            // so this can be detected early (and is actually prevented from getting into the
            // container as a result)
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.Unknown, () =>
            {
                AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", 1),
                                                    new MicroExport(AdaptationConstants.AdapterContractName, metadata, adapter));
            });
        }

        [TestMethod]
        public void AdapterExportingFuncReturningObject_ShouldThrowRollback()
        {
            IDictionary<string, object> metadata = AdapterFactory.CreateAdapterMetadata("OldContract", "NewContract");

            Func<Export, object> adapter = (e) => null;

            // After rejection adapters must be executed to support events during recomposition,
            // so this can be detected early (and is actually prevented from getting into the
            // container as a result)
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.Unknown, () =>
            {
                AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", 1),
                                                        new MicroExport(AdaptationConstants.AdapterContractName, metadata, adapter));
            });
            
        }

        [TestMethod]
        public void AdapterThrowingDuringAdaptViaGetExport_ShouldThrowRollback()
        {
            Exception exceptionToThrow = new Exception();
            var adapter = AdapterFactory.CreateAdapter("OldContract", "NewContract", e =>
            {
                throw exceptionToThrow;
            });

            var adaptMethodDefinition = (ICompositionElement)adapter.ExportDefinitions.First();

            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", 1));
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(adapter);
            
            // After rejection adapters must be executed to support events during recomposition,
            // so this can be detected early (and is actually prevented from getting into the
            // container as a result)
            ExceptionAssert.Throws<ChangeRejectedException>(() =>
            {
                container.Compose(batch);
            });
        }

        [TestMethod]
        public void AdapterThrowingDuringAdaptViaPartWithOneImport_ShouldThrowRollback()
        {
            Exception exceptionToThrow = new Exception();
            var adapter = AdapterFactory.CreateAdapter("OldContract", "NewContract", export =>
            {
                throw exceptionToThrow;
            });

            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", "Value"));
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(adapter);
            
            // After rejection adapters must be executed to support events during recomposition,
            // so this can be detected early (and is actually prevented from getting into the
            // container as a result)
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.Unknown,
                RetryMode.DoNotRetry, () =>
            {
                container.Compose(batch);
            });
        }

        [TestMethod]
        public void AdapterThrowingDuringAdaptViaPartWithMultipleImports_ShouldThrowRollback()
        {
            Exception exceptionToThrow = new Exception();
            var adapter = AdapterFactory.CreateAdapter("OldContract", "NewContract", export =>
            {
                throw exceptionToThrow;
            });

            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", "Value"));
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(adapter);
            
            // After rejection adapters must be executed to support events during recomposition,
            // so this can be detected early (and is actually prevented from getting into the
            // container as a result)
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.Unknown,
                                          RetryMode.DoNotRetry, () =>
            {
                container.Compose(batch);
            });
        }

        [TestMethod]
        public void AdapterReturningExportWithDifferentContractThanToContract_ShouldThrowRollback()
        {
            var adapter = AdapterFactory.CreateAdapter("OldContract", "NewContract", export => export);

            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", "Value"));
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(adapter);
            
            // After rejection adapters must be executed to support events during recomposition,
            // so this can be detected early (and is actually prevented from getting into the
            // container as a result)
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.Unknown, () =>
            {
                container.Compose(batch);
            });
        }

        [TestMethod]
        public void AdapterReturningExportWithDifferentContractThanToContractBasedOnCase_ShouldThrowRollback()
        {
            var adapter = AdapterFactory.CreateAdapter("OldContract", "NewContract", export => 
                {
                    return ExportFactory.Create("newcontract", () => export.Value);                    
                });

            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", "Value"));
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(adapter);

            // After rejection adapters must be executed to support events during recomposition,
            // so this can be detected early (and is actually prevented from getting into the
            // container as a result)
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.Unknown,
                                          RetryMode.DoNotRetry, () =>
                                          {
                                              container.Compose(batch);
                                          });
        }

        [TestMethod]
        public void RecomposingOfAnAdaptedContract_ShouldRecomposeNewContractOnce()
        {
            var importer = PartFactory.CreateImporter("NewContract", true);
            var adapter1 = AdapterFactory.CreateAdapter("OldContract", "NewContract");
            var adapter2 = AdapterFactory.CreateAdapter("OldContract", "NewContract");
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", 1));
            CompositionBatch batch = new CompositionBatch();

            batch.AddParts(importer, adapter1, adapter2);
            container.Compose(batch);

            Assert.AreEqual(1, importer.ImportSatisfiedCount);

            batch = new CompositionBatch();
            batch.AddExportedValue("OldContract", 2);
            container.Compose(batch);

            Assert.AreEqual(2, importer.ImportSatisfiedCount);
        }

        [TestMethod]
        public void AdapterAloneInContainer_ShouldNotThrow()
        {
            var adapter = AdapterFactory.CreateAdapter("OldContract", "NewContract");
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(adapter);
        }

        [TestMethod]
        public void AskingForToContractForAdapterWithNoFromContracts_ShouldReturnNull()
        {
            var adapter = AdapterFactory.CreateAdapter("OldContract", "NewContract");
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(adapter);

            Assert.IsFalse(container.IsPresent("NewContract"));
        }

        [TestMethod]
        public void AdapterInContainer_DoesNotCauseExportToBePulledWhenAdapting()
        {
            var adapter = AdapterFactory.CreateAdapter("OldContract", "NewContract");
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(adapter);
            CompositionBatch batch = new CompositionBatch();

            int calledCount = 0;

            batch.AddExport(ExportFactory.Create("OldContract", () =>
            {
                calledCount++;
                return "Value";
            }));

            container.Compose(batch);

            Assert.AreEqual(0, calledCount);

            var exports = container.GetExports(typeof(object), (Type)null, "NewContract");

            Assert.AreEqual(0, calledCount);
            Assert.AreEqual(1, exports.Count());
            Assert.AreEqual("Value", exports.First().Value);
            Assert.AreEqual(1, calledCount);
        }

        [TestMethod]
        public void ReflectionAdapterInContainer_CanAdapt()
        {
            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(new MicroExport("OldContract", 1));
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(new Adapter());
            container.Compose(batch);

            var export = container.GetExport<int>("NewContract");

            Assert.AreEqual(1, export.Value);
        }
        [TestMethod]
        public void GetExportOfT1_IntToStringAdapterInContainerWithOneExport_ShouldReturnOneExport()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", typeof(string),
                                                                          new MicroExport("OldContract", 10));

            var export = container.GetExport<string>();

            Assert.AreEqual("10", export.Value);
        }

        [TestMethod]
        public void GetExportOfT2_IntToStringAdapterInContainerWithOneExport_ShouldReturnOneExport()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 10));

            var export = container.GetExport<string>("NewContract");

            Assert.AreEqual("10", export.Value);
        }

        [TestMethod]
        public void GetExportOfTTMetadataView1_IntToStringAdapterInContainerWithOneExport_ShouldReturnOneExport()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", typeof(string),
                                                                          new MicroExport("OldContract", 10));

            var export = container.GetExport<string, object>();

            Assert.AreEqual("10", export.Value);
        }

        [TestMethod]
        public void GetExportOfTTMetadataView2_IntToStringAdapterInContainerWithOneExport_ShouldReturnOneExport()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 10));

            var export = container.GetExport<string, object>("NewContract");

            Assert.AreEqual("10", export.Value);
        }

        [TestMethod]
        public void GetExports1_IntToStringAdapterInContainerWithOneExportAskingForExactlyOne_ShouldReturnOneExport()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 10));

            var definition = ImportDefinitionFactory.Create("NewContract", ImportCardinality.ExactlyOne);

            var exports = container.GetExports(definition);

            ExportsAssert.AreEqual(exports, "10");
        }

        [TestMethod]
        public void GetExports1_IntToStringAdapterInContainerWithOneExporttAskingForZeroOrOne_ShouldReturnOneExport()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 10));

            var definition = ImportDefinitionFactory.Create("NewContract", ImportCardinality.ZeroOrOne);

            var exports = container.GetExports(definition);

            ExportsAssert.AreEqual(exports, "10");
        }

        [TestMethod]
        public void GetExports1_IntToStringAdapterInContainerWithOneExporttAskingForZeroOrMore_ShouldReturnOneExport()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 10));

            var definition = ImportDefinitionFactory.Create("NewContract", ImportCardinality.ZeroOrMore);

            var exports = container.GetExports(definition);

            ExportsAssert.AreEqual(exports, "10");
        }

        [TestMethod]
        public void GetExports2_IntToStringAdapterInContainerWithOneExport_ShouldReturnOneExport()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 10));

            var exports = container.GetExports(typeof(string), (Type)null, "NewContract");

            ExportsAssert.AreEqual(exports, "10");
        }

        [TestMethod]
        public void GetExportsOfT1_IntToStringAdapterInContainerWithOneExport_ShouldReturnOneExport()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", typeof(string),
                                                                          new MicroExport("OldContract", 10));

            var exports = container.GetExports<string>();

            ExportsAssert.AreEqual(exports, "10");
        }

        [TestMethod]
        public void GetExportsOfT2_IntToStringAdapterInContainerWithOneExport_ShouldReturnOneExport()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 10));

            var exports = container.GetExports<string>("NewContract");

            ExportsAssert.AreEqual(exports, "10");
        }

        [TestMethod]
        public void GetExportsOfTTMetadataView1_IntToStringAdapterInContainerWithOneExport_ShouldReturnOneExport()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", typeof(string),
                                                                          new MicroExport("OldContract", 10));

            var exports = container.GetExports<string, object>();

            ExportsAssert.AreEqual(exports, "10");
        }

        [TestMethod]
        public void GetExportsOfTTMetadataView2_IntToStringAdapterInContainerWithOneExport_ShouldReturnOneExport()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 10));

            var exports = container.GetExports<string, object>("NewContract");

            ExportsAssert.AreEqual(exports, "10");
        }

        [TestMethod]
        public void GetExportedValueOfT1_IntToStringAdapterInContainerWithOneExport_ShouldReturnOneExport()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", typeof(string),
                                                                          new MicroExport("OldContract", 10));

            var exportedValue = container.GetExportedValue<string>();

            Assert.AreEqual("10", exportedValue);
        }

        [TestMethod]
        public void GetExportedValueOfT2_IntToStringAdapterInContainerWithOneExport_ShouldReturnOneExport()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 10));

            var exportedValue = container.GetExportedValue<string>("NewContract");

            Assert.AreEqual("10", exportedValue);
        }

        [TestMethod]
        public void GetExportedValueOrDefaultOfT1_IntToStringAdapterInContainerWithOneExport_ShouldReturnOneExport()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", typeof(string),
                                                                          new MicroExport("OldContract", 10));

            var exportedValue = container.GetExportedValueOrDefault<string>();

            Assert.AreEqual("10", exportedValue);
        }

        [TestMethod]
        public void GetExportedValueOrDefaultOfT2_IntToStringAdapterInContainerWithOneExport_ShouldReturnOneExport()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 10));

            var exportedValue = container.GetExportedValueOrDefault<string>("NewContract");

            Assert.AreEqual("10", exportedValue);
        }

        [TestMethod]
        public void GetExportedValuesOfT1_IntToStringAdapterInContainerWithOneExport_ShouldReturnOneExport()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", typeof(string),
                                                                          new MicroExport("OldContract", 10));

            var exportedValues = container.GetExportedValues<string>();

            EnumerableAssert.AreEqual(exportedValues, "10");
        }

        [TestMethod]
        public void GetExportedValuesOfT2_IntToStringAdapterInContainerWithOneExport_ShouldReturnOneExport()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 10));

            var exportedValues = container.GetExportedValues<string>("NewContract");

            EnumerableAssert.AreEqual(exportedValues, "10");
        }

        [TestMethod]
        public void GetExportOfT1_IntToStringAdapterInContainerWithMultipleExports_ThrowsCardinalityMismatch()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", typeof(string),
                                                                          new MicroExport("OldContract", 1, 2, 3));

            ExceptionAssert.Throws<ImportCardinalityMismatchException>(() =>
            {
                container.GetExport<string>();
            });
        }

        [TestMethod]
        public void GetExportOfT2_IntToStringAdapterInContainerWithMultipleExports_ThrowsCardinalityMismatch()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 1, 2, 3));

            ExceptionAssert.Throws<ImportCardinalityMismatchException>(() =>
            {
                container.GetExport<string>("NewContract");
            });
        }


        [TestMethod]
        public void GetExportOfTTMetadataView1_IntToStringAdapterInContainerWithMultipleExports_ThrowsCardinalityMismatch()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", typeof(string),
                                                                          new MicroExport("OldContract", 1, 2, 3));

            ExceptionAssert.Throws<ImportCardinalityMismatchException>(() =>
            {
                container.GetExport<string, object>();
            });
        }

        [TestMethod]
        public void GetExportOfTTMetadataView2_IntToStringAdapterInContainerWithMultipleExports_ThrowsCardinalityMismatch()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 1, 2, 3));

            ExceptionAssert.Throws<ImportCardinalityMismatchException>(() =>
            {
                container.GetExport<string, object>("NewContract");
            });
        }

        [TestMethod]
        public void GetExports2_IntToStringAdapterInContainerWithMultipleExportsAskingForExactlyOne_ShouldThrowCardinalityMismatch()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 1, 2, 3));

            var definition = ImportDefinitionFactory.Create("NewContract", ImportCardinality.ExactlyOne);

            ExceptionAssert.Throws<ImportCardinalityMismatchException>(() =>
            {
                container.GetExports(definition);
            });
        }

        [TestMethod]
        public void GetExports2_IntToStringAdapterInContainerWithMultipleExportsAskingForZeroOrOne_ShouldRetrunZero()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 1, 2, 3));

            var definition = ImportDefinitionFactory.Create("NewContract", ImportCardinality.ZeroOrOne);

            Assert.AreEqual(0, container.GetExports(definition).Count());
        }

        [TestMethod]
        public void GetExports2_IntToStringAdapterInContainerWithMultipleExportsAskingForZeroOrMore_ShouldReturnMultipleExports()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 1, 2, 3));

            var definition = ImportDefinitionFactory.Create("NewContract", ImportCardinality.ZeroOrMore);

            var exports = container.GetExports(definition);

            ExportsAssert.AreEqual(exports, "1", "2", "3");
        }

        [TestMethod]
        public void GetExports2_IntToStringAdapterInContainerWithMultipleExports_ShouldReturnMultipleExports()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 1, 2, 3));

            var exports = container.GetExports(typeof(string), (Type)null, "NewContract");

            ExportsAssert.AreEqual(exports, "1", "2", "3");
        }

        [TestMethod]
        public void GetExportsOfT1_IntToStringAdapterInContainerWithMultipleExports_ShouldReturnMultipleExports()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", typeof(string),
                                                                          new MicroExport("OldContract", 1, 2, 3));

            var exports = container.GetExports<string>();

            ExportsAssert.AreEqual(exports, "1", "2", "3");
        }

        [TestMethod]
        public void GetExportsOfT2_IntToStringAdapterInContainerWithMultipleExports_ShouldReturnMultipleExports()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 1, 2, 3));

            var exports = container.GetExports<string>("NewContract");

            ExportsAssert.AreEqual(exports, "1", "2", "3");
        }

        [TestMethod]
        public void GetExportedValueOfT1_IntToStringAdapterInContainerWithMultipleExports_ShouldThrowCardinalityMismatch()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", typeof(string),
                                                                          new MicroExport("OldContract", 1, 2, 3));
            ExceptionAssert.Throws<ImportCardinalityMismatchException>(() =>
            {
                container.GetExportedValue<string>();
            });
        }

        [TestMethod]
        public void GetExportedValueOfT2_IntToStringAdapterInContainerWithMultipleExports_ShouldThrowCardinalityMismatch()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 1, 2, 3));
            ExceptionAssert.Throws<ImportCardinalityMismatchException>(() =>
            {
                container.GetExportedValue<string>("NewContract");
            });
        }

        [TestMethod]
        public void GetExportedValueOrDefaultOfT1_IntToStringAdapterInContainerWithMultipleExports_ShouldReturnZero()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", typeof(string),
                                                                          new MicroExport("OldContract", 1, 2, 3));

            Assert.AreEqual(null, container.GetExportedValueOrDefault<string>());
        }

        [TestMethod]
        public void GetExportedValueOrDefaultOfT2_IntToStringAdapterInContainerWithMultipleExports_ShouldReturnZero()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 1, 2, 3));

            Assert.AreEqual(null, container.GetExportedValueOrDefault<string>("NewContract"));
        }

        [TestMethod]
        public void GetExportedValuesOfT1_IntToStringAdapterInContainerWithMultipleExports_ShouldReturnMultipleExports()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", typeof(string),
                                                                          new MicroExport("OldContract", 1, 2, 3));

            var exportedValues = container.GetExportedValues<string>();

            Assert.AreEqual(3, exportedValues.Count());
            Assert.AreEqual("1", exportedValues.ElementAt(0));
            Assert.AreEqual("2", exportedValues.ElementAt(1));
            Assert.AreEqual("3", exportedValues.ElementAt(2));

        }

        [TestMethod]
        public void GetExportedValuesOfT2_IntToStringAdapterInContainerWithMultipleExports_ShouldReturnMultipleExports()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 1, 2, 3));

            var exportedValues = container.GetExportedValues<string>("NewContract");

            Assert.AreEqual(3, exportedValues.Count());
            Assert.AreEqual("1", exportedValues.ElementAt(0));
            Assert.AreEqual("2", exportedValues.ElementAt(1));
            Assert.AreEqual("3", exportedValues.ElementAt(2));
        }

        [TestMethod]
        public void GetExports1_AskingForExactlyOneAndAllAndIntToStringAdapterInContainer_ShouldThrowCardinalityMismatch()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 1, 2, 3));

            var definition = ImportDefinitionFactory.Create(contract => true, ImportCardinality.ExactlyOne);

            ExceptionAssert.Throws<ImportCardinalityMismatchException>(() =>
            {
                container.GetExports(definition);
            });
        }

        [TestMethod]
        public void GetExports1_AskingForZeroOrOneAndAllAndIntToStringAdapterInContainer_ShouldReturnZero()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 1, 2, 3));

            var definition = ImportDefinitionFactory.Create(contract => true, ImportCardinality.ZeroOrOne);

            Assert.AreEqual(0, container.GetExports(definition).Count());
        }

        [TestMethod]
        public void GetExports1_AskingForZeroOrMoreAndAllAndIntToStringAdapterInContainer_ShouldReturnAll()
        {
            AdaptingCompositionContainer container = GetCompositionContainerWithIntToStringAdapter("OldContract", "NewContract",
                                                                          new MicroExport("OldContract", 1, 2, 3));

            var definition = ImportDefinitionFactory.Create(contract => true, ImportCardinality.ZeroOrMore);

            var exports = container.GetExports(definition);

            Assert.AreEqual(7, exports.Count());  // Including the adapter itself
            Assert.AreEqual(1, exports.ElementAt(0).Value);
            Assert.AreEqual(2, exports.ElementAt(1).Value);
            Assert.AreEqual(3, exports.ElementAt(2).Value);
            Assert.AreEqual("1", exports.ElementAt(4).Value);
            Assert.AreEqual("2", exports.ElementAt(5).Value);
            Assert.AreEqual("3", exports.ElementAt(6).Value);
        }

        private static AdaptingCompositionContainer GetCompositionContainerWithIntToStringAdapter(object fromContractNameOrType, object toContractNameOrType, params MicroExport[] exports)
        {
            string contractName = AdapterFactory.ContractNameFromNameOrType(toContractNameOrType);

            Func<Export, Export> adapt = (export) =>
            {   // Adapts an int export to a string export

                Dictionary<string, object> metadata = new Dictionary<string, object>(export.Metadata);
                metadata[CompositionConstants.ExportTypeIdentityMetadataName] = AttributedModelServices.GetTypeIdentity(typeof(string));

                return ExportFactory.Create(contractName, metadata, () =>
                {
                    int value = (int)export.Value;

                    return value.ToString();
                });
            };

            return GetCompositionContainerWithAdapter(fromContractNameOrType, toContractNameOrType, adapt, exports);
        }

        private static AdaptingCompositionContainer GetCompositionContainerWithNullAdapter(object fromContractNameOrType, object toContractNameOrType, params MicroExport[] exports)
        {
            return GetCompositionContainerWithAdapter(fromContractNameOrType, toContractNameOrType, export => null, exports);
        }

        private static AdaptingCompositionContainer GetCompositionContainerWithAdapter(object fromContractNameOrType, object toContractNameOrType, Func<Export, Export> adapt, params MicroExport[] exports)
        {
            var adapter = AdapterFactory.CreateAdapter(fromContractNameOrType, toContractNameOrType, adapt);

            AdaptingCompositionContainer container = AdaptingContainerFactory.Create(exports);
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(adapter);
            container.Compose(batch);

            return container;
        }


        private static Expression<Func<ExportDefinition, bool>> ConstraintFromContract(string contractName)
        {
            return ConstraintFactory.Create(contractName);
        }

        public class Adapter
        {
            [Export(AdaptationConstants.AdapterContractName)]
            [ExportMetadata(AdaptationConstants.AdapterFromContractMetadataName, "OldContract")]
            [ExportMetadata(AdaptationConstants.AdapterToContractMetadataName, "NewContract")]
            public Export Adapt(Export export)
            {
                return ExportFactory.Create("NewContract",
                                  export.Definition.Metadata,
                                  () => export.Value);
            }
        }

        public class ImportAdaptedContract
        {
            [Import("NewContract")]
            public string Import
            {
                get;
                set;
            }
        }

        public class ImportAdaptedContracts
        {
            private Collection<string> _imports = new Collection<string>();

            [ImportMany("NewContract")]
            public Collection<string> Imports
            {
                get { return _imports; }
            }
        }
    }
}
