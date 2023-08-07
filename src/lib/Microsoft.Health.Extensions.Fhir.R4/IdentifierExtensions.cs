// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Security.Cryptography;
using System.Text;
using EnsureThat;

namespace Microsoft.Health.Extensions.Fhir
{
    public static class IdentifierExtensions
    {
        public static string ComputeHashForIdentifier(this Hl7.Fhir.Model.Identifier identifier)
        {
            EnsureArg.IsNotNullOrWhiteSpace(identifier.Value, nameof(identifier.Value));

            string plainTextSystemAndId = $"{identifier.System}_{identifier.Value}";

            using (SHA256 hashAlgorithm = SHA256.Create())
            {
                byte[] bytes = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(plainTextSystemAndId));

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
