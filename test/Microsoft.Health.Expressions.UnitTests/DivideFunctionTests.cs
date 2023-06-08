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
    public class DivideFunctionTests
    {
        private JmesPath _jmesPath;
        private JmesPath.Expression _expression;

        public DivideFunctionTests()
        {
            _jmesPath = new JmesPath();
            _jmesPath.FunctionRepository.Register<DivideFunction>();
            _expression = _jmesPath.Parse("divide(left, right)");
        }

        [Theory]
        [InlineData(100, 25, 4)]
        [InlineData(100, -1, -100)]
        [InlineData(0, long.MaxValue, 0)]
        [InlineData(0, long.MinValue, 0)]
        public void Integer_Division_Succeeds(long leftOperand, long rightOperand, long result)
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
        [InlineData(100, 25, 4)]
        [InlineData(100, -1, -100)]
        [InlineData(0, double.MaxValue, 0)]
        [InlineData(0, double.MinValue, 0)]
        public void Double_Division_Succeeds(double leftOperand, double rightOperand, double result)
        {
            var data = JObject.FromObject(new
            {
                left = leftOperand,
                right = rightOperand,
            });

            var jmesArgument = _expression.Transform(data);
            Assert.Equal(JTokenType.Float, jmesArgument.Token.Type);
            Assert.Equal(result, jmesArgument.Token.Value<double>());
        }

        [Theory]
        [InlineData(100, 25.0, 4)]
        [InlineData(100, -1, -100)]
        [InlineData(0, double.MaxValue, 0)]
        [InlineData(0, double.MinValue, 0)]
        public void Mixed_Division_Succeeds(long leftOperand, double rightOperand, double result)
        {
            var data = JObject.FromObject(new
            {
                left = leftOperand,
                right = rightOperand,
            });

            var jmesArgument = _expression.Transform(data);
            Assert.Equal(JTokenType.Float, jmesArgument.Token.Type);
            Assert.Equal(result, jmesArgument.Token.Value<double>());
        }

        [Fact]
        public void Bad_Data_Throws_Exception()
        {
            var data = JObject.FromObject(new
            {
                left = 100,
                right = 0,
            });

            Assert.Throws<DivideByZeroException>(() => _expression.Transform(data));

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
            Assert.Throws<Exception>(() => _jmesPath.Parse("divide()"));
            Assert.Throws<Exception>(() => _jmesPath.Parse("divide(left"));
            Assert.Throws<Exception>(() => _jmesPath.Parse("divide(left, right, up"));
        }
    }
}
