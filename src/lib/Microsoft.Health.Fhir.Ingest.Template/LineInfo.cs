// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class LineInfo : ILineInfo
    {
        /// <summary>
        /// Returns a default, empty ILineInfo singleton.
        /// </summary>
        public static readonly ILineInfo Default = new EmptyLineInfo();

        public virtual int LineNumber { get; set; } = -1;

        public virtual int LinePosition { get; set; } = -1;

        public bool HasLineInfo()
        {
            return LineNumber >= 0 && LinePosition >= 0;
        }

        private class EmptyLineInfo : ILineInfo
        {
            public int LineNumber { get => -1; set => throw new System.NotImplementedException(); }

            public int LinePosition { get => -1; set => throw new System.NotImplementedException(); }

            public bool HasLineInfo() => false;
        }
    }
}