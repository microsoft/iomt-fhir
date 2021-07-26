// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using DevLab.JmesPath;
using Microsoft.Health.Fhir.Ingest.Template;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class JmesPathExpressionEvaluatorTests
    {
        private JmesPathExpressionEvaluator _singleValueExpressionEvaluator;
        private JmesPathExpressionEvaluator _projectedExpressionEvaluator;
        private JmesPath _jmesPath;

        public JmesPathExpressionEvaluatorTests()
        {
            _jmesPath = new JmesPath();
            _singleValueExpressionEvaluator = new JmesPathExpressionEvaluator(_jmesPath, "testProperty");

            _projectedExpressionEvaluator = new JmesPathExpressionEvaluator(_jmesPath, "property[].name");
        }

        [Fact]
        public void When_InvalidParametersProvided_ExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new JmesPathExpressionEvaluator(null, null));
            Assert.Throws<ArgumentNullException>(() => new JmesPathExpressionEvaluator(_jmesPath, null));
            Assert.Throws<ArgumentException>(() => new JmesPathExpressionEvaluator(_jmesPath, string.Empty));
        }

        [Theory]
        [InlineData(".")]
        [InlineData("?")]
        [InlineData("[?]")]
        public void When_InvalidExpressionProvided_ExceptionIsThrown(string badExpression)
        {
            var exception = Assert.Throws<TemplateExpressionException>(() => new JmesPathExpressionEvaluator(_jmesPath, badExpression));
            Assert.StartsWith("The following JmesPath expression could not be parsed", exception.Message);
        }

        [Fact]
        public void When_SelectSingleToken_And_SingleTokenExtracted_Succeeds()
        {
            var data = JObject.FromObject(new { testProperty = new string[] { "a", "b" } });
            var result = _singleValueExpressionEvaluator.SelectToken(data);
            Assert.NotNull(result);
            Assert.IsType<JArray>(result);
        }

        [Fact]
        public void When_SelectSingleToken_And_NoTokensAreExtracted_Succeeds()
        {
            var data = JObject.FromObject(new { });
            var token = _singleValueExpressionEvaluator.SelectToken(data);
            Assert.Null(token);
        }

        [Fact]
        public void When_SelectSingleToken_And_MultipleTokensAreExtracted_ExceptionIsThrown()
        {
            var data = JObject.FromObject(new
            {
                property = new[]
                {
                    new { name = "value1" },
                    new { name = "value2" },
                },
            });

            Assert.Throws<TemplateExpressionException>(() => _projectedExpressionEvaluator.SelectToken(data));
        }

        [Fact]
        public void When_SelectMultipleTokens_And_SingleTokenExtracted_Succeeds()
        {
            var data = JObject.FromObject(new { testProperty = new string[] { "a", "b" } });
            var result = _singleValueExpressionEvaluator.SelectTokens(data);
            Assert.NotNull(result);
            Assert.Collection(
                result,
                i =>
                {
                    Assert.Collection(
                        i.Values<string>(),
                        j =>
                        {
                            Assert.Equal("a", j);
                        },
                        j =>
                        {
                            Assert.Equal("b", j);
                        });
                });
        }

        [Fact]
        public void When_SelectMultipleTokens_And_NoTokensAreExtracted_Succeeds()
        {
            var data = JObject.FromObject(new { });
            var token = _singleValueExpressionEvaluator.SelectTokens(data);
            Assert.Empty(token);
        }

        [Fact]
        public void When_SelectMultipleTokens_And_MultipleTokensAreExtracted_Succeeds()
        {
            var data = JObject.FromObject(new
            {
                property = new[]
                {
                    new { name = "value1" },
                    new { name = "value2" },
                },
            });

            var tokens = _projectedExpressionEvaluator.SelectTokens(data);

            Assert.Collection(
                tokens,
                i =>
                {
                    Assert.Equal("value1", i.Value<string>());
                },
                i =>
                {
                    Assert.Equal("value2", i.Value<string>());
                });
        }
    }
}
