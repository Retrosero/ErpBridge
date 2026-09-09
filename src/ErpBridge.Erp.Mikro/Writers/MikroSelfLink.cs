namespace ErpBridge.Erp.Mikro.Writers;

/// <summary>
/// Helpers for Mikro's <c>*_RECid_DBCno</c> / <c>*_RECid_RECno</c> self-link pair.
///
/// <para>
/// Every Mikro master and movement table carries this pair and enforces a
/// <b>unique</b> index over it. For a row created in the database it points at
/// itself: <c>RECid_DBCno = 0</c> and <c>RECid_RECno = RECno</c> — verified
/// against a live V15 install where 4558 of 4559 <c>STOKLAR</c> rows and 522 of
/// 524 <c>CARI_HESAPLAR</c> rows follow it (the exceptions are records copied in
/// from another database, which is what the pair exists to express).
/// </para>
///
/// <para>
/// <b>Why a seed is needed.</b> <c>RECno</c> is an IDENTITY column, so its value
/// is unknown until the INSERT completes — yet the unique index rejects a
/// placeholder of <c>0</c> as soon as a second row uses it. Each INSERT
/// therefore seeds the link with a unique negative number and a follow-up UPDATE
/// inside the same transaction resolves it to the row's own identity. Negative
/// values can never collide with a real RECno.
/// </para>
/// </summary>
public static class MikroSelfLink
{
    /// <summary>
    /// Active-DB number for a row that lives in the database that created it.
    /// Always <c>0</c> for the agent, which never writes cross-database.
    /// </summary>
    public const short ActiveDbNo = 0;

    /// <summary>
    /// T-SQL fragment that declares <c>@SelfLinkSeed</c> — a unique negative
    /// placeholder for the <c>*_RECid_RECno</c> column. Prepend it to any INSERT
    /// that binds <c>@SelfLinkSeed</c>.
    /// </summary>
    public const string SeedDeclaration = "DECLARE @SelfLinkSeed INT = -ABS(CHECKSUM(NEWID()));";

    /// <summary>
    /// Build the UPDATE that resolves a freshly inserted row's self-link to its
    /// own identity.
    /// </summary>
    /// <param name="table">Mikro table name, e.g. <c>CARI_HESAP_HAREKETLERI</c>.</param>
    /// <param name="prefix">That table's column prefix, e.g. <c>cha</c>.</param>
    /// <returns>
    /// A statement binding <c>@ActiveDbNo</c> and <c>@Recno</c>. Table and prefix
    /// are compile-time constants supplied by the writers, never user input.
    /// </returns>
    public static string BuildUpdate(string table, string prefix) => $@"
UPDATE {table}
SET {prefix}_RECid_DBCno = @ActiveDbNo,
    {prefix}_RECid_RECno = @Recno
WHERE {prefix}_RECno = @Recno;";

    /// <summary>
    /// Build the UPDATE that resolves a V16 row's self-link, keyed by the
    /// application-generated Guid because the identity is not round-tripped.
    /// </summary>
    public static string BuildUpdateByGuid(string table, string prefix) => $@"
UPDATE {table}
SET {prefix}_RECid_DBCno = @ActiveDbNo,
    {prefix}_RECid_RECno = {prefix}_RECno
WHERE {prefix}_Guid = @RowGuid;";
}
