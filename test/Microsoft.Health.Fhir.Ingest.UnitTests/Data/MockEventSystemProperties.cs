// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest
{
    internal class MockEventSystemProperties : IReadOnlyDictionary<string, object>
    {
        private readonly Dictionary<string, object> _properties;

        public MockEventSystemProperties(JToken properties)
        {
            _properties = properties.ToObject<Dictionary<string, object>>();
        }

        public MockEventSystemProperties(long sequenceNumber, DateTime enqueueTime, long offset, string partitionKey)
        {
            _properties = new Dictionary<string, object>
                {
                    { "x-opt-sequence-number", sequenceNumber },
                    { "x-opt-enqueued-time", enqueueTime },
                    { "x-opt-offset", offset },
                    { "x-opt-partition-key", partitionKey },
                };
        }

        public IEnumerable<string> Keys => _properties.Keys;

        public IEnumerable<object> Values => _properties.Values;

        public int Count => _properties.Count();

        public object this[string key]
        {
            get
            {
                return _properties[key];
            }
        }

        public bool ContainsKey(string key)
        {
            return _properties.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _properties.GetEnumerator();
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            return _properties.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _properties.GetEnumerator();
        }
    }
}