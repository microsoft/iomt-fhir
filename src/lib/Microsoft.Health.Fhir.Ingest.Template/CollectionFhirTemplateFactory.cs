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
        public CollectionFhirTemplateFactory(params ITemplateFactory<TemplateContainer, IFhirTemplate>[] factories)
            : base(factories)
        {
        }

        private CollectionFhirTemplateFactory()
            : base(new CodeValueFhirTemplateFactory())
        {
        }

        public static CollectionFhirTemplateFactory Default { get; } = new CollectionFhirTemplateFactory();

        protected override string TargetTemplateTypeName => "CollectionFhirTemplate";

        protected override ILookupTemplate<IFhirTemplate> BuildCollectionTemplate(JArray templateCollection, ICollection<TemplateError> errors)
        {
            EnsureArg.IsNotNull(templateCollection, nameof(templateCollection));
            EnsureArg.IsNotNull(errors, nameof(errors));

            var lookupTemplate = new FhirLookupTemplate();
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
                    errors.Add(new TemplateError(ex.Message, ex.GetLineInfo));
                }
                catch (JsonSerializationException ex)
                {
                    errors.Add(new TemplateError(ex.Message, new LineInfo() { LineNumber = ex.LineNumber, LinePosition = ex.LinePosition }));
                }
                catch (AggregateException ex)
                {
                    errors.Add(new TemplateError(ex.Message));

                    foreach (var innerException in ex.InnerExceptions)
                    {
                        if (innerException is InvalidTemplateException ite)
                        {
                            errors.Add(new TemplateError(ite.Message, ite.GetLineInfo));
                        }
                        else
                        {
                            errors.Add(new TemplateError(innerException.Message));
                        }
                    }
                }
            }

            return lookupTemplate;
        }
    }
}
