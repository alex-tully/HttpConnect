using System;
using System.Net.Http;
using HttpConnect.Content;
using HttpConnect.Headers;

namespace HttpConnect
{
    public class HttpConnectRequest
    {
        public HttpConnectRequest(HttpMethod method, Uri requestUri)
        {
            Method = method;
            RequestUri = requestUri ?? throw new ArgumentNullException(nameof(requestUri));
        }

        public HttpMethod Method { get; set; }

        public Uri RequestUri { get; set; }

        public HttpConnectRequestHeaders Headers { get; } = new HttpConnectRequestHeaders();

        public HttpConnectRequestContent Content { get; set; }
    }
}
