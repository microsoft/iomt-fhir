// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Template.CalculatedFunction
{
    public interface IExpressionEvaluatorFactory
    {
        /// <summary>
        /// Creates an expression evaluator which will evaluate data based on the supplied expression
        /// </summary>
        /// <param name="expression">The expression to use when performing an evaluation</param>
        /// <param name="defaultLanguage">The default expression language to use, if not supplied within the expression itself</param>
        /// <returns>The expressio evaluator</returns>
        IExpressionEvaluator Create(Expression expression, ExpressionLanguage defaultLanguage);
    }
}