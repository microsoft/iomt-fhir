// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Common
{
    public static class DisposeExtensions
    {
        /// <summary>
        /// Tries to dispose the provided managed object if the reference provided is not null.
        /// </summary>
        /// <param name="managedObj">Object to dispose.</param>
        /// <returns>True if object is disposed, false if the reference is null and dispose was not invoked.</returns>
        public static bool TryDispose(this IDisposable managedObj)
        {
            if (managedObj == null)
            {
                return false;
            }

            managedObj.Dispose();
            return true;
        }
    }
}
