// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using DevLab.JmesPath.Functions;
using DevLab.JmesPath.Interop;
using Microsoft.Health.Logging.Telemetry;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Expressions.UnitTests
{
    public class AssemblyExpressionRegisterTests
    {
        private IRegisterFunctions _registerFunctions;
        private ITelemetryLogger _telemetryLogger;

        public AssemblyExpressionRegisterTests()
        {
            _registerFunctions = Substitute.For<IRegisterFunctions>();
            _telemetryLogger = Substitute.For<ITelemetryLogger>();
        }

        [Fact]
        public void AllExpressionsAreLoadedFromAssembly()
        {
            var register = new AssemblyExpressionRegister(typeof(IExpressionRegister).Assembly, _telemetryLogger);
            register.RegisterExpressions(_registerFunctions);
            _registerFunctions.Received(8).Register(Arg.Any<string>(), Arg.Any<JmesPathFunction>());
        }

        [Fact]
        public void NoExpressionsAreLoadedFromAssembly()
        {
            var register = new AssemblyExpressionRegister(typeof(Assert).Assembly, _telemetryLogger);
            register.RegisterExpressions(_registerFunctions);
            _registerFunctions.Received(0).Register(Arg.Any<string>(), Arg.Any<JmesPathFunction>());
        }
    }
}
