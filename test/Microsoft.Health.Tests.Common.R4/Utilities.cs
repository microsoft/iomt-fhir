// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Net.Http;
using Hl7.Fhir.Rest;
using NSubstitute;

namespace Microsoft.Health.Tests.Common
{
    public static class Utilities
    {
        public static MockFhirResourceHttpMessageHandler CreateMockMessageHandler()
        {
            return Substitute.ForPartsOf<MockFhirResourceHttpMessageHandler>();
        }

        public static FhirClient CreateMockFhirClient(HttpMessageHandler messageHandler = null)
        {
            return Substitute.For<FhirClient>("https://localhost", FhirClientSettings.CreateDefault(), messageHandler ?? CreateMockMessageHandler(), null);
        }
    }
}
