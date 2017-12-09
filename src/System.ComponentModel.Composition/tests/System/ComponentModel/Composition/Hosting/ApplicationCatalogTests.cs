// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.UnitTesting;
using Xunit;

namespace System.ComponentModel.Composition
{
    public class ApplicationCatalogTests
    {
        // This is a glorious do nothing ReflectionContext
        public class ApplicationCatalogTestsReflectionContext : ReflectionContext
        {
            public override Assembly MapAssembly(Assembly assembly)
            {
                return assembly;
            }

            public override TypeInfo MapType(TypeInfo type)

            {
                return type;
            }
       }

        public class Worker : MarshalByRefObject
        {
            internal void DoWork(Action work)
            {
                work();
            }
        }

#if FEATURE_FIXCOMPILE
        public class Application : TemporaryDirectory
        {
            public void AppMain(Action work)
            {
                ApplicationFilesCopier(
                    typeof(Application).Assembly.Location, 
                    typeof(Assert).Assembly.Location, 
                    typeof(EnumerableAssert).Assembly.Location, 
                    typeof(TransparentTestCase).Assembly.Location,
                    typeof(System.Reflection.ReflectionContext).Assembly.Location,
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "System.ComponentModel.Composition.UnitTests.ReflectionContextTestAssemblyOne.dll"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "System.ComponentModel.Composition.UnitTests.ReflectionContextTestAssemblyTwo.dll"));

                PermissionSet ps = new PermissionSet(PermissionState.Unrestricted);

                //Create a new sandboxed domain 
                AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
                setup.ApplicationBase = DirectoryPath;
                setup.PrivateBinPath = SubDirectoryPath;
                setup.ShadowCopyFiles = "true";

                AppDomain newDomain = AppDomain.CreateDomain("Application Domain"+ Guid.NewGuid(), null, setup, ps);
                Worker remoteWorker = (Worker)newDomain.CreateInstanceAndUnwrap(
                    Assembly.GetExecutingAssembly().FullName,
                    typeof(Worker).FullName);
                
                Exception exception = null;
                try
                {
                    remoteWorker.DoWork(work);
                }
                catch(Exception e)
                {
                    exception = e;
                }
                finally
                {
                    AppDomain.Unload(newDomain);
                }
                
                GC.Collect();
                GC.WaitForPendingFinalizers();
                if(exception != null)
                {
                    throw exception;
                }
            }
            
            private void ApplicationFilesCopier(string application, params string[] fileNames)
            {
                Console.WriteLine("DirectoryPath: {0}", DirectoryPath);

                //Because we test a "private" version of our binary we need to copy it down too.
                string componentModelDll = typeof(ImportAttribute).Assembly.Location;
                File.Copy(componentModelDll, Path.Combine(DirectoryPath, Path.GetFileName(componentModelDll)));

                File.Copy(application, Path.Combine(DirectoryPath, Path.GetFileName(application)));

                Directory.CreateDirectory(SubDirectoryPath);
                foreach (string fileName in fileNames)
                {
                    File.Copy(fileName, Path.Combine(SubDirectoryPath, Path.GetFileName(fileName)));
                }
            }

            public string SubDirectoryPath 
            {
                get
                {
                    return Path.Combine(DirectoryPath, "AddOns");
                }
            }
        }
        [Fact]
        public void Constructor1_NullReflectionContextArgument_ShouldThrowArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("reflectionContext", () =>
            {
                new ApplicationCatalog((ReflectionContext)null);
            });
        }

        [Fact]
        public void Constructor3_NullBothArguments_ShouldThrowArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("reflectionContext", () =>
            {
                new ApplicationCatalog((ReflectionContext)null, (ICompositionElement)null);
            });
        }

        [Fact]
        public void Constructor2_NullDefinitionOriginArgument_ShouldThrowArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("definitionOrigin", () =>
            {
                new ApplicationCatalog((ICompositionElement)null);
            });
        }

        [Fact]
        public void Constructor3_NullDefinitionOriginArgument_ShouldThrowArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("definitionOrigin", () =>
            {
                new ApplicationCatalog((ICompositionElement)null);
            });
        }

        [Fact]
        public void ICompositionElementDisplayName_ShouldIncludeCatalogTypeNameAndDirectoryPath()
        {
            using(var app = new Application())
            {
                app.AppMain( () =>
                {
                    var catalog = (ICompositionElement)new ApplicationCatalog();

                    string expected = string.Format("ApplicationCatalog (Path=\"{0}\") (PrivateProbingPath=\"{1}\")", AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath);
                    string result = (string)(catalog.DisplayName);
                    //Cross AppDomain AssertFailedException does not marshall
                    if(expected != result)
                    {
                        throw new Exception("Assert.AreEqual(expected, result);");
                    }
                });
            }
        }

        [Fact]
        public void ToString_ShouldReturnICompositionElementDisplayName()
        {
            using(var app = new Application())
            {
                app.AppMain( () =>
                {
                    var catalog = (ICompositionElement)new ApplicationCatalog();
                    string expected = catalog.ToString();
                    string result = catalog.DisplayName;
                    Assert.AreEqual(expected, result);
                });
            }
        }

        [Fact]
        public void ICompositionElementDisplayName_WhenCatalogDisposed_ShouldNotThrow()
        {
            using(var app = new Application())
            {
                app.AppMain( () =>
                {
                    var catalog = new ApplicationCatalog();
                    catalog.Dispose();
                    var displayName = ((ICompositionElement)catalog).DisplayName;
                });
            }
        }

        [Fact]
        public void ICompositionElementOrigin_WhenCatalogDisposed_ShouldNotThrow()
        {
            using(var app = new Application())
            {
                app.AppMain( () =>
                {
                    var catalog = new ApplicationCatalog();
                    catalog.Dispose();
                    var origin = ((ICompositionElement)catalog).Origin;
                });
            }
        }

        [Fact]
        public void Parts_WhenCatalogDisposed_ShouldThrowObjectDisposed()
        {
            ExceptionAssert.Throws<ObjectDisposedException>(RetryMode.DoNotRetry, () =>
            {
                using(var app = new Application())
                {
                    app.AppMain( () =>
                    {
                        var catalog = new ApplicationCatalog();
                        catalog.Dispose();
                        var parts = catalog.Parts;
                    });
                }
            });
        }

        [Fact]
        public void GetEnumerator_WhenCatalogDisposed_ShouldThrowObjectDisposed()
        {
            ExceptionAssert.Throws<ObjectDisposedException>(RetryMode.DoNotRetry, () =>
            {
                using(var app = new Application())
                {
                    app.AppMain( () =>
                    {
                        var catalog = new ApplicationCatalog();
                        catalog.Dispose();
                        var enumerator = catalog.GetEnumerator();
                    });
                }
            });
        }

        [Fact]
        public void GetExports_WhenCatalogDisposed_ShouldThrowObjectDisposed()
        {
            ExceptionAssert.Throws<ObjectDisposedException>(RetryMode.DoNotRetry, () =>
            {
                using(var app = new Application())
                {
                    app.AppMain( () =>
                    {
                        var catalog = new ApplicationCatalog();
                        catalog.Dispose();
            
                        var definition = ImportDefinitionFactory.Create();
                        catalog.GetExports(definition);
                    });
                }
            });
        }

        [Fact]
        public void ToString_WhenCatalogDisposed_ShouldNotThrow()
        {
            using(var app = new Application())
            {
                app.AppMain( () =>
                {
                    var catalog = new ApplicationCatalog();
                    catalog.Dispose();
                    catalog.ToString();
                });
            }
        }

        [Fact]
        public void GetExports_NullAsConstraintArgument_ShouldThrowArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("definition", () =>
            {
                using(var app = new Application())
                {
                    app.AppMain( () =>
                    {
                        var catalog = new ApplicationCatalog();
                        catalog.GetExports((ImportDefinition)null);
                    });
                }
            });
        }


        [Fact]
        public void Dispose_ShouldNotThrow()
        {
            using(var app = new Application())
            {
                app.AppMain( () =>
                {
                    using (var catalog = new ApplicationCatalog())
                    {
                    }
                });
            }
        }

        [Fact]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            using(var app = new Application())
            {
                app.AppMain( () =>
                {
                    var catalog = new ApplicationCatalog();
                    catalog.Dispose();
                    catalog.Dispose();
                    catalog.Dispose();
                });
            }
        }

        [Fact]
        public void Test_Parts()
        {
            using(var app = new Application())
            {
                app.AppMain( () =>
                {
                    using(var catalog = new ApplicationCatalog())
                    {
                        var result = catalog.Parts.Count();

                        //Cross AppDomain AssertFailedException does not marshall
                        if(result < 0)
                        {
                            throw new Exception("Assert.IsTrue(result > 0);");
                        }
                    }
                });
            }
        }

        [Fact]
        public void Test_GetEnumerator()
        {
            using(var app = new Application())
            {
                app.AppMain( () =>
                {
                    using(var catalog = new ApplicationCatalog())
                    {
                        var result = catalog.Count();
                        //Cross AppDomain AssertFailedException does not marshall
                        if(result < 0)
                        {
                            throw new Exception("Assert.IsTrue(result > 0);");
                        }
                    }
                });
            }
        }
#endif

        [Fact]
        public void ExecuteOnCreationThread()
        {
            // Add a proper test for event notification on caller thread
        }
    }
}
