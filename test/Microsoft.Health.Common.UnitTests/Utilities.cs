// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Extensions.Fhir.Repository;
using NSubstitute;

namespace Microsoft.Health.Common
{
    public static class Utilities
    {
        public static IFhirServiceRepository CreateMockFhirClient()
        {
            return Substitute.For<IFhirServiceRepository>();
        }
    }
}
