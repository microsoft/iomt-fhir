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
        /// Generates a hashcode given the supplied input.
        /// </summary>
        /// <param name="value">The value to generate a hash code for</param>
        /// <returns>The hash code value represented as an integer</returns>
        int GenerateHashCode(string value);
    }
}
