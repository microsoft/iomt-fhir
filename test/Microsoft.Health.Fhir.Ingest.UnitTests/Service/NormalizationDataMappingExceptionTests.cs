// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class NormalizationDataMappingExceptionTests
    {
        [Fact]
        public void GivenNullException_WhenCtor_ThenNullArgumentExceptionThrown_Test()
        {
            Assert.Throws<ArgumentNullException>(() => new NormalizationDataMappingException(null));
        }

        [Fact]
        public void GivenSingleException_WhenCtor_ThenMessageSet_Test()
        {
            var ex = new Exception("message");
            var ndmex = new NormalizationDataMappingException(ex);

            Assert.Equal(ex.Message, ndmex.Message);
        }

        [Fact]
        public void GivenMultipleExceptions_WhenCtor_ThenEachMessageSet_Test()
        {
            var ex2 = new Exception("nested b");
            var ex1 = new Exception("nested a", ex2);
            var ex = new Exception("message", ex1);

            var ndmex = new NormalizationDataMappingException(ex);

            var messages = ndmex.Message.Split('\n');

            Assert.Equal(3, messages.Length);
            Assert.Equal(ex.Message, messages[0]);
            Assert.Equal($"1:{ex1.Message}", messages[1]);
            Assert.Equal($"2:{ex2.Message}", messages[2]);
        }
    }
}
