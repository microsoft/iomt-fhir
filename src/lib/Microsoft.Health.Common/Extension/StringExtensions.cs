// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;

namespace Microsoft.Health.Common.Extension
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns a value indicating whether a specified substring occurs within this string, using the provided comparison rule.
        ///
        /// Note: This method is provided on the String object in .Net Core 2.1 and later.
        /// </summary>
        /// <param name="stringToSearch">The string to search</param>
        /// <param name="stringToSeek">The string to seek</param>
        /// <param name="comparison">The comparison rule</param>
        /// <returns>True if the string to seek is contained within the string to search.</returns>
        public static bool Contains(this string stringToSearch, string stringToSeek, StringComparison comparison)
        {
            EnsureArg.IsNotNull(stringToSearch, nameof(stringToSearch));
            return stringToSearch.IndexOf(stringToSeek, comparison) >= 0;
        }
    }
}
