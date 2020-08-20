// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace Microsoft.Health.Tests.Common
{
    public class FileDataAttribute : DataAttribute
    {
        private readonly string[] _filePaths;

        public FileDataAttribute(params string[] filePaths)
        {
            _filePaths = filePaths;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            ICollection<string> fileContents = new List<string>();
            foreach (string filePath in _filePaths)
            {
                var path = Path.IsPathRooted(filePath) ? filePath : Path.Combine(Directory.GetCurrentDirectory(), filePath);

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"File {path} not found.");
                }

                fileContents.Add(File.ReadAllText(path));
            }

            yield return fileContents.ToArray();
        }
    }
}
