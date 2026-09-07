---
name: spadmin-license-modules
description: Implement SpAdmin API key, license expiry, subscription plan, module catalog and tenant feature flag override behavior.
---

# SpAdmin License Modules

Use this skill for API keys, license dates, expiry behavior, subscription plans and module flags.

## Rules

- API keys are generated once, displayed once and stored only as hashes.
- Refresh tokens are stored only as hashes.
- API keys are for activation/renewal; agents use tokens for normal operation.
- Effective modules are plan defaults plus tenant overrides.
- SuperAdmin is required for license expiry changes and module overrides.
- Expired tenants cannot accept mobile documents or lease agent jobs.
- Expiry never deletes Mikro ERP records.

## Required Tests

- Valid, expired, revoked and wrong-tenant API key paths.
- Plan module defaults.
- Tenant module override enable/disable.
- Non-SuperAdmin blocked from license/module mutation.
- Expiry returns purge command to agent.
