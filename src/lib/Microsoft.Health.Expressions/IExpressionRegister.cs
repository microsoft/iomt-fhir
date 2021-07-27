// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using DevLab.JmesPath.Interop;

namespace Microsoft.Health.Expressions
{
    public interface IExpressionRegister
    {
        void RegisterExpressions(IRegisterFunctions functionRegister);
    }
}
