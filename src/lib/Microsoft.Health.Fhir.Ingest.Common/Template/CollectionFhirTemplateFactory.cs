// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CollectionFhirTemplateFactory : CollectionTemplateFactory<IFhirTemplate, ILookupTemplate<IFhirTemplate>>
    {
        private static readonly ITemplateFactory<string, ILookupTemplate<IFhirTemplate>> DefaultFactory = new CollectionFhirTemplateFactory();

        private CollectionFhirTemplateFactory()
            : base(new CodeValueFhirTemplateFactory())
        {
        }

        public CollectionFhirTemplateFactory(params ITemplateFactory<TemplateContainer, IFhirTemplate>[] factories)
            : base(factories)
        {
        }

        public static ITemplateFactory<string, ILookupTemplate<IFhirTemplate>> Default => DefaultFactory;

        protected override string TargetTemplateTypeName => "CollectionFhirTemplate";

        protected override ILookupTemplate<IFhirTemplate> BuildCollectionTemplate(JArray templateCollection)
        {
            EnsureArg.IsNotNull(templateCollection, nameof(templateCollection));

            var lookupTemplate = new FhirLookupTemplate();
            foreach (var token in templateCollection)
            {
                var container = token.ToObject<TemplateContainer>();
                var createdTemplate = TemplateFactories.Evaluate(container);
                lookupTemplate.RegisterTemplate(createdTemplate);
            }

            return lookupTemplate;
        }
    }
}
