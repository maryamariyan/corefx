// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.Eventing.Reader;
using Xunit;

namespace System.Diagnostics.Tests
{
    public class EventLogInformationTests
    {
        [ConditionalTheory(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        [InlineData(true)]
        [InlineData(false)]
        public void GetLogInformation_NullLogName_Throws(bool usingDefaultCtor)
        {
            using (var session = usingDefaultCtor ? new EventLogSession() : new EventLogSession(null))
            {
                Assert.Throws<ArgumentNullException>(() => session.GetLogInformation(null, PathType.LogName));
            }
        }

        [ConditionalTheory(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        [InlineData(true)]
        [InlineData(false)]
        public void GetLogInformation_UsingLogName_DoesNotThrow(bool usingDefaultCtor)
        {
            using (var session = usingDefaultCtor ? new EventLogSession() : new EventLogSession(null))
            {
                EventLogInformation logInfo = session.GetLogInformation("Application", PathType.LogName);
                Assert.NotNull(logInfo.CreationTime);
                Assert.NotNull(logInfo.LastAccessTime);
                Assert.NotNull(logInfo.LastWriteTime);
                Assert.NotNull(logInfo.FileSize);
                Assert.NotNull(logInfo.Attributes);
                Assert.NotNull(logInfo.RecordCount);
                Assert.NotNull(logInfo.OldestRecordNumber);
                Assert.NotNull(logInfo.IsLogFull);
            }
        }
    }
}