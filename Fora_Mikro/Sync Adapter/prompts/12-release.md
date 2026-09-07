# 12 - Release Prompt

Implement installer and release workflow.

Installer:

- MSI/MSIX/WiX package.
- Windows Service install/uninstall.
- Auto-start option.
- Firewall behavior documented.
- Clean uninstall policy.

Release:

- Signed installer or trusted download channel.
- Versioned local store migrations.
- Rollback plan.
- Update compatibility tests.

Acceptance:

- Clean Windows machine install succeeds.
- Service starts after reboot.
- UI can configure API key and Mikro SQL connection.
- Uninstall removes service.
- Customer data retention follows documented policy.
