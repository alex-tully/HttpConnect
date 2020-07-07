using System;
using FluentAssertions;
using HttpConnect.Headers;
using Xunit;

namespace HttpConnect.Tests.Headers
{
    public class AcceptHeaderTests
    {
        [Fact]
        public void ThrowsWhenMediaTypeIsNullEmptyOrWhiteSpace()
        {
            Assert.Throws<ArgumentException>(() => new AcceptHeader(null));
            Assert.Throws<ArgumentException>(() => new AcceptHeader(string.Empty));
            Assert.Throws<ArgumentException>(() => new AcceptHeader("\t    "));
        }

        [Fact]
        public void WhenConstructedSetsTheNamePropertyToAccept()
        {
            var header = new AcceptHeader("media-type");

            header.Name.Should().Be("Accept");
        }

        [Fact]
        public void WhenConstructedSetsTheValueProperty()
        {
            string expectedValue = "application/json";

            var header = new AcceptHeader(expectedValue);

            header.Value.Should().Be(expectedValue);
        }
    }
}
