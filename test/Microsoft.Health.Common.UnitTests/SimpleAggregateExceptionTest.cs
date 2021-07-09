// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Health.Common.UnitTests
{
    public class SimpleAggregateExceptionTest
    {
        [Fact]
        public void When_BadArgumentsSupplied_ExceptionIsThrown()
        {
            Assert.Throws<ArgumentException>(() => new SimpleAggregateException(new List<Exception>()));
            Assert.Throws<ArgumentException>(() => new SimpleAggregateException(string.Empty, new List<Exception>() { new Exception() }));
        }

        [Fact]
        public void When_SuppliedWithExceptions_ExpectedMessageCreated()
        {
            var exceptions = Enumerable.Range(0, 3).Select(i => new Exception("Mock Exception")).ToList();
            var expectedMessage = @"One or more exceptions have occured:
---- System.Exception: Mock Exception
---- System.Exception: Mock Exception
---- System.Exception: Mock Exception
";
            var exception = new SimpleAggregateException(exceptions);
            Assert.Equal(3, exception.InnerExceptions.Count);
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void When_SuppliedWithExceptions_AndMessage_ExpectedMessageCreated()
        {
            var exceptions = Enumerable.Range(0, 3).Select(i => new Exception("Mock Exception")).ToList();
            var expectedMessage = @"Custom exception message:
---- System.Exception: Mock Exception
---- System.Exception: Mock Exception
---- System.Exception: Mock Exception
";
            var exception = new SimpleAggregateException("Custom exception message", exceptions);
            Assert.Equal(3, exception.InnerExceptions.Count);
            Assert.Equal(expectedMessage, exception.Message);
        }
    }
}
