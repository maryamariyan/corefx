// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Threading;
using Xunit;

namespace System.Diagnostics.Tests
{
    public class EventLogConfigurationTests
    {
        [ConditionalFact(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        public void Properties_DoNotThrow()
        {
            // string log = "ClearTest";
            // string source = "Source_" + nameof(ClearLog);

            // try
            // {
            //     EventLog.CreateEventSource(source, log);
            //     using (EventLog eventLog = new EventLog())
            //     {
            //         eventLog.Source = source;
            //         eventLog.Clear();
            //         Assert.Equal(0, Helpers.RetryOnWin7((() => eventLog.Entries.Count)));
            //         Helpers.RetryOnWin7(() => eventLog.WriteEntry("Writing to event log."));
            //         Helpers.WaitForEventLog(eventLog, 1);
            //         Assert.Equal(1, Helpers.RetryOnWin7((() => eventLog.Entries.Count)));
            //     }
            // }
            // finally
            // {
            //     EventLog.DeleteEventSource(source);
            //     Helpers.RetryOnWin7(() => EventLog.Delete(log));
            // }
            var configuration = new EventLogConfiguration("Application");

            string logName = configuration.LogName;
            EventLogType logType = configuration.LogType;
            EventLogIsolation logIsolation = configuration.LogIsolation;
            bool isEnabled = configuration.IsEnabled;
            bool isClassicLog = configuration.IsClassicLog;
            string securityDescriptor = configuration.SecurityDescriptor;
            string logFilePath = configuration.LogFilePath;
            long maximumSizeInBytes = configuration.MaximumSizeInBytes;
            EventLogMode logMode = configuration.LogMode;
            string owningProviderName = configuration.OwningProviderName;
            IEnumerable<string> providerNames = configuration.ProviderNames;
            int? providerLevel = configuration.ProviderLevel;
            long? providerKeywords = configuration.ProviderKeywords;
            int? providerBufferSize = configuration.ProviderBufferSize;
            int? providerMinimumNumberOfBuffers = configuration.ProviderMinimumNumberOfBuffers;
            int? providerMaximumNumberOfBuffers = configuration.ProviderMaximumNumberOfBuffers;
            int? providerLatency = configuration.ProviderLatency;
            Guid? providerControlGuid = configuration.ProviderControlGuid;

            // configuration.LogMode = EventLogMode.Circular;
            // configuration.LogMode = EventLogMode.AutoBackup;
            // configuration.LogMode = EventLogMode.Retain;
            // configuration.SaveChanges();
            // configuration.Dispose();
        }
    }
}
