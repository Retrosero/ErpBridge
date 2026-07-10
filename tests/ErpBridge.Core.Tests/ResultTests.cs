using ErpBridge.Shared;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Core.Tests;

/// <summary>
/// Unit tests for the <see cref="Result{T}"/> monad.
/// </summary>
public class ResultTests
{
    [Fact]
    public void Ok_success_carries_value_and_is_success()
    {
        var result = Result<int>.Ok(5);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(5);
        result.Error.Should().BeNull();
        result.ErrorCode.Should().BeNull();
    }

    [Fact]
    public void Fail_with_code_and_message_is_failure()
    {
        var result = Result<int>.Fail("ERR_X", "explosion");

        result.IsSuccess.Should().BeFalse();
        result.Value.Should().Be(0);
        result.ErrorCode.Should().Be("ERR_X");
        result.Error.Should().Be("explosion");
    }

    [Fact]
    public void Fail_with_error_record_is_failure()
    {
        var result = Result<int>.Fail(new Error("ERR_Y", "boom"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("ERR_Y");
        result.Error.Should().Be("boom");
    }

    [Fact]
    public void ValueOrThrow_returns_value_for_success()
    {
        Result<int>.Ok(42).ValueOrThrow().Should().Be(42);
    }

    [Fact]
    public void ValueOrThrow_throws_for_failure()
    {
        var act = () => Result<int>.Fail("ERR_Z", "nope").ValueOrThrow();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ERR_Z*");
    }
}
