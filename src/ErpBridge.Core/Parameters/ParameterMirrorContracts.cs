namespace ErpBridge.Core.Parameters;

/// <summary>
/// One ERP company this agent is assigned to, as the centre names it.
///
/// The agent knows the Mikro database it is pointed at, not the id the centre uses for it, so
/// <see cref="SourceDatabase"/> is what turns one into the other.
/// </summary>
public sealed record AgentErpCompany(
    Guid Id,
    string Code,
    string Name,
    string SourceDatabase,
    int CompanyNo,
    int BranchNo);

/// <summary>One parameter as the centre says a company's Mikro table should hold it.</summary>
public sealed record AgentParameterRow(
    string ParametreProgram,
    string ParametreUser,
    string AnaGrubu,
    string AltGrubu,
    int ParametreID,
    string ParametreAdi,
    string ParametreDegeri);

/// <summary>
/// Everything one company's parameter table should contain — the complete desired state, not a
/// delta, so the agent can make the table match without either side remembering what was sent last.
/// </summary>
public sealed record AgentParameterState(
    Guid ErpCompanyId,
    string SourceDatabase,
    long Revision,
    IReadOnlyList<AgentParameterRow> Items);

/// <summary>A row the agent found in Mikro holding something the centre did not set.</summary>
public sealed record AgentParameterDrift(
    string ParametreProgram,
    string ParametreUser,
    string AnaGrubu,
    string AltGrubu,
    int ParametreID,
    string Expected,
    string? Found);

/// <summary>What one mirror run did, sent back so the panel can say whether it happened.</summary>
public sealed record AgentParameterMirrorReport(
    Guid ErpCompanyId,
    long AppliedRevision,
    int Inserted,
    int Updated,
    int Deleted,
    int Failed,
    string? ErrorText,
    IReadOnlyList<AgentParameterDrift> Drifts);

/// <summary>One row read out of a customer's <c>_FORA_PARAMETRELER</c>, exactly as Fora stored it.</summary>
public sealed record ForaScanRow(
    string ParametreProgram,
    string ParametreUser,
    string AnaGrubu,
    string AltGrubu,
    int ParametreID,
    string ParametreAdi,
    string ParametreDegeri);

/// <summary>
/// What the centre made of a scan: how much it could place, and what it could not.
/// </summary>
/// <param name="Unknown">Rows whose parameter the catalogue does not declare.</param>
/// <param name="UnmatchedUsers">Usernames with no active mobile user; no user is opened for them.</param>
public sealed record ForaImportResult(
    Guid BatchId,
    int Scanned,
    int Matched,
    int Unknown,
    IReadOnlyList<string> UnmatchedUsers);
