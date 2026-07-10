using System.Security.Cryptography;
using ErpBridge.Core.Stores;
using ErpBridge.LocalStore.ProtectedConfig;
using FluentAssertions;
using Xunit;

namespace ErpBridge.LocalStore.Tests.ProtectedConfig;

/// <summary>
/// Tests for <see cref="AesProtectedConfigProvider"/>. Exercises the AES-256-GCM roundtrip,
/// non-deterministic nonce uniqueness, UTF-8 fidelity, and tag-verification behaviour.
/// </summary>
public class AesProtectedConfigProviderTests
{
    private static IProtectedConfigProvider NewProvider(byte[]? key = null)
    {
        return new AesProtectedConfigProvider(key ?? RandomNumberGenerator.GetBytes(32));
    }

    [Fact]
    public void Roundtrip_returns_original_plaintext()
    {
        IProtectedConfigProvider sut = NewProvider();

        var protectedValue = sut.Protect("hello-world");
        var decrypted = sut.Unprotect(protectedValue);

        decrypted.Should().Be("hello-world");
    }

    [Fact]
    public void Roundtrip_handles_empty_string()
    {
        IProtectedConfigProvider sut = NewProvider();

        var protectedValue = sut.Protect(string.Empty);
        var decrypted = sut.Unprotect(protectedValue);

        decrypted.Should().Be(string.Empty);
        sut.IsProtected(protectedValue).Should().BeTrue();
    }

    [Fact]
    public void Roundtrip_preserves_unicode_characters()
    {
        IProtectedConfigProvider sut = NewProvider();

        // Turkish characters; verifies UTF-8 path of the protected blob.
        const string input = "Şifre: güvenlik-123 ğüşiöçĞÜŞİÖÇ";
        var protectedValue = sut.Protect(input);

        sut.Unprotect(protectedValue).Should().Be(input);
    }

    [Fact]
    public void Roundtrip_handles_payload_larger_than_one_kib()
    {
        IProtectedConfigProvider sut = NewProvider();

        var input = new string('A', 1000);
        var protectedValue = sut.Protect(input);

        sut.Unprotect(protectedValue).Should().Be(input);
    }

    [Fact]
    public void Same_plaintext_produces_different_protected_values_due_to_unique_nonce()
    {
        IProtectedConfigProvider sut = NewProvider();

        var first = sut.Protect("repeat-secret");
        var second = sut.Protect("repeat-secret");

        first.Should().NotBe(second, "AES-GCM must use a fresh 96-bit nonce per call.");
        // Both still decrypt to the same plaintext.
        sut.Unprotect(first).Should().Be("repeat-secret");
        sut.Unprotect(second).Should().Be("repeat-secret");
    }

    [Fact]
    public void Unprotect_throws_when_tag_is_tampered()
    {
        IProtectedConfigProvider sut = NewProvider();

        var protectedValue = sut.Protect("tamper-me");
        var base64 = protectedValue[AesProtectedConfigProvider.ProtectedPrefix.Length..];
        var blob = Convert.FromBase64String(base64);

        // Flip the last byte of the appended tag — the GCM tag covers the ciphertext
        // so any single-bit change must cause decryption to fail.
        blob[blob.Length - 1] ^= 0xFF;

        var tampered = AesProtectedConfigProvider.ProtectedPrefix + Convert.ToBase64String(blob);

        var act = () => sut.Unprotect(tampered);
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Unprotect_throws_when_ciphertext_is_tampered()
    {
        IProtectedConfigProvider sut = NewProvider();

        var protectedValue = sut.Protect("ciphertext-tampered");
        var base64 = protectedValue[AesProtectedConfigProvider.ProtectedPrefix.Length..];
        var blob = Convert.FromBase64String(base64);

        // Flip the first byte of the ciphertext body (right after the nonce).
        blob[AesProtectedConfigProvider.NonceSize] ^= 0xFF;

        var tampered = AesProtectedConfigProvider.ProtectedPrefix + Convert.ToBase64String(blob);

        var act = () => sut.Unprotect(tampered);
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Unprotect_throws_when_value_lacks_protected_marker()
    {
        IProtectedConfigProvider sut = NewProvider();

        var act = () => sut.Unprotect("plaintext-not-encrypted");
        act.Should().Throw<CryptographicException>()
            .WithMessage("*not in protected form*");
    }

    [Fact]
    public void Unprotect_throws_when_protected_value_is_too_short()
    {
        IProtectedConfigProvider sut = NewProvider();

        // Just enough to clear IsProtected() (the prefix is appended) but trivially short.
        var truncated = AesProtectedConfigProvider.ProtectedPrefix + Convert.ToBase64String(new byte[5]);

        var act = () => sut.Unprotect(truncated);
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Constructor_rejects_key_of_wrong_length()
    {
        var act = () => new AesProtectedConfigProvider(new byte[16]); // AES-128 size but we require AES-256.
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsProtected_returns_false_for_arbitrary_strings()
    {
        IProtectedConfigProvider sut = NewProvider();

        sut.IsProtected(string.Empty).Should().BeFalse();
        sut.IsProtected("plaintext").Should().BeFalse();
        sut.IsProtected("ENC:v1:").Should().BeFalse("the marker is case-sensitive and lowercase");
    }
}
