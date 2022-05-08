// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class TemplateExpressionExceptionTests
    {
        [Fact]
        public void When_ExceptionIsCreatedWithNoParams_AppropriateMessageIsProduced()
        {
            var exception = new TemplateExpressionException();
            Assert.NotEmpty(exception.Message);
        }

        [Fact]
        public void When_ExceptionIsCreatedWithEmptyLineInfo_AppropriateMessageIsProduced()
        {
            var exception = new TemplateExpressionException("test", new LineInfo());
            Assert.Equal("test", exception.Message);
        }

        [Fact]
        public void When_ExceptionIsCreatedWithLineInfo_AppropriateMessageIsProduced()
        {
            var exception = new TemplateExpressionException("test", new LineInfo() { LineNumber = 1, LinePosition = 1, });
            Assert.Equal("Line Number: 1, Position: 1. test", exception.Message);
        }
    }
}