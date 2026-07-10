using ErpBridge.Shared;
using FluentAssertions;

namespace ErpBridge.Shared.Tests;

/// <summary>
/// Unit tests for <see cref="ConnectionStringMasker"/> — the single chokepoint
/// for keeping SQL passwords out of log files. The masker must mask
/// <c>Password=</c>, <c>Pwd=</c>, <c>User ID=</c>, and <c>UID=</c> fragments
/// regardless of casing, and must leave the rest of the connection string
/// untouched.
/// </summary>
public class ConnectionStringMaskerTests
{
    [Fact]
    public void MaskPassword_redacts_the_canonical_Password_key()
    {
        var input = "Password=secret123;User=sa";

        var masked = ConnectionStringMasker.MaskPassword(input);

        masked.Should().Be("Password=***REDACTED***;User=sa");
    }

    [Fact]
    public void MaskPassword_redacts_the_short_Pwd_key()
    {
        var input = "Data Source=localhost;Pwd=secret;Initial Catalog=db";

        var masked = ConnectionStringMasker.MaskPassword(input);

        masked.Should().Be("Data Source=localhost;Pwd=***REDACTED***;Initial Catalog=db");
    }

    [Fact]
    public void MaskPassword_redacts_User_ID_and_UID_keys()
    {
        var input = "User ID=sa;Password=foo;UID=admin;Pwd=bar";

        var masked = ConnectionStringMasker.MaskPassword(input);

        masked.Should().Contain("User ID=***REDACTED***");
        masked.Should().Contain("Password=***REDACTED***");
        masked.Should().Contain("UID=***REDACTED***");
        masked.Should().Contain("Pwd=***REDACTED***");
    }

    [Fact]
    public void MaskPassword_is_case_insensitive_for_keys()
    {
        var input = "password=foo;PWD=bar;uSeR iD=baz;uid=qux";

        var masked = ConnectionStringMasker.MaskPassword(input);

        // The key prefix is preserved verbatim (case-sensitive replacement of value part only).
        masked.Should().Contain("password=***REDACTED***");
        masked.Should().Contain("PWD=***REDACTED***");
        masked.Should().Contain("uSeR iD=***REDACTED***");
        masked.Should().Contain("uid=***REDACTED***");
    }

    [Fact]
    public void MaskPassword_handles_multiple_equals_signs_in_value()
    {
        // Connection-string values may legally contain '=' (rare, but valid).
        // The masker must consume the entire value up to the next ';' — and the
        // already-substituted Password=***REDACTED*** must NOT be re-matched by
        // a second regex pass over the substituted string.
        var input = "Password=ab=cd=ef;User ID=sa";

        var masked = ConnectionStringMasker.MaskPassword(input);

        // Both keys are redacted — Password AND User ID are secret-bearing keys.
        masked.Should().Be("Password=***REDACTED***;User ID=***REDACTED***");
        masked.Should().NotContain("ab=cd=ef");
        masked.Should().NotContain("sa");
    }

    [Fact]
    public void MaskPassword_returns_empty_string_for_null_or_empty()
    {
        ConnectionStringMasker.MaskPassword(null).Should().Be(string.Empty);
        ConnectionStringMasker.MaskPassword(string.Empty).Should().Be(string.Empty);
    }

    [Fact]
    public void MaskPassword_does_not_touch_unrelated_keys()
    {
        var input = "Server=tcp:example.com,1433;Database=MIKRO16;Connect Timeout=30";

        var masked = ConnectionStringMasker.MaskPassword(input);

        masked.Should().Be(input);
    }

    [Fact]
    public void MaskPassword_handles_trailing_password_without_semicolon()
    {
        var input = "Password=foo";

        var masked = ConnectionStringMasker.MaskPassword(input);

        masked.Should().Be("Password=***REDACTED***");
    }

    [Fact]
    public void MaskForLog_redacts_only_when_a_secret_key_is_present()
    {
        var plain = "Connection established at 2026-07-09 10:00:00";

        ConnectionStringMasker.MaskForLog(plain).Should().Be(plain);
    }

    [Fact]
    public void MaskForLog_redacts_password_inside_arbitrary_log_text()
    {
        var logLine = "Failed to open connection (Password=secret;Database=foo)";

        var masked = ConnectionStringMasker.MaskForLog(logLine);

        masked.Should().Contain("Password=***REDACTED***");
        masked.Should().NotContain("secret");
        masked.Should().Contain("Database=foo");
    }

    [Fact]
    public void MaskForLog_returns_empty_string_for_null_or_empty()
    {
        ConnectionStringMasker.MaskForLog(null).Should().Be(string.Empty);
        ConnectionStringMasker.MaskForLog(string.Empty).Should().Be(string.Empty);
    }

    [Fact]
    public void RedactedMarker_is_stable_across_calls()
    {
        ConnectionStringMasker.RedactedMarker.Should().Be("***REDACTED***");
    }
}