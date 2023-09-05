// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Text;
using EnsureThat;

namespace Microsoft.Health.Logging
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Returns a concatenation of this Exceptions message along with the messages of all Inner Exceptions.
        /// </summary>
        /// <param name="ex">The root exception</param>
        /// <returns>A concatenation of the root and inner exceptions</returns>
        public static string JoinInnerMessages(this Exception ex)
        {
            EnsureArg.IsNotNull(ex, nameof(ex));

            var sb = new StringBuilder(ex.Message);

            var innerException = ex.InnerException;
            int exceptionCount = 1;

            while (innerException != null)
            {
                sb.Append($"\n{++exceptionCount}:{innerException.Message}");
                innerException = innerException.InnerException;
            }

            return sb.ToString();
        }
    }
}
