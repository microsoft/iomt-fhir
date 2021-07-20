// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using DevLab.JmesPath;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Expressions
{
    public class PowerFunctionTests
    {
        private JmesPath _jmesPath;
        private JmesPath.Expression _expression;

        public PowerFunctionTests()
        {
            _jmesPath = new JmesPath();
            _jmesPath.FunctionRepository.Register<PowerFunction>();
            _expression = _jmesPath.Parse("pow(left, right)");
        }

        [Theory]
        [InlineData(0, 0, 1)]
        [InlineData(1, 0, 1)]
        [InlineData(1, 1, 1)]
        [InlineData(10, 2, 100)]
        public void Integer_Power_Succeeds(long leftOperand, long rightOperand, long result)
        {
            var data = JObject.FromObject(new
            {
                left = leftOperand,
                right = rightOperand,
            });

            var jmesArgument = _expression.Transform(data);
            Assert.Equal(JTokenType.Integer, jmesArgument.Token.Type);
            Assert.Equal(result, jmesArgument.Token.Value<long>());
        }

        [Theory]
        [InlineData(0, 0, 1)]
        [InlineData(1, 0, 1)]
        [InlineData(1, 1, 1)]
        [InlineData(10, 2, 100)]
        [InlineData(10, 2.5, 316.2278)]
        [InlineData(100, -2, 0.0001)]
        public void Double_Power_Succeeds(double leftOperand, double rightOperand, double result)
        {
            var data = JObject.FromObject(new
            {
                left = leftOperand,
                right = rightOperand,
            });

            var jmesArgument = _expression.Transform(data);
            Assert.Equal(JTokenType.Float, jmesArgument.Token.Type);
            Assert.Equal(result, jmesArgument.Token.Value<double>(), 4);
        }

        [Fact]
        public void Bad_Data_Throws_Exception()
        {
            var data = JObject.FromObject(new
            {
                left = long.MaxValue,
                right = 10,
            });

            Assert.Throws<OverflowException>(() => _expression.Transform(data));

            data = JObject.FromObject(new
            {
                left = "bad",
                right = 100,
            });

            Assert.Throws<Exception>(() => _expression.Transform(data));

            data = JObject.FromObject(new
            {
                left = new int[1, 2, 3],
                right = 100,
            });

            Assert.Throws<Exception>(() => _expression.Transform(data));

            data = JObject.FromObject(new
            {
                left = new
                {
                    value = 1,
                },
                right = 100,
            });

            Assert.Throws<Exception>(() => _expression.Transform(data));
        }

        [Fact]
        public void Invalid_Argument_Count_Throws_Exception()
        {
            Assert.Throws<Exception>(() => _jmesPath.Parse("pow()"));
            Assert.Throws<Exception>(() => _jmesPath.Parse("pow(left"));
            Assert.Throws<Exception>(() => _jmesPath.Parse("pow(left, right, up"));
        }
    }
}
