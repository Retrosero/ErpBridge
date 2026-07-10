using System.Text.RegularExpressions;
using ErpBridge.Shared;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Core.Tests;

/// <summary>
/// Unit tests for <see cref="HashUtil"/> — used to compute the idempotency checksum.
/// </summary>
public class HashUtilTests
{
    [Fact]
    public void Sha256Hex_produces_64_lowercase_hex_chars()
    {
        var digest = HashUtil.Sha256Hex("hello");

        digest.Should().HaveLength(64);
        digest.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void Sha256Hex_is_deterministic_for_same_input()
    {
        var a = HashUtil.Sha256Hex("erpbridge");
        var b = HashUtil.Sha256Hex("erpbridge");

        a.Should().Be(b);
    }

    [Fact]
    public void Sha256Hex_differs_for_different_input()
    {
        var a = HashUtil.Sha256Hex("one");
        var b = HashUtil.Sha256Hex("two");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Sha256Hex_matches_known_digest_for_empty_string()
    {
        // SHA-256("") = e3b0c44...b855
        var digest = HashUtil.Sha256Hex(string.Empty);

        digest.Should().Be("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855");
    }

    [Fact]
    public void Sha256Hex_treats_null_as_empty()
    {
        var nullResult = HashUtil.Sha256Hex(null!);
        var emptyResult = HashUtil.Sha256Hex(string.Empty);

        nullResult.Should().Be(emptyResult);
    }
}
