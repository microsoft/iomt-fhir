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
    public class IotConnectorValidator : IIotConnectorValidator
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

            if (contentTemplate != null && fhirTemplate != null)
            {
                CheckForTemplateCompatibility(contentTemplate, fhirTemplate, validationResult);
            }

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

        private void CheckForTemplateCompatibility(IContentTemplate contentTemplate, ILookupTemplate<IFhirTemplate> fhirTemplate, ValidationResult validationResult)
        {
            var deviceTemplates = new List<MeasurementExtractor>();
            var fhirTemplates = new List<CodeValueFhirTemplate>();
            var availableFhirTemplates = string.Empty;

            // TODO: Confirm that outer template factories are always collections for both Device and Fhir Mappings. This implies that
            // customers must always wrap their templates inside of a CollectionXXX Template.

            if (contentTemplate is CollectionContentTemplate collectionContentTemplate)
            {
                deviceTemplates.AddRange(collectionContentTemplate.Templates.Select(t => t as MeasurementExtractor));
            }

            if (fhirTemplate is FhirLookupTemplate fhirLookupTemplate)
            {
                fhirTemplates.AddRange(fhirLookupTemplate.Templates.Select(t => t as CodeValueFhirTemplate));
                availableFhirTemplates = string.Join(",", fhirTemplates.Select(t => t.TypeName));
            }

            foreach (var extractor in deviceTemplates)
            {
                try
                {
                    var innerTemplate = extractor.Template;
                    var matchingFhirTemplate = fhirTemplate.GetTemplate(innerTemplate.TypeName) as CodeValueFhirTemplate;
                    var fhirTemplateValues = new List<FhirValueType>();
                    fhirTemplateValues.Add(matchingFhirTemplate.Value);

                    if (matchingFhirTemplate.Components != null)
                    {
                        foreach (var c in matchingFhirTemplate.Components)
                        {
                            fhirTemplateValues.Add(c.Value);
                        }
                    }

                    var availableFhirValueNames = fhirTemplateValues.Where(v => v != null).Select(v => v.ValueName).ToHashSet();

                    // Ensure all values are present
                    if (extractor.Template.Values != null)
                    {
                        foreach (var v in extractor.Template.Values)
                        {
                            if (!availableFhirValueNames.Contains(v.ValueName))
                            {
                                validationResult.Warnings.Add($"The value [{v.ValueName}] in Device Mapping [{extractor.Template.TypeName}] is not represented within the Fhir Template of type [{innerTemplate.TypeName}]. No value will appear inside of Observations.");
                            }
                        }
                    }
                }
                catch (TemplateNotFoundException)
                {
                    validationResult.Warnings.Add($"No matching Fhir Template exists for Device Mapping [{extractor.Template.TypeName}]. Ensure case matches. Available Fhir Templates: [{availableFhirTemplates}] ");
                }
                catch (Exception e)
                {
                    CaptureException(validationResult, e);
                }
            }
        }

        private void ProcessDeviceEvent(JToken deviceEvent, IContentTemplate contentTemplate, ValidationResult validationResult)
        {
            try
            {
                foreach (var m in contentTemplate.GetMeasurements(deviceEvent))
                {
                    validationResult.Measurements.Add(m);
                }

                if (validationResult.Measurements.Count == 0)
                {
                    validationResult.Warnings.Add("No measurements were produced for the given device data");
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
            catch (TemplateNotFoundException e)
            {
                validationResult.Exceptions.Add($"No Fhir Template exists with the type name [{e.Message}]. Ensure that all Fhir Template type names match Device Mapping type names (including casing)");
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
