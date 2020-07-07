using System;
using FluentAssertions;
using HttpConnect.Content;
using Xunit;

namespace HttpConnect.Tests.Content
{
    public class JsonContentTests
    {
        [Fact]
        public void ThrowsWhenContentIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new JsonContent(null));
        }

        [Fact]
        public void WhenConstructedSetsTheContentTypeHeader()
        {
            var content = new JsonContent("{}");

            content.Headers.ContentType.Value.Should().Be("application/json");
        }

        [Fact]
        public void WhenConstructedThenCanAccessInitialContentThroughContentProperty()
        {
            object content = new { test = "test" };

            var stringContent = new JsonContent(content);

            stringContent.Content.Should().Be(content);
        }

        [Fact]
        public void WhenSerializedThenReturnsJsonSerializedContent()
        {
            object content = new { test = "test" };

            var stringContent = new JsonContent(content);

            stringContent.Serialize().Should().Be("{\"test\":\"test\"}");
        }
    }
}
