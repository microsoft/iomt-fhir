using Azure.Messaging.EventHubs;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Events.Telemetry;
using Microsoft.Health.Events.Telemetry.Exceptions;
using Microsoft.Health.Logging.Telemetry;
using NSubstitute;
using System;
using System.Net.Sockets;
using Xunit;

namespace Microsoft.Health.Events.UnitTest
{
    public class EventHubExceptionTelemetryProcessorTests
    {
        [Theory]
        [InlineData(typeof(EventHubsException), new object [] { false, "test" }, "EventHubErrorOperationError")]
        [InlineData(typeof(SocketException), null, "EventHubErrorSocketError")]
        [InlineData(typeof(UnauthorizedAccessException), null, "EventHubErrorAuthorizationError")]
        [InlineData(typeof(Exception), null, "EventHubErrorGeneralError")]
        public void GivenExceptionTypes_WhenProcessExpection_ThenExceptionLoggedAndEventHubErrorMetricLogged_Test(Type exType, object[] param, string expectedErrorMetricName)
        {
            var logger = Substitute.For<ITelemetryLogger>();
            Exception ex = Activator.CreateInstance(exType, param) as Exception;

            EventHubExceptionTelemetryProcessor.ProcessException(ex, logger);

            logger.Received(1).LogError(ex);
            logger.Received(1).LogMetric(Arg.Is<Metric>(m =>
                m.Name.Equals(expectedErrorMetricName) &&
                ValidateEventHubErrorMetricProperties(m)),
                1);
        }

        [Theory]
        [InlineData(typeof(SocketException))]
        public void GivenExceptionAndShouldNotLogMetric_WhenProcessExpection_ThenExceptionLoggedAndEventHubErrorMetricNotLogged_Test(System.Type exType)
        {
            var logger = Substitute.For<ITelemetryLogger>();
            var ex = Activator.CreateInstance(exType) as Exception;

            EventHubExceptionTelemetryProcessor.ProcessException(ex, logger, shouldLogMetric: false);

            logger.Received(1).LogError(ex);
            logger.DidNotReceiveWithAnyArgs().LogMetric(null, default);
        }

        [Theory]
        [InlineData(typeof(Exception))]
        public void GivenExceptionAndErrorMetricName_WhenProcessExpection_ThenExceptionLoggedAndErrorMetricNameLogged_Test(System.Type exType)
        {
            var logger = Substitute.For<ITelemetryLogger>();
            var ex = Activator.CreateInstance(exType) as Exception;

            EventHubExceptionTelemetryProcessor.ProcessException(ex, logger, errorMetricName: EventHubErrorCode.EventHubPartitionInitFailed.ToString());

            logger.Received(1).LogError(ex);
            logger.Received(1).LogMetric(Arg.Is<Metric>(m =>
                m.Name.Equals(EventHubErrorCode.EventHubPartitionInitFailed.ToString()) &&
                ValidateEventHubErrorMetricProperties(m)),
                1);
        }

        private bool ValidateEventHubErrorMetricProperties(Metric metric)
        {
            return metric.Dimensions["Category"].Equals(Category.Errors) &&
                metric.Dimensions["ErrorType"].Equals(ErrorType.EventHubError);
        }
    }
}
