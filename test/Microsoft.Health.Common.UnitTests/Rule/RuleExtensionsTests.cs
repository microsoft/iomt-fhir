// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using NSubstitute;
using Xunit;

namespace Microsoft.Health.Common.Rule
{
    public class RuleExtensionsTests
    {
        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(false, false, true)]
        [InlineData(true, true, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(false, false, false)]
        public void GivenTwoRules_WhenAnd_ThenAndResult_Test(bool rule1Result, bool rule2Result, bool shortCircuit)
        {
            var expected = rule1Result && rule2Result;

            var rule1 = Substitute.For<IRule<object>>();
            rule1.IsTrue(Arg.Any<object>()).Returns(rule1Result);
            var rule2 = Substitute.For<IRule<object>>();
            rule2.IsTrue(Arg.Any<object>()).Returns(rule2Result);

            Assert.Equal(expected, rule1.And(rule2, shortCircuit).IsTrue(null));

            rule1.ReceivedWithAnyArgs(1).IsTrue(null);
            if (shortCircuit && !rule1Result)
            {
                rule2.DidNotReceiveWithAnyArgs().IsTrue(null);
            }
            else
            {
                rule2.ReceivedWithAnyArgs(1).IsTrue(null);
            }
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(false, false, true)]
        [InlineData(true, true, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(false, false, false)]
        public void GivenTwoRules_WhenOr_ThenOrResult_Test(bool rule1Result, bool rule2Result, bool shortCircuit)
        {
            var expected = rule1Result || rule2Result;

            var rule1 = Substitute.For<IRule<object>>();
            rule1.IsTrue(Arg.Any<object>()).Returns(rule1Result);
            var rule2 = Substitute.For<IRule<object>>();
            rule2.IsTrue(Arg.Any<object>()).Returns(rule2Result);

            Assert.Equal(expected, rule1.Or(rule2, shortCircuit).IsTrue(null));

            rule1.ReceivedWithAnyArgs(1).IsTrue(null);
            if (shortCircuit && rule1Result)
            {
                rule2.DidNotReceiveWithAnyArgs().IsTrue(null);
            }
            else
            {
                rule2.ReceivedWithAnyArgs(1).IsTrue(null);
            }
        }
    }
}
