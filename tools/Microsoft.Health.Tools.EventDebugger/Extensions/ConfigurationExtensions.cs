// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Health.Tools.EventDebugger.Extensions
{
    public static class ConfigurationExtensions
    {
        public static string GetArgument(this IConfiguration configuration, string key, bool required = false)
        {
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            var value = configuration[key];
            if (required && string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Missing value for configuration item {key}");
            }

            return value;
        }
    }
}