using ErpBridge.CentralApi.Native;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Native;

/// <summary>
/// Codex #192: a void takes a job's movement rows by key. Keys are <c>"{job}|{suffix}"</c> and, once reversed,
/// <c>"{job}|{suffix}|void"</c>; a job whose own id starts with <c>"{job}|"</c> must not be mistaken for it.
/// </summary>
public sealed class NativeMovementOwnershipTests
{
    [Theory]
    [InlineData("abc|1", true)]
    [InlineData("abc|sale", true)]
    [InlineData("abc|1|void", true)]
    [InlineData("abc|1|1", false)]
    [InlineData("abc|1|1|void", false)]
    [InlineData("abc|", false)]
    [InlineData("abcd|1", false)]
    [InlineData("abc", false)]
    public void A_row_belongs_to_the_job_only_by_its_exact_shape(string recordKey, bool owned) =>
        NativeDocumentProcessor.OwnedByJob(recordKey, "abc").Should().Be(owned);
}
