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
    /// This abstract class provides a base that can be used to generate templates of type CalculatedFunctionContentTemplate.
    /// </summary>
    /// <typeparam name="TModel">The class that is used to generate the template.</typeparam>
    public abstract class CalculatedContentTemplateGenerator<TModel> : TemplateGenerator<CalculatedFunctionContentTemplate, TModel>
        where TModel : class, new()
    {
        internal override TemplateType TemplateType => TemplateType.CalculatedContent;

        internal override async Task PopulateTemplate(TModel model, CalculatedFunctionContentTemplate template, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>()
            {
                Task.Run(async () => template.TypeName = await GetTypeName(model, cancellationToken)),
                Task.Run(async () => template.TypeMatchExpression = await GetTypeMatchExpression(model, cancellationToken)),
                Task.Run(async () => template.DeviceIdExpression = await GetDeviceIdExpression(model, cancellationToken)),
                Task.Run(async () => template.TimestampExpression = await GetTimestampExpression(model, cancellationToken)),
                Task.Run(async () => template.PatientIdExpression = await GetPatientIdExpression(model, cancellationToken)),
                Task.Run(async () => template.EncounterIdExpression = await GetEncounterIdExpression(model, cancellationToken)),
                Task.Run(async () => template.CorrelationIdExpression = await GetCorrelationIdExpression(model, cancellationToken)),
                Task.Run(async () => template.Values = await GetValues(model, cancellationToken)),
            };

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Provides a value for the TypeMatchExpression property for a CalculatedFunctionContentTemplate object.
        /// </summary>
        /// <remarks>
        /// The TypeMatchExpression property is used to identify JSON data that should be processed using the given template.
        /// Expressions can be provided in JSONPath or JMESPath formats.
        /// This method MUST be implemented.
        /// </remarks>
        /// <param name="model">The model that the CalculatedFunctionContentTemplate is generated from.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="TemplateExpression"/></returns>
        public abstract Task<TemplateExpression> GetTypeMatchExpression(TModel model, CancellationToken cancellationToken);

        /// <summary>
        /// Provides a value for the DeviceIdExpression property for a CalculatedFunctionContentTemplate object.
        /// </summary>
        /// <remarks>
        /// The DeviceIdExpression property is used to identify where the device id can be found in JSON data.
        /// Device Ids are used to lookup and/or create Device Resources in FHIR and the Device Resource is linked
        /// to the resulting Observation Resource.
        /// Expressions can be provided in JSONPath or JMESPath formats.
        /// This method MUST be implemented.
        /// </remarks>
        /// <param name="model">The model that the CalculatedFunctionContentTemplate is generated from.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="TemplateExpression"/></returns>
        public abstract Task<TemplateExpression> GetDeviceIdExpression(TModel model, CancellationToken cancellationToken);

        /// <summary>
        /// Provides a value for the TimestampExpression property for a CalculatedFunctionContentTemplate object.
        /// </summary>
        /// <remarks>
        /// The TimestampExpression property is used to identify where the timestamp can be found in JSON data.
        /// Timestamps are used to set the time that a device measurement occurred. The timestamp is recorded to
        /// the Observation Resource.
        /// Expressions can be provided in JSONPath or JMESPath formats.
        /// This method MUST be implemented.
        /// </remarks>
        /// <param name="model">The model that the CalculatedFunctionContentTemplate is generated from.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="TemplateExpression"/></returns>
        public abstract Task<TemplateExpression> GetTimestampExpression(TModel model, CancellationToken cancellationToken);

        /// <summary>
        /// Provides a value for the PatientIdExpression property for a CalculatedFunctionContentTemplate object.
        /// </summary>
        /// <remarks>
        /// The PatientIdExpression property is used to identify where a patient id can be found in JSON data.
        /// Patient Ids are used when the IoMT Connector is in 'Create' mode to create a Patient Resource, and
        /// Link the Patient Resource to a Device Resource, and subsequently to the Observation Resource.
        /// Expressions can be provided in JSONPath or JMESPath formats.
        /// Implementation of this method is optional.
        /// </remarks>
        /// <param name="model">The model that the CalculatedFunctionContentTemplate is generated from.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="TemplateExpression"/></returns>
        public virtual Task<TemplateExpression> GetPatientIdExpression(TModel model, CancellationToken cancellationToken)
        {
            return Task.FromResult<TemplateExpression>(null);
        }

        /// <summary>
        /// Provides a value for the EncounterIdExpression property for a CalculatedFunctionContentTemplate object.
        /// </summary>
        /// <remarks>
        /// The EncounterIdExpression property is used to identify where an encounter id can be found in JSON data.
        /// Encounter Ids are used to link the Observation resource to an Encounter Resource.
        /// Expressions can be provided in JSONPath or JMESPath formats.
        /// Implementation of this method is optional.
        /// </remarks>
        /// <param name="model">The model that the CalculatedFunctionContentTemplate is generated from.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="TemplateExpression"/></returns>
        public virtual Task<TemplateExpression> GetEncounterIdExpression(TModel model, CancellationToken cancellationToken)
        {
            return Task.FromResult<TemplateExpression>(null);
        }

        /// <summary>
        /// Provides a value for the CorrelationIdExpression property for a CalculatedFunctionContentTemplate object.
        /// </summary>
        /// <remarks>
        /// The CorrelationIdExpression property is used to identify where a correlation id can be found in JSON data.
        /// Correlation Ids can be used to group all measurements that share the same device, type, and correlation id.
        /// This should also use the SampledData value type so all values with in the Observation period can be represented.
        /// Expressions can be provided in JSONPath or JMESPath formats.
        /// Implementation of this method is optional.
        /// </remarks>
        /// <param name="model">The model that the CalculatedFunctionContentTemplate is generated from.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="TemplateExpression"/></returns>
        public virtual Task<TemplateExpression> GetCorrelationIdExpression(TModel model, CancellationToken cancellationToken)
        {
            return Task.FromResult<TemplateExpression>(null);
        }

        /// <summary>
        /// Provides a value for the Values property for a CalculatedFunctionContentTemplate object.
        /// </summary>
        /// <remarks>
        /// The Values property is a list of CalculatedFunctionValueExpression used to identify where measurement values can be found in JSON data.
        /// Values are measurements taken from a device. There can be more that one value for a single Observation Resource, for example
        /// blood pressure measurements might contain diastolic and systolic values.
        /// Implementation of this method is optional.
        /// </remarks>
        /// <param name="model">The model that the CalculatedFunctionContentTemplate is generated from.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="CalculatedFunctionValueExpression"/></returns>
        public virtual Task<IList<CalculatedFunctionValueExpression>> GetValues(TModel model, CancellationToken cancellationToken)
        {
            return Task.FromResult<IList<CalculatedFunctionValueExpression>>(null);
        }
    }
}