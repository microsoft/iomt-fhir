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
    /// <summary>
    /// Creates a single template from a collection a provided templates based on registered factories.
    /// </summary>
    public class CollectionContentTemplateFactory : CollectionTemplateFactory<IContentTemplate, IContentTemplate>
    {
        private CollectionContentTemplateFactory()
            : base(
                  new JsonPathContentTemplateFactory(),
                  new IotJsonPathContentTemplateFactory())
        {
        }

        public CollectionContentTemplateFactory(params ITemplateFactory<TemplateContainer, IContentTemplate>[] factories)
            : base(factories)
        {
        }

        public static CollectionContentTemplateFactory Default { get; } = new CollectionContentTemplateFactory();

        protected override string TargetTemplateTypeName => "CollectionContentTemplate";

        public ITemplateContext<IContentTemplate> Create(string input)
        {
            var context = new TemplateContext<IContentTemplate>();

            context.Template = Create(input, context.Errors);

            return context;
        }

        protected override IContentTemplate BuildCollectionTemplate(JArray templateCollection, ICollection<Exception> errorContext)
        {
            EnsureArg.IsNotNull(templateCollection, nameof(templateCollection));
            EnsureArg.IsNotNull(errorContext, nameof(errorContext));

            var template = new CollectionContentTemplate();
            foreach (var token in templateCollection)
            {
                try
                {
                    var container = token.ToObject<TemplateContainer>();
                    var createdTemplate = TemplateFactories.Evaluate(container);
                    template.RegisterTemplate(createdTemplate);
                }
                catch (JsonSerializationException jse)
                {
                    errorContext.Add(jse);
                }
            }

            return template;
        }
    }
}
