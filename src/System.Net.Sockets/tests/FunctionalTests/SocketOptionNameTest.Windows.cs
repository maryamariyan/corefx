// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Net.Sockets.Tests
{
    public partial class SocketOptionNameTest
    {
        private unsafe static int setsockopt(int socket, int level, int option_name, void* option_value, uint option_len)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
