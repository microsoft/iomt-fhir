// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Template.Generator
{
    public enum TemplateType
    {
        /// <summary>
        /// A template which supports writing expressions using one of several expressions languages.
        /// Supports data transformation via the use of JmesPath functions
        /// </summary>
        CalculatedContent,

        /// <summary>
        /// A template which supports writing expressions using JsonPath
        /// </summary>
        JsonPathContent,

        /// <summary>
        /// Supports messages sent via Azure Iot Hub or via the Legacy Export Data feature of Azure Iot Central.
        /// </summary>
        IotCentralJsonPathContent,

        /// <summary>
        /// Supports messages sent via the Export Data feature of Azure Iot Central.
        /// </summary>
        IotJsonPathContent,

        /// <summary>
        /// The CodeValueFhirTemplate is currently the only template supported in FHIR mapping at this time.
        /// It allows you defined codes, the effective period, and value of the observation.
        /// Multiple value types are supported: SampledData, CodeableConcept, String, and Quantity.
        /// In addition to these configurable values the identifier for the observation,
        /// along with linking to the proper device and patient are handled automatically.
        /// An additional code used by IoMT FHIR Connector for Azure is also added.
        /// </summary>
        CodeValueFhir,
    }
}
