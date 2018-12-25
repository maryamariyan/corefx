// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.even

using System.Diagnostics.Eventing.Reader;
using Xunit;

namespace System.Diagnostics.Tests
{
    public class EventPropertyContextTests
    {
        [ConditionalFact(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        public void Dispose_DoesNotThrow()
        {
            using (var x = new DerivedEventPropertyContext()) { }
        }

        private class DerivedEventPropertyContext : EventPropertyContext { }
    }
}
