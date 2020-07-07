using System;
using System.Collections.Generic;
using FluentAssertions;
using HttpConnect.Content;
using Xunit;

namespace HttpConnect.Tests.Content
{
    public class FormUrlEncodedContentTests
    {
        [Fact]
        public void ThrowsWhenContentIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FormUrlEncodedContent(null));
        }

        [Fact]
        public void WhenConstructedSetsTheContentTypeHeader()
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                ["test"] = "test"
            });

            content.Headers.ContentType.Value.Should().Be("application/x-www-form-urlencoded");
        }

        [Fact]
        public void WhenConstructedThenCanAccessContentThroughContentProperty()
        {
            var content = new Dictionary<string, string>
            {
                ["test-1"] = "test-1",
                ["test-2"] = "test-2",
                ["test-3"] = "test-3",
                ["test-4"] = "test-4",
                ["test-5"] = "test-5"
            };

            var stringContent = new FormUrlEncodedContent(content);

            stringContent.Content.Should().BeEquivalentTo(content);
        }

        [Fact]
        public void WhenSerializedThenReturnsFormUrlEncodedSerializedContent()
        {
            // we expect values to have been encoded
            var content = new Dictionary<string, string>
            {
                ["test-1"] = "test 1",
                ["test-2"] = "test 2",
                ["test-3"] = "",
                ["test 4"] = "test 4",
                ["test-5"] = "test-5"
            };
            string expected = "test-1=test+1&test-2=test+2&test-3=&test+4=test+4&test-5=test-5";

            var stringContent = new FormUrlEncodedContent(content);

            stringContent.Serialize().Should().Be(expected);
        }

        [Fact]
        public void WhenSerializedAndContentEmptyThenReturnsEmptyString()
        {
            // we expect values to have been encoded
            var content = new Dictionary<string, string>();

            var stringContent = new FormUrlEncodedContent(content);

            stringContent.Serialize().Should().Be(string.Empty);
        }
    }
}
