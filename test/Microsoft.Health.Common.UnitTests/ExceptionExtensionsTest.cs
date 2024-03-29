// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Extension;
using Microsoft.Health.Common.Telemetry.Exceptions;
using Xunit;

namespace Microsoft.Health.Common
{
    public class ExceptionExtensionsTest
    {
        [Fact]
        public void ExceptionNotLoggedByDefault()
        {
            var exception = new Exception();
            Assert.False(exception.IsLogForwardingEnabled());
        }

        [Fact]
        public void ExceptionSetLogForwarding()
        {
            var exception = new Exception();
            exception.SetLogForwarding(true);
            Assert.True(exception.IsLogForwardingEnabled());
            exception.SetLogForwarding(false);
            Assert.False(exception.IsLogForwardingEnabled());
        }

        [Fact]
        public void ThirdPartyLoggedException_Enables_LogForwarding()
        {
            var exception = new TestException();
            Assert.True(exception.IsLogForwardingEnabled());
        }

        private class TestException : ThirdPartyLoggedFormattableException
        {
        }
    }
}
