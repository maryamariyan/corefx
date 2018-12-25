// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.even

using System.Diagnostics.Eventing.Reader;
using Xunit;

namespace System.Diagnostics.Tests
{
    public partial class EventLogWatcherTests
    {
        [ConditionalFact(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        public void Ctor_EventLogWatcherTests()
        {
            // coverage 1
            var eventLogWatcher = new EventLogWatcher("Application");

            // coverage 2
            EventLogQuery eventLogQuery = new EventLogQuery("Application", PathType.LogName, "*[System]");
            using (var eventLog = new EventLogReader(eventLogQuery))
            using (var record = eventLog.ReadEvent())
            {
                Assert.NotNull(record);
                eventLogWatcher = new EventLogWatcher(eventLogQuery, record.Bookmark);
            }
        }
    }
}
