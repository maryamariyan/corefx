// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using Xunit;

namespace System.Diagnostics.Tests
{
    public abstract class DebugTests
    {
        protected abstract bool DebugUsesTraceListeners { get; }
        private static readonly WriteLogger s_writeLogger = new WriteLogger();

        public DebugTests()
        {
            if (DebugUsesTraceListeners)
            {
                // Triggers code to wire up TraceListeners with Debug
                Assert.Equal(1, Trace.Listeners.Count);
            }
            s_writeLogger.OriginalProvider = Debug.Provider;
        }

        protected void VerifyLogged(Action test, string expectedOutput)
        {
            Debug.Provider = s_writeLogger;
            try
            {
                s_writeLogger.Clear();
                test();
#if DEBUG
                Assert.Equal(expectedOutput, s_writeLogger.LoggedOutput);
#else
                Assert.Equal(string.Empty, s_writeLogger.LoggedOutput);
#endif
            }
            finally
            {
                Debug.Provider = s_writeLogger.OriginalProvider;
            }

            // Then also use the actual logger for this platform, just to verify
            // that nothing fails.
            test();
        }

        protected void VerifyAssert(Action test, params string[] expectedOutputStrings)
        {
            s_writeLogger.MockAssert = true;
            Debug.Provider = s_writeLogger;

            try
            {
                s_writeLogger.Clear();
                test();
#if DEBUG
                for (int i = 0; i < expectedOutputStrings.Length; i++)
                {
                    Assert.Contains(expectedOutputStrings[i], s_writeLogger.LoggedOutput);
                    Assert.Contains(expectedOutputStrings[i], s_writeLogger.AssertUIOutput);
                }
#else
                Assert.Equal(string.Empty, s_writeLogger.LoggedOutput);
                Assert.Equal(string.Empty, s_writeLogger.AssertUIOutput);
#endif

            }
            finally
            {
                s_writeLogger.MockAssert = false;
                Debug.Provider = s_writeLogger.OriginalProvider;
            }
        }

        // Use this instead to show application for WPF
        // As is I won't be able to customize DefaultTraceListener 
        private class WriteLogger : DebugProvider
        {
            public DebugProvider OriginalProvider { get; set; }

            public bool MockAssert { get; set; } = false;

            public string LoggedOutput { get; private set; }

            public string AssertUIOutput { get; private set; }

            public void Clear()
            {
                LoggedOutput = string.Empty;
                AssertUIOutput = string.Empty;
            }

            public override void ShowDialog(string stackTrace, string message, string detailMessage, string errorSource)
            {
                if (!MockAssert)
                {
                    OriginalProvider.ShowDialog(stackTrace, message, detailMessage, errorSource);
                    return;
                }
                AssertUIOutput += stackTrace + message + detailMessage + errorSource;
            }

            public override void Write(string message)
            {
                Assert.NotNull(message);
                LoggedOutput += message;
            }
        }
    }
}
