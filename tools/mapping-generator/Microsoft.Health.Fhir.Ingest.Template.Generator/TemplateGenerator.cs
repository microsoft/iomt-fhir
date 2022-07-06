// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Template.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Health.Fhir.Ingest.Template.Generator
{
    public abstract class TemplateGenerator<TTemplate, TModel> : ITemplateGenerator<TModel>
        where TTemplate : class, new()
        where TModel : class, new()
    {
        /// <summary>
        /// JsonSerializer that will ensure the serialized properties are camel cased, null values are ignored,
        /// line info is removed, and Enums are serialized to strings.
        /// </summary>
        internal JsonSerializer Serializer { get; } = JsonSerializer.Create(new JsonSerializerSettings
        {
            ContractResolver = new LineAwareContractResolver(new CamelCasePropertyNamesContractResolver()),
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Converters = new List<JsonConverter> { new StringEnumConverter() },
        });

        internal abstract TemplateType TemplateType { get; }

        public virtual async Task<JObject> GenerateTemplate(TModel model, CancellationToken cancellationToken)
        {
            EnsureArg.IsNotNull(model, nameof(model));

            TTemplate template = (TTemplate)Activator.CreateInstance(typeof(TTemplate));

            await PopulateTemplate(model, template, cancellationToken);

            var templateContainer = new TemplateContainer
            {
                TemplateType = TemplateType.ToString(),
                Template = JObject.FromObject(template, Serializer),
            };

            return JObject.FromObject(templateContainer, Serializer);
        }

        internal abstract Task PopulateTemplate(TModel model, TTemplate template, CancellationToken cancellationToken);

        /// <summary>
        /// Provides a value for the TypeName property for a Template object.
        /// </summary>
        /// <remarks>
        /// The TypeName property is used to correlate device content templates with FHIR mapping templates.
        /// This method MUST be implemented.
        /// </remarks>
        /// <param name="model">The model that the CalculatedFunctionContentTemplate is generated from.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="string"/></returns>
        public abstract Task<string> GetTypeName(TModel model, CancellationToken cancellationToken);
    }
}
