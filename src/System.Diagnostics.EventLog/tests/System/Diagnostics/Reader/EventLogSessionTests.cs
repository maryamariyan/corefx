// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Security;
using System.Threading;
using Xunit;

namespace System.Diagnostics.Tests
{
    public class EventLogSessionTests : FileCleanupTestBase
    {
        [ConditionalTheory(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        [InlineData(true)]
        [InlineData(false)]
        public void Methods_DoNotThrow(bool usingDefaultCtor)
        {
            using (var session = usingDefaultCtor ? new EventLogSession() : new EventLogSession(null))
            {
                Assert.NotEmpty(session.GetProviderNames());
                Assert.NotEmpty(session.GetLogNames());

                Assert.Throws<ArgumentNullException>(() => session.ExportLogAndMessages(null, PathType.LogName, "Application", GetTestFilePath()));
                session.ExportLogAndMessages("Application", PathType.LogName, "Application", GetTestFilePath());
                session.ExportLogAndMessages("Application", PathType.LogName, "Application", GetTestFilePath(), false, targetCultureInfo: null);

                Assert.Throws<ArgumentNullException>(() => session.ExportLog(null, PathType.LogName, "Application", GetTestFilePath()));
                Assert.Throws<ArgumentNullException>(() => session.ExportLog("Application", PathType.LogName, "Application", null));
                Assert.Throws<ArgumentOutOfRangeException>(() => session.ExportLog("Application", (PathType)0, "Application", GetTestFilePath()));
                Assert.Throws<EventLogNotFoundException>(() => session.ExportLog("Application", PathType.FilePath, "Application", GetTestFilePath()));
                session.ExportLog("Application", PathType.LogName, "Application", GetTestFilePath(), tolerateQueryErrors: true);

                Assert.Throws<ArgumentNullException>(() => session.ClearLog(null));
                Assert.Throws<ArgumentNullException>(() => session.ClearLog(null, backupPath: GetTestFilePath()));
                //session.ClearLog("logName");

                session.CancelCurrentOperations();
            }
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        public void GetProviderNames_WithPassword_Throws()
        {
            var password = new SecureString();
            password.AppendChar('a');
            using (var session = new EventLogSession(null, null, null, password, SessionAuthentication.Default))
            {
                Assert.Throws<UnauthorizedAccessException>(() => session.GetProviderNames());
            }
        }
    }
}
