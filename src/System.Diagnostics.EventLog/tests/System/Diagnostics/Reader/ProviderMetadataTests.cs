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
        public void ProviderNameTests()
        {
            string log = "Application";
            string source = "Source_" + nameof(ProviderNameTests);

            try
            {
                if (EventLog.SourceExists(source))
                {
                    EventLog.DeleteEventSource(source);
                }

                EventLog.CreateEventSource(source, log);
            }
            finally
            {
                Assert.Throws<EventLogNotFoundException>(() => new ProviderMetadata("Source_Does_Not_Exist"));
                foreach (string sourceName in new [] { "", source})
                {
                    var providerMetadata = new ProviderMetadata(sourceName);
                    Assert.Null(providerMetadata.DisplayName);
                    Assert.Equal(sourceName, providerMetadata.Name);
                    Assert.Equal(new Guid(), providerMetadata.Id);
                    Assert.Empty(providerMetadata.Events);
                    Assert.Empty(providerMetadata.Keywords);
                    Assert.Empty(providerMetadata.Levels);
                    Assert.Empty(providerMetadata.Opcodes);
                    Assert.Empty(providerMetadata.Tasks);
                    Assert.NotEmpty(providerMetadata.LogLinks);
                    foreach (var logLink in providerMetadata.LogLinks)
                    {
                        Assert.True(logLink.IsImported);
                        Assert.Equal(log, logLink.DisplayName);
                        Assert.Equal(log, logLink.LogName);
                    }
                    if (sourceName.Equals(source))
                    {
                        Assert.Contains("EventLogMessages.dll", providerMetadata.MessageFilePath);
                        Assert.Contains("EventLogMessages.dll", providerMetadata.HelpLink.ToString());
                    }
                    else
                    {
                        Assert.Null(providerMetadata.MessageFilePath);
                        Assert.Null(providerMetadata.HelpLink);
                    }
                    Assert.Null(providerMetadata.ResourceFilePath);
                    Assert.Null(providerMetadata.ParameterFilePath);
                    providerMetadata.Dispose();
                }
            }
        }
               
        //  [ConditionalFact(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        //  public void EventMetadata()
        //  {
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
        //  }
        //[ConditionalFact(typeof(Helpers), nameof(Helpers.SupportsEventLogs))]
        public void Properties_DoNotThrow()
        {
            using (var session = new EventLogSession())
            {
                Assert.NotEmpty(session.GetProviderNames());
                foreach (string providerName in session.GetProviderNames())
                {
                    //Console.WriteLine("pname "+providerName);
                    ProviderMetadata providerMetadata;// = new ProviderMetadata(providerName);
                    try {
                    providerMetadata = new ProviderMetadata(providerName);
                    } catch(EventLogException) { continue; }
                    try {
                    IList<EventKeyword> keywords = providerMetadata.Keywords;
                    foreach (var x in keywords)
                    {
                        // Assert.NotEmpty(x.DisplayName);
                        Assert.NotEmpty(x.Name);
                        Assert.NotNull(x.Value);
                        //Console.WriteLine("DisplayName " + x.DisplayName);
                        //Console.WriteLine("Name " + x.Name);
                        //Console.WriteLine("Value " + x.Value);
                    }
                    } catch (EventLogException) { }
                    // string name = providerMetadata.Name;
                    //Console.WriteLine("name " + providerMetadata.Name);
                    // Guid id = providerMetadata.Id;
                    //Console.WriteLine("id " + providerMetadata.Id);
                    // string messageFilePath = providerMetadata.MessageFilePath;
                    //Console.WriteLine("messageFilePath " + providerMetadata.MessageFilePath);
                    // string resourceFilePath = providerMetadata.ResourceFilePath;
                    //Console.WriteLine("ResourceFilePath " + providerMetadata.ResourceFilePath);
                    // string parameterFilePath = providerMetadata.ParameterFilePath;
                    //Console.WriteLine("parameterFilePath " + providerMetadata.ParameterFilePath);
                    // Uri helpLink = providerMetadata.HelpLink;
                    try {
                    //Console.WriteLine("HelpLink " + providerMetadata.HelpLink);
                    } catch(EventLogException) { }
                    try {
                    // string displayName = providerMetadata.DisplayName;
                    //Console.WriteLine("displayName " + providerMetadata.DisplayName);
                    } catch (EventLogException) { }
                    // IList<EventLogLink> logLinks = providerMetadata.LogLinks;
                    foreach (var x in providerMetadata.LogLinks)
                    {
                        //Console.WriteLine("logLinks displayname: " + x.DisplayName + " isImported " + x.IsImported);
                        Assert.NotEmpty(x.LogName);
                    }
                    try {
                    // IList<EventLevel> levels = providerMetadata.Levels;
                    foreach (var x in providerMetadata.Levels)
                        {}//Console.WriteLine("Levels " + x);
                    } catch(EventLogException) { }
                    try 
                    {
                    // IList<EventOpcode> ppcodes = providerMetadata.Opcodes;
                        foreach (var opcode in providerMetadata.Opcodes)
                        {
                            if (opcode != null)
                            {
                                Assert.NotNull("Opcodes " + opcode.Value);
                                //Console.WriteLine("Opcodes " + opcode.DisplayName);
                            }
                        }
                    } catch(EventLogException) { }
                    // IEnumerable<EventMetadata> events = providerMetadata.Events;
                    try {
                    foreach (var x in providerMetadata.Events) {
                        //Console.WriteLine("events " + x.LogLink);
                        if(x.LogLink != null) // calls PrepareData()
                        {
                            //Console.WriteLine("events " + x.LogLink.LogName);
                            //Console.WriteLine("events " + x.LogLink.IsImported);
                            //Console.WriteLine("events " + x.LogLink.DisplayName);
                        }
                        //Console.WriteLine("events " + x.Level);
                        if(x.Level != null) // calls PrepareData()
                        {
                            //Console.WriteLine("events " + x.Level.Name);
                            //Console.WriteLine("events " + x.Level.DisplayName);
                        }
                        //Console.WriteLine("events " + x.Id);
                        //Console.WriteLine("events " + x.Version);
                        //Console.WriteLine("events " + x.Opcode);
                        if(x.Opcode != null) // calls PrepareData()
                        {
                            Assert.NotNull("Opcodes " + x.Opcode.Value);
                            //Console.WriteLine("Opcodes " + x.Opcode.DisplayName);
                        }
                        //Console.WriteLine("events " + x.Task);
                        if(x.Task != null) // calls PrepareData()
                        {
                            //Console.WriteLine("events " + x.Task.Name);
                            //Console.WriteLine("events " + x.Task.DisplayName);
                            //Console.WriteLine("events " + x.Task.Value);
                            //Console.WriteLine("events " + x.Task.EventGuid);
                        }
                        //Console.WriteLine("events " + x.Keywords);
                        if(x.Keywords != null) // calls PrepareData()
                            foreach(var keyword in x.Keywords)
                            {
                                if (keyword != null)
                                {
                                    //Console.WriteLine("events " + keyword.Name);
                                    //Console.WriteLine("events " + keyword.DisplayName);
                                }
                            }
                        //Console.WriteLine("events " + x.Template);
                        //Console.WriteLine("events " + x.Description);
                        }
                    } catch (EventLogInvalidDataException) { }
                     catch(EventLogException) { }
                    // Assert.NotEmpty(events);
                    // IList<EventTask> tasks = providerMetadata.Tasks;
                    try {
                    if (providerMetadata.Tasks !=null)
                        foreach (var x in providerMetadata.Tasks)
                        {
                            //Console.WriteLine("Tasks " + x);
                            //Console.WriteLine("DisplayName " + x.DisplayName);
                            //Console.WriteLine("Name " + x.Name);
                            //Console.WriteLine("Value " + x.Value);
                            //Console.WriteLine("Value " + x.EventGuid);
                        }
                    } catch(EventLogNotFoundException) { }
                     catch(EventLogException) { }
                    providerMetadata.Dispose();
                }
            }
        }
    }
}
