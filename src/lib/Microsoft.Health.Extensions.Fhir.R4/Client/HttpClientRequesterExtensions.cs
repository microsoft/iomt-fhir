// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using EnsureThat;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Utility;

namespace Microsoft.Health.Extensions.Fhir.Client
{
    public static class HttpClientRequesterExtensions
    {
        public static EntryResponse ToEntryResponse(this HttpResponseMessage response, byte[] body)
        {
            EnsureArg.IsNotNull(response, nameof(response));

            var result = new EntryResponse
            {
                Status = ((int)response.StatusCode).ToString(),
                ResponseUri = response.RequestMessage.RequestUri,
                Body = body,
                Location = response.Headers.Location?.OriginalString ?? response.Content.Headers.ContentLocation?.OriginalString,
                LastModified = response.Content.Headers.LastModified,
                Etag = response.Headers.ETag?.Tag.Trim('\"'),
                ContentType = response.Content.Headers.ContentType?.MediaType,
            };
            SetHeaders(result, response.Headers);

            return result;
        }

        public static void SetHeaders(EntryResponse interaction, HttpResponseHeaders headers)
        {
            EnsureArg.IsNotNull(interaction, nameof(interaction));
            EnsureArg.IsNotNull(headers, nameof(headers));

            foreach (var header in headers)
            {
                interaction.Headers.Add(header.Key, header.Value.ToList().FirstOrDefault());
            }
        }

        public static HttpRequestMessage ToHttpRequestMessage(this EntryRequest entry, Uri baseUrl, FhirClientSettings settings)
        {
            EnsureArg.IsNotNull(entry, nameof(entry));
            EnsureArg.IsNotNull(baseUrl, nameof(baseUrl));
            EnsureArg.IsNotNull(settings, nameof(settings));

            if (entry.RequestBodyContent != null && !(entry.Method == HTTPVerb.POST || entry.Method == HTTPVerb.PUT || entry.Method == HTTPVerb.PATCH))
            {
                throw Error.InvalidOperation("Cannot have a body on an Http " + entry.Method.ToString());
            }

            // Create an absolute uri when the interaction.Url is relative.
            var uri = new Uri(entry.Url, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri)
            {
                uri = HttpUtil.MakeAbsoluteToBase(uri, baseUrl);
            }

            var location = new RestUrl(uri);

            if (settings.UseFormatParameter)
            {
                location.AddParam(HttpUtil.RESTPARAM_FORMAT, ContentType.BuildFormatParam(settings.PreferredFormat));
            }

            var request = new HttpRequestMessage(GetMethod(entry.Method), location.Uri);

            request.Headers.Add("User-Agent", ".NET FhirClient for FHIR " + entry.Agent);

            if (!settings.UseFormatParameter && !string.IsNullOrEmpty(entry.Headers.Accept))
            {
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(entry.Headers.Accept));
            }

            if (entry.Headers.IfMatch != null)
            {
                request.Headers.Add("If-Match", entry.Headers.IfMatch);
            }

            if (entry.Headers.IfNoneMatch != null)
            {
                request.Headers.Add("If-None-Match", entry.Headers.IfNoneMatch);
            }

            if (entry.Headers.IfModifiedSince != null)
            {
                request.Headers.IfModifiedSince = entry.Headers.IfModifiedSince.Value.UtcDateTime;
            }

            if (entry.Headers.IfNoneExist != null)
            {
                request.Headers.Add("If-None-Exist", entry.Headers.IfNoneExist);
            }

            var interactionType = entry.Type;

            bool CanHaveReturnPreference() => entry.Type == InteractionType.Create ||
              entry.Type == InteractionType.Update ||
              entry.Type == InteractionType.Patch;

            if (CanHaveReturnPreference() && settings.PreferredReturn != null)
            {
                if (settings.PreferredReturn == Prefer.RespondAsync)
                {
                    request.Headers.Add("Prefer", PrimitiveTypeConverter.ConvertTo<string>(settings.PreferredReturn));
                }
                else
                {
                    request.Headers.Add("Prefer", "return=" + PrimitiveTypeConverter.ConvertTo<string>(settings.PreferredReturn));
                }
            }
            else if (interactionType == InteractionType.Search && settings.PreferredParameterHandling != null)
            {
                List<string> preferHeader = new List<string>();
                if (settings.PreferredParameterHandling.HasValue)
                {
                    preferHeader.Add("handling=" + settings.PreferredParameterHandling.GetLiteral());
                }

                if (settings.PreferredReturn.HasValue && settings.PreferredReturn == Prefer.RespondAsync)
                {
                    preferHeader.Add(settings.PreferredReturn.GetLiteral());
                }

                if (preferHeader.Count > 0)
                {
                    request.Headers.Add("Prefer", string.Join(", ", preferHeader));
                }
            }

            if (entry.RequestBodyContent != null)
            {
                SetContentAndContentType(request, entry.RequestBodyContent, entry.ContentType);
            }

            return request;
        }

        /// <summary>
        /// Converts bundle http verb to corresponding <see cref="HttpMethod"/>.
        /// </summary>
        /// <param name="verb"><see cref="HTTPVerb"/> specified by input bundle.</param>
        /// <returns><see cref="HttpMethod"/> corresponding to verb specified in input bundle.</returns>
        private static HttpMethod GetMethod(HTTPVerb? verb)
        {
            switch (verb)
            {
                case HTTPVerb.GET:
                    return HttpMethod.Get;
                case HTTPVerb.POST:
                    return HttpMethod.Post;
                case HTTPVerb.PUT:
                    return HttpMethod.Put;
                case HTTPVerb.DELETE:
                    return HttpMethod.Delete;
                case HTTPVerb.HEAD:
                    return HttpMethod.Head;
                case HTTPVerb.PATCH:
                    return new HttpMethod("PATCH");
            }

            throw new HttpRequestException($"Valid HttpVerb could not be found for verb type: [{verb}]");
        }

        private static void SetContentAndContentType(HttpRequestMessage request, byte[] data, string contentType)
        {
            if (data == null)
            {
                throw Error.ArgumentNull(nameof(data));
            }

            request.Content = new ByteArrayContent(data);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        }

        public static HttpWebRequest ToHttpWebRequest(this EntryRequest entry, Uri baseUrl, FhirClientSettings settings)
        {
            System.Diagnostics.Debug.WriteLine("{0}: {1}", entry.Method, entry.Url);

            if (entry.RequestBodyContent != null && !(entry.Method == HTTPVerb.POST || entry.Method == HTTPVerb.PUT || entry.Method == HTTPVerb.PATCH))
            {
                throw Error.InvalidOperation("Cannot have a body on an Http " + entry.Method.ToString());
            }

            // Create an absolute uri when the interaction.Url is relative.
            var uri = new Uri(entry.Url, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri)
            {
                uri = HttpUtil.MakeAbsoluteToBase(uri, baseUrl);
            }

            var location = new RestUrl(uri);

            if (settings.UseFormatParameter)
            {
                location.AddParam(HttpUtil.RESTPARAM_FORMAT, ContentType.BuildFormatParam(settings.PreferredFormat));
            }

            var request = (HttpWebRequest)WebRequest.Create(location.Uri);
            request.Method = entry.Method.ToString();

            if (!settings.UseFormatParameter)
            {
                request.Accept = entry.Headers.Accept;
            }

            request.ContentType = entry.ContentType;

            if (entry.Headers.IfMatch != null)
            {
                request.Headers["If-Match"] = entry.Headers.IfMatch;
            }

            if (entry.Headers.IfNoneMatch != null)
            {
                request.Headers["If-None-Match"] = entry.Headers.IfNoneMatch;
            }

            if (entry.Headers.IfModifiedSince != null)
            {
                request.IfModifiedSince = entry.Headers.IfModifiedSince.Value.UtcDateTime;
            }

            if (entry.Headers.IfNoneExist != null)
            {
                request.Headers["If-None-Exist"] = entry.Headers.IfNoneExist;
            }

            if (CanHaveReturnPreference() && settings.PreferredReturn.HasValue)
            {
                if (settings.PreferredReturn == Prefer.RespondAsync)
                {
                    request.Headers["Prefer"] = PrimitiveTypeConverter.ConvertTo<string>(settings.PreferredReturn);
                }
                else
                {
                    request.Headers["Prefer"] = "return=" + PrimitiveTypeConverter.ConvertTo<string>(settings.PreferredReturn);
                }
            }
            else if (entry.Type == InteractionType.Search)
            {
                var preferHeader = new List<string>();
                if (settings.PreferredParameterHandling.HasValue)
                {
                    preferHeader.Add("handling=" + settings.PreferredParameterHandling.GetLiteral());
                }

                if (settings.PreferredReturn.HasValue && settings.PreferredReturn == Prefer.RespondAsync)
                {
                    preferHeader.Add(settings.PreferredReturn.GetLiteral());
                }

                if (preferHeader.Count > 0)
                {
                    request.Headers["Prefer"] = string.Join(", ", preferHeader);
                }
            }

            bool CanHaveReturnPreference() => entry.Type == InteractionType.Create ||
                 entry.Type == InteractionType.Update ||
                 entry.Type == InteractionType.Patch;

            return request;
        }
    }
}
