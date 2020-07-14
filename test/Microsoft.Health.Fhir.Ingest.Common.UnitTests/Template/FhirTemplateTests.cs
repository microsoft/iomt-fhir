// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class FhirTemplateTests
    {
        [Fact]
        public void GivenDefaultFhirTemplate_WhenGetPeriodInterval_ThenValueIsSingle_Test()
        {
            var template = new TestFhirTemplate();

            Assert.Equal(ObservationPeriodInterval.Single, template.PeriodInterval);
        }

        private class TestFhirTemplate : FhirTemplate
        {
        }
    }
}
