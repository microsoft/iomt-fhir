// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template.Generator.UnitTests.Samples
{
    public class TestTemplateCollectionGenerator : TemplateCollectionGenerator<TestModel>
    {
        protected override bool RequireUniqueTemplateTypeNames => true;

        protected override TemplateCollectionType CollectionType => TemplateCollectionType.CollectionContent;

        public IList<JObject> TemplateResponses { get; } = new List<JObject>();

        public override Task<JObject> GetTemplate(TestModel model, CancellationToken cancellationToken)
        {
            if (TemplateResponses.Count > 0)
            {
                JObject templateResponse = TemplateResponses.First();
                TemplateResponses.Remove(templateResponse);

                return Task.FromResult(templateResponse);
            }

            return Task.FromResult<JObject>(null);
        }
    }
}
