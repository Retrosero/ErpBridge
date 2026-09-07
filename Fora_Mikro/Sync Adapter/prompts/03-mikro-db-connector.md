# 03 - Mikro DB Connector Prompt

Implement Mikro SQL connection and schema discovery.

Services:

- `MikroDbConnector`
- `MikroVersionDetector`
- `SchemaExplorer`
- `AllowedTableRegistry`

Requirements:

- Test SQL Server connection without exposing password.
- List allowed Mikro databases.
- Detect V15 vs V16.
- Cache schema metadata in local SQLite.
- Expose only allowlisted table/entity metadata to the rest of the app.

Version rules:

- V15 uses RECno and RECid linkage.
- V16 uses Guid/uid linkage.
- Business services must not branch directly on column names; use version adapters.

Tests:

- Connection success/failure.
- Wrong credentials redacted.
- V15 schema detected.
- V16 schema detected.
- Non-allowlisted table is rejected.
