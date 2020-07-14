// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    /// <summary>
    /// Creates a single template from a collection a provided templates based on registered factories.
    /// </summary>
    public class CollectionContentTemplateFactory : CollectionTemplateFactory<IContentTemplate, IContentTemplate>
    {
        private static readonly ITemplateFactory<string, IContentTemplate> DefaultFactory = new CollectionContentTemplateFactory();

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

        public static ITemplateFactory<string, IContentTemplate> Default => DefaultFactory;

        protected override string TargetTemplateTypeName => "CollectionContentTemplate";

        protected override IContentTemplate BuildCollectionTemplate(JArray templateCollection)
        {
            EnsureArg.IsNotNull(templateCollection, nameof(templateCollection));

            var template = new CollectionContentTemplate();
            foreach (var token in templateCollection)
            {
                var container = token.ToObject<TemplateContainer>();
                var createdTemplate = TemplateFactories.Evaluate(container);
                template.RegisterTemplate(createdTemplate);
            }

            return template;
        }
    }
}
