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
    public class AddFunctionTests
    {
        private JmesPath _jmesPath;
        private JmesPath.Expression _expression;

        public AddFunctionTests()
        {
            _jmesPath = new JmesPath();
            _jmesPath.FunctionRepository.Register<AddFunction>();
            _expression = _jmesPath.Parse("add(left, right)");
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(-1, 100, 99)]
        [InlineData(0, long.MaxValue, long.MaxValue)]
        [InlineData(0, long.MinValue, long.MinValue)]
        public void Integer_Addition_Succeeds(long leftOperand, long rightOperand, long result)
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
        [InlineData(0.0, 0.0, 0.0)]
        [InlineData(-1.0, 100.0, 99.0)]
        [InlineData(-1.0, 10.5, 9.5)]
        [InlineData(0, double.MaxValue, double.MaxValue)]
        [InlineData(0, double.MinValue, double.MinValue)]
        public void Double_Addition_Succeeds(double leftOperand, double rightOperand, double result)
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
        [InlineData(0, 0, 0.0)]
        [InlineData(-1, 100.0, 99.0)]
        [InlineData(-1, 10.5, 9.5)]
        [InlineData(0, double.MaxValue, double.MaxValue)]
        [InlineData(0, double.MinValue, double.MinValue)]
        public void Mixed_Addition_Succeeds(long leftOperand, double rightOperand, double result)
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
            Assert.Throws<Exception>(() => _jmesPath.Parse("add()"));
            Assert.Throws<Exception>(() => _jmesPath.Parse("add(left"));
            Assert.Throws<Exception>(() => _jmesPath.Parse("add(left, right, up"));
        }
    }
}
