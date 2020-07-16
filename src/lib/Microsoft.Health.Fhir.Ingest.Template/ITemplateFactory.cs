// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public interface ITemplateFactory<TInput, TTOutput>
    {
        TTOutput Create(TInput input);

        TTOutput Create(TInput input, out IList<string> errors);
    }
}
