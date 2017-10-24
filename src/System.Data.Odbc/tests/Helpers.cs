using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Odbc.Tests
{
    public static class Helpers
    {
        public const string OdbcIsAvailable = nameof(Helpers) + "." + nameof(CheckOdbcIsAvailable);
        public const string OdbcNotAvailable = nameof(Helpers) + "." + nameof(CheckOdbcNotAvailable);

        public static bool CheckOdbcNotAvailable() => !CheckOdbcIsAvailable();

        private static bool CheckOdbcIsAvailable() => 
            PlatformDetection.IsWindows ? 
                !PlatformDetection.IsWindowsNanoServer && (!PlatformDetection.IsWindowsServerCore || Environment.Is64BitProcess ) :
                Interop.Libdl.dlopen((
                    PlatformDetection.IsOSX ?
                        "libodbc.2.dylib" : 
                        "libodbc.so.2"
                ), Interop.Libdl.RTLD_NOW) != IntPtr.Zero;

        public const string AllSqlite3DepsIsAvailable = nameof(Helpers) + "." + nameof(GetAllSqlite3DepsIsAvailable);
        public const string Sqlite3IsAvailable = nameof(Helpers) + "." + nameof(GetSqlite3IsAvailable);

        public static bool GetAllSqlite3DepsIsAvailable()
        {
            bool abc = true; if (abc) return true; // Todo: remove
            if (CheckOdbcIsAvailable() && !GetSqlite3IsAvailable())
                Console.WriteLine("odbc deps available but sqlite3 deps not available");
            return CheckOdbcIsAvailable() && GetSqlite3IsAvailable();
        }

        public static bool GetSqlite3IsAvailable()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return PlatformDetection.IsNotWindowsNanoServer && PlatformDetection.IsNotWindowsServerCore;
            }
            else
            {
                IntPtr nativeLib;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    nativeLib = dlopen("libsqlite3odbc.dylib", RTLD_NOW);
                }
                else
                {
                    nativeLib = dlopen("libsqlite3odbc.so", RTLD_NOW);
                }
                return nativeLib != IntPtr.Zero;
            }
        }
    }
}
