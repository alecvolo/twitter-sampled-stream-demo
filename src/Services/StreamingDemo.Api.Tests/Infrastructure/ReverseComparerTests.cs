using FluentAssertions;
using StreamingDemo.Api.Infrastructure;
using Xunit;

namespace StreamingDemo.Api.Tests.Infrastructure
{
    public class ReverseComparerTests
    {
        [Theory]
        [InlineData("value1", "value2")]
        [InlineData("value", "value")]
        [InlineData(null, null)]
        public void ReverseComparer_Should_Compare_In_Reserve_Order(string value1, string value2)
        {
            var stringComparer = StringComparer.CurrentCulture;
            var reverseComparer = new ReverseComparer<string>(stringComparer);
            (stringComparer.Compare(value1, value2) + reverseComparer.Compare(value1, value2)).Should().Be(0);
        }
    }
}