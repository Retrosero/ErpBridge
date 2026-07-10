using ErpBridge.Shared;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Core.Tests;

/// <summary>
/// Unit tests for <see cref="StringExtensions"/>.
/// </summary>
public class StringExtensionsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void SafeTrim_returns_null_for_null_or_whitespace(string? input)
    {
        input.SafeTrim().Should().BeNull();
    }

    [Fact]
    public void SafeTrim_trims_surrounding_whitespace()
    {
        "  hello  ".SafeTrim().Should().Be("hello");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\r\n")]
    public void IsNullOrEmptyInvariant_true_for_empty_whitespace(string? input)
    {
        input.IsNullOrEmptyInvariant().Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmptyInvariant_false_for_real_text()
    {
        "x".IsNullOrEmptyInvariant().Should().BeFalse();
        "  x  ".IsNullOrEmptyInvariant().Should().BeFalse();
    }
}
