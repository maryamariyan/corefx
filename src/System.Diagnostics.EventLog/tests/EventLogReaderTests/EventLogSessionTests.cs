﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.Eventing.Reader;
using System.Threading;
using Xunit;

namespace System.Diagnostics.Tests
{
    public class EventLogSessionTests
    {
        [ConditionalFact(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        public void Methods_DoNotThrow()
        {
            var session = new EventLogSession();
            Assert.NotEmpty(session.GetProviderNames());
            Assert.NotEmpty(session.GetLogNames());
            session.CancelCurrentOperations();
            session.Dispose();
        }
    }
}
