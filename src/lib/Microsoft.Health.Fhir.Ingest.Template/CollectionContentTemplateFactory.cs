// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
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
        public CollectionContentTemplateFactory(params ITemplateFactory<TemplateContainer, IContentTemplate>[] factories)
            : base(factories)
        {
        }

        public CollectionContentTemplateFactory(IEnumerable<ITemplateFactory<TemplateContainer, IContentTemplate>> factories)
            : base(factories.ToArray())
        {
        }

        protected override string TargetTemplateTypeName => "CollectionContentTemplate";

        protected override IContentTemplate BuildCollectionTemplate(JArray templateCollection, ICollection<TemplateError> errors)
        {
            EnsureArg.IsNotNull(templateCollection, nameof(templateCollection));
            EnsureArg.IsNotNull(errors, nameof(errors));

            var collectionTemplate = new CollectionContentTemplate();
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
                    errors.Add(new TemplateError(ex.Message));
                }
                catch (JsonSerializationException ex)
                {
                    errors.Add(new TemplateError(ex.Message));
                }
            }

            return collectionTemplate;
        }
    }
}
