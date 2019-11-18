// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Azure.EventHubs;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Config
{
    public class EventHubMeasurementCollectorOptionsTests
    {
        [Fact]
        public void GivenEventHubConnectionStringAndName_WhenAddSenderFollowedByGet_ThenEventHubClientCreatedAndReturned_Test()
        {
            var options = new EventHubMeasurementCollectorOptions();

            var cs = CreateTestConnectionString();
            options.AddSender("test", cs);

            var client = options.GetEventHubClient("test", cs);
            Assert.NotNull(client);
        }

        [Fact]
        public void GivenEventHubNullConnectionStringAndName_WhenAddSender_ThenInvalidOperationExceptionThrown_Test()
        {
            var options = new EventHubMeasurementCollectorOptions();
            var ex = Assert.Throws<InvalidOperationException>(() => options.GetEventHubClient("test", null));
            Assert.Contains("test", ex.Message);
        }

        [Fact]
        public void GivenNewEventHubConnectionStringAndName_WhenGetEventHubClient_ThenEventHubClientReturned_Test()
        {
            var options = new EventHubMeasurementCollectorOptions();

            var cs = CreateTestConnectionString();
            var client = options.GetEventHubClient("test", cs);
            Assert.NotNull(client);
        }

        [Fact]
        public void GivenRepeatedEventHubConnectionStringAndName_WhenGetEventHubClient_ThenSameEventHubClientReturned_Test()
        {
            var options = new EventHubMeasurementCollectorOptions();

            var cs = CreateTestConnectionString();
            var client1 = options.GetEventHubClient("test", cs);
            Assert.NotNull(client1);

            var client2 = options.GetEventHubClient("test", null);
            Assert.NotNull(client2);

            Assert.Equal(client1, client2);
        }

        [Fact]
        public void GivenRepeatedDifferentEventHubConnectionStringAndName_WhenGetEventHubClient_ThenDifferentEventHubClientReturned_Test()
        {
            var options = new EventHubMeasurementCollectorOptions();

            var cs = CreateTestConnectionString();
            var client1 = options.GetEventHubClient("test1", cs);
            Assert.NotNull(client1);

            var client2 = options.GetEventHubClient("test2", cs);
            Assert.NotNull(client2);

            Assert.NotEqual(client1, client2);
        }

        [Fact]
        public void GivenEventHubConnectionStringWithoutEntityPathAndName_WhenAddSender_ThenNameUsedForEntityPath_Test()
        {
            var options = new EventHubMeasurementCollectorOptions();

            var cs = "Endpoint=sb://test/;SharedAccessKeyName=name;SharedAccessKey=key;";
            var client = options.GetEventHubClient("testA", cs);
            Assert.NotNull(client);

            Assert.Equal("testA", client.EventHubName);
        }

        private static string CreateTestConnectionString()
        {
            return new EventHubsConnectionStringBuilder(endpointAddress: new System.Uri("https://test"), entityPath: "test", sharedAccessKeyName: "name", sharedAccessKey: "key")
                .ToString();
        }
    }
}
