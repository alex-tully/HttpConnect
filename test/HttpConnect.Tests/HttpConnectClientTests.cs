using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using HttpConnect.Content;
using HttpConnect.Headers;
using Xunit;

namespace HttpConnect.Tests
{
    public class HttpConnectClientTests
    {
        public class TheConstructor
        {
            [Fact]
            public void WhenBaseUriIsUsedAndIsNotAbsoluteUriThenThrows()
            {
                Assert.Throws<ArgumentException>(() => new HttpConnectClient(new Uri("/relative", UriKind.Relative)));
            }
        }

        public class TheSendAsyncMethod : HttpConnectClientTests
        {
            [Fact]
            public async Task WhenRequestIsNullTheThrows()
            {
                var client = new HttpConnectClient();

                await Assert.ThrowsAsync<ArgumentNullException>(() => client.SendAsync(null, CancellationToken.None));
            }

            [Fact]
            public async Task WhenBuiltWithSimpleMiddlewareThenReturnsExpectedResponse()
            {
                var expectedResponse = new HttpConnectResponse(HttpStatusCode.OK);
                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return Task.CompletedTask;
                    });
                });

                var actualResponse = await client.SendAsync(new HttpConnectRequest(HttpMethod.Get, new Uri("https://www.example.com")), CancellationToken.None);

                actualResponse.Should().Be(expectedResponse);
            }

            [Fact]
            public async Task WhenBuiltWithMultipleMiddlewaresThenAllMiddlewareExecuted()
            {
                int executionCounter = 0;
                Func<HttpConnectRequestDelegate, HttpConnectRequestDelegate> middleware = 
                    (next) => (ctx) => { executionCounter++; return next(ctx); };
                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                });

                await client.SendAsync(new HttpConnectRequest(HttpMethod.Get, new Uri("https://www.example.com")), CancellationToken.None);

                executionCounter.Should().Be(8);
            }

            [Theory]
            [InlineData(HttpStatusCode.Continue)]
            [InlineData(HttpStatusCode.SwitchingProtocols)]
            [InlineData(HttpStatusCode.Ambiguous)]
            [InlineData(HttpStatusCode.Moved)]
            [InlineData(HttpStatusCode.Found)]
            [InlineData(HttpStatusCode.SeeOther)]
            [InlineData(HttpStatusCode.NotModified)]
            [InlineData(HttpStatusCode.UseProxy)]
            [InlineData(HttpStatusCode.Unused)]
            [InlineData(HttpStatusCode.TemporaryRedirect)]
            [InlineData(HttpStatusCode.BadRequest)]
            [InlineData(HttpStatusCode.Unauthorized)]
            [InlineData(HttpStatusCode.PaymentRequired)]
            [InlineData(HttpStatusCode.Forbidden)]
            [InlineData(HttpStatusCode.NotFound)]
            [InlineData(HttpStatusCode.MethodNotAllowed)]
            [InlineData(HttpStatusCode.NotAcceptable)]
            [InlineData(HttpStatusCode.ProxyAuthenticationRequired)]
            [InlineData(HttpStatusCode.RequestTimeout)]
            [InlineData(HttpStatusCode.Conflict)]
            [InlineData(HttpStatusCode.Gone)]
            [InlineData(HttpStatusCode.LengthRequired)]
            [InlineData(HttpStatusCode.PreconditionFailed)]
            [InlineData(HttpStatusCode.RequestEntityTooLarge)]
            [InlineData(HttpStatusCode.RequestUriTooLong)]
            [InlineData(HttpStatusCode.UnsupportedMediaType)]
            [InlineData(HttpStatusCode.RequestedRangeNotSatisfiable)]
            [InlineData(HttpStatusCode.ExpectationFailed)]
            [InlineData(HttpStatusCode.UpgradeRequired)]
            [InlineData(HttpStatusCode.InternalServerError)]
            [InlineData(HttpStatusCode.NotImplemented)]
            [InlineData(HttpStatusCode.BadGateway)]
            [InlineData(HttpStatusCode.ServiceUnavailable)]
            [InlineData(HttpStatusCode.GatewayTimeout)]
            [InlineData(HttpStatusCode.HttpVersionNotSupported)]
            public async Task WhenMiddlewareReturnsNonSuccessStatusCodeThenResponseIsNotSuccessful(HttpStatusCode statusCode)
            {
                var expectedResponse = new HttpConnectResponse(statusCode);

                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return Task.CompletedTask;
                    });
                });

                var actualResponse = await client.SendAsync(new HttpConnectRequest(HttpMethod.Get, new Uri("https://www.example.com")), CancellationToken.None);

                actualResponse.IsSuccess.Should().BeFalse();
            }

            [Fact]
            public async Task WhenBuiltWithMiddlewareThatThrowsExceptionThenReturnsExceptionInResponse()
            {
                Exception expectedException = new Exception("test exception");
                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        throw expectedException;
                    });
                });

                var actualResponse = await client.SendAsync(new HttpConnectRequest(HttpMethod.Get, new Uri("https://www.example.com")), CancellationToken.None);

                actualResponse.Exception.Should().Be(expectedException);
            }


            [Fact]
            public async Task WhenBuiltWithMiddlewareThatThrowsExceptionThenRequestStatusIsError()
            {
                Exception expectedException = new Exception("test exception");
                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        throw expectedException;
                    });
                });

                var actualResponse = await client.SendAsync(new HttpConnectRequest(HttpMethod.Get, new Uri("https://www.example.com")), CancellationToken.None);

                actualResponse.Status.Should().Be(HttpConnectResponseStatus.Error);
            }

            [Fact]
            public async Task WhenBuiltWithMiddlewareThatThrowsExceptionAfterResponseHasBeenSetThenOriginalResponseReturnedWithError()
            {
                HttpConnectResponse expectedResponse = new HttpConnectResponse(HttpStatusCode.Unauthorized);
                Exception expectedException = new Exception("test exception");
                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return next(ctx);
                    });
                    pipline.Use(next => ctx =>
                    {
                        throw expectedException;
                    });
                });

                var actualResponse = await client.SendAsync(
                    new HttpConnectRequest(HttpMethod.Head, new Uri("https://www.example.com")), 
                    CancellationToken.None);

                actualResponse.Should().BeEquivalentTo(expectedResponse);
                actualResponse.Status.Should().Be(HttpConnectResponseStatus.Error);
            }

            [Fact]
            public async Task WhenRequestUsesRelativeUriAndNoBaseUrlThenThrows()
            {
                var client = new HttpConnectClient();

                Exception ex = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => client.SendAsync(
                        new HttpConnectRequest(HttpMethod.Get, new Uri("/relative/url", UriKind.Relative)), 
                        CancellationToken.None));
            }

            [Fact]
            public async Task WhenClientIsSetupWithBaseUrlAndRequestUsesRelativeUriTheRequestUriUpdatedWithBaseUrl()
            {
                var client = new HttpConnectClient(new Uri("https://www.example.com"));
                var request = new HttpConnectRequest(HttpMethod.Get, new Uri("/relative/url", UriKind.Relative));

                await client.SendAsync(request, CancellationToken.None);

                request.RequestUri.Should().Be(new Uri("https://www.example.com/relative/url"));
            }
        }

        public class TheSendAsync_T_Method : HttpConnectClientTests
        {
            [Fact]
            public async Task WhenRequestIsNullTheThrows()
            {
                var client = new HttpConnectClient();

                await Assert.ThrowsAsync<ArgumentNullException>(() => client.SendAsync<int>(null, CancellationToken.None));
            }

            [Fact]
            public async Task WhenBuiltWithSimpleMiddlewareThenReturnsExpectedResponse()
            {
                var expectedResponse = new HttpConnectResponse<int>(HttpStatusCode.OK);
                expectedResponse.Content = new HttpConnectResponseContent();
                expectedResponse.Content.Headers.ContentType = new ContentTypeHeader("application/json");
                expectedResponse.Content.Content = "1";
                expectedResponse.Data = 1;
                expectedResponse.Status = HttpConnectResponseStatus.Completed;

                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return Task.CompletedTask;
                    });
                });

                var actualResponse = await client.SendAsync<int>(new HttpConnectRequest(HttpMethod.Get, new Uri("https://www.example.com")), CancellationToken.None);

                actualResponse.Should().BeEquivalentTo(expectedResponse);
            }

            [Fact]
            public async Task WhenBuiltWithMultipleMiddlewaresThenAllMiddlewareExecuted()
            {
                int executionCounter = 0;
                Func<HttpConnectRequestDelegate, HttpConnectRequestDelegate> middleware =
                    (next) => (ctx) => { executionCounter++; return next(ctx); };
                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                });

                await client.SendAsync<int>(new HttpConnectRequest(HttpMethod.Get, new Uri("https://www.example.com")), CancellationToken.None);

                executionCounter.Should().Be(8);
            }

            [Theory]
            [InlineData("application/json; charset=utf-8", "{\"name\":\"openwrks\"}")]
            [InlineData("text/json; charset=utf-8", "{\"name\":\"openwrks\"}")]
            public async Task WhenResponseContentTypeContainsCharsetThenCanDeserialize(string contentType, string content)
            {
                var expectedResponse = new HttpConnectResponse<TestType>(HttpStatusCode.OK);
                expectedResponse.Content = new HttpConnectResponseContent();
                expectedResponse.Content.Headers.ContentType = new ContentTypeHeader(contentType);
                expectedResponse.Content.Content = content;
                expectedResponse.Data = new TestType { Name = "openwrks" };
                expectedResponse.Status = HttpConnectResponseStatus.Completed;

                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return Task.CompletedTask;
                    });
                });

                var actualResponse = await client.SendAsync<TestType>(new HttpConnectRequest(HttpMethod.Get, new Uri("https://www.example.com")), CancellationToken.None);

                actualResponse.Should().BeEquivalentTo(expectedResponse);
            }

            [Theory]
            [InlineData("plain/text", "some text")]
            [InlineData("text/html", "<html></html>")]
            [InlineData("application/xml", "<xml>data</xml>")]
            public async Task WhenResponseContentTypeCannotBeDeserializedThenReturnsDefault(string contentType, string content)
            {
                var expectedResponse = new HttpConnectResponse<string>(HttpStatusCode.OK);
                expectedResponse.Content = new HttpConnectResponseContent();
                expectedResponse.Content.Headers.ContentType = new ContentTypeHeader(contentType);
                expectedResponse.Content.Content = content;
                expectedResponse.Data = null;
                expectedResponse.Status = HttpConnectResponseStatus.Completed;

                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return Task.CompletedTask;
                    });
                });

                var actualResponse = await client.SendAsync<string>(new HttpConnectRequest(HttpMethod.Get, new Uri("https://www.example.com")), CancellationToken.None);

                actualResponse.Should().BeEquivalentTo(expectedResponse);
            }

            [Theory]
            [InlineData("application/json; charset=utf-8", "INVALID JSON")]
            [InlineData("text/json; charset=utf-8", "INVALID JSON")]
            public async Task WhenDeserializationThrowsThenCapturedInResponseException(string contentType, string content)
            {
                var expectedResponse = new HttpConnectResponse<TestType>(HttpStatusCode.OK);
                expectedResponse.Content = new HttpConnectResponseContent();
                expectedResponse.Content.Headers.ContentType = new ContentTypeHeader(contentType);
                expectedResponse.Content.Content = content;
                expectedResponse.Status = HttpConnectResponseStatus.Completed;

                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return Task.CompletedTask;
                    });
                });

                var actualResponse = await client.SendAsync<TestType>(
                    new HttpConnectRequest(HttpMethod.Get, new Uri("https://www.example.com")), 
                    CancellationToken.None);

                actualResponse.IsSuccess.Should().BeFalse();
                actualResponse.Exception.Should().NotBeNull();
                actualResponse.Data.Should().BeNull();
            }

            [Theory]
            [InlineData(HttpStatusCode.Continue)]
            [InlineData(HttpStatusCode.SwitchingProtocols)]
            [InlineData(HttpStatusCode.Ambiguous)]
            [InlineData(HttpStatusCode.Moved)]
            [InlineData(HttpStatusCode.Found)]
            [InlineData(HttpStatusCode.SeeOther)]
            [InlineData(HttpStatusCode.NotModified)]
            [InlineData(HttpStatusCode.UseProxy)]
            [InlineData(HttpStatusCode.Unused)]
            [InlineData(HttpStatusCode.TemporaryRedirect)]
            [InlineData(HttpStatusCode.BadRequest)]
            [InlineData(HttpStatusCode.Unauthorized)]
            [InlineData(HttpStatusCode.PaymentRequired)]
            [InlineData(HttpStatusCode.Forbidden)]
            [InlineData(HttpStatusCode.NotFound)]
            [InlineData(HttpStatusCode.MethodNotAllowed)]
            [InlineData(HttpStatusCode.NotAcceptable)]
            [InlineData(HttpStatusCode.ProxyAuthenticationRequired)]
            [InlineData(HttpStatusCode.RequestTimeout)]
            [InlineData(HttpStatusCode.Conflict)]
            [InlineData(HttpStatusCode.Gone)]
            [InlineData(HttpStatusCode.LengthRequired)]
            [InlineData(HttpStatusCode.PreconditionFailed)]
            [InlineData(HttpStatusCode.RequestEntityTooLarge)]
            [InlineData(HttpStatusCode.RequestUriTooLong)]
            [InlineData(HttpStatusCode.UnsupportedMediaType)]
            [InlineData(HttpStatusCode.RequestedRangeNotSatisfiable)]
            [InlineData(HttpStatusCode.ExpectationFailed)]
            [InlineData(HttpStatusCode.UpgradeRequired)]
            [InlineData(HttpStatusCode.InternalServerError)]
            [InlineData(HttpStatusCode.NotImplemented)]
            [InlineData(HttpStatusCode.BadGateway)]
            [InlineData(HttpStatusCode.ServiceUnavailable)]
            [InlineData(HttpStatusCode.GatewayTimeout)]
            [InlineData(HttpStatusCode.HttpVersionNotSupported)]
            public async Task WhenMiddlewareReturnsNonSuccessStatusCodeThenResponseIsNotSuccessful(HttpStatusCode statusCode)
            {
                var expectedResponse = new HttpConnectResponse<int>(statusCode);

                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return Task.CompletedTask;
                    });
                });

                var actualResponse = await client.SendAsync<int>(new HttpConnectRequest(HttpMethod.Get, new Uri("https://www.example.com")), CancellationToken.None);

                actualResponse.IsSuccess.Should().BeFalse();
            }

            [Theory]
            [InlineData(HttpStatusCode.Continue)]
            [InlineData(HttpStatusCode.SwitchingProtocols)]
            [InlineData(HttpStatusCode.Ambiguous)]
            [InlineData(HttpStatusCode.Moved)]
            [InlineData(HttpStatusCode.Found)]
            [InlineData(HttpStatusCode.SeeOther)]
            [InlineData(HttpStatusCode.NotModified)]
            [InlineData(HttpStatusCode.UseProxy)]
            [InlineData(HttpStatusCode.Unused)]
            [InlineData(HttpStatusCode.TemporaryRedirect)]
            [InlineData(HttpStatusCode.BadRequest)]
            [InlineData(HttpStatusCode.Unauthorized)]
            [InlineData(HttpStatusCode.PaymentRequired)]
            [InlineData(HttpStatusCode.Forbidden)]
            [InlineData(HttpStatusCode.NotFound)]
            [InlineData(HttpStatusCode.MethodNotAllowed)]
            [InlineData(HttpStatusCode.NotAcceptable)]
            [InlineData(HttpStatusCode.ProxyAuthenticationRequired)]
            [InlineData(HttpStatusCode.RequestTimeout)]
            [InlineData(HttpStatusCode.Conflict)]
            [InlineData(HttpStatusCode.Gone)]
            [InlineData(HttpStatusCode.LengthRequired)]
            [InlineData(HttpStatusCode.PreconditionFailed)]
            [InlineData(HttpStatusCode.RequestEntityTooLarge)]
            [InlineData(HttpStatusCode.RequestUriTooLong)]
            [InlineData(HttpStatusCode.UnsupportedMediaType)]
            [InlineData(HttpStatusCode.RequestedRangeNotSatisfiable)]
            [InlineData(HttpStatusCode.ExpectationFailed)]
            [InlineData(HttpStatusCode.UpgradeRequired)]
            [InlineData(HttpStatusCode.InternalServerError)]
            [InlineData(HttpStatusCode.NotImplemented)]
            [InlineData(HttpStatusCode.BadGateway)]
            [InlineData(HttpStatusCode.ServiceUnavailable)]
            [InlineData(HttpStatusCode.GatewayTimeout)]
            [InlineData(HttpStatusCode.HttpVersionNotSupported)]
            public async Task WhenMiddlewareReturnsNonSuccessResponseThenDoesNotDeserializeResponse(HttpStatusCode statusCode)
            {
                var expectedResponse = new HttpConnectResponse<int>(statusCode);
                expectedResponse.Content = new HttpConnectResponseContent();
                expectedResponse.Content.Headers.ContentType = new ContentTypeHeader("application/json");
                expectedResponse.Content.Content = "{\"error\":\"test_error\"";
                expectedResponse.Data = default;
                expectedResponse.Status = HttpConnectResponseStatus.Completed;

                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return Task.CompletedTask;
                    });
                });

                var actualResponse = await client.SendAsync<int>(new HttpConnectRequest(HttpMethod.Get, new Uri("https://www.example.com")), CancellationToken.None);

                actualResponse.Should().BeEquivalentTo(expectedResponse);
            }

            [Fact]
            public async Task WhenBuiltWithMiddlewareThatThrowsExceptionThenReturnsExceptionInResponse()
            {
                Exception expectedException = new Exception("test exception");
                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        throw expectedException;
                    });
                });

                var actualResponse = await client.SendAsync<int>(new HttpConnectRequest(HttpMethod.Get, new Uri("https://www.example.com")), CancellationToken.None);

                actualResponse.Exception.Should().Be(expectedException);
            }

            [Fact]
            public async Task WhenBuiltWithMiddlewareThatThrowsExceptionThenRequestStatusIsError()
            {
                Exception expectedException = new Exception("test exception");
                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        throw expectedException;
                    });
                });

                var actualResponse = await client.SendAsync<int>(new HttpConnectRequest(HttpMethod.Get, new Uri("https://www.example.com")), CancellationToken.None);

                actualResponse.Status.Should().Be(HttpConnectResponseStatus.Error);
            }

            [Fact]
            public async Task WhenBuiltWithMiddlewareThatThrowsExceptionAfterResponseHasBeenSetThenOriginalResponseReturnedWithError()
            {
                HttpConnectResponse<int> expectedResponse = new HttpConnectResponse<int>(HttpStatusCode.Unauthorized);
                Exception expectedException = new Exception("test exception");
                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return next(ctx);
                    });
                    pipline.Use(next => ctx =>
                    {
                        throw expectedException;
                    });
                });

                var actualResponse = await client.SendAsync<int>(
                    new HttpConnectRequest(HttpMethod.Head, new Uri("https://www.example.com")),
                    CancellationToken.None);

                actualResponse.Should().BeEquivalentTo(expectedResponse);
                actualResponse.Status.Should().Be(HttpConnectResponseStatus.Error);
            }

            [Fact]
            public async Task WhenRequestUsesRelativeUriAndNoBaseUrlThenThrows()
            {
                var client = new HttpConnectClient();

                Exception ex = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => client.SendAsync<int>(
                        new HttpConnectRequest(HttpMethod.Get, new Uri("/relative/url", UriKind.Relative)),
                        CancellationToken.None));
            }

            [Fact]
            public async Task WhenClientIsSetupWithBaseUrlAndRequestUsesRelativeUriTheRequestUriUpdatedWithBaseUrl()
            {
                var client = new HttpConnectClient(new Uri("https://www.example.com"));
                var request = new HttpConnectRequest(HttpMethod.Get, new Uri("/relative/url", UriKind.Relative));

                await client.SendAsync<int>(request, CancellationToken.None);

                request.RequestUri.Should().Be(new Uri("https://www.example.com/relative/url"));
            }
        }

        public class TheGetAsyncMethod : HttpConnectClientTests
        {
            [Fact]
            public async Task WhenBuiltWithSimpleMiddlewareThenReturnsExpectedResponse()
            {
                var expectedResponse = new HttpConnectResponse(HttpStatusCode.OK);
                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return Task.CompletedTask;
                    });
                });

                var actualResponse = await client.GetAsync(new Uri("https://www.example.com"), CancellationToken.None);

                actualResponse.Should().Be(expectedResponse);
            }

            [Fact]
            public async Task WhenBuiltWithMultipleMiddlewaresThenAllMiddlewareExecuted()
            {
                int executionCounter = 0;
                Func<HttpConnectRequestDelegate, HttpConnectRequestDelegate> middleware =
                    (next) => (ctx) => { executionCounter++; return next(ctx); };
                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                });

                await client.GetAsync(new Uri("https://www.example.com"), CancellationToken.None);

                executionCounter.Should().Be(8);
            }

            [Theory]
            [InlineData(HttpStatusCode.Continue)]
            [InlineData(HttpStatusCode.SwitchingProtocols)]
            [InlineData(HttpStatusCode.Ambiguous)]
            [InlineData(HttpStatusCode.Moved)]
            [InlineData(HttpStatusCode.Found)]
            [InlineData(HttpStatusCode.SeeOther)]
            [InlineData(HttpStatusCode.NotModified)]
            [InlineData(HttpStatusCode.UseProxy)]
            [InlineData(HttpStatusCode.Unused)]
            [InlineData(HttpStatusCode.TemporaryRedirect)]
            [InlineData(HttpStatusCode.BadRequest)]
            [InlineData(HttpStatusCode.Unauthorized)]
            [InlineData(HttpStatusCode.PaymentRequired)]
            [InlineData(HttpStatusCode.Forbidden)]
            [InlineData(HttpStatusCode.NotFound)]
            [InlineData(HttpStatusCode.MethodNotAllowed)]
            [InlineData(HttpStatusCode.NotAcceptable)]
            [InlineData(HttpStatusCode.ProxyAuthenticationRequired)]
            [InlineData(HttpStatusCode.RequestTimeout)]
            [InlineData(HttpStatusCode.Conflict)]
            [InlineData(HttpStatusCode.Gone)]
            [InlineData(HttpStatusCode.LengthRequired)]
            [InlineData(HttpStatusCode.PreconditionFailed)]
            [InlineData(HttpStatusCode.RequestEntityTooLarge)]
            [InlineData(HttpStatusCode.RequestUriTooLong)]
            [InlineData(HttpStatusCode.UnsupportedMediaType)]
            [InlineData(HttpStatusCode.RequestedRangeNotSatisfiable)]
            [InlineData(HttpStatusCode.ExpectationFailed)]
            [InlineData(HttpStatusCode.UpgradeRequired)]
            [InlineData(HttpStatusCode.InternalServerError)]
            [InlineData(HttpStatusCode.NotImplemented)]
            [InlineData(HttpStatusCode.BadGateway)]
            [InlineData(HttpStatusCode.ServiceUnavailable)]
            [InlineData(HttpStatusCode.GatewayTimeout)]
            [InlineData(HttpStatusCode.HttpVersionNotSupported)]
            public async Task WhenMiddlewareReturnsNonSuccessStatusCodeThenResponseIsNotSuccessful(HttpStatusCode statusCode)
            {
                var expectedResponse = new HttpConnectResponse(statusCode);

                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return Task.CompletedTask;
                    });
                });

                var actualResponse = await client.GetAsync(new Uri("https://www.example.com"), CancellationToken.None);

                actualResponse.IsSuccess.Should().BeFalse();
            }

            [Fact]
            public async Task WhenBuiltWithMiddlewareThatThrowsExceptionThenReturnsExceptionInResponse()
            {
                Exception expectedException = new Exception("test exception");
                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        throw expectedException;
                    });
                });

                var actualResponse = await client.GetAsync(new Uri("https://www.example.com"), CancellationToken.None);

                actualResponse.Exception.Should().Be(expectedException);
            }


            [Fact]
            public async Task WhenBuiltWithMiddlewareThatThrowsExceptionThenRequestStatusIsError()
            {
                Exception expectedException = new Exception("test exception");
                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        throw expectedException;
                    });
                });

                var actualResponse = await client.GetAsync(new Uri("https://www.example.com"), CancellationToken.None);

                actualResponse.Status.Should().Be(HttpConnectResponseStatus.Error);
            }

            [Fact]
            public async Task WhenBuiltWithMiddlewareThatThrowsExceptionAfterResponseHasBeenSetThenOriginalResponseReturnedWithError()
            {
                HttpConnectResponse expectedResponse = new HttpConnectResponse(HttpStatusCode.Unauthorized);
                Exception expectedException = new Exception("test exception");
                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return next(ctx);
                    });
                    pipline.Use(next => ctx =>
                    {
                        throw expectedException;
                    });
                });

                var actualResponse = await client.GetAsync(new Uri("https://www.example.com"), CancellationToken.None);

                actualResponse.Should().BeEquivalentTo(expectedResponse);
                actualResponse.Status.Should().Be(HttpConnectResponseStatus.Error);
            }

            [Fact]
            public async Task WhenRequestUsesRelativeUriAndNoBaseUrlThenThrows()
            {
                var client = new HttpConnectClient();

                Exception ex = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => client.GetAsync(new Uri("/relative/url", UriKind.Relative), CancellationToken.None));
            }

            [Fact]
            public async Task WhenClientIsSetupWithBaseUrlAndRequestUsesRelativeUriTheRequestUriUpdatedWithBaseUrl()
            {
                HttpConnectRequest request = null;
                var client = new HttpConnectClient(new Uri("https://www.example.com"), (pipeline) =>
                {
                    pipeline.Use((next) => (ctx) =>
                    {
                        request = ctx.Request;
                        return Task.CompletedTask;
                    });
                });

                await client.GetAsync(new Uri("/relative/url", UriKind.Relative), CancellationToken.None);

                request.RequestUri.Should().Be(new Uri("https://www.example.com/relative/url"));
            }
        }

        public class TheGetAsync_T_Method : HttpConnectClientTests
        {
            [Fact]
            public async Task WhenBuiltWithSimpleMiddlewareThenReturnsExpectedResponse()
            {
                var expectedResponse = new HttpConnectResponse<int>(HttpStatusCode.OK);
                expectedResponse.Content = new HttpConnectResponseContent();
                expectedResponse.Content.Headers.ContentType = new ContentTypeHeader("application/json");
                expectedResponse.Content.Content = "1";
                expectedResponse.Data = 1;
                expectedResponse.Status = HttpConnectResponseStatus.Completed;

                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return Task.CompletedTask;
                    });
                });

                var actualResponse = await client.GetAsync<int>(new Uri("https://www.example.com"), CancellationToken.None);

                actualResponse.Should().BeEquivalentTo(expectedResponse);
            }

            [Fact]
            public async Task WhenBuiltWithMultipleMiddlewaresThenAllMiddlewareExecuted()
            {
                int executionCounter = 0;
                Func<HttpConnectRequestDelegate, HttpConnectRequestDelegate> middleware =
                    (next) => (ctx) => { executionCounter++; return next(ctx); };
                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                    pipline.Use(middleware);
                });

                await client.GetAsync<int>(new Uri("https://www.example.com"), CancellationToken.None);

                executionCounter.Should().Be(8);
            }

            [Theory]
            [InlineData("application/json; charset=utf-8", "{\"name\":\"openwrks\"}")]
            [InlineData("text/json; charset=utf-8", "{\"name\":\"openwrks\"}")]
            public async Task WhenResponseContentTypeContainsCharsetThenCanDeserialize(string contentType, string content)
            {
                var expectedResponse = new HttpConnectResponse<TestType>(HttpStatusCode.OK);
                expectedResponse.Content = new HttpConnectResponseContent();
                expectedResponse.Content.Headers.ContentType = new ContentTypeHeader(contentType);
                expectedResponse.Content.Content = content;
                expectedResponse.Data = new TestType { Name = "openwrks" };
                expectedResponse.Status = HttpConnectResponseStatus.Completed;

                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return Task.CompletedTask;
                    });
                });

                var actualResponse = await client.GetAsync<TestType>(new Uri("https://www.example.com"), CancellationToken.None);

                actualResponse.Should().BeEquivalentTo(expectedResponse);
            }

            [Theory]
            [InlineData("plain/text", "some text")]
            [InlineData("text/html", "<html></html>")]
            [InlineData("application/xml", "<xml>data</xml>")]
            public async Task WhenResponseContentTypeCannotBeDeserializedThenReturnsDefault(string contentType, string content)
            {
                var expectedResponse = new HttpConnectResponse<string>(HttpStatusCode.OK);
                expectedResponse.Content = new HttpConnectResponseContent();
                expectedResponse.Content.Headers.ContentType = new ContentTypeHeader(contentType);
                expectedResponse.Content.Content = content;
                expectedResponse.Data = null;
                expectedResponse.Status = HttpConnectResponseStatus.Completed;

                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return Task.CompletedTask;
                    });
                });

                var actualResponse = await client.GetAsync<string>(new Uri("https://www.example.com"), CancellationToken.None);

                actualResponse.Should().BeEquivalentTo(expectedResponse);
            }

            [Theory]
            [InlineData("application/json; charset=utf-8", "INVALID JSON")]
            [InlineData("text/json; charset=utf-8", "INVALID JSON")]
            public async Task WhenDeserializationThrowsThenCapturedInResponseException(string contentType, string content)
            {
                var expectedResponse = new HttpConnectResponse<TestType>(HttpStatusCode.OK);
                expectedResponse.Content = new HttpConnectResponseContent();
                expectedResponse.Content.Headers.ContentType = new ContentTypeHeader(contentType);
                expectedResponse.Content.Content = content;
                expectedResponse.Status = HttpConnectResponseStatus.Completed;

                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return Task.CompletedTask;
                    });
                });

                var actualResponse = await client.GetAsync<TestType>(new Uri("https://www.example.com"), CancellationToken.None);

                actualResponse.IsSuccess.Should().BeFalse();
                actualResponse.Exception.Should().NotBeNull();
                actualResponse.Data.Should().BeNull();
            }

            [Theory]
            [InlineData(HttpStatusCode.Continue)]
            [InlineData(HttpStatusCode.SwitchingProtocols)]
            [InlineData(HttpStatusCode.Ambiguous)]
            [InlineData(HttpStatusCode.Moved)]
            [InlineData(HttpStatusCode.Found)]
            [InlineData(HttpStatusCode.SeeOther)]
            [InlineData(HttpStatusCode.NotModified)]
            [InlineData(HttpStatusCode.UseProxy)]
            [InlineData(HttpStatusCode.Unused)]
            [InlineData(HttpStatusCode.TemporaryRedirect)]
            [InlineData(HttpStatusCode.BadRequest)]
            [InlineData(HttpStatusCode.Unauthorized)]
            [InlineData(HttpStatusCode.PaymentRequired)]
            [InlineData(HttpStatusCode.Forbidden)]
            [InlineData(HttpStatusCode.NotFound)]
            [InlineData(HttpStatusCode.MethodNotAllowed)]
            [InlineData(HttpStatusCode.NotAcceptable)]
            [InlineData(HttpStatusCode.ProxyAuthenticationRequired)]
            [InlineData(HttpStatusCode.RequestTimeout)]
            [InlineData(HttpStatusCode.Conflict)]
            [InlineData(HttpStatusCode.Gone)]
            [InlineData(HttpStatusCode.LengthRequired)]
            [InlineData(HttpStatusCode.PreconditionFailed)]
            [InlineData(HttpStatusCode.RequestEntityTooLarge)]
            [InlineData(HttpStatusCode.RequestUriTooLong)]
            [InlineData(HttpStatusCode.UnsupportedMediaType)]
            [InlineData(HttpStatusCode.RequestedRangeNotSatisfiable)]
            [InlineData(HttpStatusCode.ExpectationFailed)]
            [InlineData(HttpStatusCode.UpgradeRequired)]
            [InlineData(HttpStatusCode.InternalServerError)]
            [InlineData(HttpStatusCode.NotImplemented)]
            [InlineData(HttpStatusCode.BadGateway)]
            [InlineData(HttpStatusCode.ServiceUnavailable)]
            [InlineData(HttpStatusCode.GatewayTimeout)]
            [InlineData(HttpStatusCode.HttpVersionNotSupported)]
            public async Task WhenMiddlewareReturnsNonSuccessStatusCodeThenResponseIsNotSuccessful(HttpStatusCode statusCode)
            {
                var expectedResponse = new HttpConnectResponse<int>(statusCode);

                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return Task.CompletedTask;
                    });
                });

                var actualResponse = await client.GetAsync<int>(new Uri("https://www.example.com"), CancellationToken.None);

                actualResponse.IsSuccess.Should().BeFalse();
            }

            [Theory]
            [InlineData(HttpStatusCode.Continue)]
            [InlineData(HttpStatusCode.SwitchingProtocols)]
            [InlineData(HttpStatusCode.Ambiguous)]
            [InlineData(HttpStatusCode.Moved)]
            [InlineData(HttpStatusCode.Found)]
            [InlineData(HttpStatusCode.SeeOther)]
            [InlineData(HttpStatusCode.NotModified)]
            [InlineData(HttpStatusCode.UseProxy)]
            [InlineData(HttpStatusCode.Unused)]
            [InlineData(HttpStatusCode.TemporaryRedirect)]
            [InlineData(HttpStatusCode.BadRequest)]
            [InlineData(HttpStatusCode.Unauthorized)]
            [InlineData(HttpStatusCode.PaymentRequired)]
            [InlineData(HttpStatusCode.Forbidden)]
            [InlineData(HttpStatusCode.NotFound)]
            [InlineData(HttpStatusCode.MethodNotAllowed)]
            [InlineData(HttpStatusCode.NotAcceptable)]
            [InlineData(HttpStatusCode.ProxyAuthenticationRequired)]
            [InlineData(HttpStatusCode.RequestTimeout)]
            [InlineData(HttpStatusCode.Conflict)]
            [InlineData(HttpStatusCode.Gone)]
            [InlineData(HttpStatusCode.LengthRequired)]
            [InlineData(HttpStatusCode.PreconditionFailed)]
            [InlineData(HttpStatusCode.RequestEntityTooLarge)]
            [InlineData(HttpStatusCode.RequestUriTooLong)]
            [InlineData(HttpStatusCode.UnsupportedMediaType)]
            [InlineData(HttpStatusCode.RequestedRangeNotSatisfiable)]
            [InlineData(HttpStatusCode.ExpectationFailed)]
            [InlineData(HttpStatusCode.UpgradeRequired)]
            [InlineData(HttpStatusCode.InternalServerError)]
            [InlineData(HttpStatusCode.NotImplemented)]
            [InlineData(HttpStatusCode.BadGateway)]
            [InlineData(HttpStatusCode.ServiceUnavailable)]
            [InlineData(HttpStatusCode.GatewayTimeout)]
            [InlineData(HttpStatusCode.HttpVersionNotSupported)]
            public async Task WhenMiddlewareReturnsNonSuccessResponseThenDoesNotDeserializeResponse(HttpStatusCode statusCode)
            {
                var expectedResponse = new HttpConnectResponse<int>(statusCode);
                expectedResponse.Content = new HttpConnectResponseContent();
                expectedResponse.Content.Headers.ContentType = new ContentTypeHeader("application/json");
                expectedResponse.Content.Content = "{\"error\":\"test_error\"";
                expectedResponse.Data = default;
                expectedResponse.Status = HttpConnectResponseStatus.Completed;

                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return Task.CompletedTask;
                    });
                });

                var actualResponse = await client.GetAsync<int>(new Uri("https://www.example.com"), CancellationToken.None);

                actualResponse.Should().BeEquivalentTo(expectedResponse);
            }

            [Fact]
            public async Task WhenBuiltWithMiddlewareThatThrowsExceptionThenReturnsExceptionInResponse()
            {
                Exception expectedException = new Exception("test exception");
                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        throw expectedException;
                    });
                });

                var actualResponse = await client.GetAsync<int>(new Uri("https://www.example.com"), CancellationToken.None);

                actualResponse.Exception.Should().Be(expectedException);
            }

            [Fact]
            public async Task WhenBuiltWithMiddlewareThatThrowsExceptionAfterResponseHasBeenSetThenOriginalResponseReturnedWithError()
            {
                HttpConnectResponse<int> expectedResponse = new HttpConnectResponse<int>(HttpStatusCode.Unauthorized);
                Exception expectedException = new Exception("test exception");
                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        ctx.Response = expectedResponse;
                        return next(ctx);
                    });
                    pipline.Use(next => ctx =>
                    {
                        throw expectedException;
                    });
                });

                var actualResponse = await client.GetAsync<int>(new Uri("https://www.example.com"), CancellationToken.None);

                actualResponse.Should().BeEquivalentTo(expectedResponse);
                actualResponse.Status.Should().Be(HttpConnectResponseStatus.Error);
            }

            [Fact]
            public async Task WhenBuiltWithMiddlewareThatThrowsExceptionThenRequestStatusIsError()
            {
                Exception expectedException = new Exception("test exception");
                var client = new HttpConnectClient((pipline) =>
                {
                    pipline.Use(next => ctx =>
                    {
                        throw expectedException;
                    });
                });

                var actualResponse = await client.GetAsync<int>(new Uri("https://www.example.com"), CancellationToken.None);

                actualResponse.Status.Should().Be(HttpConnectResponseStatus.Error);
            }

            [Fact]
            public async Task WhenRequestUsesRelativeUriAndNoBaseUrlThenThrows()
            {
                var client = new HttpConnectClient();

                Exception ex = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => client.GetAsync<int>(new Uri("/relative/url", UriKind.Relative), CancellationToken.None));
            }

            [Fact]
            public async Task WhenClientIsSetupWithBaseUrlAndRequestUsesRelativeUriTheRequestUriUpdatedWithBaseUrl()
            {
                HttpConnectRequest request = null;
                var client = new HttpConnectClient(new Uri("https://www.example.com"), (pipeline) =>
                {
                    pipeline.Use((next) => (ctx) =>
                    {
                        request = ctx.Request;
                        ctx.Response = new HttpConnectResponse();

                        return Task.CompletedTask;
                    });
                });

                await client.GetAsync<int>(new Uri("/relative/url", UriKind.Relative), CancellationToken.None);

                request.RequestUri.Should().Be(new Uri("https://www.example.com/relative/url"));
            }
        }

        private class TestType
        {
            public string Name { get; set; }
        }
    }
}
