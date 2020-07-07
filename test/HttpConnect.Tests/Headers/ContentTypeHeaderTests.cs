using System;
using FluentAssertions;
using HttpConnect.Headers;
using Xunit;

namespace HttpConnect.Tests.Headers
{
    public class ContentTypeHeaderTests
    {
        [Fact]
        public void ThrowsWhenMediaTypeIsNullEmptyOrWhiteSpace()
        {
            Assert.Throws<ArgumentException>(() => new ContentTypeHeader(null));
            Assert.Throws<ArgumentException>(() => new ContentTypeHeader(string.Empty));
            Assert.Throws<ArgumentException>(() => new ContentTypeHeader("\t    "));
        }

        [Fact]
        public void WhenConstructedSetsTheNamePropertyToAccept()
        {
            var header = new ContentTypeHeader("media-type");

            header.Name.Should().Be("Content-Type");
        }

        [Fact]
        public void WhenConstructedSetsTheValueProperty()
        {
            string expectedValue = "application/json";

            var header = new ContentTypeHeader(expectedValue);

            header.Value.Should().Be(expectedValue);
        }
    }
}
