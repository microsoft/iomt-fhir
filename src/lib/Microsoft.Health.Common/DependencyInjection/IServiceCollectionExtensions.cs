// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Health.Common.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Binds a specific configuration section to an instance of TConfiguration and adds the object as a singleton to the service collection.
        /// </summary>
        /// <remarks>
        /// The type name of TConfiguration is assumed to be the configuration section.
        /// </remarks>
        /// <typeparam name="TConfiguration">The object type the configuration section should be bound to.</typeparam>
        /// <param name="services"><see cref="IServiceCollection"/>The service collection the object should be added to.</param>
        /// <param name="configuration"><see cref="IConfiguration"/>The configuration containing the section to be bound.</param>
        /// <returns><see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddConfiguration<TConfiguration>(this IServiceCollection services, IConfiguration configuration)
            where TConfiguration : class, new()
        {
            EnsureArg.IsNotNull(configuration, nameof(configuration));

            var config = new TConfiguration();
            configuration.GetSection(config.GetType().Name).Bind(config);
            services.AddSingleton(config);
            return services;
        }
    }
}
