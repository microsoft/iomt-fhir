// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace Microsoft.Health.Tests.Common
{
    public class MockFhirResourceHttpMessageHandler : MockHttpMessageHandler<Resource>
    {
        protected override string GetJsonContent(Resource content)
        {
            return content?.ToJson();
        }
    }
}
