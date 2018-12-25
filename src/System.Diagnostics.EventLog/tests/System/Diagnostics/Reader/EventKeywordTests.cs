// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.Eventing.Reader;
using Xunit;

namespace System.Diagnostics.Tests
{
    public class EventKeywordTests
    {
        //[Fact]
        public void KeywordTests()
        {
            EventKeyword keyword = GetX();
            Assert.NotNull(keyword.Name);
            Assert.NotNull(keyword.DisplayName);
        }
    }
}