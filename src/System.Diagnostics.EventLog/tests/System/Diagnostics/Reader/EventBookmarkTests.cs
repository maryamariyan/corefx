// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.Eventing.Reader;
using Xunit;

namespace System.Diagnostics.Tests
{
    public class EventBookmarkTests
    {
        //[Fact]
        public void BookmarkTest()
        {
            GetBookmark("Application", PathType.LogName);
        }

        private static EventBookmark GetBookmark(string log, PathType pathType)
        {
            // string log = "Application", PathType pathType = PathType.LogName, string query = "*[System[(Level=2)]]"
            var elq = new EventLogQuery(log, pathType) {ReverseDirection = true};

            var reader = new EventLogReader(elq);
            var record = reader.ReadEvent();
            if (record != null)
                return record.Bookmark;
            return null;
        }
    }
}