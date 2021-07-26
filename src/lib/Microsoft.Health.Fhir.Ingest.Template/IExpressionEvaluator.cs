// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public interface IExpressionEvaluator
    {
        /// <summary>
        /// Evaluates the supplied data against the expression represented by this evaluator and retrieves a single JToken. If the expression
        /// would return multiple tokens, an exception will be thrown.
        /// </summary>
        /// <param name="data">The JToken to evaluate the expression against</param>
        /// <returns>A single JToken which represents the result of expression evaluation, or <c>null</c> if nothing could be found</c></returns>
        JToken SelectToken(JToken data);

        /// <summary>
        /// Evaluates the supplied data against the expression represented by this evaluator and retrieves all matching JTokens.
        /// </summary>
        /// <param name="data">The JToken to evaluate the expression against</param>
        /// <returns>A (possibly empty) enumeration of matching JTokens</returns>
        IEnumerable<JToken> SelectTokens(JToken data);
    }
}