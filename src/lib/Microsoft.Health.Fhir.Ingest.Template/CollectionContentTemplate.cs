// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Data;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CollectionContentTemplate : IContentTemplate
    {
        private readonly IList<IContentTemplate> _templates = new List<IContentTemplate>(10);

        public IReadOnlyList<IContentTemplate> Templates => (_templates as List<IContentTemplate>).AsReadOnly();

        public CollectionContentTemplate RegisterTemplate(IContentTemplate contentTemplate)
        {
            EnsureArg.IsNotNull(contentTemplate, nameof(contentTemplate));

            _templates.Add(contentTemplate);

            return this;
        }

        public IEnumerable<Measurement> GetMeasurements(JToken token)
        {
            return _templates.SelectMany(t => t.GetMeasurements(token));
        }
    }
}
