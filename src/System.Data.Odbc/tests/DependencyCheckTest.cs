// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;

namespace System.Data.Odbc.Tests
{
    public class DependencyCheckTest
    {
        [ConditionalFact(typeof(Helpers), nameof(Helpers.OdbcNotAvailable), nameof(Helpers.IsNotWindowsServerCore))]
        public void OdbcConnection_OpenWhenOdbcNotInstalled_ThrowsException()
        {
            using (var connection = new OdbcConnection(ConnectionStrings.WorkingConnection))
            {
                Assert.Throws<DllNotFoundException>(() => connection.Open()); 
            }
        }
    }
}
