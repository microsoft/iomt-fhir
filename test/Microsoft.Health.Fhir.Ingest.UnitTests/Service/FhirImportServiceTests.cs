// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Tests.Common;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class FhirImportServiceTests
    {
        [Fact]
        public void GivenObservationGroupDeviceIdAndPatientId_WhenGenerateObservationId_CorrectIdReturned_Test()
        {
            var observationGroup = Substitute.For<IObservationGroup>()
                .Mock(m => m.Name.Returns("heartrate"))
                .Mock(m => m.GetIdSegment().ReturnsForAnyArgs("segment"));

            var result = TestFhirImportService.TestGenerateObservationId(observationGroup, "deviceId", "patientId");
            Assert.Equal("patientId.deviceId.heartrate.segment", result.Identifer);
            Assert.Equal(FhirImportService.ServiceSystem, result.System);
        }

        private class TestFhirImportService : FhirImportService
        {
            public TestFhirImportService()
            {
            }

            public static (string Identifer, string System) TestGenerateObservationId(IObservationGroup observationGroup, string deviceId, string patientId)
            {
                return GenerateObservationId(observationGroup, deviceId, patientId);
            }

            public override Task ProcessAsync(ILookupTemplate<IFhirTemplate> config, IMeasurementGroup data, Func<Exception, IMeasurementGroup, Task<bool>> errorConsumer = null)
            {
                throw new NotImplementedException();
            }
        }
    }
}
