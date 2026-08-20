sed -i 's/version = 10,/version = 11,/' app/src/main/java/com/example/data/database/AppDatabase.kt
sed -i '/WmsOrderItemEntity::class,/a \        CariHesapHareketEntity::class,\n        StokHareketEntity::class,\n        BridgeBankaEntity::class,\n        CariAdresEntity::class,' app/src/main/java/com/example/data/database/AppDatabase.kt

cat >> app/src/main/java/com/example/data/database/AppDatabase.kt << 'INNEREOF'

@Dao
interface CariHesapHareketDao {
    @Query("SELECT * FROM cari_hesap_hareketleri")
    suspend fun getAll(): List<CariHesapHareketEntity>
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(items: List<CariHesapHareketEntity>)
    @Query("DELETE FROM cari_hesap_hareketleri")
    suspend fun deleteAll()
}

@Dao
interface StokHareketDao {
    @Query("SELECT * FROM stok_hareketleri")
    suspend fun getAll(): List<StokHareketEntity>
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(items: List<StokHareketEntity>)
    @Query("DELETE FROM stok_hareketleri")
    suspend fun deleteAll()
}

@Dao
interface BridgeBankaDao {
    @Query("SELECT * FROM bridge_bankalar")
    suspend fun getAll(): List<BridgeBankaEntity>
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(items: List<BridgeBankaEntity>)
    @Query("DELETE FROM bridge_bankalar")
    suspend fun deleteAll()
}

@Dao
interface CariAdresDao {
    @Query("SELECT * FROM cari_adresleri")
    suspend fun getAll(): List<CariAdresEntity>
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(items: List<CariAdresEntity>)
    @Query("DELETE FROM cari_adresleri")
    suspend fun deleteAll()
}
INNEREOF

sed -i '/abstract fun telemetryDao(): TelemetryDao/a \    abstract fun cariHesapHareketDao(): CariHesapHareketDao\n    abstract fun stokHareketDao(): StokHareketDao\n    abstract fun bridgeBankaDao(): BridgeBankaDao\n    abstract fun cariAdresDao(): CariAdresDao' app/src/main/java/com/example/data/database/AppDatabase.kt
