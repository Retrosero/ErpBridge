package com.example.data.database

import android.content.Context
import androidx.room.Room
import androidx.room.migration.Migration
import androidx.sqlite.db.SupportSQLiteDatabase

object DatabaseProvider {
    @Volatile
    private var INSTANCE: AppDatabase? = null

    val MIGRATION_8_9 = object : Migration(8, 9) {
        override fun migrate(db: SupportSQLiteDatabase) {
            db.execSQL(
                """
                CREATE TABLE IF NOT EXISTS `telemetry_events` (
                    `eventId` TEXT NOT NULL,
                    `occurredAtUtc` TEXT NOT NULL,
                    `kind` TEXT NOT NULL,
                    `severity` TEXT NOT NULL,
                    `appVersion` TEXT NOT NULL,
                    `androidVersion` TEXT NOT NULL,
                    `deviceModel` TEXT NOT NULL,
                    `screen` TEXT NOT NULL,
                    `operation` TEXT NOT NULL,
                    `exceptionType` TEXT NOT NULL,
                    `message` TEXT NOT NULL,
                    `stackTrace` TEXT NOT NULL,
                    `httpMethod` TEXT NOT NULL,
                    `httpRoute` TEXT NOT NULL,
                    `httpStatus` INTEGER,
                    `correlationId` TEXT NOT NULL,
                    `breadcrumbsJson` TEXT NOT NULL,
                    PRIMARY KEY(`eventId`)
                )
                """.trimIndent()
            )
        }
    }

    
    val MIGRATION_10_11 = object : Migration(10, 11) {
        override fun migrate(db: SupportSQLiteDatabase) {
            db.execSQL("CREATE TABLE IF NOT EXISTS `cari_hesap_hareketleri` (`id` TEXT NOT NULL, `cariKod` TEXT NOT NULL, `tarih` TEXT NOT NULL, `evrakTip` INTEGER NOT NULL, `evrakNo` TEXT NOT NULL, `tip` INTEGER NOT NULL, `tutar` REAL NOT NULL, `borcMu` INTEGER NOT NULL, `aciklama` TEXT NOT NULL, PRIMARY KEY(`id`))")
            db.execSQL("CREATE TABLE IF NOT EXISTS `stok_hareketleri` (`id` TEXT NOT NULL, `stokKod` TEXT NOT NULL, `tarih` TEXT NOT NULL, `tip` INTEGER NOT NULL, `evrakTip` INTEGER NOT NULL, `evrakNo` TEXT NOT NULL, `miktar` REAL NOT NULL, `birimFiyat` REAL NOT NULL, `tutar` REAL NOT NULL, `cariKod` TEXT NOT NULL, `depoNo` INTEGER NOT NULL, PRIMARY KEY(`id`))")
            db.execSQL("CREATE TABLE IF NOT EXISTS `bridge_bankalar` (`id` TEXT NOT NULL, `kod` TEXT NOT NULL, `isim` TEXT NOT NULL, `sube` TEXT NOT NULL, `iban` TEXT NOT NULL, `hesapNumarasi` TEXT NOT NULL, PRIMARY KEY(`id`))")
            db.execSQL("CREATE TABLE IF NOT EXISTS `cari_adresleri` (`id` TEXT NOT NULL, `cariKod` TEXT NOT NULL, `adresNo` INTEGER NOT NULL, `il` TEXT NOT NULL, `ilce` TEXT NOT NULL, `mahalle` TEXT NOT NULL, `cadde` TEXT NOT NULL, `sokak` TEXT NOT NULL, PRIMARY KEY(`id`))")
        }
    }

    val MIGRATION_9_10 = object : Migration(9, 10) {
        override fun migrate(db: SupportSQLiteDatabase) {
            db.execSQL("ALTER TABLE products ADD COLUMN transactionsJson TEXT")
        }
    }

    fun getDatabase(context: Context): AppDatabase {
        return INSTANCE ?: synchronized(this) {
            val instance = Room.databaseBuilder(
                context.applicationContext,
                AppDatabase::class.java,
                "field_force_db"
            )
            .addMigrations(MIGRATION_8_9, MIGRATION_9_10, MIGRATION_10_11)
            
            .build()
            INSTANCE = instance
            instance
        }
    }
}
