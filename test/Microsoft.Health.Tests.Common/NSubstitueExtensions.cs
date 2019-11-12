// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Tests.Common
{
    public static class NSubstitueExtensions
    {
        public static T Mock<T>(this T obj, Action<T> mock)
        {
            mock(obj);
            return obj;
        }
    }
}
