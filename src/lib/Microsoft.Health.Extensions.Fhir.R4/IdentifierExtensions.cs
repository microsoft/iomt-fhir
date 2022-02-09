// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Hl7.Fhir.Model;

namespace Microsoft.Health.Extensions.Fhir
{
    public static class IdentifierExtensions
    {
        public static string ToSearchQueryParameter(this Identifier identifier)
        {
            EnsureArg.IsNotNull(identifier, nameof(identifier));

            return $"identifier={identifier.System}|{identifier.Value}";
        }
    }
}
