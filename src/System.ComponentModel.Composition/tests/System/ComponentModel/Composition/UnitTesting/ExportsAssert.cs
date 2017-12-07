// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.UnitTesting;
using System.ComponentModel.Composition.Primitives;
using Xunit;

namespace System.ComponentModel.Composition.UnitTesting
{
    public static class ExportsAssert
    {
        public static void AreEqual<T>(IEnumerable<Export> actual, params T[] expected)
        {
            Assert.Equal((IEnumerable)expected, (IEnumerable)actual.Select(export => (T)export.Value).ToArray());
        }

        public static void AreEqual<T>(IEnumerable<Lazy<T>> actual, params T[] expected)
        {
            Assert.Equal((IEnumerable<T>)expected, (IEnumerable<T>)actual.Select(export => export.Value));
        }

        public static void AreEqual<T, TMetadataView>(IEnumerable<Lazy<T, TMetadataView>> actual, params T[] expected)
        {
            Assert.Equal((IEnumerable<T>)expected, (IEnumerable<T>)actual.Select(export => export.Value));
        }
    }
}
