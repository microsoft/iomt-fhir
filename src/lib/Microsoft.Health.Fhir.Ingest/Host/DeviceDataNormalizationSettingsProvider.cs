// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Ingest.Service;

namespace Microsoft.Health.Fhir.Ingest.Host
{
    public class DeviceDataNormalizationSettingsProvider : IExtensionConfigProvider
    {
        private readonly IOptions<NormalizationServiceOptions> _options;

        public DeviceDataNormalizationSettingsProvider(IOptions<NormalizationServiceOptions> options)
        {
            _options = options;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            EnsureArg.IsNotNull(context, nameof(context));

            context.AddBindingRule<DeviceDataNormalizationAttribute>()
                .BindToInput(attr => _options);
        }
    }
}