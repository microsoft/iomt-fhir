// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public abstract class CollectionFhirValueProcessor<TInValue, TOutValue> : IFhirValueProcessor<TInValue, TOutValue>
    {
        private static readonly Type SupportedType = typeof(FhirValueType);
        private readonly IDictionary<Type, IFhirValueProcessor<TInValue, TOutValue>> _registeredValueProcessors;

        public CollectionFhirValueProcessor(params IFhirValueProcessor<TInValue, TOutValue>[] valueProcessors)
        {
            _registeredValueProcessors = EnsureArg.HasItems(valueProcessors, nameof(valueProcessors))
                .ToDictionary(vp => vp.SupportedValueType);
        }

        public Type SupportedValueType => SupportedType;

        public TOutValue CreateValue(FhirValueType template, TInValue inValue)
        {
            return ProcessorLookup(template).CreateValue(template, inValue);
        }

        public TOutValue MergeValue(FhirValueType template, TInValue inValue, TOutValue existingValue)
        {
            return ProcessorLookup(template).MergeValue(template, inValue, existingValue);
        }

        private IFhirValueProcessor<TInValue, TOutValue> ProcessorLookup(FhirValueType template)
        {
            EnsureArg.IsNotNull(template);
            var type = template.GetType();
            if (!_registeredValueProcessors.TryGetValue(type, out var processor))
            {
                throw new NotSupportedException($"Value processor for FhirValueType {type} not supported.");
            }

            return processor;
        }
    }
}
