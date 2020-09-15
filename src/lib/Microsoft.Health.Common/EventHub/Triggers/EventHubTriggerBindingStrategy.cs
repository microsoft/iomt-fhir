// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Triggers;

namespace Microsoft.Health.Common.EventHubs
{
    // Binding strategy for an event hub triggers.
#pragma warning disable CS0618 // Type or member is obsolete
    internal class EventHubTriggerBindingStrategy : ITriggerBindingStrategy<EventData, EventHubTriggerInput>
#pragma warning restore CS0618 // Type or member is obsolete
    {
#pragma warning disable SA1404 // Code analysis suppression should have justification
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
#pragma warning restore SA1404 // Code analysis suppression should have justification
        public EventHubTriggerInput ConvertFromString(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            EventData eventData = new EventData(bytes);

            // Return a single event. Doesn't support multiple dispatch
            return EventHubTriggerInput.New(eventData);
        }

        // Single instance: Core --> EventData
        public EventData BindSingle(EventHubTriggerInput value, ValueBindingContext context)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return value.GetSingleEventData();
        }

        public EventData[] BindMultiple(EventHubTriggerInput value, ValueBindingContext context)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return value.Events;
        }

        public Dictionary<string, Type> GetBindingContract(bool isSingleDispatch = true)
        {
            var contract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            contract.Add("PartitionContext", typeof(PartitionContext));

            AddBindingContractMember(contract, "PartitionKey", typeof(string), isSingleDispatch);
            AddBindingContractMember(contract, "Offset", typeof(string), isSingleDispatch);
            AddBindingContractMember(contract, "SequenceNumber", typeof(long), isSingleDispatch);
            AddBindingContractMember(contract, "EnqueuedTimeUtc", typeof(DateTime), isSingleDispatch);
            AddBindingContractMember(contract, "Properties", typeof(IDictionary<string, object>), isSingleDispatch);
            AddBindingContractMember(contract, "SystemProperties", typeof(IDictionary<string, object>), isSingleDispatch);

            return contract;
        }

        private static void AddBindingContractMember(Dictionary<string, Type> contract, string name, Type type, bool isSingleDispatch)
        {
            if (!isSingleDispatch)
            {
                name += "Array";
            }

            contract.Add(name, isSingleDispatch ? type : type.MakeArrayType());
        }

        public Dictionary<string, object> GetBindingData(EventHubTriggerInput value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            SafeAddValue(() => bindingData.Add(nameof(value.PartitionContext), value.PartitionContext));

            if (value.IsSingleDispatch)
            {
                AddBindingData(bindingData, value.GetSingleEventData());
            }
            else
            {
                AddBindingData(bindingData, value.Events);
            }

            return bindingData;
        }

        internal static void AddBindingData(Dictionary<string, object> bindingData, EventData[] events)
        {
            int length = events.Length;
            var partitionKeys = new string[length];
            var offsets = new string[length];
            var sequenceNumbers = new long[length];
            var enqueuedTimesUtc = new DateTime[length];
            var properties = new IDictionary<string, object>[length];
            var systemProperties = new IDictionary<string, object>[length];

            SafeAddValue(() => bindingData.Add("PartitionKeyArray", partitionKeys));
            SafeAddValue(() => bindingData.Add("OffsetArray", offsets));
            SafeAddValue(() => bindingData.Add("SequenceNumberArray", sequenceNumbers));
            SafeAddValue(() => bindingData.Add("EnqueuedTimeUtcArray", enqueuedTimesUtc));
            SafeAddValue(() => bindingData.Add("PropertiesArray", properties));
            SafeAddValue(() => bindingData.Add("SystemPropertiesArray", systemProperties));

            for (int i = 0; i < events.Length; i++)
            {
                partitionKeys[i] = events[i].SystemProperties?.PartitionKey;
                offsets[i] = events[i].SystemProperties?.Offset;
                sequenceNumbers[i] = events[i].SystemProperties?.SequenceNumber ?? 0;
                enqueuedTimesUtc[i] = events[i].SystemProperties?.EnqueuedTimeUtc ?? DateTime.MinValue;
                properties[i] = events[i].Properties;
                systemProperties[i] = events[i].SystemProperties?.ToDictionary();
            }
        }

        private static void AddBindingData(Dictionary<string, object> bindingData, EventData eventData)
        {
            SafeAddValue(() => bindingData.Add(nameof(eventData.SystemProperties.PartitionKey), eventData.SystemProperties?.PartitionKey));
            SafeAddValue(() => bindingData.Add(nameof(eventData.SystemProperties.Offset), eventData.SystemProperties?.Offset));
            SafeAddValue(() => bindingData.Add(nameof(eventData.SystemProperties.SequenceNumber), eventData.SystemProperties?.SequenceNumber ?? 0));
            SafeAddValue(() => bindingData.Add(nameof(eventData.SystemProperties.EnqueuedTimeUtc), eventData.SystemProperties?.EnqueuedTimeUtc ?? DateTime.MinValue));
            SafeAddValue(() => bindingData.Add(nameof(eventData.Properties), eventData.Properties));
            SafeAddValue(() => bindingData.Add(nameof(eventData.SystemProperties), eventData.SystemProperties?.ToDictionary()));
        }

        private static void SafeAddValue(Action addValue)
        {
            try
            {
                addValue();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                // some message propery getters can throw, based on the
                // state of the message
            }
        }
    }
}