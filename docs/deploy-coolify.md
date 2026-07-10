# Deploying ErpBridge to Coolify

This document walks you through deploying the **central API** + **admin
panel** to a Coolify-managed server. It assumes Coolify v4.x and a Linux
target server reachable from the public internet.

> **TL;DR.** Two Coolify applications pointing at the same Git repo, one
> PostgreSQL database, four environment secrets, two public domains with
> Let's Encrypt certificates. ~10 minutes of clicks.

---

## 1. Prerequisites

- Coolify instance running and reachable (e.g. `https://coolify.your-host.com`).
- A Coolify "Project" (or the default one) and a "Environment" (Production is fine).
- A domain or subdomain for each service:
  - `api.erpbridge.example.com` â†’ central API
  - `admin.erpbridge.example.com` â†’ admin panel
- DNS A/AAAA records for both subdomains pointing at the Coolify server's
  public IP. Coolify uses Traefik for reverse-proxy and Let's Encrypt for
  certificates; both require resolvable DNS.

## 2. Provision a PostgreSQL database

Coolify's "Database" service type provisions a managed PostgreSQL container
alongside your applications. In the Coolify UI:

1. **+ New â†’ Database â†’ PostgreSQL 16**.
2. Name it `erpbridge-pg`.
3. Pick a database name (`erpbridge`), username (`erpbridge`).
4. **Generate a password** (Coolify stores it; copy it â€” you'll paste it
   into the central API's environment as `POSTGRES_PASSWORD`).
5. Deploy. Note the internal service hostname (something like
   `erpbridge-pg-xxxx` â€” Coolify shows it on the database's resource
   detail page).

The internal hostname is what the central API uses in its connection string
because Docker's internal DNS resolves it across the Coolify-managed
network.

## 3. Generate secrets

You need three secrets to start the central API. Treat them like root
passwords â€” never commit them, never paste them into logs, never email
them in cleartext.

| Variable | What to put | Where to store |
|---|---|---|
| `JWT_SIGNING_KEY` | 64+ ASCII characters of randomness. `openssl rand -hex 48` is a quick source. | Coolify secret |
| `ADMIN_SEED_PASSWORD` | The bootstrap admin's initial password. The admin will change it after first login (a future enhancement). | Coolify secret |
| `POSTGRES_PASSWORD` | The password you set in step 2. | Coolify secret |

You also need one **non-secret** environment variable:

| Variable | Value |
|---|---|
| `ADMIN_SEED_EMAIL` | The admin's email, e.g. `ops@erpbridge.example.com` |
| `CENTRALAPI_BASE_URL` | The admin panel needs to know where the central API lives. Use the **public** URL: `https://api.erpbridge.example.com` |
| `GIT_HASH` (optional) | Any short label; shows up in the image tag. |

## 4. Deploy the central API

In the Coolify UI:

1. **+ New â†’ Application â†’ Docker Compose**.
2. **Git Repository** â€” point at your ErpBridge fork. Branch: `main` (or
   your release branch).
3. **Docker Compose Location** â€” leave at `docker-compose.coolify.yml`.
4. **Base Directory** â€” leave empty (the file is at the repo root).
5. **Build Pack** â€” leave "Dockerfile" / "Automatic" detection on.
6. Coolify will parse the compose file and list two services:
   `centralapi` and `admin`. For the moment, click into `centralapi`.
7. On the `centralapi` service detail page:
   - **General → Port Exposes**: `4001`.
   - **General → FQDN**: `https://api.erpbridge.example.com`.
     Coolify will request the Let's Encrypt certificate automatically.
   - **Environment Variables**:
     - `POSTGRES_PASSWORD` â†’ *(Secret)* â€” paste the password from step 2.
     - `JWT_SIGNING_KEY` â†’ *(Secret)* â€” paste from step 3.
     - `ADMIN_SEED_EMAIL` â†’ `ops@erpbridge.example.com`.
     - `ADMIN_SEED_PASSWORD` â†’ *(Secret)* â€” paste from step 3.
   - **Healthchecks** â€” leave Coolify's default HTTP check on `/health`.
8. Click **Deploy**. Watch the build logs:
   - `dotnet restore` + `dotnet publish` should finish without errors.
   - The `webhook_deliveries` / `api_keys` tables are created automatically
     by EF Core's `Database.Migrate()` on first boot.
   - `EnsureSeedAdmin` runs on every boot but is idempotent; the row only
     appears if no admin with that email exists yet.
9. Smoke-test from your laptop:
   ```
   curl https://api.erpbridge.example.com/health
   ```
   Should return `{"status":"ok"}`.

## 5. Deploy the admin panel

In the Coolify UI:

1. The same compose file already defines the `admin` service. Click into
   it.
2. **General → Port Exposes**: `4002` (admin paneli; centralapi 4001).
3. **General → FQDN**: `https://admin.erpbridge.example.com`.
4. **Environment Variables**:
   - `CENTRALAPI_BASE_URL` → `https://api.erpbridge.example.com`
     *(must match the public FQDN, not the internal Docker hostname)*.
5. Click **Deploy**. Wait for the build to finish.
6. Open `https://admin.erpbridge.example.com`. You should land on the
   Blazor login page.

## 6. First-time login

1. Log in with the `ADMIN_SEED_EMAIL` and `ADMIN_SEED_PASSWORD` from step 3.
2. Go to **Tenants â†’ New Tenant**. Create your first customer tenant.
3. Go to **Licenses â†’ Issue license**. Paste the generated key into the
   Windows Agent's configuration.
4. Once the Agent has registered, you'll see it under **Agents**.
5. To start receiving customer jobs over the API:
   - **API Keys â†’ Create key** for the tenant.
   - Hand the raw `AK-...` value to the customer's backend; they call
     `POST https://api.erpbridge.example.com/api/v1/ingest/jobs` with
     `Authorization: Bearer AK-...` and `X-Tenant-Id: <guid>`.
6. To receive event callbacks in the customer's ERP:
   - **Webhooks â†’ Register endpoint**.
   - Hand the `whsec_...` secret to the customer's receiver so it can
     verify the `ErpBridge-Signature` header (HMAC-SHA256 over
     `"<timestamp>.<body>"`).

## 7. Backups

Coolify's PostgreSQL resource has a one-click backup to S3-compatible
storage. Configure it under the database's "Backups" tab. Schedule a
daily snapshot; the schema is small (no PII except the `api_keys` hash
columns and webhook secrets, both of which are non-recoverable from the
snapshot alone â€” the customer holds the raw values).

## 8. Updating

Push a commit to the configured Git branch. Coolify's "Auto Deploy"
webhook (or the manual **Deploy** button) rebuilds the affected images
and restarts the services. Migrations run automatically on first boot of
the new central API container.

For **breaking schema changes**, deploy the central API first, wait for
its healthcheck to report `ok`, then deploy the admin panel. The admin
panel is a stateless Blazor Server app; it does not own a schema.

## 9. Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| Central API keeps restarting; logs show `Jwt:SigningKey must be at least 32 bytes long` | `JWT_SIGNING_KEY` is too short. | Regenerate with `openssl rand -hex 48`. |
| `dotnet restore` fails on Mikro V15/V16 NuGets | The internal NuGet feed isn't reachable from Coolify's build runner. | Either mirror the packages, or change `NuGet.config` to point at a reachable public mirror. |
| Admin panel shows "Network error" on every action | `CENTRALAPI_BASE_URL` is set to the internal Docker hostname from inside the admin container â€” that's wrong. | Use the **public** URL `https://api.erpbridge.example.com`. |
| `401 Unauthorized` everywhere | Clock drift between Coolify's server and your laptop. JWT validation rejects Â±30s skew; bigger drift 401s every call. | Sync clocks via `chrony` or `systemd-timesyncd`. |
| `Database.Migrate()` fails with "relation already exists" | A previous deploy was interrupted mid-migration. | Connect to the database, drop the partial migration row in `__EFMigrationsHistory`, redeploy. |

---

## Appendix: environment variable reference

### Central API

| Variable | Required | Example | Notes |
|---|---|---|---|
| `ConnectionStrings__CentralApi` | yes | `Host=erpbridge-pg;Port=5432;Database=erpbridge;Username=erpbridge;Password=â€¦` | Use the **internal** Coolify hostname. |
| `Jwt__SigningKey` | yes | (64 hex chars) | â‰¥32 bytes. HS256 requires it. |
| `Jwt__Issuer` | no | `ErpBridge.CentralApi` | |
| `Jwt__Audience` | no | `ErpBridge.Agents` | |
| `Jwt__AccessTokenMinutes` | no | `60` | Agent JWT lifetime. |
| `Admin__SeedEmail` | yes | `ops@erpbridge.example.com` | Empty disables seed. |
| `Admin__SeedPassword` | yes | (bootstrap password) | Empty disables seed. |
| `Admin__SeedDisplayName` | no | `Bootstrap Admin` | |
| `PORT` | no | `4001` | Container-side port. Override only if 4001 conflicts on the host; matches the Dockerfile default. |

### Admin panel

| Variable | Required | Example | Notes |
|---|---|---|---|
| `CentralApi__BaseUrl` | yes | `https://api.erpbridge.example.com` | Public URL; do not use the internal Docker hostname. |
| `PORT` | no | `4002` | Container-side port (admin panel uses 4002, centralapi uses 4001). Set automatically by Dockerfile default; override only if those ports conflict on the host. |


