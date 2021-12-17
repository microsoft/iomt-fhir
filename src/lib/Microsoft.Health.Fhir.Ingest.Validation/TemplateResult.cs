// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Ingest.Validation
{
    public class TemplateResult : IResult
    {
        public IList<string> Exceptions { get; set; } = new List<string>();

        public IList<string> Warnings { get; set; } = new List<string>();
    }
}
