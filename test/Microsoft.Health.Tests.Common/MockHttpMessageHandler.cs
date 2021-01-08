// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Tests.Common
{
    public abstract class MockHttpMessageHandler<T> : HttpMessageHandler
    {
        public virtual T GetReturnContent(HttpRequestMessage request)
        {
            return default;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
            };

            T content = GetReturnContent(request);

            if (content != null)
            {
                response.Content = new StringContent(GetJsonContent(content), Encoding.UTF8, "application/json");
            }

            return Task.FromResult(response);
        }

        protected abstract string GetJsonContent(T content);
    }
}
