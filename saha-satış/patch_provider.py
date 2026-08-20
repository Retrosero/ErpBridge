import re

with open("app/src/main/java/com/example/data/database/DatabaseProvider.kt", "r") as f:
    content = f.read()

migration_10_11 = """
    val MIGRATION_10_11 = object : Migration(10, 11) {
        override fun migrate(db: SupportSQLiteDatabase) {
            db.execSQL("CREATE TABLE IF NOT EXISTS `cari_hesap_hareketleri` (`id` TEXT NOT NULL, `cariKod` TEXT NOT NULL, `tarih` TEXT NOT NULL, `evrakTip` INTEGER NOT NULL, `evrakNo` TEXT NOT NULL, `tip` INTEGER NOT NULL, `tutar` REAL NOT NULL, `borcMu` INTEGER NOT NULL, `aciklama` TEXT NOT NULL, PRIMARY KEY(`id`))")
            db.execSQL("CREATE TABLE IF NOT EXISTS `stok_hareketleri` (`id` TEXT NOT NULL, `stokKod` TEXT NOT NULL, `tarih` TEXT NOT NULL, `tip` INTEGER NOT NULL, `evrakTip` INTEGER NOT NULL, `evrakNo` TEXT NOT NULL, `miktar` REAL NOT NULL, `birimFiyat` REAL NOT NULL, `tutar` REAL NOT NULL, `cariKod` TEXT NOT NULL, `depoNo` INTEGER NOT NULL, PRIMARY KEY(`id`))")
            db.execSQL("CREATE TABLE IF NOT EXISTS `bridge_bankalar` (`id` TEXT NOT NULL, `kod` TEXT NOT NULL, `isim` TEXT NOT NULL, `sube` TEXT NOT NULL, `iban` TEXT NOT NULL, `hesapNumarasi` TEXT NOT NULL, PRIMARY KEY(`id`))")
            db.execSQL("CREATE TABLE IF NOT EXISTS `cari_adresleri` (`id` TEXT NOT NULL, `cariKod` TEXT NOT NULL, `adresNo` INTEGER NOT NULL, `il` TEXT NOT NULL, `ilce` TEXT NOT NULL, `mahalle` TEXT NOT NULL, `cadde` TEXT NOT NULL, `sokak` TEXT NOT NULL, PRIMARY KEY(`id`))")
        }
    }
"""

content = content.replace("val MIGRATION_9_10 = object : Migration(9, 10) {", migration_10_11 + "\n    val MIGRATION_9_10 = object : Migration(9, 10) {")

with open("app/src/main/java/com/example/data/database/DatabaseProvider.kt", "w") as f:
    f.write(content)

