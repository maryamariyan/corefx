// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.IO;
using Xunit;

namespace System.UnitTesting
{
    public static class ExtendedAssert
    {
        /// <summary>
        ///     Verifies that the two specified objects are an instance of the same type.
        /// </summary>
        public static void IsInstanceOfSameType(object expected, object actual)
        {
            if (expected == null || actual == null)
            {
                Assert.Same(expected, actual);
                return;
            }

            Assert.Same(expected.GetType(), actual.GetType());
        }
    }
}
