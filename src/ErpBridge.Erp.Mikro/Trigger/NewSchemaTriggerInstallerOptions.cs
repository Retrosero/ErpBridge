using ErpBridge.Shared;

namespace ErpBridge.Erp.Mikro.Trigger;

/// <summary>
/// Configuration for the FORA-style trigger installer
/// (<see cref="NewSchemaTriggerInstaller"/>). The installer is opt-in:
/// <see cref="Enabled"/> is <c>false</c> by default so the legacy
/// <c>_ERPB_SENKRONIZASYON</c>-based pipeline keeps running on existing
/// installations until the operator explicitly switches over.
///
/// <para>
/// The options are read from the
/// <c>ErpBridge:Erp:Mikro:NewSchemaTriggerInstaller</c> configuration
/// section. Example <c>appsettings.json</c> fragment:
/// </para>
/// <code>
/// "ErpBridge": {
///   "Erp": {
///     "Mikro": {
///       "NewSchemaTriggerInstaller": {
///         "Enabled": true,
///         "TrackedTables": [ "STOKLAR", "CARI_HESAPLAR" ],
///         "DropOnUninstall": false
///       }
///     }
///   }
/// }
/// </code>
/// </summary>
public sealed class NewSchemaTriggerInstallerOptions
{
    /// <summary>
    /// Configuration section under which the installer looks for its
    /// settings. Matches the <c>ErpBridge:Erp:Mikro:*</c> nesting used by the
    /// WPF settings tree and the rest of the Mikro adapter.
    /// </summary>
    public const string ConfigurationSection = "ErpBridge:Erp:Mikro:NewSchemaTriggerInstaller";

    /// <summary>
    /// Master switch. When <c>false</c> (the default) the installer is
    /// neither invoked at agent start-up nor exposed in the WPF. The
    /// legacy <c>_ERPB_SENKRONIZASYON</c> trigger pipeline continues to
    /// serve change events.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Optional override for the catalogue of tracked tables. When the list
    /// is empty the installer iterates the full
    /// <see cref="TrackedTableCatalog"/>; when populated only the tables
    /// whose <see cref="TrackedTableSchema.TabloAdi"/> appears (case
    /// insensitively) in the list receive a trigger. The opt-in list is
    /// useful for staged rollouts where the operator wants to switch one
    /// table over at a time.
    /// </summary>
    public string[] TrackedTables { get; set; } = Array.Empty<string>();

    /// <summary>
    /// When <c>true</c> the uninstaller additionally drops the
    /// <c>_ERPB_SYNC</c>, <c>_ERPB_SYNC_DEL</c> and
    /// <c>_ERPB_PARAMETRELER</c> shadow tables. Defaults to
    /// <c>false</c> so an operator who clicks "Uninstall" by accident does
    /// not lose the audit trail. The WPF admin screen surfaces the setting
    /// behind a confirmation dialog when set to <c>true</c>.
    /// </summary>
    public bool DropOnUninstall { get; set; } = false;
}
