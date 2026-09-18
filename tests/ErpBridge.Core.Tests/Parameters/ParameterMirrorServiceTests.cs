using ErpBridge.Core.Domain;
using ErpBridge.Core.Parameters;
using ErpBridge.Core.Stores;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ErpBridge.Core.Tests.Parameters;

/// <summary>
/// What the agent decides when it mirrors parameters (P3b): which company it can serve, what it
/// applies, and what it reports. The SQL half is elsewhere; these pin the decisions.
/// </summary>
public sealed class ParameterMirrorServiceTests
{
    private static AgentErpCompany Company(string database, string code = "MERKEZ") =>
        new(Guid.NewGuid(), code, "Merkez", database, 1, 0);

    private static AgentParameterRow Row(int id, string value) =>
        new("akilli", "plasiyer1", "", "", id, $"P{id}", value);

    private static ParameterMirrorService Build(FakeRemote remote, FakeTarget target) =>
        new(remote, target, NullLogger<ParameterMirrorService>.Instance);

    [Fact]
    public async Task Nothing_to_do_when_the_agent_is_assigned_to_nothing()
    {
        var remote = new FakeRemote();
        var target = new FakeTarget("MikroDB_V16_03");

        (await Build(remote, target).RunAsync()).Should().Be(ParameterMirrorService.RunResult.Nothing);
        target.Applied.Should().BeEmpty();
    }

    [Fact]
    public async Task Only_the_company_this_agent_has_a_database_for_is_mirrored()
    {
        var mine = Company("MikroDB_V16_03");
        var theirs = Company("MikroDB_V16_04", "SUBE");

        var remote = new FakeRemote { Companies = [mine, theirs] };
        remote.States[mine.Id] = new AgentParameterState(mine.Id, mine.SourceDatabase, 4, [Row(58, "3")]);
        remote.States[theirs.Id] = new AgentParameterState(theirs.Id, theirs.SourceDatabase, 9, [Row(58, "9")]);

        var target = new FakeTarget("MikroDB_V16_03");
        var result = await Build(remote, target).RunAsync();

        // A company the agent cannot reach is skipped, not failed: an agent pointed at one
        // database is the normal case, and a failure there would bury the ones that matter.
        result.Should().Be(new ParameterMirrorService.RunResult(Mirrored: 1, Skipped: 1, Failed: 0));
        target.Applied.Should().ContainSingle().Which.Key.Should().Be(mine.Id);
    }

    [Fact]
    public async Task The_database_name_is_matched_without_regard_to_case()
    {
        var company = Company("mikrodb_v16_03");
        var remote = new FakeRemote { Companies = [company] };
        remote.States[company.Id] = new AgentParameterState(company.Id, company.SourceDatabase, 1, [Row(58, "3")]);

        var target = new FakeTarget("MikroDB_V16_03");

        // SQL Server database names are case-insensitive; a customer typing it differently in two
        // places should not silently stop their settings reaching the phone.
        (await Build(remote, target).RunAsync()).Mirrored.Should().Be(1);
    }

    [Fact]
    public async Task A_run_reports_what_it_applied_and_the_revision_it_applied()
    {
        var company = Company("MikroDB_V16_03");
        var remote = new FakeRemote { Companies = [company] };
        remote.States[company.Id] = new AgentParameterState(company.Id, company.SourceDatabase, 7, [Row(58, "3")]);

        var target = new FakeTarget("MikroDB_V16_03")
        {
            Outcome = new ParameterMirrorOutcome(1, 2, 3,
                [new AgentParameterDrift("akilli", "plasiyer1", "", "", 58, "3", "9")]),
        };

        await Build(remote, target).RunAsync();

        var report = remote.Reports.Should().ContainSingle().Subject;
        report.AppliedRevision.Should().Be(7, "the report ties a run to what it applied");
        report.Inserted.Should().Be(1);
        report.Updated.Should().Be(2);
        report.Deleted.Should().Be(3);
        report.Failed.Should().Be(0);
        report.Drifts.Should().ContainSingle().Which.Found.Should().Be("9");
    }

    [Fact]
    public async Task A_failed_mirror_is_reported_rather_than_swallowed()
    {
        var company = Company("MikroDB_V16_03");
        var remote = new FakeRemote { Companies = [company] };
        remote.States[company.Id] = new AgentParameterState(company.Id, company.SourceDatabase, 2, [Row(58, "3")]);

        var target = new FakeTarget("MikroDB_V16_03") { Throw = new InvalidOperationException("tablo kilitli") };

        var result = await Build(remote, target).RunAsync();

        // A mirror nobody can see fail is a mirror nobody can trust, and the panel's only window
        // onto this is the report.
        result.Failed.Should().Be(1);
        var report = remote.Reports.Should().ContainSingle().Subject;
        report.Failed.Should().Be(1);
        report.ErrorText.Should().Contain("tablo kilitli");
    }

    [Fact]
    public async Task One_company_failing_does_not_stop_the_next_one()
    {
        var bad = Company("MikroDB_V16_03", "BOZUK");
        var good = Company("MikroDB_V16_03", "IYI");

        var remote = new FakeRemote { Companies = [bad, good] };
        remote.States[bad.Id] = new AgentParameterState(bad.Id, bad.SourceDatabase, 1, [Row(58, "3")]);
        remote.States[good.Id] = new AgentParameterState(good.Id, good.SourceDatabase, 1, [Row(58, "3")]);

        var target = new FakeTarget("MikroDB_V16_03") { ThrowForCompany = bad.Id, Throw = new InvalidOperationException("x") };

        var result = await Build(remote, target).RunAsync();

        result.Should().Be(new ParameterMirrorService.RunResult(Mirrored: 1, Skipped: 0, Failed: 1));
    }

    [Fact]
    public async Task Failing_to_report_a_failure_does_not_become_a_second_failure()
    {
        var company = Company("MikroDB_V16_03");
        var remote = new FakeRemote { Companies = [company], ThrowOnReport = true };
        remote.States[company.Id] = new AgentParameterState(company.Id, company.SourceDatabase, 1, [Row(58, "3")]);

        var target = new FakeTarget("MikroDB_V16_03") { Throw = new InvalidOperationException("x") };

        // The run has already gone wrong and the next one will try again; throwing here would take
        // down the worker over a message nobody was waiting for.
        var act = async () => await Build(remote, target).RunAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task A_company_with_no_state_is_left_alone()
    {
        var company = Company("MikroDB_V16_03");
        var remote = new FakeRemote { Companies = [company] };
        var target = new FakeTarget("MikroDB_V16_03");

        // An older central API has no parameters to mirror; emptying the table would be a
        // spectacular way to read "nothing yet" as "nothing should be there".
        (await Build(remote, target).RunAsync()).Mirrored.Should().Be(1);
        target.Applied.Should().BeEmpty();
        remote.Reports.Should().BeEmpty();
    }

    private sealed class FakeRemote : IRemoteApiClient
    {
        public IReadOnlyList<AgentErpCompany> Companies { get; init; } = [];

        public Dictionary<Guid, AgentParameterState> States { get; } = [];

        public List<AgentParameterMirrorReport> Reports { get; } = [];

        public bool ThrowOnReport { get; init; }

        public Task<IReadOnlyList<AgentErpCompany>> GetAgentCompaniesAsync(CancellationToken ct = default) =>
            Task.FromResult(Companies);

        public Task<AgentParameterState?> GetParameterStateAsync(Guid erpCompanyId, CancellationToken ct = default) =>
            Task.FromResult(States.GetValueOrDefault(erpCompanyId));

        public Task SendParameterMirrorReportAsync(AgentParameterMirrorReport report, CancellationToken ct = default)
        {
            if (ThrowOnReport)
            {
                throw new HttpRequestException("merkez ulaşılamıyor");
            }

            Reports.Add(report);
            return Task.CompletedTask;
        }

        // The rest of the agent's surface is not what this service touches.
        public Task<LicenseValidationResult> ValidateLicenseAsync(string licenseKey, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<AgentRegistrationResult> RegisterAgentAsync(string licenseKey, string machineId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<RemoteJob>> GetPendingJobsAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task SendAckAsync(JobAck ack, CancellationToken ct = default) => throw new NotSupportedException();
        public Task PushBootstrapDataAsync(ErpBridge.Erp.Abstractions.Sync.SyncPackage package, CancellationToken ct = default) => throw new NotSupportedException();
        public Task SendHeartbeatAsync(AgentHeartbeat heartbeat, CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class FakeTarget(string database) : IParameterMirrorTarget
    {
        public Dictionary<Guid, IReadOnlyList<AgentParameterRow>> Applied { get; } = [];

        public ParameterMirrorOutcome Outcome { get; init; } = new(0, 0, 0, []);

        public Exception? Throw { get; init; }

        public Guid? ThrowForCompany { get; init; }

        public bool CanReach(string sourceDatabase) =>
            string.Equals(database, sourceDatabase, StringComparison.OrdinalIgnoreCase);

        public Task<ParameterMirrorOutcome> ApplyAsync(
            AgentErpCompany company, IReadOnlyList<AgentParameterRow> desired, CancellationToken ct = default)
        {
            if (Throw is not null && (ThrowForCompany is null || ThrowForCompany == company.Id))
            {
                throw Throw;
            }

            Applied[company.Id] = desired;
            return Task.FromResult(Outcome);
        }
    }
}
