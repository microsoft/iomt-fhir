// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public abstract class FhirValueProcessor<TValueType, TInValue, TOutValue> : IFhirValueProcessor<TInValue, TOutValue>
        where TValueType : FhirValueType
    {
        private static readonly Type SupportedType = typeof(TValueType);

        public Type SupportedValueType => SupportedType;

        public TOutValue CreateValue(FhirValueType template, TInValue inValue)
        {
            return CreateValueImpl(CastTemplate(template), inValue);
        }

        public TOutValue MergeValue(FhirValueType template, TInValue inValue, TOutValue existingValue)
        {
            return MergeValueImpl(CastTemplate(template), inValue, existingValue);
        }

        protected TValueType CastTemplate(FhirValueType template)
        {
            EnsureArg.IsNotNull(template, nameof(template));

            if (!(template is TValueType castTemplate))
            {
                throw new NotSupportedException($"Template type {template.GetType()} does not match supported type {SupportedValueType}.");
            }

            return castTemplate;
        }

        protected abstract TOutValue CreateValueImpl(TValueType template, TInValue inValue);

        protected abstract TOutValue MergeValueImpl(TValueType template, TInValue inValue, TOutValue existingValue);
    }
}
