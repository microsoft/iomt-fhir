// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Common.Telemetry;

namespace Microsoft.Health.Extensions.Fhir.Telemetry.Metrics
{
    public class FhirClientMetrics
    {
        private static string _nameDimension = DimensionNames.Name;
        private static string _categoryDimension = DimensionNames.Category;
        private static string _errorTypeDimension = DimensionNames.ErrorType;
        private static string _errorSeverityDimension = DimensionNames.ErrorSeverity;
        private static string _operationDimension = DimensionNames.Operation;

        /// <summary>
        /// A metric recorded when there is an error reading from or connecting with a FHIR server.
        /// </summary>
        /// <param name="exceptionName">The name of the exception</param>
        /// <param name="severity">The severity of the error</param>
        /// <param name="connectorStage">The stage of the connector</param>
        public static Metric HandledException(string exceptionName, string severity, string connectorStage)
        {
            return new Metric(
                exceptionName,
                new Dictionary<string, object>
                {
                    { _nameDimension, exceptionName },
                    { _categoryDimension, Category.Errors },
                    { _errorTypeDimension, ErrorType.GeneralError },
                    { _errorSeverityDimension, severity },
                    { _operationDimension, connectorStage },
                });
        }
    }
}
