// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template.Generator
{
    public interface ITemplateGenerator<TModel>
        where TModel : class
    {
        /// <summary>
        /// Generates a Template and converts it to a JObject.
        /// </summary>
        /// <param name="model">The model used to generate the Template.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="JObject"/></returns>
        Task<JObject> GenerateTemplate(TModel model, CancellationToken cancellationToken);
    }
}
