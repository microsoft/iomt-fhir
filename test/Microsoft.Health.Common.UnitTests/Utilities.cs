// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Extensions.Fhir.Service;
using NSubstitute;

namespace Microsoft.Health.Common
{
    public static class Utilities
    {
        public static IFhirService CreateMockFhirService()
        {
            return Substitute.For<IFhirService>();
        }
    }
}
