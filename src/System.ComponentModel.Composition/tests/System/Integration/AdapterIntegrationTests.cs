// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using Xunit;

namespace Tests.Integration
{
    public class AdapterTest
    {
        [Fact]
        public void Adapter_BatchAddition()
        {
            var container = new AdaptingCompositionContainer();
            var test = new NewImporter();
            var batch = new CompositionBatch();
            batch.AddPart(test);
            batch.AddPart(new OldExporter());
            batch.AddPart(new OldNewAdapter());
            container.Compose(batch);
            Assert.AreEqual(1, test.newImport.Length, "Adapter and exporter added in the same batch should yield adapted value");
        }

        [Fact]
        public void Adapter_BatchCrossingPaths()
        {
            var container = new AdaptingCompositionContainer();
            var test = new NewImporter();
            var batch = new CompositionBatch();
            batch.AddPart(test);
            var oldExporter = batch.AddPart(new OldExporter());
            container.Compose(batch);
            Assert.AreEqual(0, test.newImport.Length, "No adapted value should be imported without adapter");

            batch = new CompositionBatch();
            var adapter = batch.AddPart(new OldNewAdapter());
            batch.RemovePart(oldExporter);
            container.Compose(batch);
            Assert.AreEqual(0, test.newImport.Length, "No adapted value should be imported without adaptee");

            batch = new CompositionBatch();
            batch.RemovePart(adapter);
            batch.AddPart(oldExporter);
            container.Compose(batch);
            Assert.AreEqual(0, test.newImport.Length, "No adapted value should be imported without adapter");
        }

        [Fact]
        public void Adapter_MultipleAdaptees()
        {
            var container = new AdaptingCompositionContainer();
            var test = new NewImporter();
            var batch = new CompositionBatch();
            batch.AddPart(test);
            batch.AddPart(new OldExporter());
            batch.AddPart(new OldExporter());
            batch.AddPart(new OldExporter());
            batch.AddPart(new OldNewAdapter());
            container.Compose(batch);
            Assert.AreEqual(3, test.newImport.Length, "Three adaptees and one adapter should yield three adapted values");
        }

        [Fact]
        public void Adapter_MultipleAdapters()
        {
            var container = new AdaptingCompositionContainer();
            var test = new NewImporter();
            var batch = new CompositionBatch();
            batch.AddPart(test);
            batch.AddPart(new OldExporter());
            batch.AddPart(new OldNewAdapter());
            batch.AddPart(new OldNewAdapter());
            batch.AddPart(new OldNewAdapter());
            container.Compose(batch);
            Assert.AreEqual(3, test.newImport.Length, "Three adapters for one adaptee should yield three adapted exports");
        }

        [Fact]
        public void Adapter_AdapteeAddedAndRemoved()
        {
            var catalog = new TypeCatalog(new Type[] { typeof(OldNewAdapter) });
            var container = new AdaptingCompositionContainer(catalog);
            var test = new NewImporter();
            var batch = new CompositionBatch();
            batch.AddPart(test);
            container.Compose(batch);
            Assert.AreEqual(0, test.newImport.Length, "No adapted values should have been imported");

            batch = new CompositionBatch();
            var oldExport = batch.AddPart(new OldExporter());
            container.Compose(batch);
            Assert.AreEqual(1, test.newImport.Length, "One adapted value should have been imported after adaptee was added");

            batch = new CompositionBatch();
            batch.RemovePart(oldExport);
            container.Compose(batch);
            Assert.AreEqual(0, test.newImport.Length, "No adapted values should have been imported after adaptee was removed");
        }

        [Fact]
        public void Adapter_AdapterAddedAndRemoved()
        {
            var container = new AdaptingCompositionContainer();
            var test = new NewImporter();
            var batch = new CompositionBatch();
            batch.AddPart(test);
            batch.AddPart(new OldExporter());
            container.Compose(batch);
            Assert.AreEqual(0, test.newImport.Length, "No adapted values should have been imported");

            batch = new CompositionBatch();
            var adapter = batch.AddPart(new OldNewAdapter());
            container.Compose(batch);
            Assert.AreEqual(1, test.newImport.Length, "One adapted value should have been imported");

            batch = new CompositionBatch();
            batch.RemovePart(adapter);
            container.Compose(batch);
            Assert.AreEqual(0, test.newImport.Length, "No adapted values should have been imported");
        }


        public interface IOldContract
        {
        }

        public interface INewContract
        {
        }

        public class NewImporter
        {
            [ImportMany(AllowRecomposition = true)]
            public INewContract[] newImport { get; set; }
        }

        public class OldNewAdapter
        {
            internal class Adapted : INewContract { }

            [Export(AdaptationConstants.AdapterContractName)]
            [ExportAdapter(typeof(IOldContract), typeof(INewContract))]
            public virtual Export Adapt(Export sourceExport)
            {
                var targetMetadata = new Dictionary<string, object>(sourceExport.Metadata);
                targetMetadata[CompositionConstants.ExportTypeIdentityMetadataName] = AttributedModelServices.GetTypeIdentity(typeof(INewContract));
                return new Export(new ExportDefinition(
                    AttributedModelServices.GetContractName(typeof(INewContract)), targetMetadata),
                    () => { return new Adapted(); });
            }
        }

        [Export(typeof(IOldContract))]
        public class OldExporter : IOldContract
        {
        }

        [MetadataAttribute]
        public class ExportAdapterAttribute : Attribute
        {
            public ExportAdapterAttribute(Type fromType, Type toType)
                : this(AttributedModelServices.GetContractName(fromType), toType)
            {
            }

            public ExportAdapterAttribute(Type fromType, string toContractName)
                : this(AttributedModelServices.GetContractName(fromType), toContractName)
            {
            }

            public ExportAdapterAttribute(string fromContractName, Type toType)
                : this(fromContractName, AttributedModelServices.GetContractName(toType))
            {
            }

            public ExportAdapterAttribute(string fromContractName, string toContractName)
            {
                FromContract = fromContractName;
                ToContract = toContractName;
            }

            public string FromContract { get; private set; }
            public string ToContract { get; private set; }
        }

        [PartNotDiscoverable]
        public class AdapterWithImport
        {
            [Import("AdapterValue")]
            public string Value { get; set; }

            [Export(AdaptationConstants.AdapterContractName)]
            [ExportAdapter("OldContract", "NewContract")]
            public virtual Export Adapt(Export sourceExport)
            {
                var targetMetadata = new Dictionary<string, object>(sourceExport.Metadata);
                targetMetadata[CompositionConstants.ExportTypeIdentityMetadataName] = AttributedModelServices.GetTypeIdentity(typeof(string));
                return new Export(new ExportDefinition("NewContract", targetMetadata),
                    () => "NewContract");
            }
        }

        public class AdapterValue
        {
            [Export("AdapterValue")]
            public string Value = "AdapterValue";

            [Export("OldContract")]
            public string OldContract = "OldContract";
        }

        [Fact]
        public void AdapterCanImportFromCatalogWhileInCatalog()
        {
            var catalog = CatalogFactory.CreateNonFilteredAttributed(typeof(AdapterWithImport), typeof(AdapterValue));
            var container = AdaptingContainerFactory.Create(catalog);

            string newContract = container.GetExportedValue<string>("NewContract");
            Assert.AreEqual("NewContract", newContract);
        }

        [Fact]
        public void AdapterCannotImportManuallyAddedWhileInCatalog()
        {
            var catalog = CatalogFactory.CreateNonFilteredAttributed(typeof(AdapterWithImport));
            var container = AdaptingContainerFactory.Create(catalog);

            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(new AdapterValue());

            container.Compose(batch);

            // Adapter was rejected because of missing import and will not be resurrected when import is added
            ExceptionAssert.Throws<ImportCardinalityMismatchException>(() =>
                container.GetExportedValue<string>("NewContract"));
        }

        [Fact]
        public void AdapterCanImportFromCatalogWhenAddedManually()
        {
            var catalog = CatalogFactory.CreateNonFilteredAttributed(typeof(AdapterValue));
            var container = AdaptingContainerFactory.Create(catalog);

            var adapter = new AdapterWithImport();

            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(adapter);
            container.Compose(batch);

            Assert.AreEqual("AdapterValue", adapter.Value);

            string newContract = container.GetExportedValue<string>("NewContract");
            Assert.AreEqual("NewContract", newContract);
        }

        [Fact]
        public void AdapterCannotImportManuallyAddedWhileAddedManually()
        {
            var container = AdaptingContainerFactory.Create();

            // Cannot add a adapter and its depending import in the same batch. 
            ExceptionAssert.Throws<ChangeRejectedException>(delegate
            {
                CompositionBatch batch = new CompositionBatch();
                batch.AddPart(new AdapterValue());
                batch.AddPart(new AdapterWithImport());
                container.Compose(batch);

            });
        }
    }
}
