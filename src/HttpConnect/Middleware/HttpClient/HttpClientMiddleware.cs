using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HttpConnect.Content;
using HttpConnect.Headers;

namespace HttpConnect.Middleware.HttpClient
{
    public class HttpClientMiddleware
    {
        private readonly System.Net.Http.HttpClient _httpClient;

        public HttpClientMiddleware(System.Net.Http.HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task Invoke(HttpConnectContext context)
        {
            context.RequestAborted.ThrowIfCancellationRequested();

            using (HttpRequestMessage httpRequestMessage = BuildHttpRequestMessage(context.Request))
            using (HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage, context.RequestAborted).ConfigureAwait(false))
            {
                context.Response = await BuildHttpConnectResponse(httpResponseMessage).ConfigureAwait(false);
                context.Response.Request = context.Request;
            }
        }

        protected HttpRequestMessage BuildHttpRequestMessage(HttpConnectRequest httpConnectRequest)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(httpConnectRequest.Method, httpConnectRequest.RequestUri);

            foreach (HttpConnectHeader header in httpConnectRequest.Headers)
                httpRequestMessage.Headers.Add(header.Name, header.Value);

            if (httpConnectRequest.Content != null)
                httpRequestMessage.Content = MapHttpConnectRequestContentToHttpContent(httpConnectRequest.Content);

            return httpRequestMessage;
        }

        protected async Task<HttpConnectResponse> BuildHttpConnectResponse(HttpResponseMessage httpResponseMessage)
        {
            HttpConnectResponseContent content = null;
            string contentType = string.Empty;
            string body = string.Empty;

            if (httpResponseMessage.Content != null)
            {
                if (httpResponseMessage.Content.Headers.ContentEncoding != null && httpResponseMessage.Content.Headers.ContentEncoding.ToString() == "application/gzip")
                {
                    contentType = httpResponseMessage.Content.Headers?.ContentType?.MediaType;
                    body = await DecompressGzip(httpResponseMessage.Content);
                }
                else
                {
                    // https://www.w3.org/Protocols/rfc2616/rfc2616-sec7.html#sec7.2.1
                    // null checks shouldn't be required as content-type should be present if content is
                    // but not everybody is a good citizen of the web.
                    contentType = httpResponseMessage.Content.Headers?.ContentType?.MediaType ?? "application/octet-stream";
                    body = await ReadContentAsString(httpResponseMessage.Content).ConfigureAwait(false);
                }
                content = new HttpConnectResponseContent();
                content.Content = body;
                content.Headers.ContentType = new ContentTypeHeader(contentType);
            }

            HttpConnectResponseHeaders headers = new HttpConnectResponseHeaders();
            foreach (var header in httpResponseMessage.Headers)
                headers.Add(header.Key, header.Value.First());

            return new HttpConnectResponse(httpResponseMessage.StatusCode)
            {
                Headers = headers,
                Content = content
            };
        }

        private async Task<string> DecompressGzip(HttpContent content)
        {
            string output;
            byte[] byteArray = await content.ReadAsByteArrayAsync();

            using (var dataStream = new MemoryStream(byteArray))
            using (var gZipStream = new GZipStream(dataStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                gZipStream.CopyTo(outputStream);
                output = Encoding.UTF8.GetString(outputStream.ToArray());
            }

            return output;
        }

        private static HttpContent MapHttpConnectRequestContentToHttpContent(HttpConnectRequestContent httpConnectRequestContent)
        {
            if (httpConnectRequestContent is GZippedContent gZippedContent)
                return new GZipContent(MapHttpConnectRequestContentToHttpContent((HttpConnectRequestContent)gZippedContent.Content));

            if (httpConnectRequestContent is Content.FormUrlEncodedContent formContent)
            {
                IEnumerable<KeyValuePair<string, string>> nameValueCollection =
                    (IEnumerable<KeyValuePair<string, string>>)formContent.Content;

                return new System.Net.Http.FormUrlEncodedContent(nameValueCollection);
            }

            HttpConnectRequestContent content = httpConnectRequestContent;
            var stringContent = new System.Net.Http.StringContent(content.Serialize(), Encoding.UTF8);
            stringContent.Headers.Remove(KnownHeaders.ContentType); // WE WANT TO SET THE CONTENT-TYPE
            stringContent.Headers.Add(content.Headers.ContentType.Name, content.Headers.ContentType.Value);

            return stringContent;
        }

        // https://github.com/skazantsev/WebDavClient/commit/e7b245700382639634e8349354e6c4a1bc3ea3cc
        private static async Task<string> ReadContentAsString(HttpContent content)
        {
            // https://github.com/dotnet/corefx/issues/5014
            // http client does not conform to RFC7231 re: charset headers
            // so best to read it out as bytes then convert using a safe endcoding
            byte[] data = await content.ReadAsByteArrayAsync().ConfigureAwait(false);
            return GetResponseEncoding(content).GetString(data);
        }

        private static Encoding GetResponseEncoding(HttpContent content)
        {
            try
            {
                return Encoding.GetEncoding(content.Headers?.ContentType?.CharSet);
            }
            catch (ArgumentException)
            {
                return Encoding.UTF8;
            }
        }
    }
}
