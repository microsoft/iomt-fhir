// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Health.Common.EventHubs
{
    internal class EventHubTriggerAttributeBindingProvider : ITriggerBindingProvider
    {
        private readonly INameResolver _nameResolver;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly IOptions<EventHubOptions> _options;
        private readonly IConverterManager _converterManager;

        public EventHubTriggerAttributeBindingProvider(
            IConfiguration configuration,
            INameResolver nameResolver,
            IConverterManager converterManager,
            IOptions<EventHubOptions> options,
            ILoggerFactory loggerFactory)
        {
            _config = configuration;
            _nameResolver = nameResolver;
            _converterManager = converterManager;
            _options = options;
            _logger = loggerFactory?.CreateLogger(LogCategories.CreateTriggerCategory("EventHub"));
        }

#pragma warning disable SA1404 // Code analysis suppression should have justification
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
#pragma warning restore SA1404 // Code analysis suppression should have justification
        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ParameterInfo parameter = context.Parameter;
            EventHubTriggerAttribute attribute = parameter.GetCustomAttribute<EventHubTriggerAttribute>(inherit: false);

            if (attribute == null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            string resolvedEventHubName = _nameResolver.ResolveWholeString(attribute.EventHubName);

            string consumerGroup = attribute.ConsumerGroup ?? PartitionReceiver.DefaultConsumerGroupName;
            string resolvedConsumerGroup = _nameResolver.ResolveWholeString(consumerGroup);

            string connectionString = null;
            if (!string.IsNullOrWhiteSpace(attribute.Connection))
            {
                attribute.Connection = _nameResolver.ResolveWholeString(attribute.Connection);
                connectionString = _config.GetConnectionStringOrSetting(attribute.Connection);
                _options.Value.AddReceiver(resolvedEventHubName, connectionString);
            }

            var eventHostListener = _options.Value.GetEventProcessorHost(_config, resolvedEventHubName, resolvedConsumerGroup);

            string storageConnectionString = _config.GetWebJobsConnectionString(ConnectionStringNames.Storage);

            Func<ListenerFactoryContext, bool, Task<IListener>> createListener =
             (factoryContext, singleDispatch) =>
             {
                 IListener listener = new EventHubListener(
                                                factoryContext.Descriptor.Id,
                                                resolvedEventHubName,
                                                resolvedConsumerGroup,
                                                connectionString,
                                                storageConnectionString,
                                                factoryContext.Executor,
                                                eventHostListener,
                                                singleDispatch,
                                                _options.Value,
                                                _logger);
                 return Task.FromResult(listener);
             };

#pragma warning disable CS0618 // Type or member is obsolete
            ITriggerBinding binding = BindingFactory.GetTriggerBinding(new EventHubTriggerBindingStrategy(), parameter, _converterManager, createListener);
#pragma warning restore CS0618 // Type or member is obsolete
            return Task.FromResult<ITriggerBinding>(binding);
        }
    } // end class
}