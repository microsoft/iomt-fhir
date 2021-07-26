// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public enum TemplateExpressionLanguage
    {
        /// <summary>
        /// Indicates that the expression language is JsonPath
        /// </summary>
        JsonPath,

        /// <summary>
        /// Indicates that the expression language is JmesPath
        /// </summary>
        JmesPath,
    }
}