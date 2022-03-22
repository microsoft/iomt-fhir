// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Validation.Models;

namespace Microsoft.Health.Fhir.Ingest.Validation.Extensions
{
    public static class IResultExtensions
    {
        public static void CaptureError(this IResult validationResult, string message, ErrorLevel errorLevel, ValidationCategory category)
        {
            EnsureArg.IsNotNull(validationResult, nameof(validationResult));
            EnsureArg.IsNotNullOrWhiteSpace(message, nameof(message));

            validationResult.Exceptions.Add(new ValidationError(message, category, errorLevel));
        }

        public static void CaptureException(this IResult validationResult, Exception exception, ValidationCategory category)
        {
            EnsureArg.IsNotNull(validationResult, nameof(validationResult));
            EnsureArg.IsNotNull(exception, nameof(exception));

            validationResult.Exceptions.Add(new ValidationError(exception.Message, category));
        }

        public static void CaptureWarning(this IResult validationResult, string warning, ValidationCategory category)
        {
            EnsureArg.IsNotNull(validationResult, nameof(validationResult));
            EnsureArg.IsNotNullOrWhiteSpace(warning, nameof(warning));

            validationResult.Exceptions.Add(new ValidationError(warning, category, ErrorLevel.WARN));
        }

        public static IEnumerable<ValidationError> GetErrors(this IResult validationResult, ErrorLevel errorLevel)
        {
            EnsureArg.IsNotNull(validationResult, nameof(validationResult));
            return validationResult.Exceptions.Where(error => errorLevel.Equals(error.Level));
        }
    }
}
