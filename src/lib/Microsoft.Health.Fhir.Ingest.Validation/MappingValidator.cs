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
using Microsoft.Health.Fhir.Ingest.Validation.Extensions;
using Microsoft.Health.Fhir.Ingest.Validation.Models;
using Newtonsoft.Json.Linq;
using Model = Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Validation
{
    public class MappingValidator : IMappingValidator
    {
        private readonly IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation> _fhirTemplateProcessor;

        private readonly CollectionTemplateFactory<IContentTemplate, IContentTemplate> _collectionTemplateFactory;
        private readonly ITemplateFactory<string, ITemplateContext<ILookupTemplate<IFhirTemplate>>> _fhirTemplateFactory;

        public MappingValidator(
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
            return PerformValidation(new List<JToken>() { deviceEvent }, deviceMappingContent, fhirMappingContent);
        }

        public ValidationResult PerformValidation(
            IEnumerable<JToken> deviceEvents,
            string deviceMappingContent,
            string fhirMappingContent)
        {
            if (string.IsNullOrWhiteSpace(deviceMappingContent) && string.IsNullOrWhiteSpace(fhirMappingContent))
            {
                throw new ArgumentException($"At least one of [{nameof(deviceMappingContent)}] or [{nameof(fhirMappingContent)}] must be provided");
            }

            var validationResult = new ValidationResult();

            IContentTemplate contentTemplate = null;
            ILookupTemplate<IFhirTemplate> fhirTemplate = null;

            if (!string.IsNullOrEmpty(deviceMappingContent))
            {
                contentTemplate = LoadDeviceTemplate(deviceMappingContent, validationResult.TemplateResult);
            }

            if (!string.IsNullOrEmpty(fhirMappingContent))
            {
                fhirTemplate = LoadFhirTemplate(fhirMappingContent, validationResult.TemplateResult);
            }

            if (contentTemplate != null && fhirTemplate != null)
            {
                CheckForTemplateCompatibility(contentTemplate, fhirTemplate, validationResult.TemplateResult);
            }

            if (validationResult.TemplateResult.GetErrors(ErrorLevel.ERROR).Count() > 0)
            {
                // Fail early since there are errors with the template.
                return validationResult;
            }

            foreach (var payload in deviceEvents)
            {
                if (payload != null && contentTemplate != null)
                {
                    var deviceResult = new DeviceResult();
                    deviceResult.DeviceEvent = payload;
                    validationResult.DeviceResults.Add(deviceResult);

                    ProcessDeviceEvent(payload, contentTemplate, deviceResult);

                    if (fhirTemplate != null)
                    {
                        foreach (var m in deviceResult.Measurements)
                        {
                            ProcessNormalizedeEvent(m, fhirTemplate, deviceResult);
                        }
                    }
                }
            }

            return validationResult;
        }

        private IContentTemplate LoadDeviceTemplate(string deviceMappingContent, TemplateResult validationResult)
        {
            try
            {
                var templateContext = _collectionTemplateFactory.Create(deviceMappingContent);
                templateContext.EnsureValid();
                return templateContext.Template;
            }
            catch (Exception e)
            {
                validationResult.CaptureException(e);
            }

            return null;
        }

        private ILookupTemplate<IFhirTemplate> LoadFhirTemplate(string fhirMappingContent, TemplateResult validationResult)
        {
            try
            {
                var fhirTemplateContext = _fhirTemplateFactory.Create(fhirMappingContent);
                fhirTemplateContext.EnsureValid();
                return fhirTemplateContext.Template;
            }
            catch (Exception e)
            {
                validationResult.CaptureException(e);
            }

            return null;
        }

        private void CheckForTemplateCompatibility(IContentTemplate contentTemplate, ILookupTemplate<IFhirTemplate> fhirTemplate, TemplateResult validationResult)
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
                availableFhirTemplates = string.Join(" ,", fhirTemplates.Select(t => t.TypeName));
            }

            foreach (var extractor in deviceTemplates)
            {
                try
                {
                    var innerTemplate = extractor.Template;
                    var matchingFhirTemplate = fhirTemplate.GetTemplate(innerTemplate.TypeName) as CodeValueFhirTemplate;
                    var availableFhirValueNames = new HashSet<string>(GetFhirValues(matchingFhirTemplate).Select(v => v.ValueName));
                    var availableFhirValueNamesDisplay = string.Join(" ,", availableFhirValueNames);

                    // Ensure all values are present
                    if (extractor.Template.Values != null)
                    {
                        foreach (var v in extractor.Template.Values)
                        {
                            if (!availableFhirValueNames.Contains(v.ValueName))
                            {
                                validationResult.CaptureWarning($"The value [{v.ValueName}] in Device Mapping [{extractor.Template.TypeName}] is not represented within the Fhir Template of type [{innerTemplate.TypeName}]. Available values are: [{availableFhirValueNamesDisplay}]. No value will appear inside of Observations.");
                            }
                        }
                    }
                }
                catch (TemplateNotFoundException)
                {
                    validationResult.CaptureWarning($"No matching Fhir Template exists for Device Mapping [{extractor.Template.TypeName}]. Ensure case matches. Available Fhir Templates: [{availableFhirTemplates}].");
                }
                catch (Exception e)
                {
                    validationResult.CaptureException(e);
                }
            }
        }

        private void ProcessDeviceEvent(JToken deviceEvent, IContentTemplate contentTemplate, DeviceResult validationResult)
        {
            try
            {
                foreach (var m in contentTemplate.GetMeasurements(deviceEvent))
                {
                    validationResult.Measurements.Add(m);
                }

                if (validationResult.Measurements.Count == 0)
                {
                    validationResult.CaptureWarning("No measurements were produced for the given device data.");
                }
            }
            catch (Exception e)
            {
                validationResult.CaptureException(e);
            }
        }

        private void ProcessNormalizedeEvent(Measurement normalizedEvent, ILookupTemplate<IFhirTemplate> fhirTemplate, DeviceResult validationResult)
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
                validationResult.CaptureError(
                    $"No Fhir Template exists with the type name [{e.Message}]. Ensure that all Fhir Template type names match Device Mapping type names (including casing)",
                    ErrorLevel.ERROR);
            }
            catch (Exception e)
            {
                validationResult.CaptureException(e);
            }
        }

        private static ISet<FhirValueType> GetFhirValues(CodeValueFhirTemplate codeValueFhirTemplate)
        {
            var fhirTemplateValues = new HashSet<FhirValueType>();

            if (codeValueFhirTemplate.Components != null)
            {
                foreach (var c in codeValueFhirTemplate.Components)
                {
                    fhirTemplateValues.Add(c.Value);
                }
            }
            else
            {
                fhirTemplateValues.Add(codeValueFhirTemplate.Value);
            }

            return fhirTemplateValues;
        }
    }
}
