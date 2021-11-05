// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public interface IHashCodeGenerator : IDisposable
    {
        /// <summary>
        /// Generates a hashcode given the supplied input. The hashcode generated will be a positive number represented as a string.
        /// The hashcode will further be restricted by applying the moduluss of the supplied range parameter.
        /// </summary>
        /// <param name="value">The value to generate a hash code for</param>
        /// <param name="range">A range to restrict the returned hashcodes to (i.e. 0 - range). Must be a greater than zero.</param>
        /// <returns>The hash code value represented as a string</returns>
        string GenerateHashCode(string value, int range = 256);
    }
}
