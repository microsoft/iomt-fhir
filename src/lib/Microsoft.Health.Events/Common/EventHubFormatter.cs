// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;

namespace Microsoft.Health.Events.Common
{
    public static class EventHubFormatter
    {
        public static string GetEventHubFQDN(string host)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(host);

            if (Uri.IsWellFormedUriString(host, UriKind.Absolute))
            {
                var uri = new Uri(host);
                host = uri.Host;
            }

            if (Uri.CheckHostName(host) != UriHostNameType.Unknown)
            {
                return host;
            }
            else
            {
                throw new Exception($"The event hub FQDN: {host} is not valid");
            }
        }
    }
}
