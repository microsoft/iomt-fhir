// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Ingest.Template.Generator
{
    /// <summary>
    /// This abstract class provides a base that can be used to generate templates of type CodeValueFhirTemplate.
    /// </summary>
    /// <typeparam name="TModel">The class that is used to generate the template. Must be type of <see cref="Template"/>.</typeparam>
    public abstract class CodeValueFhirTemplateGenerator<TModel> : TemplateGenerator<CodeValueFhirTemplate, TModel>
        where TModel : Template, new()
    {
        internal override TemplateType TemplateType => TemplateType.CodeValueFhir;

        internal override async Task PopulateTemplate(TModel model, CodeValueFhirTemplate template, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>()
            {
                Task.Run(async () => template.Category = await GetCategory(model, cancellationToken)),
                Task.Run(async () => template.Codes = await GetCodes(model, cancellationToken)),
                Task.Run(async () => template.Value = await GetValue(model, cancellationToken)),
                Task.Run(async () => template.Components = await GetComponents(model, cancellationToken)),
            };

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Provides a value for the TypeName property for the CodeValueFhirTemplate TypeName property.
        /// </summary>
        /// <remarks>
        /// The TypeName property is used to correlate device content templates with FHIR mapping templates,
        /// the TModel and CodeValueFhirTemplate TypeName properties should always be the same.
        /// If this method is not overridden, model.TypeName will be used.
        /// </remarks>
        /// <param name="model">The model that the CodeValueFhirTemplate is generated from.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="string"/></returns>
        public override Task<IEnumerable<string>> GetTypeNames(TModel model, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<string>>(new List<string>() { model.TypeName });
        }

        /// <summary>
        /// Provides a list of FHIR Codes for the CodeValueFhirTemplate Code property.
        /// </summary>
        /// <remarks>
        /// At least 1 code is required for the CodeValueFhirTemplate to pass validation. Codes are used
        /// to define the Observation Resource type in the FHIR service.
        /// This method MUST be implemented.
        /// </remarks>
        /// <param name="model">The model that the CodeValueFhirTemplate is generated from.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="FhirCode"/></returns>
        public abstract Task<IList<FhirCode>> GetCodes(TModel model, CancellationToken cancellationToken);

        /// <summary>
        /// Provides a list of FhirCodeableConcepts for the CodeValueFhirTemplate Category property.
        /// </summary>
        /// <remarks>
        /// Used to categorize Observation Resources in the FHIR service.
        /// Implementation of this method is optional.
        /// </remarks>
        /// <param name="model">The model that the CodeValueFhirTemplate is generated from.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="FhirCodeableConcept"/></returns>
        public virtual Task<IList<FhirCodeableConcept>> GetCategory(TModel model, CancellationToken cancellationToken)
        {
            return Task.FromResult<IList<FhirCodeableConcept>>(null);
        }

        /// <summary>
        /// Provides a FhirValueType for the CodeValueFhirTemplate Value property.
        /// </summary>
        /// <remarks>
        /// Used to save a value to an Observation Resource. Valid Types are <see cref="CodeableConceptFhirValueType"/>
        /// <see cref="QuantityFhirValueType"/>, <see cref="SampledDataFhirValueType"/>, and <see cref="StringFhirValueType"/>.
        /// Implementation of this method is optional.
        /// </remarks>
        /// <param name="model">The model that the CodeValueFhirTemplate is generated from.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="FhirValueType"/></returns>
        public virtual Task<FhirValueType> GetValue(TModel model, CancellationToken cancellationToken)
        {
            return Task.FromResult<FhirValueType>(null);
        }

        /// <summary>
        /// Provides a list of CodeValueMappings for the CodeValueFhirTemplate Components property.
        /// </summary>
        /// <remarks>
        /// Components are used when an Observation Resource contains multiple related values. An example of
        /// this would be blood pressure, one component would contain the systolic measurement while the other
        /// component would contain the diastolic measurement.
        /// Implementation of this method is optional.
        /// </remarks>
        /// <param name="model">The model that the CodeValueFhirTemplate is generated from.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="CodeValueMapping"/></returns>
        public virtual Task<IList<CodeValueMapping>> GetComponents(TModel model, CancellationToken cancellationToken)
        {
            return Task.FromResult<IList<CodeValueMapping>>(null);
        }
    }
}
