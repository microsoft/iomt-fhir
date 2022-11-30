﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Auth;

namespace Microsoft.Health.Extensions.Fhir.Config
{
    public class FhirClientFactoryOptions
    {
        public FhirClientFactoryOptions()
        {
        }

        public IAzureCredentialProvider CredentialProvider { get; set; }

        public bool UseManagedIdentity { get; set; }

        public static TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
    }
}
