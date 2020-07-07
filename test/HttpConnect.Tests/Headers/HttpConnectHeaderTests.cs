using System;
using FluentAssertions;
using HttpConnect.Headers;
using Xunit;

namespace HttpConnect.Tests.Headers
{
    public class HttpConnectHeaderTests
    {
        [Fact]
        public void ThrowsWhenNameIsNullEmptyOrWhiteSpace()
        {
            Assert.Throws<ArgumentException>(() => new HttpConnectHeader(null, "value"));
            Assert.Throws<ArgumentException>(() => new HttpConnectHeader(string.Empty, "value"));
            Assert.Throws<ArgumentException>(() => new HttpConnectHeader("\t    ", "value"));
        }

        [Fact]
        public void WhenConstructedSetsTheNameProperty()
        {
            var header = new HttpConnectHeader("name", "value");

            header.Name.Should().Be("name");
        }

        [Fact]
        public void WhenConstructedSetsTheValueProperty()
        {
            var header = new HttpConnectHeader("name", "value");

            header.Value.Should().Be("value");
        }

        [Fact]
        public void WhenConstructedAndValueIsNullThenSetsToStringEmpty()
        {
            var header = new HttpConnectHeader("name", null);

            header.Value.Should().BeEmpty();
        }
    }
}
