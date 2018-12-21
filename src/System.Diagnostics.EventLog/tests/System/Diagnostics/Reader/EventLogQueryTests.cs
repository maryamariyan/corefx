// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.even

using System.Diagnostics.Eventing.Reader;
using Xunit;

namespace System.Diagnostics.Tests
{
    public class EventLogQueryTests
    {
        [ConditionalFact(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        public void Ctor_PathAndQueryNull_Throws()
        {
            if (PlatformDetection.IsWindows7) // Null events in PowerShell log
                return;
            Assert.Throws<ArgumentNullException>(() => new EventLogQuery(null, PathType.LogName, null));
        }
    }
}
