using System;
using FluentAssertions;
using HttpConnect.Content;
using Xunit;

namespace HttpConnect.Tests.Content
{
    public class StringContentTests
    {
        [Fact]
        public void ThrowsWhenContentIsNullEmptyOrWhiteSpace()
        {
            Assert.Throws<ArgumentException>(() => new StringContent(null, "mediaType"));
            Assert.Throws<ArgumentException>(() => new StringContent(string.Empty, "mediaType"));
            Assert.Throws<ArgumentException>(() => new StringContent("\t    ", "mediaType"));
        }

        [Fact]
        public void ThrowsWhenMediaTypeIsNullEmptyOrWhiteSpace()
        {
            Assert.Throws<ArgumentException>(() => new StringContent("content", null));
            Assert.Throws<ArgumentException>(() => new StringContent("content", string.Empty));
            Assert.Throws<ArgumentException>(() => new StringContent("content", "\t    "));
        }

        [Fact]
        public void WhenConstructedSetsTheContentTypeHeader()
        {
            var content = new StringContent("content", "mediaType");

            content.Headers.ContentType.Value.Should().Be("mediaType");
        }

        [Fact]
        public void WhenConstructedThenCanAccessInitialContentThroughContentProperty()
        {
            string content = "THIS IS A TEST";

            var stringContent = new StringContent(content, "plain/text");

            stringContent.Content.Should().Be(content);
        }

        [Fact]
        public void WhenSerializedThenReturnsInitialContent()
        {
            string content = "THIS IS A TEST";

            var stringContent = new StringContent(content, "plain/text");

            stringContent.Serialize().Should().Be(content);
        }
    }
}
