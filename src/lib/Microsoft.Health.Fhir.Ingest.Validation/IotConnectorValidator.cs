// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Template;
using Newtonsoft.Json.Linq;
using Model = Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Validation
{
    public class IotConnectorValidator
    {
        // R4FhirLookupTemplateProcessor
        private readonly IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation> _fhirTemplateProcessor;

        private readonly CollectionTemplateFactory<IContentTemplate, IContentTemplate> _collectionTemplateFactory;
        private readonly ITemplateFactory<string, ITemplateContext<ILookupTemplate<IFhirTemplate>>> _fhirTemplateFactory;

        public IotConnectorValidator(
            CollectionTemplateFactory<IContentTemplate, IContentTemplate> collectionTemplateFactory,
            ITemplateFactory<string, ITemplateContext<ILookupTemplate<IFhirTemplate>>> fhirTemplateFactory,
            IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation> fhirTemplateProcessor)
        {
            _collectionTemplateFactory = EnsureArg.IsNotNull(collectionTemplateFactory, nameof(collectionTemplateFactory));
            _fhirTemplateFactory = EnsureArg.IsNotNull(fhirTemplateFactory, nameof(fhirTemplateFactory));
            _fhirTemplateProcessor = EnsureArg.IsNotNull(fhirTemplateProcessor, nameof(fhirTemplateProcessor));
        }

        public ValidationResult PerformValidation(
            JToken deviceEvent,
            string deviceMappingContent,
            string fhirMappingContent)
        {
            var validationResult = new ValidationResult()
            {
                DeviceEvent = deviceEvent,
            };

            IContentTemplate contentTemplate = null;
            ILookupTemplate<IFhirTemplate> fhirTemplate = null;

            if (!string.IsNullOrEmpty(deviceMappingContent))
            {
                contentTemplate = LoadDeviceTemplate(deviceMappingContent, validationResult);
            }

            if (!string.IsNullOrEmpty(fhirMappingContent))
            {
                fhirTemplate = LoadFhirTemplate(fhirMappingContent, validationResult);
            }

            CheckForTemplateCompatibility();

            if (validationResult.Exceptions.Count > 0)
            {
                // Fail early since there errors with the template.
                return validationResult;
            }

            if (deviceEvent != null && contentTemplate != null)
            {
                ProcessDeviceEvent(deviceEvent, contentTemplate, validationResult);

                if (fhirTemplate != null)
                {
                    foreach (var m in validationResult.Measurements)
                    {
                        ProcessNormalizedeEvent(m, fhirTemplate, validationResult);
                    }
                }
            }

            return validationResult;
        }

        private IContentTemplate LoadDeviceTemplate(string deviceMappingContent, ValidationResult validationResult)
        {
            try
            {
                var templateContext = _collectionTemplateFactory.Create(deviceMappingContent);
                templateContext.EnsureValid();
                return templateContext.Template;
            }
            catch (Exception e)
            {
                CaptureException(validationResult, e);
            }

            return null;
        }

        private ILookupTemplate<IFhirTemplate> LoadFhirTemplate(string fhirMappingContent, ValidationResult validationResult)
        {
            try
            {
                var fhirTemplateContext = _fhirTemplateFactory.Create(fhirMappingContent);
                fhirTemplateContext.EnsureValid();
                return fhirTemplateContext.Template;
            }
            catch (Exception e)
            {
                CaptureException(validationResult, e);
            }

            return null;
        }

        private void CheckForTemplateCompatibility()
        {
            // Check if device and fhir templates are compatible with each other
        }

        private void ProcessDeviceEvent(JToken deviceEvent, IContentTemplate contentTemplate, ValidationResult validationResult)
        {
            try
            {
                foreach (var m in contentTemplate.GetMeasurements(deviceEvent))
                {
                    validationResult.Measurements.Add(m);
                }
            }
            catch (Exception e)
            {
                CaptureException(validationResult, e);
            }
        }

        private void ProcessNormalizedeEvent(Measurement normalizedEvent, ILookupTemplate<IFhirTemplate> fhirTemplate, ValidationResult validationResult)
        {
            var measurementGroup = new MeasurementGroup
            {
                MeasureType = normalizedEvent.Type,
                CorrelationId = normalizedEvent.CorrelationId,
                DeviceId = normalizedEvent.DeviceId,
                EncounterId = normalizedEvent.EncounterId,
                PatientId = normalizedEvent.PatientId,
                Data = new List<Measurement>() { normalizedEvent },
            };

            try
            {
                // Convert Measurement to Observation Group
                var observationGroup = _fhirTemplateProcessor.CreateObservationGroups(fhirTemplate, measurementGroup).First();

                // Build HL7 Observation
                validationResult.Observations.Add(_fhirTemplateProcessor.CreateObservation(fhirTemplate, observationGroup));
            }
            catch (Exception e)
            {
                CaptureException(validationResult, e);
            }
        }

        private static void CaptureException(ValidationResult validationResult, Exception exception)
        {
            EnsureArg.IsNotNull(validationResult, nameof(validationResult));
            EnsureArg.IsNotNull(exception, nameof(exception));

            validationResult.Exceptions.Add(exception.Message);
        }
    }
}
