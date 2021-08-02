// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Tools.DataMapper.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Tools.DataMapper.Controllers.Apis
{
    /// <summary>
    /// Test normalization.
    /// </summary>
    [Route("api/test-normalization")]
    [ApiController]
    public class NormalizationTestController : ControllerBase
    {
        private readonly ILogger<NormalizationTestController> logger;
        private readonly CollectionTemplateFactory<IContentTemplate, IContentTemplate> collectionTemplateFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizationTestController"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        public NormalizationTestController(
            ILogger<NormalizationTestController> logger,
            CollectionTemplateFactory<IContentTemplate, IContentTemplate> templateFactory)
        {
            this.logger = EnsureArg.IsNotNull(logger, nameof(logger));
            this.collectionTemplateFactory = EnsureArg.IsNotNull(templateFactory, nameof(templateFactory));
        }

        /// <summary>
        /// Receive Post request.
        /// </summary>
        /// <param name="request">Test request payload.</param>
        /// <returns>Test response.</returns>
        [HttpPost]
        public IActionResult Post([FromBody] NormalizationTestRequest request)
        {
            // Validate Request.
            if (!IsValidRequest(request, out string errorMessage))
            {
                return this.BadRequest(
                    new NormalizationTestResponse()
                    {
                        Result = TestResult.Fail.ToString(),
                        Reason = errorMessage,
                    });
            }

            // Validate mapping semantic.
            var templateContext = collectionTemplateFactory.Create(request.DeviceMapping);
            if (!templateContext.IsValid(out string errors))
            {
                return this.BadRequest(
                    new NormalizationTestResponse()
                    {
                        Result = TestResult.Fail.ToString(),
                        Reason = errors,
                    });
            }

            // try normalizing and return result.
            try
            {
                var token = JToken.Parse(request.DeviceSample);
                var measurements = templateContext.Template.GetMeasurements(token);

                return this.Ok(
                    new NormalizationTestResponse()
                    {
                        Result = TestResult.Success.ToString(),
                        NormalizedData = JToken.FromObject(measurements).ToString(),
                    });
            }
            catch (Exception ex)
            {
                return this.BadRequest(
                    new NormalizationTestResponse()
                    {
                        Result = TestResult.Fail.ToString(),
                        Reason = ex.Message,
                    });
            }
        }

        private static bool IsValidRequest(NormalizationTestRequest request, out string message)
        {
            if (request == null)
            {
                message = "Request body can not be null";
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.DeviceMapping))
            {
                message = "Device Mapping can not be null";
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.DeviceSample))
            {
                message = "Device test data can not be null";
                return false;
            }

            if (!Utility.IsValidJson(request.DeviceMapping, out message) ||
                !Utility.IsValidJson(request.DeviceSample, out message))
            {
                return false;
            }

            message = null;
            return true;
        }
    }
}
