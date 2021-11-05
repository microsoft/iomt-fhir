// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class DeterministicHashCodeGenerator : IHashCodeGenerator
    {
        public void Dispose()
        {
        }

        public string GenerateHashCode(string valueToHash)
        {
            EnsureArg.IsNotNull(valueToHash, nameof(valueToHash));

            int h = 0;

            foreach (char c in valueToHash.ToCharArray())
            {
                h = unchecked((31 * h) + c);
            }

            return ((byte)(Math.Abs(h) % 256)).ToString(CultureInfo.InvariantCulture);
        }
    }
}
