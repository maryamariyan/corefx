// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Microsoft.Internal
{
    internal struct ReadLock : IDisposable
    {
        private readonly Lock _lock;
        private int _isDisposed;

        public ReadLock(Lock @lock)
        {
            this._isDisposed = 0;
            this._lock = @lock;
            this._lock.EnterReadLock();
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref this._isDisposed, 1, 0) == 0)
            {
                this._lock.ExitReadLock();
            }
        }
    }
}
