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
    public class CollectionContentTemplateFactory : CollectionTemplateFactory<IContentTemplate, ITemplateContext<IContentTemplate>>
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

        protected override ITemplateContext<IContentTemplate> BuildCollectionTemplateContext(JArray templateCollection)
        {
            EnsureArg.IsNotNull(templateCollection, nameof(templateCollection));

            var collectionTemplate = new CollectionContentTemplate();
            var collectionTemplateContext = new TemplateContext<IContentTemplate>(collectionTemplate);
            foreach (var token in templateCollection)
            {
                try
                {
                    var container = token.ToObject<TemplateContainer>();
                    var createdTemplate = TemplateFactories.Evaluate(container);
                    collectionTemplate.RegisterTemplate(createdTemplate);
                }
                catch (InvalidTemplateException ex)
                {
                    collectionTemplateContext.Errors.Add(new TemplateError(ex.Message));
                }
            }

            return collectionTemplateContext;
        }
    }
}
