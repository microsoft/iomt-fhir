// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Tools.DataMapper.Controllers.Apis
{
    /// <summary>
    /// Utilities.
    /// </summary>
    public class Utility
    {
        /// <summary>
        /// Check that the content is in valid JSON format.
        /// </summary>
        /// <param name="jsonContent">Content to check.</param>
        /// <param name="message">Violations.</param>
        /// <returns>True if the content is valid JSON.</returns>
        public static bool IsValidJson(string jsonContent, out string message)
        {
            try
            {
                JContainer.Parse(jsonContent);
                message = null;
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }
    }
}
