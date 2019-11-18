// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Common.Rule
{
    /// <summary>
    /// Interface for implementing the specification pattern
    /// </summary>
    /// <typeparam name="T">Type to evaluate</typeparam>
    public interface IRule<in T>
    {
        bool IsTrue(T entity);
    }
}
