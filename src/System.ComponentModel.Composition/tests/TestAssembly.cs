using System;
using System.ComponentModel.Composition;
using System.Reflection;

namespace System.ComponentModel.Composition
{
    public class TestAssemblyOne { }

    public class TestAssemblyTwo { }

    public class TestAssemblyThree { }

    public class TestAssemblyFour { }

    [Export]
    public class TestAssemblyOneExport { }
    
    // This is a glorious do nothing ReflectionContext
    public class ReflectionContextTestAssemblyThreeReflectionContext : ReflectionContext
    {
        private ReflectionContextTestAssemblyThreeReflectionContext() {}
        public override Assembly MapAssembly(Assembly assembly)
        {
            return assembly;
        }
        
        public override TypeInfo MapType(TypeInfo type)
        {
            return type;
        }
   }
    
    // This is a glorious do nothing ReflectionContext
    public class ReflectionContextTestAssemblyOneReflectionContext : ReflectionContext
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

    [Export]
    public class TestAssemblyTwoExport { }
    
    public class MyLittleConventionAttribute : CatalogReflectionContextAttribute
    {
        public MyLittleConventionAttribute() : base(typeof(ReflectionContextTestAssemblyTwo))
        {
        }

        // This is a glorious do nothing ReflectionContext
        public class ReflectionContextTestAssemblyTwo : ReflectionContext
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
    }
}
