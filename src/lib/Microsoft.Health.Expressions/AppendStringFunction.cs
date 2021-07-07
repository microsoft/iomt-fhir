// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Text;
using DevLab.JmesPath.Functions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Expressions
{
    public class AppendStringFunction : JmesPathFunction
    {
        public AppendStringFunction()
            : base("appendString", 2)
        {
        }

        public override void Validate(params JmesPathFunctionArgument[] args)
        {
            base.Validate();
            this.ValidatePositionalArgument(args, 0, JTokenType.String);
            this.ValidatePositionalArgument(args, 1, JTokenType.String);
            this.ValidateExpectedArgumentCount(MinArgumentCount, args.Length);
        }

        public override JToken Execute(params JmesPathFunctionArgument[] args)
        {
            var toModify = args[0].Token.Value<string>();
            var toAppend = args[1].Token.Value<string>();

            var mutableString = new StringBuilder(toModify);

            return new JValue(mutableString.Append(toAppend).ToString());
        }
    }
}
