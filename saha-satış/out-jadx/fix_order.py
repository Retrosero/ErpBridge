import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r', encoding='utf-8') as f:
    content = f.read()

# Reorder sync tasks in ErpIntegrationScreen.kt
new_tasks = """val syncTasks = remember {
        listOf(
            object : SyncTask() {
                override val name = "Cari Kartlar"
                override val description = "CARI_HESAPLAR kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncCariler(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Stok Kartları"
                override val description = "STOKLAR kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncUrunler(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Cari Hesap Hareketleri"
                override val description = "CARI_HESAP_HAREKETLERI kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncCariHareketleri(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Fatura Detayları & Kalemler"
                override val description = "faturaHareket kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncFaturaHareket(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Stok Seviyeleri (Eldeki Miktar)"
                override val description = "STOK_HAREKETTEN_ELDEKI_MIKTAR_VIEW kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncStokSeviyeleri(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Fiyat Liste Tanımları"
                override val description = "STOK_SATIS_FIYAT_LISTE_TANIMLARI kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncFiyatListeleri(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Gelişmiş Fiyat Listeleri"
                override val description = "fiyatListesi kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncFiyatListesiNew(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Cari Hesap Adresleri"
                override val description = "CARI_HESAP_ADRESLERI kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncCariAdresleri(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Cari Banka Hesapları"
                override val description = "cariBankaHesaplari kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncCariBankaHesaplari(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Banka Tanımları"
                override val description = "BANKALAR kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncBankalar(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Kasa Tanımları"
                override val description = "KASALAR kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncKasalar(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Kasa Yönetimi & Muhasebe"
                override val description = "KASALAR_YONETIM kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncKasaYonetim(ctx, url, key, log, progress)
                }
            },
            object : SyncTask() {
                override val name = "Köprü Durumu & Watermarklar"
                override val description = "Watermarks kopyalanıyor..."
                override suspend fun execute(ctx: Context, url: String, key: String, log: (String) -> Unit, progress: (Float) -> Unit) {
                    BridgeSyncHelper.syncStatusCheck(ctx, url, key, log, progress)
                }
            }
        )
    }"""

content = re.sub(
    r'val syncTasks = remember \{.*?\}\n        \)\n    \}',
    new_tasks,
    content,
    flags=re.DOTALL
)

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'w', encoding='utf-8') as f:
    f.write(content)
