// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using Hl7.Fhir.Model;
using Microsoft.Health.Fhir.Client;

namespace Microsoft.Health.Tests.Utilities
{
    public static class ExceptionUtilities
    {
        public static FhirClientException GetFhirClientException(HttpStatusCode statusCode)
        {
            var message = new HttpResponseMessage(statusCode)
            {
                RequestMessage = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"https://{statusCode}.com"),
                    Content = new StringContent(statusCode.ToString()),
                },
            };

            var response = new FhirResponse<OperationOutcome>(message, new OperationOutcome());

            return new FhirClientException(response, statusCode);
        }
    }
}
