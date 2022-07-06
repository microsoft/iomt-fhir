// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template.Generator
{
    public interface ITemplateCollectionGenerator<TModel>
        where TModel : class
    {
        /// <summary>
        /// Generates a collection of Templates formatted specifically for IoMT Connectors.
        /// </summary>
        /// <param name="model">The model used to generate the Templates.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="JObject"/></returns>
        Task<JObject> GenerateTemplateCollection(IEnumerable<TModel> model, CancellationToken cancellationToken);
    }
}
