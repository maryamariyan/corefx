// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.even

using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading;
using Xunit;

namespace System.Diagnostics.Tests
{
    public class EventLogQueryTests
    {
        [ConditionalFact(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        public void Ctor_PathAndQueryNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new EventLogQuery(null, PathType.LogName, null));
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        public void Ctor_QueryNotNull_Success()
        {
            // Todo: Use elsewhere that could assert something else with after
            EventLogQuery eventLogQuery = new EventLogQuery(null, PathType.LogName, "<QueryList/>");
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        public void Draft()
        {
            // eventMetadata.LogLink
            // eventMetadata.Level
            // Opcode
            // Task
            // Keywords
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        public void EventMetadata()
        {
            EventLogQuery eventsQuery = new EventLogQuery("Application", PathType.LogName, "*[System]");
            using (var logReader = new EventLogReader(eventsQuery))
            {
                // For each event returned from the query
                for (EventRecord eventInstance = logReader.ReadEvent();
                        eventInstance != null;
                        eventInstance = logReader.ReadEvent())
                {
                    List<object> varRepSet = new List<object>();
                    // for (int i = 0; i < eventInstance.Properties.Count; i++)
                    // {
                    //     varRepSet.Add((object)(eventInstance.Properties[i].Value.ToString()));
                    // }
                    string description = eventInstance.FormatDescription(null);

                    string description1 = eventInstance.FormatDescription();
                    // Assert.NotEmpty(varRepSet);
                }
            }
        }
    }
}
