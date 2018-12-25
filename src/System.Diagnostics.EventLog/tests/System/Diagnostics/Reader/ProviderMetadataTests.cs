// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Threading;
using Xunit;

namespace System.Diagnostics.Tests
{
    public class ProviderMetadataTests
    {
        [ConditionalFact(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        public void Properties_DoNotThrow()
        {
            using (var session = new EventLogSession())
            {
                Assert.NotEmpty(session.GetProviderNames());
                foreach (var providerName in session.GetProviderNames())
                {
                    ProviderMetadata providerMetadata = new ProviderMetadata(providerName);
                    IList<EventKeyword> keywords = providerMetadata.Keywords;
                    foreach (var x in keywords)
                    {
                        Assert.NotEmpty(x.DisplayName);
                        Assert.NotEmpty(x.Name);
                        Assert.NotNull(x.Value);
                        Console.WriteLine("DisplayName " + x.DisplayName);
                        Console.WriteLine("Name " + x.Name);
                        Console.WriteLine("Value " + x.Value);
                    }
                    // string name = providerMetadata.Name;
                    Console.WriteLine("name " + providerMetadata.Name);
                    // Guid id = providerMetadata.Id;
                    Console.WriteLine("id " + providerMetadata.Id);
                    // string messageFilePath = providerMetadata.MessageFilePath;
                    Console.WriteLine("messageFilePath " + providerMetadata.MessageFilePath);
                    // string resourceFilePath = providerMetadata.ResourceFilePath;
                    Console.WriteLine("ResourceFilePath " + providerMetadata.ResourceFilePath);
                    // string parameterFilePath = providerMetadata.ParameterFilePath;
                    Console.WriteLine("parameterFilePath " + providerMetadata.ParameterFilePath);
                    // Uri helpLink = providerMetadata.HelpLink;
                    Console.WriteLine("HelpLink " + providerMetadata.HelpLink);
                    // string displayName = providerMetadata.DisplayName;
                    Console.WriteLine("displayName " + providerMetadata.DisplayName);
                    // IList<EventLogLink> logLinks = providerMetadata.LogLinks;
                    foreach (var x in providerMetadata.LogLinks)
                        Console.WriteLine("logLinks " + x);
                    // IList<EventLevel> levels = providerMetadata.Levels;
                    foreach (var x in providerMetadata.Levels)
                        Console.WriteLine("Levels " + x);
                    // IList<EventOpcode> ppcodes = providerMetadata.Opcodes;
                    foreach (var x in providerMetadata.Opcodes)
                        Console.WriteLine("Opcodes " + x);
                    // IEnumerable<EventMetadata> events = providerMetadata.Events;
                    foreach (var x in providerMetadata.Events)
                        Console.WriteLine("events " + x);
                    // Assert.NotEmpty(events);
                    // IList<EventTask> tasks = providerMetadata.Tasks;
                    if (providerMetadata.Tasks !=null)
                    foreach (var x in providerMetadata.Tasks)
                        Console.WriteLine("Tasks " + x);
                }
            }
        }

        // [ConditionalFact(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        // public void Properties_DoNotThrow()
        // {
        //     using (var session = new EventLogSession())
        //     {
        //         Assert.NotEmpty(session.GetProviderNames());
        //         Console.WriteLine();
        //         ProviderMetadata providerMetadata = new ProviderMetadata(session.GetProviderNames().GetEnumerator().Current);
        //         string name = providerMetadata.Name;
        //         Guid id = providerMetadata.Id;
        //         string messageFilePath = providerMetadata.MessageFilePath;
        //         string resourceFilePath = providerMetadata.ResourceFilePath;
        //         string parameterFilePath = providerMetadata.ParameterFilePath;
        //         Uri helpLink = providerMetadata.HelpLink;
        //         string displayName = providerMetadata.DisplayName;
        //         IList<EventLogLink> logLinks = providerMetadata.LogLinks;
        //         IList<EventLevel> levels = providerMetadata.Levels;
        //         IList<EventOpcode> ppcodes = providerMetadata.Opcodes;
        //         IList<EventKeyword> keywords = providerMetadata.Keywords;
        //         IEnumerable<EventMetadata> events = providerMetadata.Events;
        //         Assert.Empty(events);
        //         IList<EventTask> tasks = providerMetadata.Tasks;
        //     }
        // }
    
        // [ConditionalFact(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        // public void Draft()
        // {
        //     // eventMetadata.LogLink
        //     // eventMetadata.Level
        //     // Opcode
        //     // Task
        //     // Keywords
        // }

        // [ConditionalFact(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        // public void EventMetadata()
        // {
        //     EventLogQuery eventsQuery = new EventLogQuery("Application", PathType.LogName, "*[System]");
        //     using (var logReader = new EventLogReader(eventsQuery))
        //     {
        //         // For each event returned from the query
        //         for (EventRecord eventInstance = logReader.ReadEvent();
        //                 eventInstance != null;
        //                 eventInstance = logReader.ReadEvent())
        //         {
        //             List<object> varRepSet = new List<object>();
        //             // for (int i = 0; i < eventInstance.Properties.Count; i++)
        //             // {
        //             //     varRepSet.Add((object)(eventInstance.Properties[i].Value.ToString()));
        //             // }
        //             string description = eventInstance.FormatDescription(null);

        //             string description1 = eventInstance.FormatDescription();
        //             // Assert.NotEmpty(varRepSet);
        //         }
        //     }
        // }
    }
}
