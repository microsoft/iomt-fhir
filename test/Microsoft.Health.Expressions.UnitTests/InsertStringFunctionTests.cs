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
    public class InsertStringFunctionTests
    {
        private JmesPath _jmesPath;
        private JmesPath.Expression _expression;

        public InsertStringFunctionTests()
        {
            _jmesPath = new JmesPath();
            _jmesPath.FunctionRepository.Register<InsertStringFunction>();
            _expression = _jmesPath.Parse("insertString(original, toInsert, pos)");
        }

        [Theory]
        [InlineData("myString", "!!", 8, "myString!!")]
        [InlineData("ppy", "ha", 0, "happy")]
        [InlineData("suess", "cc", 2, "success")]
        public void InsertString_Succeeds(string original, string toInsert, long pos, string expected)
        {
            var data = JObject.FromObject(new
            {
                original = original,
                toInsert = toInsert,
                pos = pos,
            });

            var jmesArgument = _expression.Transform(data);
            Assert.Equal(JTokenType.String, jmesArgument.Token.Type);
            Assert.Equal(expected, jmesArgument.Token.Value<string>());
        }

        [Theory]
        [InlineData("myString", "!!", "myString!!")]
        [InlineData("sample ", "text", "sample text")]
        public void InsertString_AtEnd_Succeeds(string original, string toInsert, string expected)
        {
            var data = JObject.FromObject(new
            {
                original = original,
                toInsert = toInsert,
            });

            _expression = _jmesPath.Parse("insertString(original, toInsert, length(original))");

            var jmesArgument = _expression.Transform(data);
            Assert.Equal(JTokenType.String, jmesArgument.Token.Type);
            Assert.Equal(expected, jmesArgument.Token.Value<string>());
        }

        [Theory]
        [InlineData("myString", "!!", 9)]
        [InlineData("ppy", "ha", -1)]
        public void Inserting_OutOfBounds_Throws_Exception(string original, string toInsert, long pos)
        {
            var data = JObject.FromObject(new
            {
                original = original,
                toInsert = toInsert,
                pos = pos,
            });

            Assert.Throws<ArgumentOutOfRangeException>(() => _expression.Transform(data));
        }

        [Fact]
        public void Bad_Data_Throws_Exception()
        {
            var data = JObject.FromObject(new
            {
                original = 6,
                toInsert = "value",
                pos = 0,
            });

            Assert.Throws<Exception>(() => _expression.Transform(data));

            data = JObject.FromObject(new
            {
                original = "text",
                toInsert = 123,
                pos = 0,
            });

            Assert.Throws<Exception>(() => _expression.Transform(data));

            data = JObject.FromObject(new
            {
                original = "text",
                toInsert = "toInsert",
                pos = 1.5,
            });

            Assert.Throws<Exception>(() => _expression.Transform(data));

            data = JObject.FromObject(new
            {
                original = new int[1, 2, 3],
                toInsert = "value",
                pos = 0,
            });

            Assert.Throws<Exception>(() => _expression.Transform(data));

            data = JObject.FromObject(new
            {
                original = new
                {
                    bad = "value",
                },
                toInsert = "value",
                pos = 0,
            });

            Assert.Throws<Exception>(() => _expression.Transform(data));

            data = JObject.FromObject(new
            {
                original = new
                {
                    bad = "value",
                },
                toInsert = "value",
                pos = 0,
            });

            Assert.Throws<Exception>(() => _expression.Transform(data));
        }

        [Fact]
        public void Invalid_Argument_Count_Throws_Exception()
        {
            Assert.Throws<Exception>(() => _jmesPath.Parse("insertString()"));
            Assert.Throws<Exception>(() => _jmesPath.Parse("insertString(original)"));
            Assert.Throws<Exception>(() => _jmesPath.Parse("insertString(original, toInsert)"));
            Assert.Throws<Exception>(() => _jmesPath.Parse("insertString(original, toInsert, pos, extraParam)"));
        }
    }
}
