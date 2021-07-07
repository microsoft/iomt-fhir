// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Xunit;
using Model = Hl7.Fhir.Model;

namespace Microsoft.Health.Extensions.Fhir.R4.UnitTests
{
    public class ModelExtensionsTests
    {
        [Fact]
        public void GivenDeviceWithValidPatientReference_GetId_ThenReferenceIdentifierIsReturned_Test()
        {
            var device = new Model.Device
            {
                Id = "1",
                Patient = new Model.ResourceReference("Patient/123"),
            };

            var patientIdentifier = device.Patient?.GetId<Model.Patient>();
            Assert.Equal("123", patientIdentifier);
        }

        [Fact]
        public void GivenDeviceWithInvalidPatientReference_GetId_ThenNullIsReturned_Test()
        {
            var device = new Model.Device
            {
                Id = "1",
                Patient = new Model.ResourceReference("Not a reference in the form of: ResourceName/Identifier"),
            };

            var patientReference = device.Patient?.GetId<Model.Patient>();
            Assert.Null(patientReference);
        }

        [Fact]
        public void GivenDeviceWithPatientReferenceQueryParam_GetId_ThenNullIsReturned_Test()
        {
            var device = new Model.Device
            {
                Id = "1",
                Patient = new Model.ResourceReference("Patient/123?_id=123"),
            };

            var patientReference = device.Patient?.GetId<Model.Patient>();
            Assert.Null(patientReference);
        }
    }
}
