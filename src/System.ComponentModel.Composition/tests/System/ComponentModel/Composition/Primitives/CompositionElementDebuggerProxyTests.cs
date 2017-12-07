// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System.ComponentModel.Composition.Primitives;
using System.UnitTesting;
using Xunit;

namespace System.ComponentModel.Composition.Hosting
{
    public class CompositionElementDebuggerProxyTests
    {
        [Fact]
        public void Constructor_NullAsElementArgument_ShouldThrowArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("element", () =>
            {
                new CompositionElementDebuggerProxy((CompositionElement)null);
            });
        }

        [Fact]
        public void Constructor_ValueAsElementArgument_ShouldSetDisplayNameProperty()
        {
            var expectations = Expectations.GetObjectsReferenceTypes();

            foreach (var e in expectations)
            {
                var element = CreateCompositionElement(e);

                var proxy = new CompositionElementDebuggerProxy(element);

                Assert.Same(element.DisplayName, proxy.DisplayName);
            }            
        }

        [Fact]
        public void Constructor_ValueAsElementArgument_ShouldSetOriginProperty()
        {
            var expectations = Expectations.GetObjectsReferenceTypes();

            foreach (var e in expectations)
            {
                var element = CreateCompositionElement(e);

                var proxy = new CompositionElementDebuggerProxy(element);

                Assert.Same(element.Origin, proxy.Origin);
            }
        }

        [Fact]
        public void Constructor_ValueAsElementArgument_ShouldSetUnderlyingObjectProperty()
        {
            var expectations = Expectations.GetObjectsReferenceTypes();

            foreach (var e in expectations)
            {
                var element = CreateCompositionElement(e);

                var proxy = new CompositionElementDebuggerProxy(element);

                Assert.Same(element.UnderlyingObject, proxy.UnderlyingObject);
            }
        }

        private static CompositionElement CreateCompositionElement(object underlyingObject)
        {
            return new CompositionElement(underlyingObject);
        }
   }
}
