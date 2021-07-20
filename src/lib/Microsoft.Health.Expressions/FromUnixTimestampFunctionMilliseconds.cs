// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using DevLab.JmesPath.Functions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Expressions
{
    public class FromUnixTimestampFunctionMilliseconds : JmesPathFunction
    {
        public FromUnixTimestampFunctionMilliseconds()
            : base("fromUnixTimestampMs", 1)
        {
        }

        public override void Validate(params JmesPathFunctionArgument[] args)
        {
            base.Validate();
            this.ValidatePositionalArgument(args, 0, JTokenType.Integer);
            this.ValidateExpectedArgumentCount(MinArgumentCount, args.Length);
        }

        public override JToken Execute(params JmesPathFunctionArgument[] args)
        {
            var toConvert = args[0].Token.Value<long>();

            return new JValue(DateTimeOffset.FromUnixTimeMilliseconds(toConvert).UtcDateTime);
        }
    }
}
