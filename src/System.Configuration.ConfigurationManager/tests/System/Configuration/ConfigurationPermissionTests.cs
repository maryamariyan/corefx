// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.ConfigurationTests
{
    using System.Configuration;
    using System.Security;
    using System.Security.Permissions;
    using Xunit;

    public class ConfigurationPermissionTests
    {
        [Fact]
        public static void ConfigurationPermissionCallMethods()
        {
            ConfigurationPermission cp = new ConfigurationPermission(new PermissionState());
            bool isunrestricted = cp.IsUnrestricted();
            IPermission other = cp.Copy();
            other = cp.Union(other);
            other = cp.Intersect(other);
            bool isSubsetOf = cp.IsSubsetOf(other);
            SecurityElement se = new SecurityElement("");
            cp.FromXml(se);
            se = cp.ToXml();
        }

        [Fact]
        public static void ConfigurationPermissionAttributeCallMethods()
        {
            ConfigurationPermissionAttribute cpa = new ConfigurationPermissionAttribute(new SecurityAction());
            IPermission ip = cpa.CreatePermission();
        }
    }
}