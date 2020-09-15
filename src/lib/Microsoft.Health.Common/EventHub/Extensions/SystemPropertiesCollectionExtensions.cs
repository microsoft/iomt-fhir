// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using static Microsoft.Azure.EventHubs.EventData;

namespace Microsoft.Health.Common.EventHubs
{
    internal static class SystemPropertiesCollectionExtensions
    {
        internal static IDictionary<string, object> ToDictionary(this SystemPropertiesCollection collection)
        {
            IDictionary<string, object> modifiedDictionary = new Dictionary<string, object>(collection);

            // Following is needed to maintain structure of bindingdata: https://github.com/Azure/azure-webjobs-sdk/pull/1849
            modifiedDictionary["SequenceNumber"] = collection.SequenceNumber;
            modifiedDictionary["Offset"] = collection.Offset;
            modifiedDictionary["PartitionKey"] = collection.PartitionKey;
            modifiedDictionary["EnqueuedTimeUtc"] = collection.EnqueuedTimeUtc;
            return modifiedDictionary;
        }
    }
}
