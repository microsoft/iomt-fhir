// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Hl7.Fhir.Model;
using Microsoft.Health.Extensions.Fhir.Search;
using Xunit;

namespace Microsoft.Health.Extensions.Fhir.R4.UnitTests
{
    public class SearchExtensionsTests
    {
        [Fact]
        public void GivenIdentifierIsNull_WhenToExpandedSearchQueryParameters_Throws()
        {
            Identifier identifier = null;
            Assert.Throws<ArgumentNullException>(() => identifier.ToExpandedSearchQueryParameters());
        }

        [Fact]
        public void GivenIdentifierValueIsNull_WhenToExpandedSearchQueryParameters_Throws()
        {
            Identifier identifier = new Identifier();
            Assert.Throws<ArgumentNullException>(() => identifier.ToExpandedSearchQueryParameters());
        }

        [Fact]
        public void GivenIdentifierValueDoesNotContainTimePeriod_WhenToExpandedSearchQueryParameters_DefaultSearchStringReturned()
        {
            Identifier identifier = new Identifier
            {
                Value = "patient.device.typeName.correlationId",
            };

            string parameters = identifier.ToExpandedSearchQueryParameters();

            Assert.Equal("identifier=patient.device.typeName.correlationId", parameters);
        }

        [Fact]
        public void GivenIdentifierValueTimePeriodsDoNotHaveExpectedFormat_WhenToExpandedSearchQueryParameters_DefaultSearchStringReturned()
        {
            Identifier identifier = new Identifier
            {
                Value = "patient.device.typeName.2022090912131415Z.2022090912131415Z",
            };

            string parameters = identifier.ToExpandedSearchQueryParameters();

            Assert.Equal("identifier=patient.device.typeName.2022090912131415Z.2022090912131415Z", parameters);
        }

        [Fact]
        public void GivenIdentifierValueTimePeriodsHaveExpectedFormat_WhenToExpandedSearchQueryParameters_ExpandedSearchStringReturned()
        {
            Identifier identifier = new Identifier
            {
                Value = "patient.device.typeName.20220909121314Z.20220909121314Z",
            };

            string parameters = identifier.ToExpandedSearchQueryParameters();

            Assert.Equal("subject=patient&device=device&code=typeName&date=ge2022-09-09T12:13:14Z&date=le2022-09-09T12:13:14Z", parameters);
        }

        [Fact]
        public void GivenIdentifierValueTypeNameHasMultipleComponents_WhenToExpandedSearchQueryParameters_ExpandedSearchStringReturned()
        {
            Identifier identifier = new Identifier
            {
                Value = "patient.device.type.name.components.20220909121314Z.20220909121314Z",
            };

            string parameters = identifier.ToExpandedSearchQueryParameters();

            Assert.Equal("subject=patient&device=device&code=type.name.components&date=ge2022-09-09T12:13:14Z&date=le2022-09-09T12:13:14Z", parameters);
        }
    }
}
