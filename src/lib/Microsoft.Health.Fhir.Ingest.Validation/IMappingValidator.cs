// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.Ingest.Validation.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Validation
{
    public interface IMappingValidator
    {
        /// <summary>
        /// Performs validation of Device and Fhir Mapping templates. The templates will be first be validated individually and then validated for compatibility
        /// with each other. Finally, if a device event is supplied Measurements and Fhir Observations will attempt to be built from it. Any errors/warnings
        /// found will be captured in the resulting ValidationResult object.
        ///
        /// At least one of deviceMappingContent or fhirMappingContent must be provided.
        /// </summary>
        /// <param name="deviceEvent">A sample DeviceEvent. Optional</param>
        /// <param name="deviceMappingContent">A device mapping template. Optional</param>
        /// <param name="fhirMappingContent">A fhir mapping template. Optional</param>
        /// <returns>A ValidationResult object</returns>
        ValidationResult PerformValidation(JToken deviceEvent, string deviceMappingContent, string fhirMappingContent);

        /// <summary>
        /// Performs validation of Device and Fhir Mapping templates. The templates will be first be validated individually and then validated for compatibility
        /// with each other. Finally, if device events are supplied Measurements and Fhir Observations will attempt to be built from them. Any errors/warnings
        /// found will be captured in the resulting ValidationResult object.
        ///
        /// At least one of deviceMappingContent or fhirMappingContent must be provided.
        /// </summary>
        /// <param name="deviceEvents">A collection of DeviceEvents. Optional</param>
        /// <param name="deviceMappingContent">A device mapping template. Optional</param>
        /// <param name="fhirMappingContent">A fhir mapping template. Optional</param>
        /// <param name="aggregateDeviceEvents">Indicates if DeviceResults should be aggregated</param>
        /// <returns>A ValidationResult object</returns>
        ValidationResult PerformValidation(IEnumerable<JToken> deviceEvents, string deviceMappingContent, string fhirMappingContent, bool aggregateDeviceEvents = false);
    }
}