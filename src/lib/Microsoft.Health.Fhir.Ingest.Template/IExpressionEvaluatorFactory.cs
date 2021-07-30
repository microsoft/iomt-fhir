// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public interface IExpressionEvaluatorFactory
    {
        /// <summary>
        /// Creates an expression evaluator which will evaluate data based on the supplied expression
        /// </summary>
        /// <param name="expression">The expression to use when performing an evaluation</param>
        /// <returns>The expression evaluator</returns>
        IExpressionEvaluator Create(TemplateExpression expression);
    }
}