// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CollectionFhirTemplateFactory : CollectionTemplateFactory<IFhirTemplate, ITemplateContext<ILookupTemplate<IFhirTemplate>>>
    {
        private CollectionFhirTemplateFactory()
            : base(new CodeValueFhirTemplateFactory())
        {
        }

        public CollectionFhirTemplateFactory(params ITemplateFactory<TemplateContainer, IFhirTemplate>[] factories)
            : base(factories)
        {
        }

        public static CollectionFhirTemplateFactory Default { get; } = new CollectionFhirTemplateFactory();

        protected override string TargetTemplateTypeName => "CollectionFhirTemplate";

        protected override ITemplateContext<ILookupTemplate<IFhirTemplate>> BuildCollectionTemplateContext(JArray templateCollection)
        {
            EnsureArg.IsNotNull(templateCollection, nameof(templateCollection));

            var lookupTemplate = new FhirLookupTemplate();
            var lookupTemplateContext = new TemplateContext<ILookupTemplate<IFhirTemplate>>(lookupTemplate);
            foreach (var token in templateCollection)
            {
                try
                {
                    var container = token.ToObject<TemplateContainer>();
                    var createdTemplate = TemplateFactories.Evaluate(container);
                    lookupTemplate.RegisterTemplate(createdTemplate);
                }
                catch (InvalidTemplateException ex)
                {
                    lookupTemplateContext.Errors.Add(new TemplateError(ex.Message));
                }
            }

            return lookupTemplateContext;
        }
    }
}
