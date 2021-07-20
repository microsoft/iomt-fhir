// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using DevLab.JmesPath.Functions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Expressions
{
    public class DivideFunction : JmesPathFunction
    {
        public DivideFunction()
            : base("divide", 2)
        {
        }

        public override void Validate(params JmesPathFunctionArgument[] args)
        {
            base.Validate();
            this.ValidatePositionalArgumentIsNumber(args, 0);
            this.ValidatePositionalArgumentIsNumber(args, 1);
            this.ValidateExpectedArgumentCount(MinArgumentCount, args.Length);
        }

        public override JToken Execute(params JmesPathFunctionArgument[] args)
        {
            if (args[0].Token.Type == JTokenType.Float || args[1].Token.Type == JTokenType.Float)
            {
                return new JValue(args[0].Token.Value<double>() / args[1].Token.Value<double>());
            }
            else
            {
                return new JValue(args[0].Token.Value<long>() / args[1].Token.Value<long>());
            }
        }
    }
}
