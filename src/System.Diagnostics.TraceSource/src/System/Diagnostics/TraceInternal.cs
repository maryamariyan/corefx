// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using System.IO;
using System.Collections;
using System.Reflection;

namespace System.Diagnostics
{
    internal class TraceDebug
    {
        public bool AutoFlush { get { return TraceInternal.AutoFlush; } set { TraceInternal.AutoFlush = value; } }
        public void Assert(bool condition) { TraceInternal.Assert(condition); }
        public void Assert(bool condition, string message) { TraceInternal.Assert(condition, message); }
        public void Assert(bool condition, string message, string detailMessage) { TraceInternal.Assert(condition, message, detailMessage); }
        public void Close() { TraceInternal.Close(); }
        public void Fail(string message) { TraceInternal.Fail(message); }
        public void Fail(string message, string detailMessage) { TraceInternal.Fail(message, detailMessage); }
        public void Flush() { TraceInternal.Flush(); }
        public void Indent() { TraceInternal.Indent(); }
        public int IndentLevel { get { return TraceInternal.IndentLevel; } set { TraceInternal.IndentLevel = value; } }
        public int IndentSize { get { return TraceInternal.IndentSize; } set { TraceInternal.IndentSize = value; } }
        public void Unindent() { TraceInternal.Unindent(); }
        public void Write(object value) { TraceInternal.Write(value); }
        public void Write(object value, string category) { TraceInternal.Write(value, category); }
        public void Write(string message) { TraceInternal.Write(message); }
        public void Write(string message, string category) { TraceInternal.Write(message, category); }
        public void WriteLine(object value) { TraceInternal.WriteLine(value); }
        public void WriteLine(object value, string category) { TraceInternal.WriteLine(value, category); }
        public void WriteLine(string message) { TraceInternal.WriteLine(message); }
        public void WriteLine(string message, string category) { TraceInternal.WriteLine(message, category); }
    }

    internal static class TraceInternal 
    {
        private static volatile string s_appName = null;
        private static volatile TraceListenerCollection s_listeners;
        private static volatile bool s_autoFlush;
        private static volatile bool s_useGlobalLock;
        [ThreadStatic]
        private static int t_indentLevel;
        private static volatile int s_indentSize;
        private static volatile bool s_settingsInitialized;


        // this is internal so TraceSource can use it.  We want to lock on the same object because both TraceInternal and 
        // TraceSource could be writing to the same listeners at the same time. 
        internal static readonly object critSec = new object();

        public static TraceListenerCollection Listeners
        {
            get
            {
                InitializeSettings();
                if (s_listeners == null)
                {
                    lock (critSec)
                    {
                        if (s_listeners == null)
                        {
                            // In the absence of config support, the listeners by default add
                            // DefaultTraceListener to the listener collection.
                            s_listeners = new TraceListenerCollection();
                            TraceListener defaultListener = new DefaultTraceListener();
                            defaultListener.IndentLevel = t_indentLevel;
                            defaultListener.IndentSize = s_indentSize;
                            s_listeners.Add(defaultListener);

                            // instead of base class:
                            // Approach2: we could set up Debug delegates here to TraceInternal implementation
                            Type debugImplementation = Type.GetType("System.Diagnostics.DebugDelegateWrapper", throwOnError: false);

                            FieldInfo fieldHook = debugImplementation.GetField("AutoFlushGet", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Func<bool>(() => TraceInternal.AutoFlush));

                            fieldHook = debugImplementation.GetField("AutoFlushSet", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action<bool>((value) => { TraceInternal.AutoFlush = value; }));

                            fieldHook = debugImplementation.GetField("IndentLevelGet", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Func<int>(() => TraceInternal.IndentLevel));

                            fieldHook = debugImplementation.GetField("IndentLevelSet", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action<int>((value) => { TraceInternal.IndentLevel = value; }));

                            fieldHook = debugImplementation.GetField("IndentLevelGet", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Func<int>(() => TraceInternal.IndentSize));

                            fieldHook = debugImplementation.GetField("IndentLevelSet", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action<int>((value) => { TraceInternal.IndentSize = value; }));
                            
                            fieldHook = debugImplementation.GetField("AssertOverload1", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action<bool>((condition) => TraceInternal.Assert(condition)));
                            
                            fieldHook = debugImplementation.GetField("AssertOverload2", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action<bool, string>((condition, message) => TraceInternal.Assert(condition, message)));
                            
                            fieldHook = debugImplementation.GetField("AssertOverload3", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action<bool, string, string>((condition, message, detailMessage) => TraceInternal.Assert(condition, message, detailMessage)));
                            
                            fieldHook = debugImplementation.GetField("Close", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action(() => TraceInternal.Close()));
                            
                            fieldHook = debugImplementation.GetField("FailOverload1", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action<string>((message) => TraceInternal.Fail(message)));
                            
                            fieldHook = debugImplementation.GetField("FailOverload2", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action<string, string>((message, detailMessage) => TraceInternal.Fail(message, detailMessage)));
                            
                            fieldHook = debugImplementation.GetField("Flush", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action(() => TraceInternal.Flush()));
                            
                            fieldHook = debugImplementation.GetField("Indent", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action(() => TraceInternal.Indent()));
                            
                            fieldHook = debugImplementation.GetField("Unindent", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action(() => TraceInternal.Unindent()));

                            fieldHook = debugImplementation.GetField("WriteOverload1", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action<object>((value) => TraceInternal.Write(value)));
                            
                            fieldHook = debugImplementation.GetField("WriteOverload2", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action<object, string>((value, category) => TraceInternal.Write(value, category)));

                            fieldHook = debugImplementation.GetField("WriteOverload3", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action<string>((value) => TraceInternal.Write(value)));
                            
                            fieldHook = debugImplementation.GetField("WriteOverload4", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action<string, string>((value, category) => TraceInternal.Write(value, category)));

                            fieldHook = debugImplementation.GetField("WriteLineOverload1", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action<object>((value) => TraceInternal.WriteLine(value)));
                            
                            fieldHook = debugImplementation.GetField("WriteLineOverload2", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action<object, string>((value, category) => TraceInternal.WriteLine(value, category)));

                            fieldHook = debugImplementation.GetField("WriteLineOverload3", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action<string>((value) => TraceInternal.WriteLine(value)));
                            
                            fieldHook = debugImplementation.GetField("WriteLineOverload4", BindingFlags.Static | BindingFlags.NonPublic);
                            fieldHook.SetValue(null, new Action<string, string>((value, category) => TraceInternal.WriteLine(value, category)));
                        }
                    }
                }
                return s_listeners;
            }
        }

        internal static string AppName
        {
            get
            {
                if (s_appName == null)
                {
                    s_appName = Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty;
                }
                return s_appName;
            }
        }

        public static bool AutoFlush
        {
            get
            {
                InitializeSettings();
                return s_autoFlush;
            }

            set
            {
                InitializeSettings();
                s_autoFlush = value;
            }
        }

        public static bool UseGlobalLock
        {
            get
            {
                InitializeSettings();
                return s_useGlobalLock;
            }

            set
            {
                InitializeSettings();
                s_useGlobalLock = value;
            }
        }

        public static int IndentLevel
        {
            get { return t_indentLevel; }

            set
            {
                // Use global lock
                lock (critSec)
                {
                    // We don't want to throw here -- it is very bad form to have debug or trace
                    // code throw exceptions!
                    if (value < 0)
                    {
                        value = 0;
                    }
                    t_indentLevel = value;

                    if (s_listeners != null)
                    {
                        foreach (TraceListener listener in Listeners)
                        {
                            listener.IndentLevel = t_indentLevel;
                        }
                    }
                }
            }
        }

        public static int IndentSize
        {
            get
            {
                InitializeSettings();
                return s_indentSize;
            }

            set
            {
                InitializeSettings();
                SetIndentSize(value);
            }
        }

        private static void SetIndentSize(int value)
        {
            // Use global lock
            lock (critSec)
            {
                // We don't want to throw here -- it is very bad form to have debug or trace
                // code throw exceptions!            
                if (value < 0)
                {
                    value = 0;
                }

                s_indentSize = value;

                if (s_listeners != null)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.IndentSize = s_indentSize;
                    }
                }
            }
        }

        public static void Indent()
        {
            // Use global lock
            lock (critSec)
            {
                InitializeSettings();
                if (t_indentLevel < int.MaxValue)
                {
                    t_indentLevel++;
                }
                foreach (TraceListener listener in Listeners)
                {
                    listener.IndentLevel = t_indentLevel;
                }
            }
        }

        public static void Unindent()
        {
            // Use global lock
            lock (critSec)
            {
                InitializeSettings();
                if (t_indentLevel > 0)
                {
                    t_indentLevel--;
                }
                foreach (TraceListener listener in Listeners)
                {
                    listener.IndentLevel = t_indentLevel;
                }
            }
        }

        public static void Flush()
        {
            if (s_listeners != null)
            {
                if (UseGlobalLock)
                {
                    lock (critSec)
                    {
                        foreach (TraceListener listener in Listeners)
                        {
                            listener.Flush();
                        }
                    }
                }
                else
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        if (!listener.IsThreadSafe)
                        {
                            lock (listener)
                            {
                                listener.Flush();
                            }
                        }
                        else
                        {
                            listener.Flush();
                        }
                    }
                }
            }
        }

        public static void Close()
        {
            if (s_listeners != null)
            {
                // Use global lock
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.Close();
                    }
                }
            }
        }

        public static void Assert(bool condition)
        {
            if (condition) return;
            Fail(string.Empty);
        }

        public static void Assert(bool condition, string message)
        {
            if (condition) return;
            Fail(message);
        }

        public static void Assert(bool condition, string message, string detailMessage)
        {
            if (condition) return;
            Fail(message, detailMessage);
        }

        public static void Fail(string message)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.Fail(message);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
            else
            {
                foreach (TraceListener listener in Listeners)
                {
                    if (!listener.IsThreadSafe)
                    {
                        lock (listener)
                        {
                            listener.Fail(message);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else
                    {
                        listener.Fail(message);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
        }

        public static void Fail(string message, string detailMessage)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.Fail(message, detailMessage);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
            else
            {
                foreach (TraceListener listener in Listeners)
                {
                    if (!listener.IsThreadSafe)
                    {
                        lock (listener)
                        {
                            listener.Fail(message, detailMessage);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else
                    {
                        listener.Fail(message, detailMessage);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
        }

        private static void InitializeSettings()
        {
            if (!s_settingsInitialized)
            {
                // we should avoid 2 threads altering the state concurrently for predictable behavior
                // though it may not be strictly necessary at present
                lock (critSec)
                {
                    if (!s_settingsInitialized)
                    {
                        SetIndentSize(DiagnosticsConfiguration.IndentSize);
                        s_autoFlush = DiagnosticsConfiguration.AutoFlush;
                        s_useGlobalLock = DiagnosticsConfiguration.UseGlobalLock;
                        s_settingsInitialized = true;
                    }
                }
            }
        }

        // This method refreshes all the data from the configuration file, so that updated to the configuration file are mirrored
        // in the System.Diagnostics.Trace class
        internal static void Refresh()
        {
            lock (critSec)
            {
                s_settingsInitialized = false;
                s_listeners = null;
            }
            InitializeSettings();
        }

        public static void TraceEvent(TraceEventType eventType, int id, string format, params object[] args)
        {
            TraceEventCache EventCache = new TraceEventCache();

            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    if (args == null)
                    {
                        foreach (TraceListener listener in Listeners)
                        {
                            listener.TraceEvent(EventCache, AppName, eventType, id, format);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else
                    {
                        foreach (TraceListener listener in Listeners)
                        {
                            listener.TraceEvent(EventCache, AppName, eventType, id, format, args);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                }
            }
            else
            {
                if (args == null)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        if (!listener.IsThreadSafe)
                        {
                            lock (listener)
                            {
                                listener.TraceEvent(EventCache, AppName, eventType, id, format);
                                if (AutoFlush) listener.Flush();
                            }
                        }
                        else
                        {
                            listener.TraceEvent(EventCache, AppName, eventType, id, format);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                }
                else
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        if (!listener.IsThreadSafe)
                        {
                            lock (listener)
                            {
                                listener.TraceEvent(EventCache, AppName, eventType, id, format, args);
                                if (AutoFlush) listener.Flush();
                            }
                        }
                        else
                        {
                            listener.TraceEvent(EventCache, AppName, eventType, id, format, args);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                }
            }
        }


        public static void Write(string message)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.Write(message);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
            else
            {
                foreach (TraceListener listener in Listeners)
                {
                    if (!listener.IsThreadSafe)
                    {
                        lock (listener)
                        {
                            listener.Write(message);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else
                    {
                        listener.Write(message);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
        }

        public static void Write(object value)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.Write(value);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
            else
            {
                foreach (TraceListener listener in Listeners)
                {
                    if (!listener.IsThreadSafe)
                    {
                        lock (listener)
                        {
                            listener.Write(value);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else
                    {
                        listener.Write(value);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
        }

        public static void Write(string message, string category)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.Write(message, category);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
            else
            {
                foreach (TraceListener listener in Listeners)
                {
                    if (!listener.IsThreadSafe)
                    {
                        lock (listener)
                        {
                            listener.Write(message, category);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else
                    {
                        listener.Write(message, category);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
        }

        public static void Write(object value, string category)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.Write(value, category);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
            else
            {
                foreach (TraceListener listener in Listeners)
                {
                    if (!listener.IsThreadSafe)
                    {
                        lock (listener)
                        {
                            listener.Write(value, category);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else
                    {
                        listener.Write(value, category);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
        }

        public static void WriteLine(string message)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.WriteLine(message);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
            else
            {
                foreach (TraceListener listener in Listeners)
                {
                    if (!listener.IsThreadSafe)
                    {
                        lock (listener)
                        {
                            listener.WriteLine(message);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else
                    {
                        listener.WriteLine(message);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
        }

        public static void WriteLine(object value)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.WriteLine(value);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
            else
            {
                foreach (TraceListener listener in Listeners)
                {
                    if (!listener.IsThreadSafe)
                    {
                        lock (listener)
                        {
                            listener.WriteLine(value);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else
                    {
                        listener.WriteLine(value);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
        }

        public static void WriteLine(string message, string category)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.WriteLine(message, category);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
            else
            {
                foreach (TraceListener listener in Listeners)
                {
                    if (!listener.IsThreadSafe)
                    {
                        lock (listener)
                        {
                            listener.WriteLine(message, category);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else
                    {
                        listener.WriteLine(message, category);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
        }

        public static void WriteLine(object value, string category)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.WriteLine(value, category);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
            else
            {
                foreach (TraceListener listener in Listeners)
                {
                    if (!listener.IsThreadSafe)
                    {
                        lock (listener)
                        {
                            listener.WriteLine(value, category);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else
                    {
                        listener.WriteLine(value, category);
                        if (AutoFlush) listener.Flush();
                    }
                }
            }
        }

        public static void WriteIf(bool condition, string message)
        {
            if (condition)
                Write(message);
        }

        public static void WriteIf(bool condition, object value)
        {
            if (condition)
                Write(value);
        }

        public static void WriteIf(bool condition, string message, string category)
        {
            if (condition)
                Write(message, category);
        }

        public static void WriteIf(bool condition, object value, string category)
        {
            if (condition)
                Write(value, category);
        }

        public static void WriteLineIf(bool condition, string message)
        {
            if (condition)
                WriteLine(message);
        }

        public static void WriteLineIf(bool condition, object value)
        {
            if (condition)
                WriteLine(value);
        }

        public static void WriteLineIf(bool condition, string message, string category)
        {
            if (condition)
                WriteLine(message, category);
        }

        public static void WriteLineIf(bool condition, object value, string category)
        {
            if (condition)
                WriteLine(value, category);
        }
    }
}
