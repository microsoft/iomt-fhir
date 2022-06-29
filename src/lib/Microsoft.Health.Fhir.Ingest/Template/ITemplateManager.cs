// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public interface ITemplateManager
    {
        byte[] GetTemplate(string templateName);

        string GetTemplateAsString(string templateName);

        Task<string> GetTemplateContentIfChangedSince(string templateName, DateTimeOffset contentTimestamp = default, CancellationToken cancellationToken = default);
    }
}
