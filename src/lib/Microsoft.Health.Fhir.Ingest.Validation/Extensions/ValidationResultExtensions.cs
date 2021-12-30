// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Linq;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Validation.Models;

namespace Microsoft.Health.Fhir.Ingest.Validation.Extensions
{
    public static class ValidationResultExtensions
    {
        /// <summary>
        /// Determines if an exception of a specific level is held within the ValidationResult.
        /// </summary>
        /// <param name="validationResult">The ValidationResult</param>
        /// <param name="errorLevel">The ErrorLevel to look for</param>
        /// <returns>true if any exception within the ValidationResult is at the specified error level; otherwise false</returns>
        public static bool AnyException(this ValidationResult validationResult, ErrorLevel errorLevel)
        {
            EnsureArg.IsNotNull(validationResult, nameof(validationResult));
            return validationResult.TemplateResult.Exceptions.Any(error => errorLevel.Equals(error.Level)) ||
                validationResult.DeviceResults.Any(deviceResult => deviceResult.Exceptions.Any(error => errorLevel.Equals(errorLevel)));
        }
    }
}
