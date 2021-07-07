// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using DevLab.JmesPath.Functions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Expressions
{
    public static class JmesPathExpressionExtensions
    {
        public static void ValidatePositionalArgumentIsNumber(this JmesPathFunction jmesPathFunction, JmesPathFunctionArgument[] args, int pos)
        {
            ValidatePositionalArgument(jmesPathFunction, args, pos, JTokenType.Integer, JTokenType.Float);
        }

        public static void ValidatePositionalArgument(this JmesPathFunction jmesPathFunction, JmesPathFunctionArgument[] args, int pos, params JTokenType[] types)
        {
            if (!types.Any(t => t == args[pos].Token.Type))
            {
                throw new Exception($"Error: invalid-type, function {jmesPathFunction.Name} expects argument {pos} to be one of the following: {string.Join("|", types)}");
            }
        }

        public static void ValidateExpectedArgumentCount(this JmesPathFunction jmesPathFunction, int expected, int actual)
        {
            if (expected != actual)
            {
                throw new Exception($"Incorrect number of arguments provided to function {jmesPathFunction.Name}. Expected {expected} but recieved {actual}");
            }
        }
    }
}
