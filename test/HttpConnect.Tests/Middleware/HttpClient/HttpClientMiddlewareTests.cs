using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using HttpConnect.Content;
using HttpConnect.Headers;
using HttpConnect.Middleware.HttpClient;
using Xunit;

namespace HttpConnect.Tests.Middleware.HttpClient
{
    using HttpClient = System.Net.Http.HttpClient;
    using FormUrlEncodedContent = HttpConnect.Content.FormUrlEncodedContent;
    using StringContent = HttpConnect.Content.StringContent;
    using System.Threading;
    using Newtonsoft.Json;
    using System.IO;
    using Newtonsoft.Json.Linq;
    using System.IO.Compression;

    public class HttpClientMiddlewareTests
    {
        private HttpClientMiddlewareTester _middleware;

        public HttpClientMiddlewareTests()
        {
            _middleware = new HttpClientMiddlewareTester();
        }

        public class TheInvokeMethod : HttpClientMiddlewareTests
        {
            [Fact]
            public async Task SendsTheRequestThroughTheHttpClient()
            {
                bool invoked = false;
                var handler = new TestHandler(() =>
                {
                    invoked = true;
                    return new HttpResponseMessage(HttpStatusCode.OK);
                });
                var middleware = new HttpClientMiddlewareTester(handler);
                var request = CreateRequest();
                var context = new HttpConnectContext(CreateRequest());

                await middleware.Invoke(context);

                invoked.Should().BeTrue();
            }

            [Fact]
            public async Task SetsThenRequestOnTheResponse()
            {
                var middleware = new HttpClientMiddlewareTester(
                    new TestHandler(() => new HttpResponseMessage(HttpStatusCode.OK)));
                var request = CreateRequest();
                var context = new HttpConnectContext(request);

                await middleware.Invoke(context);

                context.Response.Request.Should().Be(request);
            }

            private class TestHandler : DelegatingHandler
            {
                private readonly Func<HttpResponseMessage> _handlerFunc;

                public TestHandler(Func<HttpResponseMessage> handlerFunc)
                {
                    _handlerFunc = handlerFunc;
                }

                protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                {
                    return Task.FromResult(_handlerFunc());
                }
            }
        }

        public class TheBuildRequestMessageMethod : HttpClientMiddlewareTests
        {
            [Theory]
            [InlineData("Delete")]
            [InlineData("Get")]
            [InlineData("Head")]
            [InlineData("Options")]
            [InlineData("Post")]
            [InlineData("Put")]
            [InlineData("Patch")]
            [InlineData("Trace")]
            public void SetsTheRequestMethod(string method)
            {
                HttpMethod httpMethod = new HttpMethod(method);
                HttpConnectRequest httpConnectRequest = CreateRequest(httpMethod);

                HttpRequestMessage httpRequestMessage = _middleware.BuildRequestMessageTester(httpConnectRequest);

                httpRequestMessage.Method.Should().Be(httpMethod);
            }

            [Fact]
            public void SetsTheRequestUri()
            {
                HttpConnectRequest httpConnectRequest = CreateGetRequest(new Uri("http://www.example.com/test/endpoint"));

                HttpRequestMessage httpRequestMessage = _middleware.BuildRequestMessageTester(httpConnectRequest);

                httpRequestMessage.RequestUri.Should().Be(new Uri("http://www.example.com/test/endpoint"));
            }

            [Fact]
            public void SetsTheContentFromJsonContent()
            {
                JsonContent jsonContent = new JsonContent(new { test = "test" });
                HttpConnectRequest request = CreatePostRequest(content: jsonContent);

                HttpRequestMessage httpRequestMessage = _middleware.BuildRequestMessageTester(request);

                httpRequestMessage.Content.Headers.GetValues("Content-Type").Single().Should().Be("application/json");
                string content = httpRequestMessage.Content.ReadAsStringAsync().Result;
                content.Should().Be(jsonContent.Serialize());
            }

            [Fact]
            public void SetsTheContentFromFormUrlContent()
            {
                FormUrlEncodedContent formContent = new FormUrlEncodedContent(
                    new Dictionary<string, string> { ["test-1"] = "test-1", ["test-2"] = "test-2" });
                HttpConnectRequest request = CreatePostRequest(content: formContent);

                HttpRequestMessage httpRequestMessage = _middleware.BuildRequestMessageTester(request);

                httpRequestMessage.Content.Headers.GetValues("Content-Type").Single().Should().Be("application/x-www-form-urlencoded");
                string content = httpRequestMessage.Content.ReadAsStringAsync().Result;
                content.Should().Be(formContent.Serialize());
            }

            [Fact]
            public void SetsTheContentFromStringContent()
            {
                StringContent stringContent = new StringContent("THIS IS A TEST", "text/plain");
                HttpConnectRequest request = CreatePostRequest(content: stringContent);

                HttpRequestMessage httpRequestMessage = _middleware.BuildRequestMessageTester(request);

                httpRequestMessage.Content.Headers.GetValues("Content-Type").Single().Should().Be("text/plain");
                string content = httpRequestMessage.Content.ReadAsStringAsync().Result;
                content.Should().Be(stringContent.Serialize());
            }

            [Fact]
            public void CompressedTheContentIfGzipIsProvidedOnTheHeader()
            {
                GZippedContent gzippedContent = new GZippedContent(new JsonContent(new { test = "test" }));
                HttpConnectRequest request = CreatePostRequest(content: gzippedContent);
                HttpRequestMessage httpRequestMessage = _middleware.BuildRequestMessageTester(request);
                var content = httpRequestMessage.Content;
                var contentHeader = content.Headers.SingleOrDefault(h => h.Key == "Content-Encoding");
                contentHeader.Should().NotBeNull();
                contentHeader.Value.SingleOrDefault(v => v == "gzip").Should().NotBeNull();
                content.Should().NotBeNull();

                var stream = content.ReadAsStreamAsync().Result;

                var serializer = new JsonSerializer();

                string jsonString = "";
                using (System.IO.MemoryStream output = new System.IO.MemoryStream())
                {
                    using (System.IO.Compression.GZipStream sr = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress))
                    {
                        sr.CopyTo(output);
                    }

                    jsonString = Encoding.UTF8.GetString(output.GetBuffer(), 0, (int)output.Length);
                }

                JObject json = JObject.Parse(jsonString);
                json["test"].ToString().Should().Be("test");
            }

            [Fact]
            public void DoesNotSetTheContentWhenRequestHasNoContent()
            {
                HttpConnectRequest request = CreatePostRequest(content: null);

                HttpRequestMessage httpRequestMessage = _middleware.BuildRequestMessageTester(request);

                httpRequestMessage.Content.Should().BeNull();
            }

            [Fact]
            public void SetsTheHeaders()
            {
                HttpConnectRequest httpConnectRequest = CreateRequest();
                httpConnectRequest.Headers.Authorization = new AuthorizationHeader("Bearer", "abcdef");
                httpConnectRequest.Headers.Accept = new AcceptHeader("application/vnd.nokia.n-gage.data");
                httpConnectRequest.Headers.Add("User-Agent", "TEST");
                httpConnectRequest.Headers.Add("X-Custom", "Custom");

                var httpRequest = _middleware.BuildRequestMessageTester(httpConnectRequest);

                httpRequest.Headers.GetValues("User-Agent").Should().Contain("TEST");
                httpRequest.Headers.GetValues("Accept").Should().Contain("application/vnd.nokia.n-gage.data");
                httpRequest.Headers.GetValues("Authorization").Single().Should().Be("Bearer abcdef");
                httpRequest.Headers.GetValues("X-Custom").Single().Should().Be("Custom");
            }

            private static HttpConnectRequest CreateGetRequest(Uri requestUri = null)
            {
                return new HttpConnectRequest(HttpMethod.Get, requestUri ?? new Uri("https://www.example.com"));
            }

            private static HttpConnectRequest CreatePostRequest(Uri requestUri = null, HttpConnectRequestContent content = null)
            {
                return new HttpConnectRequest(HttpMethod.Post, requestUri ?? new Uri("https://www.example.com"))
                {
                    Content = content
                };
            }
        }

        public class TheBuildResponseMethod : HttpClientMiddlewareTests
        {
            [Theory]
            [InlineData(HttpStatusCode.OK)]
            [InlineData(HttpStatusCode.Created)]
            [InlineData(HttpStatusCode.BadRequest)]
            [InlineData(HttpStatusCode.InternalServerError)]
            public async Task SetsStatusCodeInTheResponse(HttpStatusCode statusCode)
            {
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage(statusCode)
                {
                    Content = new System.Net.Http.StringContent("{}"),
                    Headers =
                    {
                        { "peanut", "butter" },
                        { "ele", "phant" }
                    }
                };

                HttpConnectResponse httpConnectResponse = await _middleware.BuildHttpConnectResponseTester(httpResponseMessage);

                httpConnectResponse.StatusCode.Should().Be(statusCode);
            }

            [Fact]
            public async Task SetsContentInTheResponseWhenContentIsPresent()
            {
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new System.Net.Http.StringContent("{}", Encoding.UTF8, "application/json")
                };

                HttpConnectResponse httpConnectResponse = await _middleware.BuildHttpConnectResponseTester(httpResponseMessage);

                httpConnectResponse.Content.Should().NotBeNull();
                httpConnectResponse.Content.Content.Should().Be("{}");
            }

            [Fact]
            public async Task SetsTheContentInTheResponseWhenTheResponseIsGZipped()
            {
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new System.Net.Http.ByteArrayContent(TestZip("testData"))
                };
                httpResponseMessage.Content.Headers.TryAddWithoutValidation("Content-Encoding", "application/gzip");
                httpResponseMessage.Content.Headers.Add("Content-Type", "application/json");

                HttpConnectResponse httpConnectResponse = await _middleware.BuildHttpConnectResponseTester(httpResponseMessage);

                httpConnectResponse.Content.Should().NotBeNull();
                httpConnectResponse.Content.Content.Should().Be("testData");
            }

            [Fact]
            public async Task DoesNotSetContentInTheResponseWhenNoContentReturned()
            {
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = null
                };

                HttpConnectResponse httpConnectResponse = await _middleware.BuildHttpConnectResponseTester(httpResponseMessage);

                httpConnectResponse.Content.Should().BeNull();
            }

            [Fact]
            public async Task SetsHeadersInTheResponse()
            {
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Headers =
                    {
                        { "peanut", "butter" },
                        { "ele", "phant" }
                    }
                };

                HttpConnectResponse httpConnectResponse = await _middleware.BuildHttpConnectResponseTester(httpResponseMessage);

                httpConnectResponse.Headers.Should().HaveCount(2);
                httpConnectResponse.Headers.Single(h => h.Name == "peanut" && h.Value == "butter");
                httpConnectResponse.Headers.Single(h => h.Name == "ele" && h.Value == "phant");
            }
            
            [Fact]
            public async Task BUG_HandlesMissingContentTypeInHttpResponse()
            {
                HttpContent content = new System.Net.Http.StringContent("<html><head><title></title><!-- <script language=\"javascript\">window.location.replace(\"http://www.cashmerecentre.com/Default.aspx\");</script> --></head><body></body></html>");
                content.Headers.Remove("content-type"); // remove content-type header

                HttpResponseMessage httpResponseMessage = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = content,
                    Headers =
                    {
                        { "Refresh", "0;URL=http://www.cashmerecentre.com/Default.aspx" },
                        { "X-Powered-By-Plesk", "PleskWin" },
                        { "X-Frame-Options", "SAMEORIGIN" },
                        { "Date", "Wed, 09 Nov 2016 16:26:57 GMT" },
                        { "Server", "Microsoft-IIS/7.5" },
                        { "X-Powered-By", "ASP.NET" },
                    }
                };

                // would throw if there was an issue
                await _middleware.BuildHttpConnectResponseTester(httpResponseMessage);
            }

            [Theory]
            [InlineData("\"utf-8\"")]
            [InlineData("\"UTF-8\"")]
            [InlineData("lkdnfv")]
            [InlineData("non-compliant")]
            public async Task HandlesHttpClientBugWithContentTypeCharsetHeaderInHttpResponse(string charSet)
            {
                HttpContent content = new System.Net.Http.StringContent("<!DOCTYPE html></html>");
                content.Headers.ContentType.CharSet = charSet; // change the charset

                HttpResponseMessage httpResponseMessage = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = content,
                    Headers =
                    {
                        { "Date", "Thu, 10 Nov 2016 09:54:10 GMT" },
                        { "Server", "Apache" },
                        {
                            "Set-Cookie", new[]
                            {
                                "PHPSESSID = 08qguiu4j2caomkg37qtdk05o0; path =/; HttpOnly",
                                "bypassStaticCache = deleted; expires = Thu, 01 - Jan - 1970 00:00:01 GMT; path =/; httponly",
                                "bypassStaticCache = deleted; expires = Thu, 01 - Jan - 1970 00:00:01 GMT; path =/; httponly"
                            }
                        },
                        { "Cache-Control", "must-revalidate, no-cache, max-age=0" },
                        { "Pragma", "no-cache" },
                        { "Vary", "Accept-Encoding" },
                        { "Connection", "close" }
                    }
                };

                // would throw if there was an issue
                await _middleware.BuildHttpConnectResponseTester(httpResponseMessage);
            }
        }

        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        private static byte[] TestZip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        private sealed class HttpClientMiddlewareTester : HttpClientMiddleware
        {
            public HttpClientMiddlewareTester()
                : base(new HttpClient())
            {
            }

            public HttpClientMiddlewareTester(HttpMessageHandler handler)
                : base(new HttpClient(handler))
            {
            }

            public HttpRequestMessage BuildRequestMessageTester(HttpConnectRequest request)
            {
                return BuildHttpRequestMessage(request);
            }

            public async Task<HttpConnectResponse> BuildHttpConnectResponseTester(HttpResponseMessage responseMessage)
            {
                return await BuildHttpConnectResponse(responseMessage);
            }
        }

        private static HttpConnectRequest CreateRequest(HttpMethod httpMethod = null, Uri requestUri = null)
        {
            return new HttpConnectRequest(httpMethod ?? HttpMethod.Get, requestUri ?? new Uri("https://www.example.com"));
        }
    }
}
