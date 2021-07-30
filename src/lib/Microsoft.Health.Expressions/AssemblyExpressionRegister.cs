// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevLab.JmesPath.Functions;
using DevLab.JmesPath.Interop;
using EnsureThat;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Expressions
{
    /// <summary>
    /// Discovers all JmesPathFunctions within the given assembly and registers them with a function register
    /// </summary>
    public class AssemblyExpressionRegister : IExpressionRegister
    {
        private Assembly _assembly;
        private ITelemetryLogger _logger;

        public AssemblyExpressionRegister(
            Assembly assembly,
            ITelemetryLogger logger)
        {
            _assembly = EnsureArg.IsNotNull(assembly, nameof(assembly));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
        }

        public void RegisterExpressions(IRegisterFunctions functionRegister)
        {
            EnsureArg.IsNotNull(functionRegister, nameof(functionRegister));

            foreach (JmesPathFunction function in GetJmesPathFunctions())
            {
                functionRegister.Register(function.Name, function);
                _logger.LogTrace($"Registered function '{function.GetType().Name}' with name '{function.Name}'");
            }
        }

        private IEnumerable<JmesPathFunction> GetJmesPathFunctions()
        {
            var targetType = typeof(JmesPathFunction);
            return _assembly
                .GetTypes()
                .Where(type => targetType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                .Select(type => Activator.CreateInstance(type) as JmesPathFunction)
                .ToList();
        }
    }
}
