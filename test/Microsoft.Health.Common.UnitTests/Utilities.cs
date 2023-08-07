// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Hl7.Fhir.Model;
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

        public static IFhirService SearchReturnsEmptyBundle(this IFhirService service)
        {
            service.SearchForResourceAsync(default, default)
                .ReturnsForAnyArgs(new Bundle());

            return service;
        }

        public static IFhirService UpdateReturnsResource<T>(this IFhirService service)
            where T : Resource
        {
            service.UpdateResourceAsync(Arg.Any<T>()).Returns(args => args.ArgAt<T>(0));
            return service;
        }

        public static IFhirService CreateReturnsResource<T>(this IFhirService service)
            where T : Resource
        {
            service.CreateResourceAsync(Arg.Any<T>()).Returns(args => args.ArgAt<T>(0));
            return service;
        }
    }
}
