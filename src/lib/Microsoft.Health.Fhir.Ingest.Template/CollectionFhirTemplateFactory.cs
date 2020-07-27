// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using EnsureThat;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CollectionFhirTemplateFactory : CollectionTemplateFactory<IFhirTemplate, ILookupTemplate<IFhirTemplate>>
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

        public ITemplateContext<ILookupTemplate<IFhirTemplate>> Create(string input)
        {
            var context = new TemplateContext<ILookupTemplate<IFhirTemplate>>();

            context.Template = Create(input, context.Errors);

            return context;
        }

        protected override ILookupTemplate<IFhirTemplate> BuildCollectionTemplate(JArray templateCollection, ICollection<Exception> errorContext)
        {
            EnsureArg.IsNotNull(templateCollection, nameof(templateCollection));
            EnsureArg.IsNotNull(errorContext, nameof(errorContext));

            var lookupTemplate = new FhirLookupTemplate();
            foreach (var token in templateCollection)
            {
                try
                {
                    var container = token.ToObject<TemplateContainer>();
                    var createdTemplate = TemplateFactories.Evaluate(container);
                    lookupTemplate.RegisterTemplate(createdTemplate);
                }
                catch (JsonSerializationException jse)
                {
                    errorContext.Add(jse);
                }
            }

            return lookupTemplate;
        }
    }
}
