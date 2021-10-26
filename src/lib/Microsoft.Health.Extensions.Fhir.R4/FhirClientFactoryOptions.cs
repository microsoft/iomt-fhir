// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Auth;

namespace Microsoft.Health.Extensions.Fhir
{
    public class FhirClientFactoryOptions
    {
        public IAzureCredentialProvider CredentialProvider { get; set; }

        public bool UseManagedIdentity { get; set; } = false;

        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    }
}
