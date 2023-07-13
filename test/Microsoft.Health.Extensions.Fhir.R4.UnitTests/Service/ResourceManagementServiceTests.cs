// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using Hl7.Fhir.Model;
using Microsoft.Health.Extensions.Fhir.Service;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Extensions.Fhir.R4.UnitTests.Service
{
    public class ResourceManagementServiceTests
    {
        [Fact]
        public async void GivenResourceFound_WhenEnsureResourceByIdentityAsync_ThenFoundResourceReturned_Test()
        {
            var identiferValue = "123";
            var identifierSystem = "abc";
            var patient = new Patient { Id = "1" };
            var fhirClient = Substitute.For<IFhirService>();
            fhirClient.SearchForResourceAsync(ResourceType.Patient, "identifier=abc|123")
                .Returns(
                    new Bundle
                    {
                        Entry = new List<Bundle.EntryComponent> { new Bundle.EntryComponent { Resource = patient } },
                    });

            var rms = new ResourceManagementService(fhirClient);
            var setterCalled = false;
            Action<Patient, Identifier> setter = (p, i) => { setterCalled = true; };

            var result = await rms.EnsureResourceByIdentityAsync(identiferValue, identifierSystem, setter);

            // Verify get path used
            Assert.False(setterCalled);
            Assert.Equal(expected: patient, actual: result);
            await fhirClient.DidNotReceiveWithAnyArgs().CreateResourceAsync<Patient>(default, default, default, default);
            await fhirClient.DidNotReceiveWithAnyArgs().UpdateResourceAsync<Patient>(default, default, default, default);
            await fhirClient.ReceivedWithAnyArgs().SearchForResourceAsync(default, default, default, default);
            await fhirClient.Received(1).SearchForResourceAsync(ResourceType.Patient, "identifier=abc|123", default, default);
        }

        [Fact]
        public async void GivenResourceNotFound_WhenEnsureResourceByIdentityAsync_ThenResourceCreatedAndReturned_Test()
        {
            var identiferValue = "123";
            var identifierSystem = "abc";
            var newPatient = new Patient { Id = "1" };
            var fhirClient = Substitute.For<IFhirService>();
            fhirClient.SearchForResourceAsync(default, default)
                .ReturnsForAnyArgs(new Bundle());
            fhirClient.CreateResourceAsync(Arg.Any<Patient>()).Returns(newPatient);

            var rms = new ResourceManagementService(fhirClient);
            var setterCalled = false;
            Action<Patient, Identifier> setter = (p, i) =>
            {
                setterCalled = true;
                p.Identifier = new List<Identifier> { new Identifier { Value = identiferValue, System = identifierSystem } };
            };

            var result = await rms.EnsureResourceByIdentityAsync(identiferValue, identifierSystem, setter);

            // Verify create path used
            Assert.True(setterCalled);
            Assert.Equal(expected: newPatient, actual: result);
            await fhirClient.DidNotReceiveWithAnyArgs().UpdateResourceAsync<Patient>(default, default, default, default);
            await fhirClient.Received(1).SearchForResourceAsync(ResourceType.Patient, "identifier=abc|123", default, default);
            await fhirClient.Received(1).CreateResourceAsync(
                Arg.Is<Patient>(p => p.Identifier[0].Value == identiferValue && p.Identifier[0].System == identifierSystem),
                null,
                null,
                Arg.Any<CancellationToken>());
        }
    }
}
