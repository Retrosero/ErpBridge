using Dapper;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.LocalStore.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ErpBridge.LocalStore.Stores;

/// <summary>
/// Placeholder returned for any secret-shaped config value during <c>Load</c> when
/// the configured <see cref="IProtectedConfigProvider"/> cannot (or refuses to) decrypt it.
/// </summary>
internal static class AgentConfigMasks
{
    public const string RedactedPlaceholder = "***REDACTED***";
}

/// <summary>
/// SQLite-backed <see cref="IAgentConfigStore"/>. Persists every <see cref="AgentConfig"/>
/// property as one row in the <c>agent_config</c> table.
/// </summary>
/// <remarks>
/// <para>
/// Secret rows (<c>is_secret = 1</c>) are routed through the configured
/// <see cref="IProtectedConfigProvider"/> on write. The protected blob is stored in the
/// dedicated <c>protected_value</c> column with the matching <c>protection_version</c>;
/// the legacy <c>value</c> column remains populated with a sentinel placeholder so
/// pre-Track-2 readers (or any external SQLite inspector) never see plaintext at rest.
/// </para>
/// <para>
/// On read the store prefers <c>protected_value</c>; when the configured provider can
/// decrypt it, the plaintext is returned. If decryption fails — wrong key, missing
/// DPAPI scope, tampered ciphertext — the row is masked with <c>***REDACTED***</c> rather
/// than leaking the failure to the caller.
/// </para>
/// <para>
/// Plaintext rows from pre-Track-2 deployments (no <c>protected_value</c>) are masked
/// immediately to honour the SKILL.md §3 kural 7 invariant (secrets never leak).
/// </para>
/// </remarks>
public sealed class SqliteAgentConfigStore : IAgentConfigStore
{
    /// <summary>
    /// Schema version stamped on every secret row written by this store. Bump it whenever
    /// the on-disk protection format changes so older readers can detect migration needs.
    /// </summary>
    public const int CurrentProtectionVersion = 1;

    /// <summary>Sentinel string written into the legacy <c>value</c> column for secret rows.</summary>
    public const string LegacySecretSentinel = "";

    private static readonly string[] SecretKeys =
    {
        nameof(AgentConfig.SqlPassword),
        nameof(AgentConfig.LicenseKey),
    };

    private static readonly string[] NormalKeys =
    {
        nameof(AgentConfig.TenantId),
        nameof(AgentConfig.ErpType),
        nameof(AgentConfig.SqlServer),
        nameof(AgentConfig.SqlUserName),
        nameof(AgentConfig.MikroDatabaseName),
        nameof(AgentConfig.CompanyNo),
        nameof(AgentConfig.BranchNo),
        nameof(AgentConfig.ApiBaseUrl),
        nameof(AgentConfig.UseWindowsAuth),
    };

    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly IProtectedConfigProvider _protectedConfigProvider;
    private readonly ILogger<SqliteAgentConfigStore> _logger;

    public SqliteAgentConfigStore(
        SqliteConnectionFactory connectionFactory,
        IProtectedConfigProvider protectedConfigProvider)
        : this(connectionFactory, protectedConfigProvider, NullLogger<SqliteAgentConfigStore>.Instance)
    {
    }

    /// <summary>
    /// Full constructor — used by tests that want to observe (or suppress) the
    /// diagnostic log lines emitted when decryption fails.
    /// </summary>
    public SqliteAgentConfigStore(
        SqliteConnectionFactory connectionFactory,
        IProtectedConfigProvider protectedConfigProvider,
        ILogger<SqliteAgentConfigStore> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _protectedConfigProvider = protectedConfigProvider ?? throw new ArgumentNullException(nameof(protectedConfigProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<AgentConfig?> LoadAsync(CancellationToken ct = default)
    {
        var rows = (await ReadAllAsync(ct).ConfigureAwait(false)).ToList();

        if (rows.Count == 0)
        {
            return null;
        }

        var config = new AgentConfig();
        foreach (var row in rows)
        {
            ApplyRow(config, row);
        }

        return config;
    }

    /// <inheritdoc />
    public async Task SaveAsync(AgentConfig config, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        var nowUtc = DateTime.UtcNow.ToString("O");

        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);
        await using var tx = await connection.BeginTransactionAsync(ct).ConfigureAwait(false);

        var upsertSql = @"
INSERT INTO agent_config (key, value, is_secret, updated_at, protected_value, protection_version)
VALUES (@key, @value, @isSecret, @updatedAt, @protectedValue, @protectionVersion)
ON CONFLICT(key) DO UPDATE SET
    value = excluded.value,
    is_secret = excluded.is_secret,
    updated_at = excluded.updated_at,
    protected_value = excluded.protected_value,
    protection_version = excluded.protection_version;";

        foreach (var key in NormalKeys)
        {
            var raw = ExtractString(config, key);
            await connection.ExecuteAsync(new CommandDefinition(upsertSql, new
            {
                key,
                value = raw ?? string.Empty,
                isSecret = 0,
                updatedAt = nowUtc,
                protectedValue = (string?)null,
                protectionVersion = (int?)0,
            }, transaction: tx, cancellationToken: ct)).ConfigureAwait(false);
        }

        foreach (var key in SecretKeys)
        {
            var raw = ExtractString(config, key);
            // Always go through the protector on write — plain or encrypted blobs are
            // both legitimate storage forms, but the protector is the single entry point
            // so swapping providers later doesn't require touching this method again.
            var protectedValue = raw is null ? string.Empty : _protectedConfigProvider.Protect(raw);
            await connection.ExecuteAsync(new CommandDefinition(upsertSql, new
            {
                key,
                value = LegacySecretSentinel,
                isSecret = 1,
                updatedAt = nowUtc,
                protectedValue = string.IsNullOrEmpty(protectedValue) ? null : protectedValue,
                protectionVersion = string.IsNullOrEmpty(protectedValue) ? 0 : CurrentProtectionVersion,
            }, transaction: tx, cancellationToken: ct)).ConfigureAwait(false);
        }

        await tx.CommitAsync(ct).ConfigureAwait(false);
    }

    private async Task<IEnumerable<AgentConfigRow>> ReadAllAsync(CancellationToken ct)
    {
        const string sql = @"
SELECT key AS [Key],
       value AS [Value],
       is_secret AS [IsSecret],
       protected_value AS [ProtectedValue],
       protection_version AS [ProtectionVersion]
FROM agent_config;";

        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);
        return await connection.QueryAsync<AgentConfigRow>(
            new CommandDefinition(sql, cancellationToken: ct)).ConfigureAwait(false);
    }

    private void ApplyRow(AgentConfig config, AgentConfigRow row)
    {
        var effective = row.Value;

        if (row.IsSecret != 0)
        {
            // Prefer the dedicated protected_value column when present.
            if (!string.IsNullOrEmpty(row.ProtectedValue))
            {
                if (_protectedConfigProvider.IsProtected(row.ProtectedValue))
                {
                    try
                    {
                        effective = _protectedConfigProvider.Unprotect(row.ProtectedValue);
                    }
                    catch (Exception ex)
                    {
                        // Wrong key, missing DPAPI scope, tampered ciphertext — never propagate
                        // the failure to the caller. Log at Warning without leaking the value.
                        _logger.LogWarning(ex, "Failed to decrypt protected agent_config row '{Key}'; redacting.", row.Key);
                        effective = AgentConfigMasks.RedactedPlaceholder;
                    }
                }
                else
                {
                    // Provider says the column is not in a recognised protected form — treat
                    // as tamper and redact.
                    _logger.LogWarning("agent_config row '{Key}' has protected_value without recognised marker; redacting.", row.Key);
                    effective = AgentConfigMasks.RedactedPlaceholder;
                }
            }
            else if (!string.IsNullOrEmpty(row.Value) && _protectedConfigProvider.IsProtected(row.Value))
            {
                // Legacy Track-2-pre row: a previous NoOp-era store wrote a plaintext value,
                // but a newer provider finds it in protected form on the legacy column.
                try
                {
                    effective = _protectedConfigProvider.Unprotect(row.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to decrypt legacy protected agent_config row '{Key}'; redacting.", row.Key);
                    effective = AgentConfigMasks.RedactedPlaceholder;
                }
            }
            else
            {
                // No protected blob — mask immediately. Honours SKILL.md §3 kural 7
                // (plaintext secrets from older deployments never leak).
                effective = AgentConfigMasks.RedactedPlaceholder;
            }
        }

        switch (row.Key)
        {
            case nameof(AgentConfig.LicenseKey):
                config.LicenseKey = effective;
                break;
            case nameof(AgentConfig.TenantId):
                config.TenantId = effective;
                break;
            case nameof(AgentConfig.ErpType):
                if (Enum.TryParse<ErpType>(row.Value, ignoreCase: true, out var erp))
                {
                    config.ErpType = erp;
                }
                break;
            case nameof(AgentConfig.SqlServer):
                config.SqlServer = effective;
                break;
            case nameof(AgentConfig.SqlUserName):
                config.SqlUserName = effective;
                break;
            case nameof(AgentConfig.SqlPassword):
                config.SqlPassword = effective;
                break;
            case nameof(AgentConfig.MikroDatabaseName):
                config.MikroDatabaseName = effective;
                break;
            case nameof(AgentConfig.CompanyNo):
                if (int.TryParse(row.Value, out var company))
                {
                    config.CompanyNo = company;
                }
                break;
            case nameof(AgentConfig.BranchNo):
                if (int.TryParse(row.Value, out var branch))
                {
                    config.BranchNo = branch;
                }
                break;
            case nameof(AgentConfig.ApiBaseUrl):
                config.ApiBaseUrl = effective;
                break;
            case nameof(AgentConfig.UseWindowsAuth):
                if (bool.TryParse(row.Value, out var win))
                {
                    config.UseWindowsAuth = win;
                }
                break;
        }
    }

    private static string? ExtractString(AgentConfig config, string key) => key switch
    {
        nameof(AgentConfig.LicenseKey) => config.LicenseKey,
        nameof(AgentConfig.TenantId) => config.TenantId,
        nameof(AgentConfig.ErpType) => config.ErpType.ToString(),
        nameof(AgentConfig.SqlServer) => config.SqlServer,
        nameof(AgentConfig.SqlUserName) => config.SqlUserName,
        nameof(AgentConfig.SqlPassword) => config.SqlPassword,
        nameof(AgentConfig.MikroDatabaseName) => config.MikroDatabaseName,
        nameof(AgentConfig.CompanyNo) => config.CompanyNo.ToString(System.Globalization.CultureInfo.InvariantCulture),
        nameof(AgentConfig.BranchNo) => config.BranchNo.ToString(System.Globalization.CultureInfo.InvariantCulture),
        nameof(AgentConfig.ApiBaseUrl) => config.ApiBaseUrl,
        nameof(AgentConfig.UseWindowsAuth) => config.UseWindowsAuth ? "true" : "false",
        _ => null,
    };

    private sealed class AgentConfigRow
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public long IsSecret { get; set; }
        public string? ProtectedValue { get; set; }
        public long? ProtectionVersion { get; set; }
    }
}
