---
name: spadmin-control-plane
description: Build and maintain the SpAdmin central control plane for tenants, agents, heartbeat, jobs, config versions and support operations.
---

# SpAdmin Control Plane

Use this skill whenever implementing tenant management, agent activation, heartbeat, job queue, config versioning or support operations.

## Rules

- Every operational record is tenant scoped with `tenant_id`.
- Agent connections are outbound from customer machines to SpAdmin.
- SpAdmin never connects directly to customer Mikro SQL.
- Heartbeat returns license status, enabled modules, config version and commands.
- Jobs are leased to active, authorized devices only.
- Ack is accepted only for matching tenant, device and leased job.
- Config version increases when license, plan, module or tenant settings change.

## Required Tests

- Cross-tenant access denied.
- Expired tenant receives no jobs.
- Disabled module jobs are not leased.
- Device mismatch rejected.
- Config version changes after module override.
