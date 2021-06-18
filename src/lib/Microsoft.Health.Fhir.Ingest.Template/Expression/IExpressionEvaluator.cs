// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template.Expression
{
    public interface IExpressionEvaluator
    {
        ///
        /// Evaluates the supplied data against the expression represented by this evaluator
        ///
        JToken Evaluate(JToken data);
    }
}