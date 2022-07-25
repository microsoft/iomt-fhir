// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using EnsureThat;
using Microsoft.Health.Common.Auth;
using Microsoft.Health.Events.Common;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Events.EventProducers
{
    public class EventProducerClientFactory : IEventProducerClientFactory
    {
        private readonly ITelemetryLogger _logger;

        public EventProducerClientFactory(ITelemetryLogger telemetryLogger)
        {
            _logger = EnsureArg.IsNotNull(telemetryLogger, nameof(telemetryLogger));
        }

        public EventHubProducerClient GetEventHubProducerClient(EventHubClientOptions options, IAzureCredentialProvider provider = null)
        {
            EnsureArg.IsNotNull(options);

            if (options.AuthenticationType == AuthenticationType.ManagedIdentity)
            {
                EnsureArg.IsNotNull(options.EventHubName);
                EnsureArg.IsNotNull(options.EventHubNamespaceFQDN);

                var tokenCredential = new DefaultAzureCredential();
                var eventHubFQDN = EventHubFormatter.GetEventHubFQDN(options.EventHubNamespaceFQDN);
                return new EventHubProducerClient(eventHubFQDN, options.EventHubName, tokenCredential);
            }
            else if (options.AuthenticationType == AuthenticationType.ConnectionString)
            {
                EnsureArg.IsNotNull(options.ConnectionString);

                return new EventHubProducerClient(options.ConnectionString);
            }
            else if (options.AuthenticationType == AuthenticationType.Custom)
            {
                EnsureArg.IsNotNull(options.EventHubName);
                EnsureArg.IsNotNull(options.EventHubNamespaceFQDN);
                EnsureArg.IsNotNull(provider);

                var eventHubFQDN = EventHubFormatter.GetEventHubFQDN(options.EventHubNamespaceFQDN);
                var clientOptions = new EventHubProducerClientOptions()
                {
                    RetryOptions = new EventHubsRetryOptions()
                    {
                        CustomRetryPolicy = new CustomEventHubsRetryPolicy(_logger),
                    },
                    ConnectionOptions = new EventHubConnectionOptions()
                    {
                        SendBufferSizeInBytes = 1024 * 64,
                    },
                };

                var clients = Enumerable.Range(0, options.InternalClientCount)
                    .Select(_ => new EventHubProducerClient(eventHubFQDN, options.EventHubName, provider.GetCredential(), clientOptions))
                    .ToList();
                return new EventHubProducerClient(eventHubFQDN, options.EventHubName, provider.GetCredential(), clientOptions);
            }
            else
            {
                var ex = $"Unable to create Event Hub producer client for {options.EventHubName}";
                var message = "No authentication type was specified for EventHubClientOptions.";
                throw new Exception($"{ex} {message}");
            }
        }

        private class CustomEventHubsRetryPolicy : EventHubsRetryPolicy
        {
            private readonly ITelemetryLogger _logger;
            private int _maxRetryAttempts = 3;
            private TimeSpan _tryTimeout = TimeSpan.FromMinutes(1);
            private TimeSpan _minTryTimeout = TimeSpan.FromMilliseconds(500);

            public CustomEventHubsRetryPolicy(ITelemetryLogger telemetryLogger, int maxRetryAttempts = 3)
            {
                _logger = EnsureArg.IsNotNull(telemetryLogger, nameof(telemetryLogger));
                _maxRetryAttempts = EnsureArg.IsGte(maxRetryAttempts, 0, nameof(maxRetryAttempts));
            }

            public override TimeSpan? CalculateRetryDelay(Exception lastException, int attemptCount)
            {
                EnsureArg.IsNotNull(lastException, nameof(lastException));
                _logger.LogTrace($"EventHub Submission Attempt {attemptCount}. Last observed error: Type:{lastException.GetType()}, Message: {lastException.Message}");

                if (attemptCount > _maxRetryAttempts)
                {
                    _logger.LogTrace($"EventHub Submission Attempt {attemptCount}. Last observed error: Type:{lastException.GetType()}, Message: {lastException.Message}. Maximum retry attempts exceeded");
                    return null;
                }

                if (attemptCount == 1)
                {
                    return _minTryTimeout;
                }
                else
                {
                    return TimeSpan.FromSeconds(Math.Min(4, Math.Pow(2, attemptCount - 1)));
                }
            }

            public override TimeSpan CalculateTryTimeout(int attemptCount)
            {
                return _tryTimeout;
            }
        }
    }
}
