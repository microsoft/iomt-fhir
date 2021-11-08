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

        public string GenerateHashCode(string valueToHash, int range = 256)
        {
            EnsureArg.IsGt(range, 0, nameof(range));
            EnsureArg.IsNotNull(valueToHash, nameof(valueToHash));

            int h = 0;

            foreach (char c in valueToHash.ToCharArray())
            {
                h = unchecked((31 * h) + c);
            }

            return (Math.Abs(h) % range).ToString(CultureInfo.InvariantCulture);
        }
    }
}
