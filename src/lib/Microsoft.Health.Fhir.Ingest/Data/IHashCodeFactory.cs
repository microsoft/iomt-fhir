// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public interface IHashCodeFactory
    {
        /// <summary>
        /// Creates a hashcode generator capable of producing deterministic hash codes. For the same value the generator will produce identical hash codes
        /// across subsequent runs of the application.
        /// </summary>
        /// <returns>A HashCode generator for deterministic hash code</returns>
        IHashCodeGenerator CreateDeterministicHashCodeGenerator();
    }
}
