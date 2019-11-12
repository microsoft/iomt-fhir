// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Common
{
    public interface IFactory<T>
    {
        T Create();
    }

    public interface IFactory<T, TConfig>
    {
        T Create(TConfig config, params object[] constructorParams);
    }
}
