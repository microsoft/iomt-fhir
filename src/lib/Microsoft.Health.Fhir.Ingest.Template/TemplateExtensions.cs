// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Template.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public static class TemplateExtensions
    {
        public static T ToValidTemplate<T>(this JToken token)
        {
            EnsureArg.IsNotNull(token, nameof(token));

            var errors = new List<ErrorContext>();

            var jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>() { new LineNumberJsonConverter() },
                MissingMemberHandling = MissingMemberHandling.Error,
                Error = (sender, args) =>
                {
                    errors.Add(args.ErrorContext);
                    args.ErrorContext.Handled = true;
                },
                TypeNameHandling = TypeNameHandling.None,
            });

            T obj = token.ToObject<T>(jsonSerializer);

            if (errors.Any())
            {
                throw new AggregateException($"Failed to deserialize the {typeof(T).Name} content", errors.Select(e => ConvertErrorContext(e)));
            }

            return obj;
        }

        private static InvalidTemplateException ConvertErrorContext(ErrorContext errorContext)
        {
            // Remove the path information if it was empty.
            var errMessage = errorContext.Error.Message;
            var messageLength = errMessage.LastIndexOf("Path", StringComparison.OrdinalIgnoreCase);
            messageLength = messageLength >= 0 ? messageLength : errMessage.Length;
            errMessage = errMessage.Substring(0, messageLength);

            var lineInfo = new LineInfo();
            if (errorContext.Error is JsonSerializationException e)
            {
                lineInfo.LineNumber = e.LineNumber;
                lineInfo.LinePosition = e.LinePosition;
            }

            return new InvalidTemplateException(errMessage, lineInfo);
        }
    }
}
