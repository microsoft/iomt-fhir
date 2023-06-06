// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Events.EventHubProcessor
{
    public class ProcessorIdProvider : IProcessorIdProvider
    {
        private string _processorId;

        public ProcessorIdProvider(string processorId)
        {
            _processorId = processorId;
        }

        public string GetProcessorId()
        {
            return _processorId;
        }
    }
}
