// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Config
{
    public class MeasurementFhirImportOptionsTests
    {
        [Fact]
        public void GivenMeasurementFhirImportOptions_WhenCreated_ThenDefaultValuesPopulated_Test()
        {
            var options = new MeasurementFhirImportOptions();

            Assert.NotNull(options.ParallelTaskOptions);
            Assert.NotNull(options.TemplateFactory);
            Assert.NotNull(options.ExceptionService);
        }
    }
}
