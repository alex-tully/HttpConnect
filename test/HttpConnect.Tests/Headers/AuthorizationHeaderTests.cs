using System;
using FluentAssertions;
using HttpConnect.Headers;
using Xunit;

namespace HttpConnect.Tests.Headers
{
    public class AuthorizationHeaderTests
    {
        [Fact]
        public void ThrowsWhenSchemeIsNullEmptyOrWhiteSpace()
        {
            Assert.Throws<ArgumentException>(() => new AuthorizationHeader(null));
            Assert.Throws<ArgumentException>(() => new AuthorizationHeader(string.Empty));
            Assert.Throws<ArgumentException>(() => new AuthorizationHeader("\t    "));

            Assert.Throws<ArgumentException>(() => new AuthorizationHeader(null, "parameter"));
            Assert.Throws<ArgumentException>(() => new AuthorizationHeader(string.Empty, "parameter"));
            Assert.Throws<ArgumentException>(() => new AuthorizationHeader("\t    ", "parameter"));
        }

        [Fact]
        public void ThrowsWhenParameterIsNullEmptyOrWhiteSpace()
        {
            Assert.Throws<ArgumentException>(() => new AuthorizationHeader("scheme", null));
            Assert.Throws<ArgumentException>(() => new AuthorizationHeader("scheme", string.Empty));
            Assert.Throws<ArgumentException>(() => new AuthorizationHeader("scheme", "\t    "));
        }

        [Fact]
        public void WhenConstructedSetsTheNamePropertyToAuthorization()
        {
            var header = new AuthorizationHeader("scheme", "parameter");

            header.Name.Should().Be("Authorization");
        }

        [Fact]
        public void WhenConstructedSetsTheValueProperty()
        {
            var header = new AuthorizationHeader("scheme", "parameter");

            header.Value.Should().Be("scheme parameter");
        }
    }
}
