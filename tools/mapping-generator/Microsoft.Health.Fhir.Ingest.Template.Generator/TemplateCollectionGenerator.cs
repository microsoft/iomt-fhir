// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Health.Common.Extension;
using Microsoft.Health.Fhir.Ingest.Template.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Health.Fhir.Ingest.Template.Generator
{
    /// <summary>
    /// This abstract class provides a base that can be used to generate template collections.
    /// </summary>
    /// <remarks>
    /// The resulting JObject represents a template collection that can be used for Device Content Mapping or FHIR Mapping.
    /// </remarks>
    /// <typeparam name="TModel">The class that is used to generate the templates.</typeparam>
    public abstract class TemplateCollectionGenerator<TModel> : ITemplateCollectionGenerator<TModel>
        where TModel : class
    {
        /// <summary>
        /// JsonSerializer that will ensure the serialized properties are camel cased.
        /// </summary>
        private readonly JsonSerializer _serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            ContractResolver = new LineAwareContractResolver(new CamelCasePropertyNamesContractResolver()),
            DefaultValueHandling = DefaultValueHandling.Ignore,
        });

        /// <summary>
        /// A Boolean that controls whether or not the generator should throw an exception if the same TypeName
        /// is found in a template that contains different content when the collection is generated.
        /// </summary>
        protected abstract bool RequireUniqueTemplateTypeNames { get; }

        /// <summary>
        /// The type of collection to be generated, either CollectionContent, or CollectionFhir.
        /// </summary>
        protected abstract TemplateCollectionType CollectionType { get; }

        public async Task<JObject> GenerateTemplateCollection(IEnumerable<TModel> model, CancellationToken cancellationToken)
        {
            JArray templateJObjects = new JArray();
            var templateTasks = model.Select(m => GetTemplate(m, cancellationToken));
            IEnumerable<JObject> templates = await Task.WhenAll(templateTasks);

            foreach (var template in templates)
            {
                if (template != null && IsTemplateUnique(template, templateJObjects))
                {
                    templateJObjects.Add(template);
                }
            }

            var templateContainer = new TemplateContainer
            {
                TemplateType = CollectionType.ToString(),
                Template = templateJObjects,
            };

            return JObject.FromObject(templateContainer, _serializer);
        }

        /// <summary>
        /// Gets a Template (in JObject format) that should be added to the collection.
        /// </summary>
        /// <param name="model">The model that the CalculatedFunctionContentTemplate is generated from.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="JObject"/></returns>
        public abstract Task<JObject> GetTemplate(TModel model, CancellationToken cancellationToken);

        private bool IsTemplateUnique(JToken template, JArray templates)
        {
            if (template == null || !templates.Any())
            {
                return true;
            }

            var templateKey = nameof(TemplateContainer.Template).ToLowercaseFirstLetterVariant();
            var typeNameKey = nameof(Template.TypeName).ToLowercaseFirstLetterVariant();

            string typeName = template[templateKey][typeNameKey].ToString();

            JToken existingTemplate = templates.FirstOrDefault(t => string.Equals(t[templateKey][typeNameKey].ToString(), typeName, StringComparison.OrdinalIgnoreCase));

            if (existingTemplate == null)
            {
                return true;
            }

            if (JToken.DeepEquals(existingTemplate, template))
            {
                return false;
            }

            if (RequireUniqueTemplateTypeNames)
            {
                throw new InvalidOperationException($"A duplicate type name for a unique template found: {typeName}");
            }

            return true;
        }
    }
}
