// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class TemplateNotFoundException : Exception
    {
        public TemplateNotFoundException(string message)
            : base(message)
        {
        }

        public TemplateNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public TemplateNotFoundException()
        {
        }
    }
}
