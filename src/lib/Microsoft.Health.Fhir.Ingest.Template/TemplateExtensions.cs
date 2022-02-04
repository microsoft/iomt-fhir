// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public static class TemplateExtensions
    {
        public static T ToValidTemplate<T>(this JToken token)
        {
            EnsureArg.IsNotNull(token, nameof(token));

            var errors = new List<Exception>();

            var jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                Error = (sender, args) =>
                {
                    errors.Add(args.ErrorContext.Error);
                    args.ErrorContext.Handled = true;
                },
                TypeNameHandling = TypeNameHandling.None,
            });

            T obj = token.ToObject<T>(jsonSerializer);

            if (errors.Any())
            {
                string errorMessage = string.Join(string.Empty, errors.Select(e => FormatErrorMessage(e.Message)));
                throw new InvalidTemplateException($"Failed to deserialize the {typeof(T).Name} content: {errorMessage}");
            }

            return obj;
        }

        private static string FormatErrorMessage(string errMessage)
        {
            // Remove the path information if it was empty.
            errMessage = errMessage.Replace("Path ''.", string.Empty);
            errMessage = "\n  " + errMessage;
            return errMessage;
        }
    }
}
