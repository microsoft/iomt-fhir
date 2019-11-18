// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Health.Common;
using Microsoft.Health.Fhir.Ingest.Data;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class IdentityLookupFactory : IFactory<IDictionary<ResourceType, string>>
    {
        private static readonly IFactory<IDictionary<ResourceType, string>> InternalInstance = new IdentityLookupFactory();
        private static readonly ResourceType[] Values = Enum.GetValues(typeof(ResourceType)).OfType<ResourceType>().ToArray();

        private IdentityLookupFactory()
        {
        }

        public static IFactory<IDictionary<ResourceType, string>> Instance => InternalInstance;

        public IDictionary<ResourceType, string> Create()
        {
            return Values.ToDictionary(k => k, rt => (string)null);
        }
    }
}
