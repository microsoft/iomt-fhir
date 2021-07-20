// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Text;
using DevLab.JmesPath.Functions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Expressions
{
    public class InsertStringFunction : JmesPathFunction
    {
        public InsertStringFunction()
            : base("insertString", 3)
        {
        }

        public override void Validate(params JmesPathFunctionArgument[] args)
        {
            base.Validate();
            this.ValidatePositionalArgument(args, 0, JTokenType.String);
            this.ValidatePositionalArgument(args, 1, JTokenType.String);
            this.ValidatePositionalArgument(args, 2, JTokenType.Integer);
            this.ValidateExpectedArgumentCount(MinArgumentCount, args.Length);
        }

        public override JToken Execute(params JmesPathFunctionArgument[] args)
        {
            var toModify = args[0].Token.Value<string>();
            var toInsert = args[1].Token.Value<string>();
            var insertPos = args[2].Token.Value<int>();

            var mutableString = new StringBuilder(toModify);

            return new JValue(mutableString.Insert(insertPos, toInsert).ToString());
        }
    }
}
