// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Text;
using Microsoft.Azure.EventHubs;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Tests.Common;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class MeasurementToEventAsyncCollectorTests
    {
        [Fact]
        public async void GivenMeasurementWithDeviceId_WhenAddAsync_ThenEventSentWithDeviceIdAsPartitionKey_Test()
        {
            var eh = Substitute.For<IEventHubService>();
            var measurement = Substitute.For<IMeasurement>().Mock(m => m.DeviceId.Returns("1"));

            var collector = new MeasurementToEventAsyncCollector(eh);

            await collector.AddAsync(measurement).ConfigureAwait(false);

            await eh.Received(1)
                .SendAsync(Arg.Is<EventData>(evt => JsonConvert.DeserializeObject<Measurement>(Encoding.UTF8.GetString(evt.Body.Array, evt.Body.Offset, evt.Body.Count)).DeviceId == measurement.DeviceId), "1");
        }

        [Fact]
        public async void GivenMeasurementWithOutDeviceId_WhenAddAsync_ThenArgumentExceptionThrown_Test()
        {
            var eh = Substitute.For<IEventHubService>();
            var measurement = Substitute.For<IMeasurement>();

            var collector = new MeasurementToEventAsyncCollector(eh);

            var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await collector.AddAsync(measurement).ConfigureAwait(false));
            Assert.Contains("DeviceId", ex.Message);
        }

        [Fact]
        public async void GivenCollector_WhenFlushAsync_OperationSuccess_Test()
        {
            var eh = Substitute.For<IEventHubService>();

            var collector = new MeasurementToEventAsyncCollector(eh);
            await collector.FlushAsync();
        }
    }
}
