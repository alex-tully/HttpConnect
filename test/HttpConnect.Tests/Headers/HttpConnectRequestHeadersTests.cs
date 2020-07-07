using System;
using FluentAssertions;
using HttpConnect.Headers;
using Xunit;

namespace HttpConnect.Tests.Headers
{
    public class HttpConnectRequestHeadersTests
    {
        [Fact]
        public void WhenAuthorizationHeaderIsAddedViaPropertyItCanBeRetrievedThroughProperty()
        {
            var headers = new HttpConnectRequestHeaders();
            headers.Authorization = new AuthorizationHeader("Bearer", "xxx");

            headers.Authorization.Should().NotBeNull();
            headers.Authorization.Value.Should().Be("Bearer xxx");
        }

        [Fact]
        public void WhenAcceptHeaderIsAddedViaPropertyItCanBeRetrievedThroughProperty()
        {
            var headers = new HttpConnectRequestHeaders();
            headers.Accept = new AcceptHeader("application/json");

            headers.Accept.Should().NotBeNull();
            headers.Accept.Value.Should().Be("application/json");
        }

        [Fact]
        public void WhenAuthorizationHeaderIsAddedItCanBeRetrievedThroughProperty()
        {
            var headers = new HttpConnectRequestHeaders();
            headers.Add("Authorization", "Bearer xxx");

            headers.Authorization.Should().NotBeNull();
            headers.Authorization.Value.Should().Be("Bearer xxx");
        }

        [Fact]
        public void WhenAcceptHeaderIsAddedItCanBeRetrievedThroughProperty()
        {
            var headers = new HttpConnectRequestHeaders();
            headers.Add("Accept", "application/json");

            headers.Accept.Should().NotBeNull();
            headers.Accept.Value.Should().Be("application/json");
        }
    }
}
