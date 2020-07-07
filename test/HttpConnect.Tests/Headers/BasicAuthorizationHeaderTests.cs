using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using HttpConnect.Headers;
using Xunit;

namespace HttpConnect.Tests.Headers
{
    public class BasicAuthorizationHeaderTests
    {
        [Fact]
        public void ThrowsWhenUsernameIsNullEmptyOrWhiteSpace()
        {
            Assert.Throws<ArgumentException>(() => new BasicAuthorizationHeader(null, "password"));
            Assert.Throws<ArgumentException>(() => new BasicAuthorizationHeader(string.Empty, "password"));
            Assert.Throws<ArgumentException>(() => new BasicAuthorizationHeader("\t    ", "password"));
        }

        [Fact]
        public void ThrowsWhenPasswordIsNullEmptyOrWhiteSpace()
        {
            Assert.Throws<ArgumentException>(() => new BasicAuthorizationHeader("username", null));
            Assert.Throws<ArgumentException>(() => new BasicAuthorizationHeader("username", string.Empty));
            Assert.Throws<ArgumentException>(() => new BasicAuthorizationHeader("username", "\t    "));
        }

        [Fact]
        public void WhenConstructedSetsTheNamePropertyToAuthorization()
        {
            var header = new BasicAuthorizationHeader("username", "password");

            header.Name.Should().Be("Authorization");
        }

        [Fact]
        public void WhenConstructedSetsTheValueProperty()
        {
            var header = new BasicAuthorizationHeader("username", "password");

            string expected = Convert.ToBase64String(Encoding.UTF8.GetBytes($"username:password"));

            header.Value.Should().Be($"Basic {expected}");
        }
    }
}
