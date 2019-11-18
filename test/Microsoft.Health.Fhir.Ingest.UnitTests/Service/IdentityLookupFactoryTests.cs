// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.Ingest.Data;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class IdentityLookupFactoryTests
    {
        private readonly ITestOutputHelper _output;

        public IdentityLookupFactoryTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GivenDefaultInstance_WhenCreate_ThenAllEnumerationValuesReturnedWithNullValue_Test()
        {
            var ids = IdentityLookupFactory.Instance.Create();

            foreach (var value in Enum.GetValues(typeof(ResourceType)))
            {
                var enumValue = (ResourceType)value;
                _output.WriteLine($"Checking for key {enumValue}.");
                Assert.Null(ids[(ResourceType)value]);
                _output.WriteLine($"Key {enumValue} found with null value.");
            }
        }
    }
}
