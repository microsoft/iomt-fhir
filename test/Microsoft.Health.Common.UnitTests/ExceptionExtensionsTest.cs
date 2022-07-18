// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Extension;
using Xunit;

namespace Microsoft.Health.Common
{
    public class ExceptionExtensionsTest
    {
        [Fact]
        public void ExceptionNotLoggedByDefault()
        {
            var exception = new Exception();
            Assert.False(exception.LogToCustomer());
        }

        [Fact]
        public void ExceptionSetLogToCustomer()
        {
            var exception = new Exception();
            exception.SetLogToCustomer(true);
            Assert.True(exception.LogToCustomer());
            exception.SetLogToCustomer(false);
            Assert.False(exception.LogToCustomer());
        }
    }
}
