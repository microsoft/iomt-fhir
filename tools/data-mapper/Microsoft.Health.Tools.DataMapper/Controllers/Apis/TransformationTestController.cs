// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Tools.DataMapper.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Tools.DataMapper.Controllers.Apis
{
    /// <summary>
    /// Test FHIR transformation.
    /// </summary>
    [Route("api/test-transformation")]
    [ApiController]
    public class TransformationTestController : ControllerBase
    {
        private const string PatientIdPlaceholder = "<Patient-Reference-On-FHIR-Server>";
        private readonly ILogger<TransformationTestController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformationTestController"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        public TransformationTestController(ILogger<TransformationTestController> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Receive Post request.
        /// </summary>
        /// <param name="request">Test request payload.</param>
        /// <returns>Test response.</returns>
        [HttpPost]
        public IActionResult Post([FromBody] TransformationTestRequest request)
        {
            // Validate Request.
            if (!IsValidRequest(request, out string errorMessage))
            {
                return this.BadRequest(
                    new TransformationTestResponse()
                    {
                        Result = TestResult.Fail.ToString(),
                        Reason = errorMessage,
                    });
            }

            // Validate mapping semantic.
            var templateContext = CollectionFhirTemplateFactory.Default.Create(request.FhirMapping);
            if (!templateContext.IsValid(out string errors))
            {
                return this.BadRequest(
                    new TransformationTestResponse()
                    {
                        Result = TestResult.Fail.ToString(),
                        Reason = errors,
                    });
            }

            FhirVersion fhirVersion = (FhirVersion)Enum.Parse(typeof(FhirVersion), request.FhirVersion, true);
            FhirIdentityResolutionType fhirIdentityResolutionType = (FhirIdentityResolutionType)Enum.Parse(typeof(FhirIdentityResolutionType), request.FhirIdentityResolutionType, true);
            IMeasurementGroup measurementGroup;
            try
            {
                measurementGroup = JToken.Parse(request.NormalizedData).ToObject<MeasurementGroup>();

                if (string.IsNullOrWhiteSpace(measurementGroup.DeviceId))
                {
                    return this.BadRequest(
                        new TransformationTestResponse()
                        {
                            Result = TestResult.Fail.ToString(),
                            Reason = "Device Id can't be null or empty.",
                        });
                }

                if (fhirIdentityResolutionType == FhirIdentityResolutionType.Create
                    && string.IsNullOrWhiteSpace(measurementGroup.PatientId))
                {
                    return this.BadRequest(
                        new TransformationTestResponse()
                        {
                            Result = TestResult.Fail.ToString(),
                            Reason = "Patient Id can't be null with identity resolution type as \"Create\".",
                        });
                }
            }
            catch (Exception ex)
            {
                return this.BadRequest(
                    new TransformationTestResponse()
                    {
                        Result = TestResult.Fail.ToString(),
                        Reason = ex.Message,
                    });
            }

            // Start processing.
            try
            {
                FhirLookupTemplateProcessor<Observation> processor = GetFhirTemplateProcessor(fhirVersion);
                var grps = processor.CreateObservationGroups(templateContext.Template, measurementGroup);

                Observation observation = null;
                string deviceId = measurementGroup.DeviceId;
                string patientId =
                    fhirIdentityResolutionType == FhirIdentityResolutionType.Create ? measurementGroup.PatientId : PatientIdPlaceholder;
                foreach (var grp in grps)
                {
                    if (observation == null)
                    {
                        observation = processor.CreateObservation(templateContext.Template, grp);
                        observation.Subject = patientId.ToReference<Patient>();
                        observation.Device = deviceId.ToReference<Device>();

                        var identity = GenerateObservationId(grp, deviceId, patientId);
                        var observerationId = new Identifier
                        {
                            System = identity.System,
                            Value = identity.Identifer,
                        };

                        observation.Identifier = new List<Identifier> { observerationId };
                    }
                    else
                    {
                        observation = processor.MergeObservation(templateContext.Template, grp, observation);
                    }
                }

                return this.Ok(
                    new TransformationTestResponse()
                    {
                        Result = TestResult.Success.ToString(),
                        FhirData = observation.ToJson(new FhirJsonSerializationSettings()
                        {
                            Pretty = true,
                        }),
                    });
            }
            catch (Exception ex)
            {
                return this.BadRequest(
                    new TransformationTestResponse()
                    {
                        Result = TestResult.Fail.ToString(),
                        Reason = ex.Message,
                    });
            }
        }

        private static FhirLookupTemplateProcessor<Observation> GetFhirTemplateProcessor(FhirVersion fhirVersion)
        {
            switch (fhirVersion)
            {
                case FhirVersion.R4:
                    return new R4FhirLookupTemplateProcessor();
                default:
                    throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }

        private static (string Identifer, string System) GenerateObservationId(IObservationGroup observationGroup, string deviceId, string patientId)
        {
            var value = $"{patientId}.{deviceId}.{observationGroup.Name}.{observationGroup.GetIdSegment()}";
            return (value, FhirImportService.ServiceSystem);
        }

        private static bool IsValidRequest(TransformationTestRequest request, out string message)
        {
            if (request == null)
            {
                message = "Request body can not be null";
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.FhirMapping))
            {
                message = "FHIR Mapping can not be null or empty.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.NormalizedData))
            {
                message = "Normalized data can not be null or empty.";
                return false;
            }

            if (!Utility.IsValidJson(request.FhirMapping, out message) ||
                !Utility.IsValidJson(request.NormalizedData, out message))
            {
                return false;
            }

            try
            {
                Enum.Parse(typeof(FhirVersion), request.FhirVersion, true);
            }
            catch (Exception)
            {
                message = $"The provided FHIR version \"{request.FhirVersion}\" is not valid or supported.";
                return false;
            }

            try
            {
                Enum.Parse(typeof(FhirIdentityResolutionType), request.FhirIdentityResolutionType, true);
            }
            catch (Exception)
            {
                message = $"The provided Identity Resolution Type \"{request.FhirIdentityResolutionType}\" is not valid or supported.";
                return false;
            }

            message = null;
            return true;
        }
    }
}
