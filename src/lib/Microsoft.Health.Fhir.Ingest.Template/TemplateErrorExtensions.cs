// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public static class TemplateErrorExtensions
    {
        public static IEnumerable<TemplateError> ConvertExceptionToTemplateErrors(this AggregateException exception)
        {
            EnsureArg.IsNotNull(exception, nameof(exception));

            yield return new TemplateError(exception.Message);

            foreach (var innerException in exception.InnerExceptions)
            {
                if (innerException is InvalidTemplateException ite)
                {
                    yield return new TemplateError(ite.Message, ite.GetLineInfo);
                }
                else
                {
                    yield return new TemplateError(innerException.Message);
                }
            }
        }
    }
}