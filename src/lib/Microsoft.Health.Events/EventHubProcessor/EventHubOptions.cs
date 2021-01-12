// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Events.EventHubProcessor
{
    public class EventHubOptions
    {
        public const string Settings = "EventHub";

        public EventHubOptions(string connectionString, string name)
        {
            EventHubConnectionString = connectionString;
            EventHubName = name;
        }

        public string EventHubConnectionString { get; set; }

        public string EventHubName { get; set; }
    }
}
