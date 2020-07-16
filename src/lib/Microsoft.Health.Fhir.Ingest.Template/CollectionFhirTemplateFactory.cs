// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
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

        protected override ILookupTemplate<IFhirTemplate> BuildCollectionTemplate(JArray templateCollection, out IList<string> errors)
        {
            EnsureArg.IsNotNull(templateCollection, nameof(templateCollection));
            List<string> collectionErrors = new List<string>();

            var lookupTemplate = new FhirLookupTemplate();
            foreach (var token in templateCollection)
            {
                var container = token.ToObject<TemplateContainer>();
                var createdTemplate = TemplateFactories.Evaluate(container, out IList<string> createdTemplateErrors);

                // Validations and error handling.
                collectionErrors.AddRange(createdTemplateErrors ?? Enumerable.Empty<string>());
                collectionErrors.AddRange(Validate(createdTemplate, lookupTemplate));

                // Processing the newly created template.
                if (createdTemplate?.TypeName != null && !lookupTemplate.HasTemplate(createdTemplate.TypeName))
                {
                    lookupTemplate.RegisterTemplate(createdTemplate);
                }
            }

            errors = collectionErrors;
            return lookupTemplate;
        }

        // Perform collection level validation.
        private IList<string> Validate(IFhirTemplate createdTemplate, FhirLookupTemplate collectionLookupTemplate)
        {
            EnsureArg.IsNotNull(createdTemplate, nameof(createdTemplate));

            IList<string> errors = new List<string>();

            if (!string.IsNullOrEmpty(createdTemplate.TypeName) && collectionLookupTemplate.HasTemplate(createdTemplate.TypeName))
            {
                errors.Add($"Duplicate template defined for {createdTemplate.TypeName}");
            }

            return errors;
        }
    }
}
