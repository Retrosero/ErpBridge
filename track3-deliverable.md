# Track 3 — ErpBridge.RemoteApi + ErpBridge.Agent.Service + ErpBridge.Agent.UI

> **Track:** 3 / 3
> **Status:** ✅ Done — all three projects build clean (0 warnings, 0 errors)
> **Date:** 2026-07-09

## Summary

Three new projects were added to the ErpBridge solution, all wiring up against the
canonical Core contracts and LocalStore stack created by other tracks. The agent
worker poll loop, heartbeat, and WPF configuration UI are all in place as
**MVP skeletons** — they are wired to the real `IAgentConfigStore`,
`ILocalQueueStore`, and `IRemoteApiClient` contracts but defer the actual Mikro
write/connect paths to Phase 3 and Phase 6 per the AGENTS.md phase plan.

## 1. Created files

### `src/ErpBridge.RemoteApi/`

| File | Lines | Purpose |
|------|------:|---------|
| `ErpBridge.RemoteApi.csproj` | 27 | net8.0 class library; refs Shared + Core; pulls in `Microsoft.Extensions.Http.Polly 8.0.11`, `Microsoft.Extensions.Options`, `Microsoft.Extensions.Logging.Abstractions` |
| `Options/CentralApiOptions.cs` | 32 | `BaseUrl`, `TimeoutSeconds`, `Jwt`, `Retry.MaxAttempts/InitialDelaySeconds` bound from the `CentralApi` section |
| `Http/HttpRemoteApiClient.cs` | 158 | `IRemoteApiClient` implementation — 5 endpoints (`/licenses/validate`, `/jobs/pending`, `/jobs/ack`, `/bootstrap`, `/agents/heartbeat`); System.Text.Json serialize/deserialize; `Authorization: Bearer {jwt}` header; 30s timeout via linked CTS; 5xx/429/HttpRequestException via Polly |
| `Authentication/JwtRotator.cs` | 38 | `IJwtTokenProvider` — placeholder for JWT rotation; reads from `IOptionsMonitor<CentralApiOptions>` today |
| `DependencyInjection/ServiceCollectionExtensions.cs` | 90 | `AddErpBridgeRemoteApi(IConfiguration)` extension; `AddHttpClient<IRemoteApiClient, HttpRemoteApiClient>` with `AddPolicyHandler(BuildRetryPolicy)`; canonical 5s/15s/60s/300s exponential backoff (capped) |

### `src/ErpBridge.Agent.Service/`

| File | Lines | Purpose |
|------|------:|---------|
| `ErpBridge.Agent.Service.csproj` | 40 | `OutputType=Exe`, `UseWindowsService=true`; refs Shared + Core + LocalStore + RemoteApi; pulls in `Microsoft.Extensions.Hosting 8.0.1`, `Microsoft.Extensions.Hosting.WindowsServices 8.0.1`, `Serilog` + 4 sinks/extensions |
| `Program.cs` | 56 | `Host.CreateDefaultBuilder` → `UseWindowsService` → `ConfigureAppConfiguration` (appsettings + env vars) → `ConfigureServices` (AddErpBridgeCore/LocalStore/RemoteApi + two hosted services) → `UseSerilog` |
| `Configuration/AgentServiceOptions.cs` | 15 | `ServiceName` for the Windows service registration |
| `Workers/AgentWorker.cs` | 109 | `BackgroundService` — 30s poll loop, calls `IRemoteApiClient.GetPendingJobsAsync`, persists each into `ILocalQueueStore`; logs and recovers from transient errors |
| `Workers/HeartbeatWorker.cs` | 68 | `BackgroundService` — 60s interval, calls `IRemoteApiClient.SendHeartbeatAsync` after pulling `IAgentConfigStore.LoadAsync` |
| `appsettings.json` | 45 | Default config; license/SQL secrets are empty |
| `appsettings.example.json` | 45 | Same as appsettings.json, kept side-by-side for the WPF operator |

### `src/ErpBridge.Agent.UI/`

| File | Lines | Purpose |
|------|------:|---------|
| `ErpBridge.Agent.UI.csproj` | 40 | `OutputType=WinExe`, `TargetFramework=net8.0-windows`, `UseWPF=true`, `TreatWarningsAsErrors=true`; refs Shared + Core + LocalStore + RemoteApi; pulls in `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Hosting`, Serilog, `Serilog.Settings.Configuration`, `Serilog.Extensions.Logging` |
| `App.xaml` | 12 | WPF application shell; merges the `AgentTheme` resource dictionary |
| `App.xaml.cs` | 54 | Builds the `ServiceProvider`, configures Serilog, resolves `MainWindow` + `AgentSettingsViewModel` |
| `Views/MainWindow.xaml` | 132 | Single-page WPF settings window with all SKILL.md §8 fields |
| `Views/MainWindow.xaml.cs` | 28 | `PasswordBox.PasswordChanged` → view-model; binding glue |
| `ViewModels/ObservableObject.cs` | 24 | Minimal `INotifyPropertyChanged` base |
| `ViewModels/AgentSettingsViewModel.cs` | 201 | All SKILL.md §8 properties + Save / Test-Connection commands; **NO Mikro connection logic in Phase 1** — just config shape validation; logs errors via `ILogger` |
| `RelayCommand.cs` | 33 | Minimal `ICommand` impl; no CommunityToolkit.Mvvm dependency |
| `Converters/BoolToVisibilityConverter.cs` | 15 | `bool` → `Visibility` (Collapsed/Visible) |
| `DependencyInjection/ServiceCollectionExtensions.cs` | 49 | `AddErpBridgeAgentUi` — wires LocalStore, view-model, Serilog adapter |
| `Themes/AgentTheme.xaml` | 12 | Brushes + static resources, including the BoolToVisibilityConverter |
| `appsettings.json` | 19 | LocalStore connection string + Serilog sinks |

### Solution file

- `ErpBridge.sln` — added `ErpBridge.RemoteApi`, `ErpBridge.Agent.Service`,
  `ErpBridge.Agent.UI` (the Mikro entry was added by Track 4 concurrently and is
  left in place).

### Bonus (Core / Shared additions required for the above to compile)

These were added because other tracks had not yet introduced the data types /
interfaces the RemoteApi needs. Each addition is **additive** and respects the
namespace layout chosen by the other tracks (`ErpBridge.Core.Domain` for data,
`ErpBridge.Core.Stores` for interfaces):

| File | Purpose |
|------|---------|
| `src/ErpBridge.Core/Domain/RemoteJob.cs` | Pending job from central API |
| `src/ErpBridge.Core/Domain/JobAck.cs` | Job ack payload |
| `src/ErpBridge.Core/Domain/AgentHeartbeat.cs` | Heartbeat payload |
| `src/ErpBridge.Core/Domain/SyncPackage.cs` | Bootstrap push payload |
| `src/ErpBridge.Core/Domain/LicenseValidationResult.cs` | License validation response |
| `src/ErpBridge.Core/Stores/IRemoteApiClient.cs` | Public interface implemented by `HttpRemoteApiClient` |
| `src/ErpBridge.Shared/Constants.cs` | Added `ErpBridgeConstants` (`DefaultSqlitePath`, `RedactedPlaceholder`) alongside the existing `AgentConstants` — required by the LocalStore track that was building in parallel |

## 2. Build results

Each project was built independently on `net8.0` (Linux SDK 8.0.422):

```text
$ dotnet build src/ErpBridge.RemoteApi/ErpBridge.RemoteApi.csproj
ErpBridge.Shared    -> .../ErpBridge.Shared.dll
ErpBridge.Core      -> .../ErpBridge.Core.dll
ErpBridge.RemoteApi -> .../ErpBridge.RemoteApi.dll
Build succeeded.
    0 Warning(s)
    0 Error(s)

$ dotnet build src/ErpBridge.Agent.Service/ErpBridge.Agent.Service.csproj
ErpBridge.Shared       -> .../ErpBridge.Shared.dll
ErpBridge.Core         -> .../ErpBridge.Core.dll
ErpBridge.LocalStore   -> .../ErpBridge.LocalStore.dll
ErpBridge.RemoteApi    -> .../ErpBridge.RemoteApi.dll
ErpBridge.Agent.Service -> .../ErpBridge.Agent.Service.dll
Build succeeded.
    0 Warning(s)
    0 Error(s)

$ dotnet build src/ErpBridge.Agent.UI/ErpBridge.Agent.UI.csproj -p:EnableWindowsTargeting=true
ErpBridge.Shared   -> .../ErpBridge.Shared.dll
ErpBridge.Core     -> .../ErpBridge.Core.dll
ErpBridge.LocalStore -> .../ErpBridge.LocalStore.dll
ErpBridge.RemoteApi -> .../ErpBridge.RemoteApi.dll
ErpBridge.Agent.UI -> .../ErpBridge.Agent.UI.dll
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Full solution build (`ErpBridge.sln`, 9 projects) also passes:

```text
$ dotnet build ErpBridge.sln -p:EnableWindowsTargeting=true
... (9 projects, all successful)
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Linux / WPF note

WPF (`net8.0-windows`) is Windows-only at runtime. The cross-compile succeeds
with `EnableWindowsTargeting=true`; a stock `dotnet build` on Linux fails with
`NETSDK1100: To build a project targeting Windows on this operating system, set
the EnableWindowsTargeting property to true.` This is expected and documented —
the WPF csproj is structurally correct, and the project will build & run natively
on Windows.

The csproj does **not** embed `EnableWindowsTargeting=true` permanently because
that property only matters on non-Windows SDK hosts. CI on Windows will build it
without any extra flag.

## 3. Rule compliance

### RemoteApi references — verified

```text
$ dotnet list src/ErpBridge.RemoteApi/ErpBridge.RemoteApi.csproj reference
..\ErpBridge.Shared\ErpBridge.Shared.csproj
..\ErpBridge.Core\ErpBridge.Core.csproj
```

✅ **No** `ErpBridge.Erp.Mikro`, `ErpBridge.LocalStore`, `ErpBridge.Agent.Service`, or
`ErpBridge.Agent.UI` reference. This keeps the central-API client ERP-agnostic
and reusable for future Logos/Paraşüt/Netsis adapters if/when they need
direct central-API hooks.

### Agent.Service references — verified

```text
$ dotnet list src/ErpBridge.Agent.Service/ErpBridge.Agent.Service.csproj reference
..\ErpBridge.Shared\ErpBridge.Shared.csproj
..\ErpBridge.Core\ErpBridge.Core.csproj
..\ErpBridge.LocalStore\ErpBridge.LocalStore.csproj
..\ErpBridge.RemoteApi\ErpBridge.RemoteApi.csproj
```

✅ **No** `ErpBridge.Erp.Mikro` reference. The worker only consumes the
`IRemoteApiClient` and `ILocalQueueStore` interfaces — actual Mikro writes will
be wired in Phase 6 via `IErpAdapter` from the abstractions layer.

### Agent.UI references — verified

```text
$ dotnet list src/ErpBridge.Agent.UI/ErpBridge.Agent.UI.csproj reference
..\ErpBridge.Shared\ErpBridge.Shared.csproj
..\ErpBridge.Core\ErpBridge.Core.csproj
..\ErpBridge.LocalStore\ErpBridge.LocalStore.csproj
..\ErpBridge.RemoteApi\ErpBridge.RemoteApi.csproj
```

✅ **No** `ErpBridge.Erp.Mikro` reference. The "Bağlantıyı test et" button only
validates the in-memory `AgentConfig` shape in Phase 1; the real Mikro handshake
plugs in via `IErpAdapter.TestConnectionAsync` in Phase 3.

### PasswordBox in UI

```text
$ grep -l PasswordBox src/ErpBridge.Agent.UI/Views/*.xaml
src/ErpBridge.Agent.UI/Views/MainWindow.xaml
src/ErpBridge.Agent.UI/Views/MainWindow.xaml.cs
```

✅ SQL password is collected via `System.Windows.Controls.PasswordBox`, not a
TextBox. The view-model binding is wired through the `PasswordChanged` code-behind
handler; the password is never logged via `ILogger` (LogError calls only print
exception messages, never the SQL password).

### `TreatWarningsAsErrors=true`

All three csproj files set `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
The Core and LocalStore projects (owned by other tracks) also enable it, and
the full solution build passes 0 warnings.

### Doc comments

All public types / public methods carry `///` doc summaries per AGENTS.md
section 10. The few non-public internals (Polly retry policy, agent service
options) are documented as well.

## 4. WPF UI grid layout (SKILL.md §8)

`MainWindow.xaml` uses a 2-column `Grid` (170px labels + `*` inputs) with one
row per field plus a status block. Layout (from the XAML):

```
+-----------------------------------------------------------+
| ErpBridge Agent Yapılandırması   (header, row 0)          |
+-----------------------------------------------------------+
| Lisans anahtarı           [ TextBox                       ] |
| SQL Server                [ TextBox                       ] |
| SQL kullanıcı adı         [ TextBox                       ] |
| SQL şifre                 [ PasswordBox    (secure)       ] |
| Mikro database adı        [ TextBox                       ] |
| Firma no                  [ TextBox                       ] |
| Şube no                   [ TextBox                       ] |
| API base URL              [ TextBox                       ] |
+-----------------------------------------------------------+
|              [ Bağlantıyı test et ]  [ Kaydet ]           |
+-----------------------------------------------------------+
| Son durum        ┌─────────────────────────────────────┐   |
|                  │ "Config doğrulandı.\n                │   |
|                  │  Sunucu: ...\n                       │   |
|                  │  Database: ...\n                     │   |
|                  │  Firma/Şube: ... / ..."              │   |
|                  └─────────────────────────────────────┘   |
+-----------------------------------------------------------+
|                                       (İşlem sürüyor...)   |
+-----------------------------------------------------------+
```

A live screen mockup can be produced on a Windows host by running:

```bash
dotnet run --project src/ErpBridge.Agent.UI -p:EnableWindowsTargeting=true
```

## 5. Polly retry pipeline (RemoteApi)

`HttpRemoteApiClient` is registered as a typed HttpClient inside
`AddErpBridgeRemoteApi`. The Polly v7 `AddPolicyHandler` is wired with:

- **Triggers:** `HttpRequestException`, `TaskCanceledException`, response with
  `5xx` or `429 TooManyRequests`
- **Backoff schedule:** `5s, 15s, 60s, 300s` (capped) — exactly the canonical
  5/15/60/300s sequence required by `docs/api-contracts.md`
- **Per-request timeout:** linked `CancellationTokenSource` cancels the call
  after `CentralApi.TimeoutSeconds` (default 30s)
- **HttpClient base timeout:** also set to `CentralApi.TimeoutSeconds` so a
  hung socket is killed at the transport layer too
- **Headers:** `Authorization: Bearer {jwt}` from `IOptionsMonitor`,
  `Accept: application/json`, `User-Agent: ErpBridge-Agent/1.0`

## 6. Agent worker poll loop

```text
ExecuteAsync (30s loop)
  ├─ IAgentConfigStore.LoadAsync
  ├─ IRemoteApiClient.GetPendingJobsAsync
  └─ foreach job → ILocalQueueStore.EnqueueAsync(new LocalJob { … })
```

`HeartbeatWorker` runs in parallel at 60s and posts an `AgentHeartbeat { Status =
"healthy" }` to `/api/v1/agents/heartbeat`. Both workers log transient errors at
warning level and continue.

## 7. Known limitations / out-of-scope for Phase 1

These are deferred to later phases per AGENTS.md and SKILL.md:

- `IErpAdapter` Mikro connection test → **Phase 3**
- `BootstrapReader` (cari/stok/fiyat/depo/kasa/banka/plasiyer) → **Phase 5**
- `SalesOrderWriter` (SIPARISLER + STOK_HAREKETLERI in a single tx) → **Phase 6**
- JWT rotation / agent registration handshake → **Phase 4** (central API)
- WPF tray icon, MSI installer, DPAPI encryption for `IProtectedConfigProvider`
  → later phases

The current skeletons are **structurally complete** for these to plug in
without breaking the public surface — `IJwtTokenProvider`, `IErpAdapter`,
`ISalesOrderWriter` etc. can all be added to the DI container without touching
the workers or the UI view-model.

## 8. End-to-end build command

```bash
cd /workspace/ErpBridge
dotnet build ErpBridge.sln -p:EnableWindowsTargeting=true
# → Build succeeded. 0 Warning(s) 0 Error(s)
```
