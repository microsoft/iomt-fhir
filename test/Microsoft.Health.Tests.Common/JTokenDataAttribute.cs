// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Xunit.Sdk;

namespace Microsoft.Health.Tests.Common
{
    public class JTokenDataAttribute : DataAttribute
    {
        private readonly string _filePath;

        public JTokenDataAttribute(string filePath)
        {
            _filePath = filePath;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var path = Path.IsPathRooted(_filePath) ? _filePath : Path.Combine(Directory.GetCurrentDirectory(), _filePath);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File {path} not found.");
            }

            foreach (var token in JArray.Parse(File.ReadAllText(path)))
            {
                yield return new object[] { token };
            }
        }
    }
}
