using System;
using FluentAssertions;
using HttpConnect.Headers;
using Xunit;

namespace HttpConnect.Tests.Headers
{
    public class HttpConnectHeadersTests
    {
        public class TheAddMethod_NameValue
        {
            [Fact]
            public void WhenAddIsUsedThenCountIncreases()
            {
                var headers = new HttpConnectHeaders();
                headers.Add("name-1", "value-1");
                headers.Add("name-2", "value-2");
                headers.Add("name-3", "value-3");
                headers.Add("name-4", "value-4");
                headers.Add("name-5", "value-5");

                headers.Should().HaveCount(5);
            }

            [Fact]
            public void WhenAddIsUsedAndNameIsTheSameThenCountDoesNotIncrease()
            {
                var headers = new HttpConnectHeaders();
                headers.Add("name-1", "value-1");
                headers.Add("name-1", "value-1");
                headers.Add("name-1", "value-1");
                headers.Add("name-1", "value-1");
                headers.Add("name-1", "value-1");

                headers.Should().HaveCount(1);
            }
        }

        public class TheTryGetValueMethod
        {
            [Fact]
            public void ReturnsTrueWhenHeaderCanBeFound()
            {
                var headers = new HttpConnectHeaders();
                headers.Add("x-header", "value");

                headers.TryGetValue("x-header", out string _).Should().BeTrue();
            }

            [Fact]
            public void ReturnsValueInOutParameterWhenHeaderCanBeFound()
            {
                var headers = new HttpConnectHeaders();
                headers.Add("x-header", "my-value");

                headers.TryGetValue("x-header", out string value);

                value.Should().Be("my-value");
            }

            [Fact]
            public void ReturnsFalseWhenHeaderCannotBeFound()
            {
                var headers = new HttpConnectHeaders();
                headers.Add("x-header", "value");

                headers.TryGetValue("UNKNOWN", out string _).Should().BeFalse();
            }

            [Fact]
            public void ReturnsNullInOutParameterWhenHeaderCannotBeFound()
            {
                var headers = new HttpConnectHeaders();
                headers.Add("x-header", "my-value");

                headers.TryGetValue("UNKNOWN", out string value);

                value.Should().BeNull();
            }
        }
    }
}
