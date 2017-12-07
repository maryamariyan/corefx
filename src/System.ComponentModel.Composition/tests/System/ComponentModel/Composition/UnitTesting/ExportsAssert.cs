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
        public static void Equal<T>(IEnumerable<Export> actual, params T[] expected)
        {
            Assert.Equal((IEnumerable)expected, (IEnumerable)actual.Select(export => (int)export.Value).ToArray());
        }
    }
}
