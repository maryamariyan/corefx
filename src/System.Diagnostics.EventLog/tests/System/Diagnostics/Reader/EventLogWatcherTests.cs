// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.even

using System.Diagnostics.Eventing.Reader;
using System.Threading;
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
            Assert.False(eventLogWatcher.Enabled);
            eventLogWatcher.Enabled = true;
            Assert.True(eventLogWatcher.Enabled);
            eventLogWatcher.Enabled = false;
            Assert.False(eventLogWatcher.Enabled);
            eventLogWatcher.Dispose();

            var bookmark = GetBookmark();
            Assert.Throws<ArgumentNullException>(() => new EventLogWatcher(null, bookmark, true));
            Assert.Throws<InvalidOperationException>(() => new EventLogWatcher(new EventLogQuery("Application", PathType.LogName, "*[System]") { ReverseDirection = true }, bookmark, true));

            // coverage 2
            var query = new EventLogQuery("Application", PathType.LogName, "*[System]");
            eventLogWatcher = new EventLogWatcher(query);
            // Assert.Null(eventLogWatcher.EventRecordWritten);
            eventLogWatcher.Dispose();

            // coverage 3
            eventLogWatcher = new EventLogWatcher(query, bookmark);
            Assert.False(eventLogWatcher.Enabled);
            eventLogWatcher.Enabled = true;
            Assert.True(eventLogWatcher.Enabled);
            eventLogWatcher.Enabled = false;
            Assert.False(eventLogWatcher.Enabled);
            eventLogWatcher.Dispose();
        }

        private EventBookmark GetBookmark()
        {
            EventBookmark bookmark;
            EventLogQuery eventLogQuery = new EventLogQuery("Application", PathType.LogName, "*[System]");
            using (var eventLog = new EventLogReader(eventLogQuery))
            using (var record = eventLog.ReadEvent())
            {
                Assert.NotNull(record);
                bookmark = record.Bookmark;
                Assert.NotNull(record.Bookmark);
            }
            return bookmark;
        }

        static AutoResetEvent signal;
        private const string message = "EventRecordWrittenTestMessage";
        private int eventCounter;

        public void RaisingEvent(string log, string methodName, bool waitOnEvent = true)
        {
            signal = new AutoResetEvent(false);
            eventCounter = 0;
            string source = "Source_" + methodName;

            try
            {
                EventLog.CreateEventSource(source, log);
                var query = new EventLogQuery(log, PathType.LogName);
                using (EventLog eventLog = new EventLog())
                using (EventLogWatcher eventLogWatcher = new EventLogWatcher(query)) //here
                {
                    eventLog.Source = source;
                    eventLogWatcher.EventRecordWritten += (s, e) =>
                    {
                        eventCounter += 1;
                        Console.WriteLine(e.EventException);
                        Console.WriteLine(e.EventRecord);
                        signal.Set();
                    };
                    Helpers.RetryOnWin7(() => eventLogWatcher.Enabled = waitOnEvent);
                    Helpers.RetryOnWin7(() => eventLog.WriteEntry(message, EventLogEntryType.Information));
                    if (waitOnEvent)
                    {
                        Assert.True(signal.WaitOne(6000));
                    }
                }
            }
            finally
            {
                EventLog.DeleteEventSource(source);
                Helpers.RetryOnWin7(() => EventLog.Delete(log));
            }
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.IsElevatedAndSupportsEventLogs))]
        public void RecordWrittenEventRaised()
        {
            RaisingEvent("EnableEvent", nameof(RecordWrittenEventRaised));
            Assert.NotEqual(0, eventCounter);
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.IsElevatedAndSupportsEventLogs))]
        public void RecordWrittenEventRaiseDisable()
        {
            RaisingEvent("DisableEvent", nameof(RecordWrittenEventRaiseDisable), waitOnEvent: false);
            Assert.Equal(0, eventCounter);
        }
    }
}
