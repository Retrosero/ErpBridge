using ErpBridge.Core.Domain;
using ErpBridge.Core.Parameters;
using ErpBridge.Core.Stores;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ErpBridge.Core.Tests.Parameters;

/// <summary>
/// When the agent scans a customer's Fora settings and what it does with nothing (P3c).
/// </summary>
public sealed class ForaImportServiceTests
{
    private static AgentErpCompany Company(string database = "MikroDB_V16_03") =>
        new(Guid.NewGuid(), "MERKEZ", "Merkez", database, 1, 0);

    private static ForaScanRow Row(int id) =>
        new("akilli", "plasiyer1", "", "", id, $"P{id}", "3");

    private static ForaImportService Build(FakeRemote remote, FakeSource source) =>
        new(remote, source, NullLogger<ForaImportService>.Instance);

    [Fact]
    public async Task A_company_this_agent_has_no_database_for_is_not_scanned()
    {
        var remote = new FakeRemote();
        var source = new FakeSource("MikroDB_V16_03") { Rows = [Row(58)] };

        (await Build(remote, source).ScanAndUploadAsync(Company("MikroDB_V16_04"))).Should().BeNull();
        source.Scanned.Should().BeFalse();
        remote.Uploads.Should().BeEmpty();
    }

    [Fact]
    public async Task A_database_with_no_Fora_installation_uploads_nothing()
    {
        var remote = new FakeRemote();
        var source = new FakeSource("MikroDB_V16_03");

        // Uploading an empty batch would put a proposal in front of somebody that proposes nothing.
        (await Build(remote, source).ScanAndUploadAsync(Company())).Should().BeNull();
        source.Scanned.Should().BeTrue();
        remote.Uploads.Should().BeEmpty();
    }

    [Fact]
    public async Task What_the_scan_found_is_uploaded_for_that_company()
    {
        var company = Company();
        var remote = new FakeRemote
        {
            Result = new ForaImportResult(Guid.NewGuid(), 2, 1, 1, ["ayrilan"]),
        };
        var source = new FakeSource("MikroDB_V16_03") { Rows = [Row(58), Row(99999)] };

        var result = await Build(remote, source).ScanAndUploadAsync(company);

        result!.Scanned.Should().Be(2);
        result.UnmatchedUsers.Should().ContainSingle();

        var upload = remote.Uploads.Should().ContainSingle().Subject;
        upload.CompanyId.Should().Be(company.Id);
        upload.Rows.Should().HaveCount(2);
    }

    private sealed class FakeRemote : IRemoteApiClient
    {
        public List<(Guid CompanyId, IReadOnlyList<ForaScanRow> Rows)> Uploads { get; } = [];

        public ForaImportResult? Result { get; init; }

        public Task<ForaImportResult?> UploadForaScanAsync(
            Guid erpCompanyId, IReadOnlyList<ForaScanRow> rows, CancellationToken ct = default)
        {
            Uploads.Add((erpCompanyId, rows));
            return Task.FromResult(Result);
        }

        public Task<LicenseValidationResult> ValidateLicenseAsync(string licenseKey, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<AgentRegistrationResult> RegisterAgentAsync(string licenseKey, string machineId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<RemoteJob>> GetPendingJobsAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task SendAckAsync(JobAck ack, CancellationToken ct = default) => throw new NotSupportedException();
        public Task PushBootstrapDataAsync(ErpBridge.Erp.Abstractions.Sync.SyncPackage package, CancellationToken ct = default) => throw new NotSupportedException();
        public Task SendHeartbeatAsync(AgentHeartbeat heartbeat, CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class FakeSource(string database) : IForaScanSource
    {
        public IReadOnlyList<ForaScanRow> Rows { get; init; } = [];

        public bool Scanned { get; private set; }

        public bool CanReach(string sourceDatabase) =>
            string.Equals(database, sourceDatabase, StringComparison.OrdinalIgnoreCase);

        public Task<IReadOnlyList<ForaScanRow>> ScanAsync(AgentErpCompany company, CancellationToken ct = default)
        {
            Scanned = true;
            return Task.FromResult(Rows);
        }
    }
}
