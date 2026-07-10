using System.Security.Cryptography;
using ErpBridge.LocalStore.ProtectedConfig;
using FluentAssertions;
using Xunit;

namespace ErpBridge.LocalStore.Tests.ProtectedConfig;

/// <summary>
/// Tests for <see cref="KeyStore"/>. Verifies file creation behaviour, idempotency,
/// and platform-appropriate file-mode hardening. Tests do not pollute
/// the real key file location; each owns a <c>Path.GetTempPath()</c>/GUID directory.
/// </summary>
public class KeyStoreTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _keyPath;

    public KeyStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"erpbridge-keystore-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _keyPath = Path.Combine(_tempDir, "protected-config.key");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }
        catch
        {
            // Best-effort cleanup; tmpdir will be reclaimed by the OS eventually.
        }
    }

    [Fact]
    public void LoadOrCreateKey_creates_file_when_absent()
    {
        File.Exists(_keyPath).Should().BeFalse("pre-condition: temp key file must not exist");

        var key = KeyStore.LoadOrCreateKey(_keyPath);

        key.Should().NotBeNull().And.HaveCount(KeyStore.KeyLengthBytes);
        File.Exists(_keyPath).Should().BeTrue();
    }

    [Fact]
    public void LoadOrCreateKey_returns_same_bytes_when_called_twice()
    {
        var first = KeyStore.LoadOrCreateKey(_keyPath);
        var second = KeyStore.LoadOrCreateKey(_keyPath);

        first.Should().Equal(second, "a pre-existing key file must not be silently regenerated");
    }

    [Fact]
    public void LoadOrCreateKey_throws_when_file_length_is_wrong()
    {
        File.WriteAllBytes(_keyPath, new byte[16]); // wrong length

        var act = () => KeyStore.LoadOrCreateKey(_keyPath);
        act.Should().Throw<CryptographicException>()
            .WithMessage("*has length 16 but 32 bytes are required*");
    }

    [Fact]
    public void RotateKey_writes_a_new_key_with_same_length()
    {
        var original = KeyStore.LoadOrCreateKey(_keyPath);
        KeyStore.RotateKey(_keyPath);
        var rotated = KeyStore.LoadOrCreateKey(_keyPath);

        rotated.Should().HaveCount(KeyStore.KeyLengthBytes);
        // Statistically the bytes must differ — 1 in 2^256 collision chance.
        rotated.Should().NotEqual(original);
    }

    [Fact]
    public void ApplyDefaultProtection_enforces_unix_600_on_linux_and_macos()
    {
        if (OperatingSystem.IsWindows())
        {
            return; // platform-skipped — Linux/macOS only.
        }

        KeyStore.LoadOrCreateKey(_keyPath);
        var mode = File.GetUnixFileMode(_keyPath);

        // The exact mode bits should be UserRead | UserWrite (0600).
        (mode & UnixFileMode.UserRead).Should().NotBe(UnixFileMode.None);
        (mode & UnixFileMode.UserWrite).Should().NotBe(UnixFileMode.None);
        (mode & UnixFileMode.GroupRead).Should().Be(UnixFileMode.None, "group must not be able to read the key");
        (mode & UnixFileMode.GroupWrite).Should().Be(UnixFileMode.None);
        (mode & UnixFileMode.OtherRead).Should().Be(UnixFileMode.None);
        (mode & UnixFileMode.OtherWrite).Should().Be(UnixFileMode.None);
    }

    [Fact]
    public void ApplyDefaultProtection_marks_file_hidden_on_windows()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // platform-skipped — Windows only.
        }

        KeyStore.LoadOrCreateKey(_keyPath);
        var attrs = File.GetAttributes(_keyPath);

        (attrs & FileAttributes.Hidden).Should().NotBe(FileAttributes.None);
    }

    [Fact]
    public void SetUnix600_is_a_noop_on_windows()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // platform-skipped — Windows only.
        }

        KeyStore.LoadOrCreateKey(_keyPath);

        // Should not throw and should not change file attributes.
        var act = () => KeyStore.SetUnix600(_keyPath);
        act.Should().NotThrow();
    }
}
