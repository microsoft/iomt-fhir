// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public static class IValidatableObjectExtensions
    {
        public static bool IsValid(this ITemplate input, out string error)
        {
            EnsureArg.IsNotNull(input, nameof(input));

            var builder = new StringBuilder();
            var validationErrors = input.Validate(new ValidationContext(input));

            if (!validationErrors.Any())
            {
                error = null;
                return true;
            }

            builder.Append("The template is invalid: ");
            foreach (var validationError in validationErrors)
            {
                builder.AppendLine($"{validationError.ErrorMessage}");
            }

            error = builder.ToString();
            return false;
        }
    }
}
