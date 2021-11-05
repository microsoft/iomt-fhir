// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class DeterministicHashCodeGeneratorTest
    {
        private readonly IHashCodeGenerator _hashCodeGenerator;

        public DeterministicHashCodeGeneratorTest()
        {
            _hashCodeGenerator = new DeterministicHashCodeGenerator();
        }

        [Theory]
        [InlineData("abba", "32")]
        [InlineData("aaabbb", "191")]
        [InlineData("bbbaaa", "33")]
        [InlineData("abcdefghijklmnopqrstuvwxyz", "173")]
        [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", "218")]
        public void When_ValueIsSupplied_HashCode_IsGenerated(string input, string result)
        {
            Assert.Equal(result, _hashCodeGenerator.GenerateHashCode(input));
        }

        [Theory]
        [InlineData("abba")]
        [InlineData("aaabbb")]
        [InlineData("bbbaaa")]
        [InlineData("abcdefghijklmnopqrstuvwxyz")]
        [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        public void When_SameValueIsSupplied_IdenticalHashCode_IsGenerated(string input)
        {
            Assert.Equal(_hashCodeGenerator.GenerateHashCode(input), _hashCodeGenerator.GenerateHashCode(input));
        }
    }
}
