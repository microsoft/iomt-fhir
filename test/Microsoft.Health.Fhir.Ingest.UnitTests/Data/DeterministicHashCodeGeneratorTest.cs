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
        [InlineData("abba", 2987040)]
        [InlineData("aaabbb", -1425371071)]
        [InlineData("bbbaaa", -1395789601)]
        [InlineData("abcdefghijklmnopqrstuvwxyz", 958031277)]
        [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", 709306586)]
        public void When_ValueIsSupplied_HashCode_IsGenerated(string input, int result)
        {
            Assert.Equal(result, _hashCodeGenerator.GenerateHashCode(input));
        }
    }
}
